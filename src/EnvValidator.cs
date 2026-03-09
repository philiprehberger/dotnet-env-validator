using System.Reflection;

namespace Philiprehberger.EnvValidator;

[AttributeUsage(AttributeTargets.Property)]
public class EnvVarAttribute : Attribute
{
    public string Name { get; }
    public bool Required { get; set; } = true;
    public string? Default { get; set; }
    public string[]? Choices { get; set; }

    public EnvVarAttribute(string name) => Name = name;
}

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base($"{errors.Count} validation error(s):\n" + string.Join("\n", errors.Select(e => $"  - {e}")))
    {
        Errors = errors;
    }
}

public static class EnvValidator
{
    public static T Validate<T>() where T : new() => Validate<T>(null);

    public static T Validate<T>(Dictionary<string, string>? source) where T : new()
    {
        var instance = new T();
        var errors = new List<string>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<EnvVarAttribute>();
            if (attr == null) continue;

            var raw = source != null
                ? (source.TryGetValue(attr.Name, out var v) ? v : null)
                : Environment.GetEnvironmentVariable(attr.Name);

            if (string.IsNullOrEmpty(raw))
            {
                if (attr.Default != null)
                    raw = attr.Default;
                else if (attr.Required)
                {
                    errors.Add($"Missing required variable: {attr.Name}");
                    continue;
                }
                else continue;
            }

            if (attr.Choices != null && !attr.Choices.Contains(raw))
            {
                errors.Add($"{attr.Name} must be one of [{string.Join(", ", attr.Choices)}], got '{raw}'");
                continue;
            }

            try
            {
                var value = ConvertValue(raw, prop.PropertyType, attr.Name);
                prop.SetValue(instance, value);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return instance;
    }

    private static object? ConvertValue(string raw, Type type, string name)
    {
        if (type == typeof(string)) return raw;
        if (type == typeof(int)) return int.TryParse(raw, out var i) ? i : throw new FormatException($"{name}: cannot convert '{raw}' to int");
        if (type == typeof(long)) return long.TryParse(raw, out var l) ? l : throw new FormatException($"{name}: cannot convert '{raw}' to long");
        if (type == typeof(double)) return double.TryParse(raw, out var d) ? d : throw new FormatException($"{name}: cannot convert '{raw}' to double");
        if (type == typeof(bool)) return raw.ToLower() switch
        {
            "true" or "1" or "yes" or "on" => true,
            "false" or "0" or "no" or "off" => false,
            _ => throw new FormatException($"{name}: cannot convert '{raw}' to bool"),
        };
        if (type == typeof(Uri)) return new Uri(raw);
        if (type == typeof(TimeSpan)) return TimeSpan.TryParse(raw, out var ts) ? ts : throw new FormatException($"{name}: cannot convert '{raw}' to TimeSpan");

        throw new NotSupportedException($"{name}: unsupported type {type.Name}");
    }
}
