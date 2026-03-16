# Philiprehberger.EnvValidator

[![CI](https://github.com/philiprehberger/dotnet-env-validator/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-env-validator/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.EnvValidator.svg)](https://www.nuget.org/packages/Philiprehberger.EnvValidator)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-env-validator)](LICENSE)

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

### Enum Support

Enum properties are parsed case-insensitively. Invalid values produce an error listing all valid options.

```csharp
public enum Environment { Development, Staging, Production }

public class AppConfig
{
    [EnvVar("APP_ENV", Default = "Development")]
    public Environment AppEnv { get; set; }
}
```

Setting `APP_ENV=production` will correctly parse to `Environment.Production`.

### Pattern Validation

Use the `Pattern` property to validate values against a regex before type conversion.

```csharp
public class AppConfig
{
    [EnvVar("API_KEY", Pattern = @"^sk-[a-zA-Z0-9]{32}$")]
    public string ApiKey { get; set; } = "";

    [EnvVar("PORT", Pattern = @"^\d{4,5}$", Default = "3000")]
    public int Port { get; set; }
}
```

If the value doesn't match, validation fails with: `Variable 'API_KEY' does not match required pattern '^sk-[a-zA-Z0-9]{32}$'`.

### Collection Parsing

Properties of type `string[]`, `int[]`, `List<string>`, or `List<int>` are parsed by splitting the value on a separator (comma by default). Whitespace around each element is trimmed.

```csharp
public class AppConfig
{
    [EnvVar("ALLOWED_ORIGINS")]
    public string[] AllowedOrigins { get; set; } = [];

    [EnvVar("RETRY_DELAYS")]
    public List<int> RetryDelays { get; set; } = [];

    [EnvVar("TAGS", Separator = ";")]
    public List<string> Tags { get; set; } = [];
}
```

Setting `ALLOWED_ORIGINS=https://example.com, https://app.example.com` parses into a two-element string array. Use the `Separator` property to change the delimiter.

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

`string`, `int`, `long`, `double`, `bool`, `Uri`, `TimeSpan`, enums, `string[]`, `int[]`, `List<string>`, `List<int>`

Bool accepts: `true`/`false`, `1`/`0`, `yes`/`no`, `on`/`off`

## Attribute Properties

| Property | Type | Default | Description |
|-----------|----------|---------|----------------------------------------------|
| `Name` | `string` | — | Environment variable name (constructor arg) |
| `Required` | `bool` | `true` | Whether the variable must be present |
| `Default` | `string?` | `null` | Fallback value when variable is missing |
| `Choices` | `string[]?` | `null` | Restrict to allowed values |
| `Pattern` | `string?` | `null` | Regex pattern the raw value must match |
| `Separator` | `string` | `","` | Delimiter for collection/array parsing |

## Development

```bash
dotnet build src/Philiprehberger.EnvValidator.csproj --configuration Release
```

## License

MIT
