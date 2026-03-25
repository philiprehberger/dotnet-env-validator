using Xunit;
using Philiprehberger.EnvValidator;

namespace Philiprehberger.EnvValidator.Tests;

public class SimpleConfig
{
    [EnvVar("APP_NAME")]
    public string AppName { get; set; } = "";

    [EnvVar("APP_PORT")]
    public int Port { get; set; }

    [EnvVar("DEBUG_MODE")]
    public bool Debug { get; set; }
}

public class OptionalConfig
{
    [EnvVar("OPT_VAR", Required = false)]
    public string? Optional { get; set; }

    [EnvVar("WITH_DEFAULT", Default = "fallback")]
    public string WithDefault { get; set; } = "";
}

public class ChoicesConfig
{
    [EnvVar("LOG_LEVEL", Choices = new[] { "debug", "info", "warn", "error" })]
    public string LogLevel { get; set; } = "";
}

public class PatternConfig
{
    [EnvVar("EMAIL", Pattern = @"^.+@.+\..+$")]
    public string Email { get; set; } = "";
}

public class CollectionConfig
{
    [EnvVar("TAGS")]
    public string[] Tags { get; set; } = [];

    [EnvVar("PORTS", Separator = ";")]
    public int[] Ports { get; set; } = [];
}

public class EnvValidatorTests
{
    [Fact]
    public void Validate_WithValidSource_BindsProperties()
    {
        var source = new Dictionary<string, string>
        {
            ["APP_NAME"] = "MyApp",
            ["APP_PORT"] = "8080",
            ["DEBUG_MODE"] = "true"
        };

        var config = EnvValidator.Validate<SimpleConfig>(source);

        Assert.Equal("MyApp", config.AppName);
        Assert.Equal(8080, config.Port);
        Assert.True(config.Debug);
    }

    [Fact]
    public void Validate_MissingRequired_ThrowsValidationException()
    {
        var source = new Dictionary<string, string>();

        var ex = Assert.Throws<ValidationException>(() => EnvValidator.Validate<SimpleConfig>(source));

        Assert.Contains(ex.Errors, e => e.Contains("APP_NAME"));
    }

    [Fact]
    public void Validate_OptionalMissing_DoesNotThrow()
    {
        var source = new Dictionary<string, string>();

        var config = EnvValidator.Validate<OptionalConfig>(source);

        Assert.Null(config.Optional);
        Assert.Equal("fallback", config.WithDefault);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("1", true)]
    [InlineData("yes", true)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("no", false)]
    public void Validate_BoolConversion_HandlesVariousFormats(string input, bool expected)
    {
        var source = new Dictionary<string, string>
        {
            ["APP_NAME"] = "test",
            ["APP_PORT"] = "80",
            ["DEBUG_MODE"] = input
        };

        var config = EnvValidator.Validate<SimpleConfig>(source);

        Assert.Equal(expected, config.Debug);
    }

    [Fact]
    public void Validate_InvalidChoice_ThrowsValidationException()
    {
        var source = new Dictionary<string, string>
        {
            ["LOG_LEVEL"] = "verbose"
        };

        var ex = Assert.Throws<ValidationException>(() => EnvValidator.Validate<ChoicesConfig>(source));

        Assert.Contains(ex.Errors, e => e.Contains("LOG_LEVEL"));
    }

    [Fact]
    public void Validate_PatternMismatch_ThrowsValidationException()
    {
        var source = new Dictionary<string, string>
        {
            ["EMAIL"] = "not-an-email"
        };

        var ex = Assert.Throws<ValidationException>(() => EnvValidator.Validate<PatternConfig>(source));

        Assert.Contains(ex.Errors, e => e.Contains("EMAIL"));
    }

    [Fact]
    public void Validate_CollectionTypes_ParsesCorrectly()
    {
        var source = new Dictionary<string, string>
        {
            ["TAGS"] = "web,api,backend",
            ["PORTS"] = "8080;8443;9090"
        };

        var config = EnvValidator.Validate<CollectionConfig>(source);

        Assert.Equal(new[] { "web", "api", "backend" }, config.Tags);
        Assert.Equal(new[] { 8080, 8443, 9090 }, config.Ports);
    }
}
