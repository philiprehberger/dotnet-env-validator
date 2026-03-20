using System.Reflection;
using System.Text.RegularExpressions;

namespace Philiprehberger.EnvValidator;

/// <summary>
/// Marks a property for binding to an environment variable.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EnvVarAttribute : Attribute
{
    /// <summary>
    /// Gets the environment variable name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets whether the variable is required. Defaults to <c>true</c>.
    /// </summary>
    public bool Required { get; set; } = true;

    /// <summary>
    /// Gets or sets the default value used when the variable is missing.
    /// </summary>
    public string? Default { get; set; }

    /// <summary>
    /// Gets or sets the allowed values for the variable.
    /// </summary>
    public string[]? Choices { get; set; }

    /// <summary>
    /// Gets or sets a regex pattern the variable value must match.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets the separator for collection types. Defaults to <c>","</c>.
    /// </summary>
    public string Separator { get; set; } = ",";

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvVarAttribute"/> class.
    /// </summary>
    /// <param name="name">The environment variable name to bind to.</param>
    public EnvVarAttribute(string name) => Name = name;
}

/// <summary>
/// Thrown when one or more environment variable validations fail.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the list of validation error messages.
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">The list of validation error messages.</param>
    public ValidationException(List<string> errors)
        : base($"{errors.Count} validation error(s):\n" + string.Join("\n", errors.Select(e => $"  - {e}")))
    {
        Errors = errors;
    }
}

/// <summary>
/// Validates and binds environment variables to a strongly-typed configuration object.
/// </summary>
public static class EnvValidator
{
    /// <summary>
    /// Validates environment variables from <see cref="Environment.GetEnvironmentVariable(string)"/> and binds them to a new instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The configuration type with <see cref="EnvVarAttribute"/>-decorated properties.</typeparam>
    /// <returns>A populated instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validations fail.</exception>
    public static T Validate<T>() where T : new() => Validate<T>(null);

    /// <summary>
    /// Validates environment variables from the given source dictionary (or the system environment if <c>null</c>) and binds them to a new instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The configuration type with <see cref="EnvVarAttribute"/>-decorated properties.</typeparam>
    /// <param name="source">An optional dictionary of variable names to values. When <c>null</c>, reads from system environment variables.</param>
    /// <returns>A populated instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validations fail.</exception>
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

            if (attr.Pattern != null && !Regex.IsMatch(raw, attr.Pattern))
            {
                errors.Add($"Variable '{attr.Name}' does not match required pattern '{attr.Pattern}'");
                continue;
            }

            try
            {
                var value = ConvertValue(raw, prop.PropertyType, attr.Name, attr.Separator);
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

    private static object? ConvertValue(string raw, Type type, string name, string separator)
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

        if (type.IsEnum)
        {
            if (Enum.TryParse(type, raw, ignoreCase: true, out var result))
                return result;
            var validValues = string.Join(", ", Enum.GetNames(type));
            throw new FormatException($"{name}: cannot convert '{raw}' to {type.Name}. Valid values: {validValues}");
        }

        if (type == typeof(string[]))
            return raw.Split(separator).Select(s => s.Trim()).ToArray();

        if (type == typeof(int[]))
            return ParseIntCollection(raw, name, separator).ToArray();

        if (type == typeof(List<string>))
            return raw.Split(separator).Select(s => s.Trim()).ToList();

        if (type == typeof(List<int>))
            return ParseIntCollection(raw, name, separator).ToList();

        throw new NotSupportedException($"{name}: unsupported type {type.Name}");
    }

    private static IEnumerable<int> ParseIntCollection(string raw, string name, string separator)
    {
        var parts = raw.Split(separator).Select(s => s.Trim());
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var val))
                throw new FormatException($"{name}: cannot convert '{part}' to int in collection");
            yield return val;
        }
    }
}
