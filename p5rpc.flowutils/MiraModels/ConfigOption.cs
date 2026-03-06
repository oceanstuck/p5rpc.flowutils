using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mira.Configurer.Config;

public class ConfigOption
{
    /// <summary>
    /// ID of option for referencing its value or related media.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Option type.
    /// Determines the input method for the option value, as well as its concrete type.
    /// </summary>
    public OptionType Type { get; set; } = OptionType.None;

    /// <summary>
    /// Option display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional title for the input form.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Optional settings to apply to the input method for the option.
    /// Available settings vary between inputs.
    /// </summary>
    public InputSettings Settings { get; set; } = [];

    /// <summary>
    /// Optional initial and default value.
    /// </summary>
    public string? Default { get; set; }

    /// <summary>
    /// Option category, and any potential parent categories.
    /// </summary>
    public string[] Category { get; set; } = [];

    /// <summary>
    /// Option description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional conditional expression required for the option to be active.
    /// </summary>
    public string? Requires { get; set; }

    /// <summary>
    /// Hint explanation for the requirement.
    /// </summary>
    public string? RequiresHint { get; set; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Id)) throw new InvalidOperationException($"Option is missing an ID.");
        if (string.IsNullOrEmpty(Name)) throw new InvalidOperationException($"Option '{Id}' is missing a name.");
            
        if (!string.IsNullOrEmpty(Requires) && string.IsNullOrEmpty(RequiresHint))
            throw new InvalidOperationException($"Option '{Id}' is missing a requirement hint.");

        // Getting the default value includes validation.
        _ = GetDefaultValue();
    }

    /// <summary>
    /// Gets the option default value.
    /// </summary>
    public object GetDefaultValue() =>
        Type switch
        {
            OptionType.Number => GetNumberDefault(),
            OptionType.Text => GetTextDefault(),
            OptionType.Toggle => GetToggleDefault(),
            OptionType.Choice => GetChoiceDefault(),
            OptionType.None or OptionType.Preset => string.Empty,
            OptionType.MultiChoice => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };

    private double GetNumberDefault()
    {
        var hasMin = Settings.TryGetValue<double>("min", out var min);
        var hasMax = Settings.TryGetValue<double>("max", out var max);

        // No default set, use either 0 or min.
        if (string.IsNullOrEmpty(Default))
        {
            // No min set, default to 0.
            if (!hasMin && !hasMax) return 0;
            
            // If 0 is within range of min/max, use it.
            if (0 >= min && 0 <= max) return 0;
            
            // Where 0 is not valid, default to min value.
            return min;
        }

        var isParsed = double.TryParse(Default, out var defaultValue);
        if (!isParsed) throw new InvalidOperationException($"Number option '{Id}' default '{Default}' could not be parsed.");
        
        if (hasMin && defaultValue < min) throw new InvalidOperationException($"Number option '{Id}' default '{Default}' less than minimum '{min}'.");
        if (hasMax && defaultValue > max) throw new InvalidOperationException($"Number option '{Id}' default '{Default}' more than maximum '{max}'.");

        return defaultValue;
    }

    private int GetChoiceDefault()
    {
        var hasChoices = Settings.TryGetValues<string>("choices", out var choices);
        if (!hasChoices || choices.Length < 1) throw new InvalidOperationException($"Choice option '{Id}' has no choices.");

        if (string.IsNullOrEmpty(Default)) return 1;

        var isParsed = int.TryParse(Default, out var defaultChoiceId);
        if (!isParsed) throw new InvalidOperationException($"Choice option '{Id}' default '{Default}' could not be parsed.");

        // Default choice ID is 1 indexed for users.
        var actualId = defaultChoiceId - 1;
        if (actualId > choices.Length) throw new InvalidOperationException($"Choice option '{Id}' default '{Default}' is more than total amount of choices '{choices.Length}'.");
        if (actualId < 0) throw new InvalidOperationException($"Choice option '{Id}' default '{Default}' is invalid.");

        return defaultChoiceId;
    }

    private bool GetToggleDefault()
    {
        if (string.IsNullOrEmpty(Default)) return false;
        
        var isParsed = bool.TryParse(Default, out var defaultValue);
        if (!isParsed) throw new InvalidOperationException($"Toggle option '{Id}' default '{Default}' could not be parsed.");
        
        return defaultValue;
    }

    private string GetTextDefault() => Default ?? string.Empty;
}

public class InputSettings : Dictionary<string, object>
{
    public bool TryGetValue<TValue>(string key, [NotNullWhen(true)] out TValue? value)
        where TValue : notnull
    {
        if (this.TryGetValue(key, out var valueObj) && valueObj is string valueStr)
        {
            try
            {
                value = ConfigurerUtils.ResolveValue<TValue>(valueStr);
                if (value != null) return true;
                
                Trace.TraceWarning("Input setting '{0}' value was not a valid {1}.", key, typeof(TValue).Name);
                return false;

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        value = default;
        return false;
    }

    public bool TryGetValues<TValue>(string key, out TValue[] values)
        where TValue : notnull
    {
        if (this.TryGetValue(key, out var valueObj))
        {
            if (valueObj is IEnumerable<object> valuesList)
            {
                try
                {
                    values = valuesList.Cast<string>().Select(ConfigurerUtils.ResolveValue<TValue>).ToArray()!;
                    return values.All(x => x != null!);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        values = [];
        return false;
    }
}