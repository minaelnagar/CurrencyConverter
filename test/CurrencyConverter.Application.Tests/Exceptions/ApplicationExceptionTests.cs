namespace CurrencyConverter.Application.Tests.Exceptions
{
	public class ApplicationExceptionTests
	{
		[Fact]
		public void Constructor_WithMessage_SetsMessageProperty()
		{
			// Arrange
			string expectedMessage = "Test message";

			// Act
			var exception = new Application.Exceptions.ApplicationException(expectedMessage);

			// Assert
			Assert.Equal(expectedMessage, exception.Message);
		}

		[Fact]
		public void Constructor_WithMessageAndInnerException_SetsProperties()
		{
			// Arrange
			string expectedMessage = "Test message";
			var innerException = new Exception("Inner exception");

			// Act
			var exception = new Application.Exceptions.ApplicationException(expectedMessage, innerException);

			// Assert
			Assert.Equal(expectedMessage, exception.Message);
			Assert.Equal(innerException, exception.InnerException);
		}
	}
}
