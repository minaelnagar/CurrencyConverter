using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Domain.Tests.Exceptions
{
	public class DomainExceptionTests
	{
		[Fact]
		public void Constructor_WithNoParameters_ShouldCreateException()
		{
			// Act
			var exception = new DomainException();

			// Assert
			exception.Should().NotBeNull();
			exception.Message.Should().Be("Exception of type 'CurrencyConverter.Domain.Exceptions.DomainException' was thrown.");
		}

		[Fact]
		public void Constructor_WithMessage_ShouldCreateExceptionWithMessage()
		{
			// Arrange
			var message = "Test error message";

			// Act
			var exception = new DomainException(message);

			// Assert
			exception.Should().NotBeNull();
			exception.Message.Should().Be(message);
		}

		[Fact]
		public void Constructor_WithMessageAndInnerException_ShouldCreateExceptionWithBoth()
		{
			// Arrange
			var message = "Test error message";
			var innerException = new InvalidOperationException("Inner exception");

			// Act
			var exception = new DomainException(message, innerException);

			// Assert
			exception.Should().NotBeNull();
			exception.Message.Should().Be(message);
			exception.InnerException.Should().BeSameAs(innerException);
		}

		[Fact]
		public void DomainException_ShouldBeSupportExceptionHandling()
		{
			// Arrange
			var message = "Test error message";
			var originalException = new DomainException(message);

			try
			{
				// Act
				throw originalException;
			}
			catch(DomainException caughtException)
			{
				// Assert
				caughtException.Should().NotBeNull();
				caughtException.Message.Should().Be(message);
				caughtException.StackTrace.Should().NotBeNull();
			}
		}

		[Fact]
		public void DomainException_WithInnerException_ShouldMaintainExceptionChain()
		{
			// Arrange
			var innerMessage = "Inner error";
			var outerMessage = "Outer error";
			var innerException = new InvalidOperationException(innerMessage);

			// Act
			var exception = new DomainException(outerMessage, innerException);

			// Assert
			exception.Message.Should().Be(outerMessage);
			exception.InnerException.Should().NotBeNull();
			exception.InnerException!.Message.Should().Be(innerMessage);
			exception.InnerException.Should().BeOfType<InvalidOperationException>();
		}
	}
}
