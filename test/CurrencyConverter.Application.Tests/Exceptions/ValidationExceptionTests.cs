namespace CurrencyConverter.Application.Tests.Exceptions
{
	public class ValidationExceptionTests
	{
		[Fact]
		public void Constructor_WithValidationFailures_GroupsErrorsByProperty()
		{
			// Arrange
			var failures = new List<ValidationFailure>
			{
				new("Property1", "Error1"),
				new("Property1", "Error2"),
				new("Property2", "Error3")
			};

			// Act
			var exception = new Application.Exceptions.ValidationException(failures);

			// Assert
			exception.Errors.Should().HaveCount(2);
			exception.Errors["Property1"].Should().HaveCount(2);
			exception.Errors["Property2"].Should().HaveCount(1);
			exception.Errors["Property1"].Should().Contain(new[] { "Error1", "Error2" });
			exception.Errors["Property2"].Should().Contain("Error3");
		}
	}
}
