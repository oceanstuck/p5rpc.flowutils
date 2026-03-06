namespace Mira.Configurer.Config;

public enum OptionType
{
    None,           // No prompt.
    Number,         // Numeric input.
    Text,           // Textbox
    Toggle,         // Toggle
    Choice,         // Combobox or radio
    MultiChoice,    // Checkboxes
    Preset,
}