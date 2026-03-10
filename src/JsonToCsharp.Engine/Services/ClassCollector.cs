namespace JsonToCsharp.Engine.Services;

/// <summary>
/// Accumulates generated class definitions during recursive tree traversal.
/// Acts as a shared registry — every recursive call to GenerateClass registers
/// its output here, so the final output is the concatenation of all classes
/// in dependency order (nested classes before the classes that reference them).
/// </summary>
internal class ClassCollector
{
    // Ordered dictionary preserves insertion order
    private readonly Dictionary<string, string> _classes = new();

    /// <summary>
    /// Registers a generated class. If the name is already taken, appends
    /// a numeric suffix until a unique name is found.
    /// Returns the final name used — the caller needs this to reference the
    /// class correctly as a property type in the parent class.
    /// </summary>
    public string Register(string className, string classBody)
    {
        if (!_classes.ContainsKey(className))
        {
            _classes[className] = classBody;
            return className;
        }

        // Naming collision
        var counter = 2;
        while (_classes.ContainsKey($"{className}{counter}"))
            counter++;

        var uniqueName = $"{className}{counter}";
        _classes[uniqueName] = classBody;
        return uniqueName;
    }

    /// <summary>
    /// Returns true if a class with this name has already been registered.
    /// </summary>
    public bool Contains(string className) => _classes.ContainsKey(className);

    /// <summary>
    /// Returns all generated class definitions concatenated in insertion order.
    /// </summary>
    public string BuildOutput() =>
        string.Join(Environment.NewLine, _classes.Values);

    public void UpdateBody(string className, string newBody)
    {
        if (_classes.ContainsKey(className))
            _classes[className] = newBody;
    }
}