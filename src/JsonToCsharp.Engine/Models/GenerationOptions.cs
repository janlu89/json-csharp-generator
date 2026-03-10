namespace JsonToCsharp.Engine.Models;

public class GenerationOptions
{
    public string RootClassName { get; set; } = "Root";
    public bool UseNullableReferenceTypes { get; set; } = true;
    public NamingConvention NamingConvention { get; set; } = NamingConvention.PascalCase;
    public AttributeStyle AttributeStyle { get; set; } = AttributeStyle.SystemTextJson;
    public bool GenerateAsRecord { get; set; } = false;
    public bool UsePreciseTypes { get; set; } = false;
    public string ArrayItemClassName { get; set; } = "Item";
}

public enum NamingConvention { PascalCase, CamelCase }
public enum AttributeStyle { None, SystemTextJson, Newtonsoft }