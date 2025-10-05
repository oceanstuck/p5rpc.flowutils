using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;

namespace RemixToolkit.Core.Configs.Models;

public class ConfigSetting
{
    private static readonly AssemblyName SHARED_NAME = new(Guid.NewGuid().ToString());
    private static readonly AssemblyBuilder SHARED_ASS = AssemblyBuilder.DefineDynamicAssembly(SHARED_NAME, AssemblyBuilderAccess.RunAndCollect);
    private static readonly ModuleBuilder SHARED_MOD = SHARED_ASS.DefineDynamicModule(SHARED_NAME.Name!);
    private static readonly Dictionary<string, Type> DEFINED_ENUMS = [];

    private const string DEFAULT_TYPE = "bool";

    /// <summary>
    /// ID of setting, used to retrieve setting value.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of setting.
    /// </summary>
    public string Type { get; set; } = DEFAULT_TYPE;

    /// <summary>
    /// (Optional) Display name for setting.
    /// </summary>
    public string? Name { get; set; } = null;

    /// <summary>
    /// (Optional) Setting category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// (Optional) Setting description.
    /// </summary>
    public string? Description { get; set; } = null;

    /// <summary>
    /// (Optional) Predefined default value for setting, else setting type default.
    /// </summary>
    public string? Default { get; set; } = null;

    /// <summary>
    /// (Optional) List to include setting value in.
    /// </summary>
    public string? List { get; set; } = null;

    /// <summary>
    /// (Optional / bool) Value to use if bool state is true.
    /// </summary>
    public string? ValueOn { get; set; } = null;

    /// <summary>
    /// (Optional / bool) Value to use if bool state is false.
    /// </summary>
    public string? ValueOff { get; set; } = null;

    /// <summary>
    /// (Required / enum) Values of an enum setting.
    /// </summary>
    public string[]? Choices { get; set; }

    public Type GetPropertyType()
        => Type switch
        {
            "bool" or "toggle" => typeof(bool),
            "string" or "text" => typeof(string),
            "enum" or "choice" => GetEnumType(),
            "int" or "number" => typeof(int),

            "byte" => typeof(byte),
            "short" => typeof(short),
            "float" => typeof(float),
            "double" => typeof(double),
            _ => throw new NotImplementedException($"Unknown setting type: {Type}"),
        };

    public object? GetDefaultValue()
    {
        var type = GetPropertyType();

        if (Type == "enum" || Type == "choice")
        {
            var enumValues = Enum.GetNames(type);

            // Gets predefined default using its index in list of choices
            // applied to the enum's list of values.
            if (Default != null) return enumValues[Array.IndexOf(Choices!, Default)];

            // If no default is set, default to first value.
            return Enum.GetNames(type).First();
        }

        // No default value set, use default value of prop type.
        if (Default == null)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);

            return null;
        }

        return Convert.ChangeType(Default, type);
    }

    private Type GetEnumType()
    {
        if (DEFINED_ENUMS.TryGetValue(Id, out var type)) return type;

        if (Choices == null || Choices.Length == 0) throw new Exception($"\"Choice\" setting must have at least 1 choice: {Id}");

        var enumName = $"enum_{DEFINED_ENUMS.Count}";
        var enumBuilder = SHARED_MOD.DefineEnum(enumName, TypeAttributes.Public, typeof(int));

        for (int i = 0; i < Choices.Length; i++)
        {
            var enumValue = enumBuilder.DefineLiteral($"option_{i}", i);
            enumValue.SetCustomAttribute(new DisplayAttributeBuilder(Choices[i]));
        }

        var enumType = enumBuilder.CreateType();
        DEFINED_ENUMS.Add(Id, enumType);
        return enumType;
    }

    private class DisplayAttributeBuilder(string name)
        : CustomAttributeBuilder(DISP_ATTR_CTOR, [], [DISP_ATTR_PROP_NAME], [name])
    {
        private static readonly Type DISP_ATTR_TYPE = typeof(DisplayAttribute);
        private static readonly ConstructorInfo DISP_ATTR_CTOR = DISP_ATTR_TYPE.GetConstructor([])!;
        private static readonly PropertyInfo DISP_ATTR_PROP_NAME = DISP_ATTR_TYPE.GetProperty("Name")!;
    }
}
