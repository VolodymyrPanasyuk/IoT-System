using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Services.IoT;

public class DataParsingService : IDataParsingService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceFieldRepository _fieldRepository;
    private readonly IFieldMappingRepository _fieldMappingRepository;
    private readonly IMeasurementDateMappingRepository _dateMappingRepository;
    private readonly IFieldMeasurementValueRepository _valueRepository;

    public DataParsingService(
        IDeviceRepository deviceRepository,
        IDeviceFieldRepository fieldRepository,
        IFieldMappingRepository fieldMappingRepository,
        IMeasurementDateMappingRepository dateMappingRepository,
        IFieldMeasurementValueRepository valueRepository)
    {
        _deviceRepository = deviceRepository;
        _fieldRepository = fieldRepository;
        _fieldMappingRepository = fieldMappingRepository;
        _dateMappingRepository = dateMappingRepository;
        _valueRepository = valueRepository;
    }

    public async Task<OperationResult<DeviceMeasurement>> ParseAndCreateMeasurementAsync(
        Guid deviceId,
        string rawData,
        DataFormat dataFormat)
    {
        return await ExecuteAsync(async Task<DeviceMeasurement> () =>
        {
            var deviceResult = await _deviceRepository.GetByIdAsync(deviceId);
            if (!deviceResult.IsSuccess || deviceResult.Data == null)
            {
                return OperationResult<DeviceMeasurement>.NotFound($"Device with ID {deviceId} not found");
            }

            var measurement = new DeviceMeasurement
            {
                DeviceId = deviceId,
                RawData = rawData,
                DataFormat = dataFormat,
                CreatedOn = DateTime.UtcNow,
                ParsedSuccessfully = true
            };

            var errors = new List<string>();

            // Parse measurement date
            var dateMappingResult = await _dateMappingRepository.GetActiveByDeviceAndFormatAsync(deviceId, dataFormat);
            var dateResult = ParseMeasurementDate(rawData, dataFormat, dateMappingResult.Data);

            if (dateResult.IsSuccess)
            {
                measurement.MeasurementDate = dateResult.Data;
            }
            else
            {
                measurement.MeasurementDate = measurement.CreatedOn;
                errors.Add($"Could not parse measurement date, using CreatedOn. Error: {string.Join(", ", dateResult.Errors)}");
            }

            // Get all active fields for device
            var fieldsResult = await _fieldRepository.GetByDeviceIdAsync(deviceId, activeOnly: true);
            if (!fieldsResult.IsSuccess || fieldsResult.Data == null || !fieldsResult.Data.Any())
            {
                measurement.ParsedSuccessfully = false;
                measurement.ParsingErrors = "No active fields configured for this device";
                return measurement;
            }

            var fieldValues = new List<FieldMeasurementValue>();

            // Parse each field
            foreach (var field in fieldsResult.Data)
            {
                var mappingResult = await _fieldMappingRepository.GetActiveByFieldAndFormatAsync(field.Id, dataFormat);

                if (mappingResult.Data == null)
                {
                    errors.Add($"No active mapping found for field '{field.FieldName}' and format {dataFormat}");
                    continue;
                }

                var mapping = mappingResult.Data;

                // Extract value using source path
                var extractResult = ExtractValue(rawData, dataFormat, mapping.SourceFieldPath);
                if (!extractResult.IsSuccess)
                {
                    errors.Add($"Field '{field.FieldName}': {string.Join(", ", extractResult.Errors)}");
                    continue;
                }

                // Apply transformations
                var transformResult = ApplyTransformations(extractResult.Data, mapping.TransformationPipeline);
                if (!transformResult.IsSuccess)
                {
                    errors.Add($"Field '{field.FieldName}': {string.Join(", ", transformResult.Errors)}");
                    continue;
                }

                fieldValues.Add(new FieldMeasurementValue
                {
                    FieldId = field.Id,
                    Value = transformResult.Data?.ToString() ?? string.Empty
                });
            }

            if (errors.Any())
            {
                measurement.ParsedSuccessfully = false;
                measurement.ParsingErrors = string.Join("; ", errors);
            }

            measurement.FieldValues = fieldValues;

            return measurement;
        });
    }

    public OperationResult<object?> ExtractValue(string rawData, DataFormat dataFormat, string? sourcePath)
    {
        return Execute(object? () =>
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                // PlainText case - return entire raw data
                return rawData;
            }

            switch (dataFormat)
            {
                case DataFormat.Json:
                    return ExtractFromJson(rawData, sourcePath);

                case DataFormat.Xml:
                    return ExtractFromXml(rawData, sourcePath);

                case DataFormat.PlainText:
                    return rawData;

                default:
                    return OperationResult<object?>.BadRequest($"Unsupported data format: {dataFormat}");
            }
        });
    }

    public OperationResult<object?> ApplyTransformations(object? value, string? transformationPipeline)
    {
        if (string.IsNullOrWhiteSpace(transformationPipeline))
        {
            return OperationResult<object?>.Success(value);
        }

        return Execute(() =>
        {
            var transformations = JsonSerializer.Deserialize<List<JsonElement>>(transformationPipeline);
            if (transformations == null || !transformations.Any())
            {
                return value;
            }

            object? currentValue = value;

            foreach (var transformation in transformations)
            {
                var type = transformation.GetProperty("type").GetString();
                var config = transformation.TryGetProperty("config", out var configElement)
                    ? configElement
                    : (JsonElement?)null;

                currentValue = ApplySingleTransformation(currentValue, type!, config);
            }

            return currentValue;
        });
    }

    public OperationResult<DateTime> ParseMeasurementDate(string rawData, DataFormat dataFormat, MeasurementDateMapping? mapping)
    {
        if (mapping == null || !mapping.IsActive)
        {
            return OperationResult<DateTime>.Success(DateTime.UtcNow);
        }

        var extractResult = ExtractValue(rawData, dataFormat, mapping.SourceFieldPath);
        if (!extractResult.IsSuccess)
        {
            return extractResult.ToOperationResult<DateTime>();
        }

        var transformResult = ApplyTransformations(extractResult.Data, mapping.TransformationPipeline);
        if (!transformResult.IsSuccess)
        {
            return transformResult.ToOperationResult<DateTime>();
        }

        return Execute(DateTime () =>
        {
            if (transformResult.Data is DateTime dateTime)
            {
                return dateTime;
            }

            if (DateTime.TryParse(transformResult.Data?.ToString(), out var parsedDate))
            {
                return parsedDate;
            }

            return OperationResult<DateTime>.BadRequest("Could not parse value as DateTime");
        });
    }

    #region Private Helper Methods

    private object? ExtractFromJson(string rawData, string path)
    {
        var json = JsonDocument.Parse(rawData);
        var parts = path.Split('.');
        JsonElement current = json.RootElement;

        foreach (var part in parts)
        {
            if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(part, out var element))
            {
                current = element;
            }
            else
            {
                throw new Exception($"Path '{part}' not found in JSON");
            }
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => current,
            JsonValueKind.Object => current,
            _ => null
        };
    }

    private object? ExtractFromXml(string rawData, string path)
    {
        var xml = XDocument.Parse(rawData);
        var parts = path.Split('.');
        XElement? current = xml.Root;

        foreach (var part in parts)
        {
            current = current?.Element(part);
            if (current == null)
            {
                throw new Exception($"Path '{part}' not found in XML");
            }
        }

        return current?.Value;
    }

    private object? ApplySingleTransformation(object? value, string type, JsonElement? config)
    {
        if (value == null) return null;

        var transformationType = Enum.Parse<TransformationType>(type);

        switch (transformationType)
        {
            case TransformationType.None:
                return value;

            // String Operations
            case TransformationType.Split:
                var delimiter = config?.GetProperty("delimiter").GetString() ?? ",";
                var position = config?.GetProperty("position").GetInt32() ?? 0;
                var parts = value.ToString()!.Split(delimiter);
                return position < parts.Length ? parts[position] : null;

            case TransformationType.Substring:
                var startIndex = config?.GetProperty("startIndex").GetInt32() ?? 0;
                var length = config?.TryGetProperty("length", out var lengthProp) == true
                    ? lengthProp.GetInt32()
                    : (int?)null;
                var str = value.ToString()!;
                return length.HasValue ? str.Substring(startIndex, length.Value) : str.Substring(startIndex);

            case TransformationType.RegexMatch:
                var pattern = config?.GetProperty("pattern").GetString()!;
                var group = config?.TryGetProperty("group", out var groupProp) == true ? groupProp.GetInt32() : 1;
                var match = Regex.Match(value.ToString()!, pattern);
                return match.Success && match.Groups.Count > group ? match.Groups[group].Value : null;

            case TransformationType.Replace:
                var oldValue = config?.GetProperty("oldValue").GetString()!;
                var newValue = config?.GetProperty("newValue").GetString()!;
                return value.ToString()!.Replace(oldValue, newValue);

            case TransformationType.Trim:
                var chars = config?.TryGetProperty("characters", out var charsProp) == true
                    ? charsProp.GetString()
                    : null;
                return chars != null ? value.ToString()!.Trim(chars.ToCharArray()) : value.ToString()!.Trim();

            // Numeric Operations
            case TransformationType.Add:
                var addValue = config?.GetProperty("value").GetDecimal() ?? 0;
                return Convert.ToDecimal(value) + addValue;

            case TransformationType.Subtract:
                var subValue = config?.GetProperty("value").GetDecimal() ?? 0;
                return Convert.ToDecimal(value) - subValue;

            case TransformationType.Multiply:
                var mulValue = config?.GetProperty("value").GetDecimal() ?? 1;
                return Convert.ToDecimal(value) * mulValue;

            case TransformationType.Divide:
                var divValue = config?.GetProperty("value").GetDecimal() ?? 1;
                return Convert.ToDecimal(value) / divValue;

            case TransformationType.Round:
                var decimals = config?.GetProperty("decimals").GetInt32() ?? 0;
                return Math.Round(Convert.ToDecimal(value), decimals);

            case TransformationType.Floor:
                return Math.Floor(Convert.ToDecimal(value));

            case TransformationType.Ceiling:
                return Math.Ceiling(Convert.ToDecimal(value));

            case TransformationType.Abs:
                return Math.Abs(Convert.ToDecimal(value));

            // Type Conversion
            case TransformationType.ToInteger:
                return Convert.ToInt32(value);

            case TransformationType.ToDecimal:
                return Convert.ToDecimal(value);

            case TransformationType.ToString:
                return value.ToString();

            case TransformationType.ToDateTime:
                var format = config?.GetProperty("format").GetString();
                var culture = config?.TryGetProperty("culture", out var cultureProp) == true
                    ? cultureProp.GetString()
                    : "en-US";
                return DateTime.ParseExact(value.ToString()!, format!, new CultureInfo(culture!));

            case TransformationType.UnixTimestamp:
                var unit = config?.TryGetProperty("unit", out var unitProp) == true
                    ? unitProp.GetString()
                    : "seconds";
                var timestamp = Convert.ToInt64(value);
                return unit == "milliseconds"
                    ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime
                    : DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

            default:
                return value;
        }
    }

    #endregion
}