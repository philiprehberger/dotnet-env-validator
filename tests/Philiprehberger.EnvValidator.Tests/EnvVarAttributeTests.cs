using Xunit;
using Philiprehberger.EnvValidator;

namespace Philiprehberger.EnvValidator.Tests;

public class EnvVarAttributeTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var attr = new EnvVarAttribute("MY_VAR");

        Assert.Equal("MY_VAR", attr.Name);
    }

    [Fact]
    public void Required_DefaultsToTrue()
    {
        var attr = new EnvVarAttribute("MY_VAR");

        Assert.True(attr.Required);
    }

    [Fact]
    public void Default_DefaultsToNull()
    {
        var attr = new EnvVarAttribute("MY_VAR");

        Assert.Null(attr.Default);
    }

    [Fact]
    public void Separator_DefaultsToComma()
    {
        var attr = new EnvVarAttribute("MY_VAR");

        Assert.Equal(",", attr.Separator);
    }

    [Fact]
    public void Choices_DefaultsToNull()
    {
        var attr = new EnvVarAttribute("MY_VAR");

        Assert.Null(attr.Choices);
    }

    [Fact]
    public void Pattern_DefaultsToNull()
    {
        var attr = new EnvVarAttribute("MY_VAR");

        Assert.Null(attr.Pattern);
    }
}
