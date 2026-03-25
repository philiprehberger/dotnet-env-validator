using Xunit;
using Philiprehberger.EnvValidator;

namespace Philiprehberger.EnvValidator.Tests;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_SetsErrors()
    {
        var errors = new List<string> { "Missing VAR_A", "Invalid VAR_B" };

        var ex = new ValidationException(errors);

        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains("Missing VAR_A", ex.Errors);
        Assert.Contains("Invalid VAR_B", ex.Errors);
    }

    [Fact]
    public void Message_ContainsErrorCount()
    {
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        var ex = new ValidationException(errors);

        Assert.Contains("3 validation error(s)", ex.Message);
    }

    [Fact]
    public void Message_ContainsIndividualErrors()
    {
        var errors = new List<string> { "Missing required variable: DB_HOST" };

        var ex = new ValidationException(errors);

        Assert.Contains("DB_HOST", ex.Message);
    }
}
