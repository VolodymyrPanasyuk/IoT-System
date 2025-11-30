using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Application.Common.Helpers;

public static class TransformationConfigHelper
{
    public static List<TransformationConfigModel> GetAllTransformationConfigs()
    {
        return
        [
            new()
            {
                Type = (int)TransformationType.None,
                TypeName = nameof(TransformationType.None),
                Category = "Direct Mapping",
                Description = "No transformation - direct value mapping",
                ConfigProperties = []
            },

            // Encoding/Decoding

            new()
            {
                Type = (int)TransformationType.HexDecode,
                TypeName = nameof(TransformationType.HexDecode),
                Category = "Encoding/Decoding",
                Description = "Decode hex string to bytes then to string/number",
                ConfigProperties =
                [
                    new() { Name = "encoding", Type = "string", Required = false, DefaultValue = "utf8", AllowedValues = ["utf8", "ascii"] },
                    new()
                    {
                        Name = "outputType", Type = "string", Required = false, DefaultValue = "string",
                        AllowedValues = ["string", "integer", "decimal"]
                    }
                ]
            },

            new()
            {
                Type = (int)TransformationType.Base64Decode,
                TypeName = nameof(TransformationType.Base64Decode),
                Category = "Encoding/Decoding",
                Description = "Decode base64 string",
                ConfigProperties =
                [
                    new() { Name = "encoding", Type = "string", Required = false, DefaultValue = "utf8", AllowedValues = ["utf8", "ascii"] },
                    new()
                    {
                        Name = "outputType", Type = "string", Required = false, DefaultValue = "string",
                        AllowedValues = ["string", "integer", "decimal"]
                    }
                ]
            },

            // String Operations

            new()
            {
                Type = (int)TransformationType.Split,
                TypeName = nameof(TransformationType.Split),
                Category = "String Operations",
                Description = "Split string by delimiter and take value at position",
                ConfigProperties =
                [
                    new() { Name = "delimiter", Type = "string", Required = true, Description = "Delimiter to split by" },
                    new() { Name = "position", Type = "integer", Required = true, Description = "Index of element to extract (0-based)" }
                ]
            },

            new()
            {
                Type = (int)TransformationType.Substring,
                TypeName = nameof(TransformationType.Substring),
                Category = "String Operations",
                Description = "Extract substring",
                ConfigProperties =
                [
                    new() { Name = "startIndex", Type = "integer", Required = true },
                    new() { Name = "length", Type = "integer", Required = false }
                ]
            },

            new()
            {
                Type = (int)TransformationType.RegexMatch,
                TypeName = nameof(TransformationType.RegexMatch),
                Category = "String Operations",
                Description = "Regex match and capture group",
                ConfigProperties =
                [
                    new() { Name = "pattern", Type = "string", Required = true },
                    new() { Name = "group", Type = "integer", Required = false, DefaultValue = 1 }
                ]
            },

            new()
            {
                Type = (int)TransformationType.Replace,
                TypeName = nameof(TransformationType.Replace),
                Category = "String Operations",
                Description = "Replace string",
                ConfigProperties =
                [
                    new() { Name = "oldValue", Type = "string", Required = true },
                    new() { Name = "newValue", Type = "string", Required = true }
                ]
            },

            new()
            {
                Type = (int)TransformationType.Trim,
                TypeName = nameof(TransformationType.Trim),
                Category = "String Operations",
                Description = "Trim whitespace or specific characters",
                ConfigProperties = [new() { Name = "characters", Type = "string", Required = false, Description = "Characters to trim (optional)" }]
            },

            // Numeric Operations

            new()
            {
                Type = (int)TransformationType.Add,
                TypeName = nameof(TransformationType.Add),
                Category = "Numeric Operations",
                Description = "Add value",
                ConfigProperties = [new() { Name = "value", Type = "decimal", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.Subtract,
                TypeName = nameof(TransformationType.Subtract),
                Category = "Numeric Operations",
                Description = "Subtract value",
                ConfigProperties = [new() { Name = "value", Type = "decimal", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.Multiply,
                TypeName = nameof(TransformationType.Multiply),
                Category = "Numeric Operations",
                Description = "Multiply by value",
                ConfigProperties = [new() { Name = "value", Type = "decimal", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.Divide,
                TypeName = nameof(TransformationType.Divide),
                Category = "Numeric Operations",
                Description = "Divide by value",
                ConfigProperties = [new() { Name = "value", Type = "decimal", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.Modulo,
                TypeName = nameof(TransformationType.Modulo),
                Category = "Numeric Operations",
                Description = "Modulo operation",
                ConfigProperties = [new() { Name = "value", Type = "decimal", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.Round,
                TypeName = nameof(TransformationType.Round),
                Category = "Numeric Operations",
                Description = "Round to decimal places",
                ConfigProperties = [new() { Name = "decimals", Type = "integer", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.Floor,
                TypeName = nameof(TransformationType.Floor),
                Category = "Numeric Operations",
                Description = "Floor value",
                ConfigProperties = []
            },

            new()
            {
                Type = (int)TransformationType.Ceiling,
                TypeName = nameof(TransformationType.Ceiling),
                Category = "Numeric Operations",
                Description = "Ceiling value",
                ConfigProperties = []
            },

            new()
            {
                Type = (int)TransformationType.Abs,
                TypeName = nameof(TransformationType.Abs),
                Category = "Numeric Operations",
                Description = "Absolute value",
                ConfigProperties = []
            },

            // Array Operations

            new()
            {
                Type = (int)TransformationType.ArrayIndex,
                TypeName = nameof(TransformationType.ArrayIndex),
                Category = "Array/Collection Operations",
                Description = "Get array element at index",
                ConfigProperties = [new() { Name = "index", Type = "integer", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.ArrayFirst,
                TypeName = nameof(TransformationType.ArrayFirst),
                Category = "Array/Collection Operations",
                Description = "Get first element",
                ConfigProperties = []
            },

            new()
            {
                Type = (int)TransformationType.ArrayLast,
                TypeName = nameof(TransformationType.ArrayLast),
                Category = "Array/Collection Operations",
                Description = "Get last element",
                ConfigProperties = []
            },

            new()
            {
                Type = (int)TransformationType.ArrayLength,
                TypeName = nameof(TransformationType.ArrayLength),
                Category = "Array/Collection Operations",
                Description = "Get array length/count",
                ConfigProperties = []
            },

            // Type Conversion

            new()
            {
                Type = (int)TransformationType.ToInteger,
                TypeName = nameof(TransformationType.ToInteger),
                Category = "Type Conversion",
                Description = "Convert to integer",
                ConfigProperties = []
            },

            new()
            {
                Type = (int)TransformationType.ToDecimal,
                TypeName = nameof(TransformationType.ToDecimal),
                Category = "Type Conversion",
                Description = "Convert to decimal",
                ConfigProperties = []
            },

            new()
            {
                Type = (int)TransformationType.ToBoolean,
                TypeName = nameof(TransformationType.ToBoolean),
                Category = "Type Conversion",
                Description = "Convert to boolean",
                ConfigProperties =
                [
                    new() { Name = "trueValues", Type = "array", Required = false, DefaultValue = new[] { "1", "true", "yes" } },
                    new() { Name = "falseValues", Type = "array", Required = false, DefaultValue = new[] { "0", "false", "no" } }
                ]
            },

            new()
            {
                Type = (int)TransformationType.ToString,
                TypeName = nameof(TransformationType.ToString),
                Category = "Type Conversion",
                Description = "Convert to string",
                ConfigProperties = []
            },

            new()
            {
                Type = (int)TransformationType.ToDateTime,
                TypeName = nameof(TransformationType.ToDateTime),
                Category = "Type Conversion",
                Description = "Parse DateTime",
                ConfigProperties =
                [
                    new() { Name = "format", Type = "string", Required = true, Description = "DateTime format pattern" },
                    new() { Name = "culture", Type = "string", Required = false, DefaultValue = "en-US" }
                ]
            },

            // Conditional/Mapping

            new()
            {
                Type = (int)TransformationType.RangeMapping,
                TypeName = nameof(TransformationType.RangeMapping),
                Category = "Conditional/Mapping",
                Description = "Map value ranges to different outputs",
                ConfigProperties = [new() { Name = "ranges", Type = "array", Required = true, Description = "Array of range objects with min, max, output" }]
            },

            new()
            {
                Type = (int)TransformationType.ValueMapping,
                TypeName = nameof(TransformationType.ValueMapping),
                Category = "Conditional/Mapping",
                Description = "Map specific values to outputs (like switch/case)",
                ConfigProperties =
                [
                    new() { Name = "mappings", Type = "object", Required = true, Description = "Object with value mappings" },
                    new() { Name = "default", Type = "string", Required = false }
                ]
            },

            new()
            {
                Type = (int)TransformationType.Conditional,
                TypeName = nameof(TransformationType.Conditional),
                Category = "Conditional/Mapping",
                Description = "Conditional operation (if-then-else)",
                ConfigProperties =
                [
                    new()
                    {
                        Name = "condition", Type = "string", Required = true,
                        AllowedValues = ["greaterThan", "lessThan", "equals", "notEquals"]
                    },

                    new() { Name = "value", Type = "decimal", Required = true },
                    new() { Name = "trueResult", Type = "string", Required = true },
                    new() { Name = "falseResult", Type = "string", Required = true }
                ]
            },

            // DateTime Operations

            new()
            {
                Type = (int)TransformationType.UnixTimestamp,
                TypeName = nameof(TransformationType.UnixTimestamp),
                Category = "Date/Time Operations",
                Description = "Parse Unix timestamp (seconds since epoch)",
                ConfigProperties =
                [
                    new()
                    {
                        Name = "unit", Type = "string", Required = false, DefaultValue = "seconds",
                        AllowedValues = ["seconds", "milliseconds"]
                    }
                ]
            },

            new()
            {
                Type = (int)TransformationType.AddTimeSpan,
                TypeName = nameof(TransformationType.AddTimeSpan),
                Category = "Date/Time Operations",
                Description = "Add time span",
                ConfigProperties =
                [
                    new() { Name = "hours", Type = "integer", Required = false, DefaultValue = 0 },
                    new() { Name = "minutes", Type = "integer", Required = false, DefaultValue = 0 }
                ]
            },

            new()
            {
                Type = (int)TransformationType.FormatDateTime,
                TypeName = nameof(TransformationType.FormatDateTime),
                Category = "Date/Time Operations",
                Description = "Format DateTime to string",
                ConfigProperties =
                [
                    new() { Name = "format", Type = "string", Required = true },
                    new() { Name = "culture", Type = "string", Required = false, DefaultValue = "en-US" }
                ]
            },

            // Bitwise Operations

            new()
            {
                Type = (int)TransformationType.BitwiseAnd,
                TypeName = nameof(TransformationType.BitwiseAnd),
                Category = "Bitwise Operations",
                Description = "Bitwise AND",
                ConfigProperties = [new() { Name = "mask", Type = "integer", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.BitwiseOr,
                TypeName = nameof(TransformationType.BitwiseOr),
                Category = "Bitwise Operations",
                Description = "Bitwise OR",
                ConfigProperties = [new() { Name = "mask", Type = "integer", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.BitwiseXor,
                TypeName = nameof(TransformationType.BitwiseXor),
                Category = "Bitwise Operations",
                Description = "Bitwise XOR",
                ConfigProperties = [new() { Name = "mask", Type = "integer", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.ShiftLeft,
                TypeName = nameof(TransformationType.ShiftLeft),
                Category = "Bitwise Operations",
                Description = "Bit shift left",
                ConfigProperties = [new() { Name = "positions", Type = "integer", Required = true }]
            },

            new()
            {
                Type = (int)TransformationType.ShiftRight,
                TypeName = nameof(TransformationType.ShiftRight),
                Category = "Bitwise Operations",
                Description = "Bit shift right",
                ConfigProperties = [new() { Name = "positions", Type = "integer", Required = true }]
            },

            // Advanced

            new()
            {
                Type = (int)TransformationType.ExtractBits,
                TypeName = nameof(TransformationType.ExtractBits),
                Category = "Advanced",
                Description = "Extract bit range and convert to number",
                ConfigProperties =
                [
                    new() { Name = "startBit", Type = "integer", Required = true },
                    new() { Name = "bitCount", Type = "integer", Required = true }
                ]
            },

            new()
            {
                Type = (int)TransformationType.ValidateChecksum,
                TypeName = nameof(TransformationType.ValidateChecksum),
                Category = "Advanced",
                Description = "Apply checksum validation",
                ConfigProperties =
                [
                    new() { Name = "algorithm", Type = "string", Required = true, AllowedValues = ["crc16", "crc32", "xor"] },
                    new() { Name = "expectedPosition", Type = "integer", Required = false, DefaultValue = -1 }
                ]
            }
        ];
    }
}