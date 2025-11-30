using IoT_System.Application.Common;
using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Services.IoT;

public class ThresholdService : IThresholdService
{
    private readonly IDeviceFieldRepository _fieldRepository;
    private readonly IFieldMeasurementValueRepository _valueRepository;

    public ThresholdService(
        IDeviceFieldRepository fieldRepository,
        IFieldMeasurementValueRepository valueRepository)
    {
        _fieldRepository = fieldRepository;
        _valueRepository = valueRepository;
    }

    public async Task<OperationResult<List<ThresholdStatus>>> CheckThresholdsAsync(DeviceMeasurement measurement)
    {
        return await ExecuteAsync(async () =>
        {
            var exceededThresholds = new List<ThresholdStatus>();

            foreach (var fieldValue in measurement.FieldValues)
            {
                var fieldResult = await _fieldRepository.GetByIdAsync(fieldValue.FieldId);
                if (!fieldResult.IsSuccess || fieldResult.Data == null) continue;

                var field = fieldResult.Data;

                // Skip if no thresholds configured
                if (!field.WarningMin.HasValue && !field.WarningMax.HasValue &&
                    !field.CriticalMin.HasValue && !field.CriticalMax.HasValue)
                {
                    continue;
                }

                // Parse value based on field data type
                decimal? numericValue = null;
                if (field.DataType == FieldDataType.Decimal || field.DataType == FieldDataType.Integer)
                {
                    if (decimal.TryParse(fieldValue.Value, out var parsed))
                    {
                        numericValue = parsed;
                    }
                }

                if (!numericValue.HasValue) continue;

                var status = GetThresholdStatus(field, numericValue);

                if (status != Constants.Thresholds.Normal)
                {
                    decimal? thresholdValue = null;

                    if (status == Constants.Thresholds.Critical)
                    {
                        thresholdValue = numericValue < 0 && field.CriticalMin.HasValue
                            ? field.CriticalMin
                            : field.CriticalMax;
                    }
                    else if (status == Constants.Thresholds.Warning)
                    {
                        thresholdValue = numericValue < 0 && field.WarningMin.HasValue
                            ? field.WarningMin
                            : field.WarningMax;
                    }

                    exceededThresholds.Add(new ThresholdStatus
                    {
                        FieldId = field.Id,
                        FieldName = field.DisplayName,
                        Status = status,
                        Value = numericValue,
                        ThresholdValue = thresholdValue,
                        Unit = field.Unit
                    });
                }
            }

            return exceededThresholds;
        });
    }

    public string GetThresholdStatus(DeviceField field, decimal? value)
    {
        if (!value.HasValue) return Constants.Thresholds.Normal;

        // Check Critical thresholds first
        if (field.CriticalMin.HasValue && value < field.CriticalMin)
        {
            return Constants.Thresholds.Critical;
        }

        if (field.CriticalMax.HasValue && value > field.CriticalMax)
        {
            return Constants.Thresholds.Critical;
        }

        // Check Warning thresholds
        if (field.WarningMin.HasValue && value < field.WarningMin)
        {
            return Constants.Thresholds.Warning;
        }

        if (field.WarningMax.HasValue && value > field.WarningMax)
        {
            return Constants.Thresholds.Warning;
        }

        return Constants.Thresholds.Normal;
    }
}