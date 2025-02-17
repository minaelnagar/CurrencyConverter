# Currency Converter API

A robust, scalable, and maintainable currency conversion API built with C# and ASP.NET Core.

## Features

- Real-time exchange rates from Frankfurter API
- Historical exchange rates with pagination
- Currency conversion with validation
- JWT authentication with role-based access
- Rate limiting and request throttling
- Redis caching
- Structured logging with Serilog and Seq
- OpenTelemetry distributed tracing
- API versioning
- Swagger documentation

## Prerequisites

- .NET 9.0 SDK
- Redis Server
- Seq Server
- JWT Token for authentication

## Setup

1. Install Redis:
```bash
choco install redis-64
```

2. Install Seq:
```bash
choco install seq
```

3. Update configuration in appsettings.json:
```json
{
  "JwtSettings": {
    "Key": "your-secure-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  }
}
```

## Running the Application

1. Start Redis Server
2. Start Seq Server
3. Run the application:
```bash
dotnet run --project src/CurrencyConverter.API
```

Access Swagger UI: `https://localhost:7125/swagger`

## Testing

Run tests with coverage:
```bash
./scripts/run-tests-with-coverage.ps1
```

## Assumptions

1. Frankfurter API is the primary data source
2. Redis is used for caching exchange rates
3. JWT tokens are validated but issued externally
4. Rate limiting is per client
5. Restricted currencies: TRY, PLN, THB, MXN

## Future Enhancements

1. Multiple exchange rate providers
2. Rate alerts and notifications
3. WebSocket support for real-time updates
4. Caching strategy optimization
5. Advanced monitoring and analytics

## API Documentation

API endpoints are versioned and require JWT authentication.

### Endpoints

- `GET /api/v1/exchangerate/latest`
- `POST /api/v1/exchangerate/convert`
- `GET /api/v1/exchangerate/historical`

For detailed API documentation, refer to Swagger UI.

## License

MIT