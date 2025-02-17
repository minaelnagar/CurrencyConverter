using CurrencyConverter.API.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.API.Tests.Extensions
{
	public class HttpRequestExtensionsTests
	{
		[Fact]
		public void IsLocal_RemoteIpAddressIsNull_ReturnsTrue()
		{
			// Arrange: Create a context with RemoteIpAddress set to null.
			var context = new DefaultHttpContext();
			context.Connection.RemoteIpAddress = null;
			// LocalIpAddress is irrelevant in this branch.
			var request = context.Request;

			// Act
			var result = request.IsLocal();

			// Assert: When RemoteIpAddress is null, IsLocal returns true.
			Assert.True(result);
		}

		[Fact]
		public void IsLocal_RemoteAndLocalIpAreNotNull_AndEqual_ReturnsTrue()
		{
			// Arrange: Both Remote and Local IP are non-null and equal.
			var context = new DefaultHttpContext();
			var ip = IPAddress.Parse("192.168.1.100");
			context.Connection.RemoteIpAddress = ip;
			context.Connection.LocalIpAddress = ip;
			var request = context.Request;

			// Act
			var result = request.IsLocal();

			// Assert: When Remote equals Local, IsLocal returns true.
			Assert.True(result);
		}

		[Fact]
		public void IsLocal_RemoteAndLocalIpAreNotNull_AndNotEqual_ReturnsFalse()
		{
			// Arrange: Both Remote and Local IP are non-null but different.
			var context = new DefaultHttpContext();
			context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
			context.Connection.LocalIpAddress = IPAddress.Parse("192.168.1.101");
			var request = context.Request;

			// Act
			var result = request.IsLocal();

			// Assert: When Remote does not equal Local, IsLocal returns false.
			Assert.False(result);
		}

		[Fact]
		public void IsLocal_RemoteIpIsLoopback_AndLocalIpIsNull_ReturnsTrue()
		{
			// Arrange: Remote IP is loopback, and LocalIpAddress is null.
			var context = new DefaultHttpContext();
			context.Connection.RemoteIpAddress = IPAddress.Loopback; // typically 127.0.0.1
			context.Connection.LocalIpAddress = null;
			var request = context.Request;

			// Act
			var result = request.IsLocal();

			// Assert: IPAddress.IsLoopback returns true, so IsLocal returns true.
			Assert.True(result);
		}

		[Fact]
		public void IsLocal_RemoteIpIsNotLoopback_AndLocalIpIsNull_ReturnsFalse()
		{
			// Arrange: Remote IP is not loopback and LocalIpAddress is null.
			var context = new DefaultHttpContext();
			context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
			context.Connection.LocalIpAddress = null;
			var request = context.Request;

			// Act
			var result = request.IsLocal();

			// Assert: Since Remote IP is not loopback, IsLocal returns false.
			Assert.False(result);
		}
	}
}
