using System.Net;

namespace CurrencyConverter.API.Extensions
{
	public static class HttpRequestExtensions
	{
		public static bool IsLocal(this HttpRequest request)
		{
			var connection = request.HttpContext.Connection;
			if(connection.RemoteIpAddress != null)
			{
				if(connection.LocalIpAddress != null)
				{
					return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
				}
				return IPAddress.IsLoopback(connection.RemoteIpAddress);
			}
			return true;
		}
	}
}
