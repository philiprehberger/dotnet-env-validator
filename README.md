# Philiprehberger.EnvValidator

Declarative environment variable validation for .NET — attribute-based binding with type coercion.

## Install

```bash
dotnet add package Philiprehberger.EnvValidator
```

## Usage

```csharp
using Philiprehberger.EnvValidator;

// Define your config class
public class AppConfig
{
    [EnvVar("DATABASE_URL")]
    public string DatabaseUrl { get; set; } = "";

    [EnvVar("PORT", Default = "3000")]
    public int Port { get; set; }

    [EnvVar("LOG_LEVEL", Required = false, Choices = new[] { "debug", "info", "warn", "error" })]
    public string LogLevel { get; set; } = "info";

    [EnvVar("ENABLE_CACHE", Default = "true")]
    public bool EnableCache { get; set; }

    [EnvVar("REQUEST_TIMEOUT", Default = "00:00:30")]
    public TimeSpan RequestTimeout { get; set; }
}

// Validate from environment
var config = EnvValidator.Validate<AppConfig>();

// Validate from a dictionary (useful for testing)
var config = EnvValidator.Validate<AppConfig>(new Dictionary<string, string>
{
    ["DATABASE_URL"] = "postgresql://localhost/mydb",
    ["PORT"] = "8080",
});
```

### Error Handling

```csharp
try
{
    var config = EnvValidator.Validate<AppConfig>();
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
        Console.WriteLine(error);
}
```

## Supported Types

`string`, `int`, `long`, `double`, `bool`, `Uri`, `TimeSpan`

Bool accepts: `true`/`false`, `1`/`0`, `yes`/`no`, `on`/`off`

## License

MIT
