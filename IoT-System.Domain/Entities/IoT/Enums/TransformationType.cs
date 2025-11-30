namespace IoT_System.Domain.Entities.IoT.Enums;

/// <summary>
/// Pre-defined transformation types for data parsing
/// Each type has specific configuration parameters in ParsingConfiguration
/// </summary>
public enum TransformationType
{
    // ====== Direct Mapping ======
    /// <summary>
    /// No transformation - direct value mapping
    /// </summary>
    None = 0,

    // ====== Encoding/Decoding ======
    /// <summary>
    /// Decode hex string to bytes then to string/number
    /// Config: { "encoding": "utf8" | "ascii", "outputType": "string" | "integer" | "decimal" }
    /// </summary>
    HexDecode = 10,

    /// <summary>
    /// Decode base64 string
    /// Config: { "encoding": "utf8" | "ascii", "outputType": "string" | "integer" | "decimal" }
    /// </summary>
    Base64Decode = 11,

    // ====== String Operations ======
    /// <summary>
    /// Split string by delimiter and take value at position
    /// Config: { "delimiter": ",", "position": 0 }
    /// </summary>
    Split = 20,

    /// <summary>
    /// Extract substring
    /// Config: { "startIndex": 0, "length": 5 }
    /// </summary>
    Substring = 21,

    /// <summary>
    /// Regex match and capture group
    /// Config: { "pattern": "temp:(\\d+\\.\\d+)", "group": 1 }
    /// </summary>
    RegexMatch = 22,

    /// <summary>
    /// Replace string
    /// Config: { "oldValue": "°C", "newValue": "" }
    /// </summary>
    Replace = 23,

    /// <summary>
    /// Trim whitespace or specific characters
    /// Config: { "characters": " \t" } (optional)
    /// </summary>
    Trim = 24,

    // ====== Numeric Operations ======
    /// <summary>
    /// Add value
    /// Config: { "value": 10 }
    /// </summary>
    Add = 30,

    /// <summary>
    /// Subtract value
    /// Config: { "value": 5 }
    /// </summary>
    Subtract = 31,

    /// <summary>
    /// Multiply by value
    /// Config: { "value": 2 }
    /// </summary>
    Multiply = 32,

    /// <summary>
    /// Divide by value
    /// Config: { "value": 10 }
    /// </summary>
    Divide = 33,

    /// <summary>
    /// Modulo operation
    /// Config: { "value": 100 }
    /// </summary>
    Modulo = 34,

    /// <summary>
    /// Round to decimal places
    /// Config: { "decimals": 2 }
    /// </summary>
    Round = 35,

    /// <summary>
    /// Floor value
    /// </summary>
    Floor = 36,

    /// <summary>
    /// Ceiling value
    /// </summary>
    Ceiling = 37,

    /// <summary>
    /// Absolute value
    /// </summary>
    Abs = 38,

    // ====== Array/Collection Operations ======
    /// <summary>
    /// Get array element at index
    /// Config: { "index": 0 }
    /// </summary>
    ArrayIndex = 40,

    /// <summary>
    /// Get first element
    /// </summary>
    ArrayFirst = 41,

    /// <summary>
    /// Get last element
    /// </summary>
    ArrayLast = 42,

    /// <summary>
    /// Get array length/count
    /// </summary>
    ArrayLength = 43,

    // ====== Type Conversion ======
    /// <summary>
    /// Convert to integer
    /// </summary>
    ToInteger = 50,

    /// <summary>
    /// Convert to decimal
    /// </summary>
    ToDecimal = 51,

    /// <summary>
    /// Convert to boolean
    /// Config: { "trueValues": ["1", "true", "yes"], "falseValues": ["0", "false", "no"] }
    /// </summary>
    ToBoolean = 52,

    /// <summary>
    /// Convert to string
    /// </summary>
    ToString = 53,

    /// <summary>
    /// Parse DateTime
    /// Config: { "format": "yyyy-MM-dd HH:mm:ss", "culture": "en-US" }
    /// </summary>
    ToDateTime = 54,

    // ====== Conditional/Mapping ======
    /// <summary>
    /// Map value ranges to different outputs
    /// Config: { "ranges": [{ "min": 0, "max": 100, "output": "low" }, ...] }
    /// </summary>
    RangeMapping = 60,

    /// <summary>
    /// Map specific values to outputs (like switch/case)
    /// Config: { "mappings": { "1": "on", "0": "off" }, "default": "unknown" }
    /// </summary>
    ValueMapping = 61,

    /// <summary>
    /// Conditional operation (if-then-else)
    /// Config: { "condition": "greaterThan", "value": 10, "trueResult": "high", "falseResult": "low" }
    /// </summary>
    Conditional = 62,

    // ====== Date/Time Operations ======
    /// <summary>
    /// Parse Unix timestamp (seconds since epoch)
    /// Config: { "unit": "seconds" | "milliseconds" }
    /// </summary>
    UnixTimestamp = 70,

    /// <summary>
    /// Add time span
    /// Config: { "hours": 2, "minutes": 30 }
    /// </summary>
    AddTimeSpan = 71,

    /// <summary>
    /// Format DateTime to string
    /// Config: { "format": "yyyy-MM-dd", "culture": "en-US" }
    /// </summary>
    FormatDateTime = 72,

    // ====== Bitwise Operations ======
    /// <summary>
    /// Bitwise AND
    /// Config: { "mask": 255 }
    /// </summary>
    BitwiseAnd = 80,

    /// <summary>
    /// Bitwise OR
    /// Config: { "mask": 255 }
    /// </summary>
    BitwiseOr = 81,

    /// <summary>
    /// Bitwise XOR
    /// Config: { "mask": 255 }
    /// </summary>
    BitwiseXor = 82,

    /// <summary>
    /// Bit shift left
    /// Config: { "positions": 2 }
    /// </summary>
    ShiftLeft = 83,

    /// <summary>
    /// Bit shift right
    /// Config: { "positions": 2 }
    /// </summary>
    ShiftRight = 84,

    // ====== Advanced ======
    /// <summary>
    /// Extract bit range and convert to number
    /// Config: { "startBit": 0, "bitCount": 8 }
    /// </summary>
    ExtractBits = 90,

    /// <summary>
    /// Apply checksum validation
    /// Config: { "algorithm": "crc16" | "crc32" | "xor", "expectedPosition": -1 }
    /// </summary>
    ValidateChecksum = 91
}