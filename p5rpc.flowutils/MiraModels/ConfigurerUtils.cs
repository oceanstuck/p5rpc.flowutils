using System.Globalization;

namespace Mira.Configurer;

public static class ConfigurerUtils
{
    /// <summary>
    /// Standardized object conversion between supported types.
    /// </summary>
    public static TValue? ResolveValue<TValue>(string? rawValue)
    {
        if (rawValue == null) return default;
        
        // Handle hex integer values.
        if (rawValue.StartsWith("0x"))
        {
            var integerValue = Convert.ToUInt64(rawValue, 16);
            return (TValue)Convert.ChangeType(integerValue, typeof(TValue), CultureInfo.InvariantCulture);
        }
        
        return (TValue)Convert.ChangeType(rawValue, typeof(TValue), CultureInfo.InvariantCulture);
    }
}