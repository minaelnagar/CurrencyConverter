# Clean previous results
Remove-Item -Path "test/**/TestResults" -Recurse -ErrorAction SilentlyContinue
Remove-Item -Path "test/**/coveragereport" -Recurse -ErrorAction SilentlyContinue

# Run tests with coverage for Domain and Application
dotnet test test/CurrencyConverter.Domain.Tests/CurrencyConverter.Domain.Tests.csproj `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput="./TestResults/" `
    /p:Include="[CurrencyConverter.Domain]*"

dotnet test test/CurrencyConverter.Application.Tests/CurrencyConverter.Application.Tests.csproj `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput="./TestResults/" `
    /p:Include="[CurrencyConverter.Application]*"

dotnet test test/CurrencyConverter.Infrastructure.Tests/CurrencyConverter.Infrastructure.Tests.csproj `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput="./TestResults/" `
    /p:Include="[CurrencyConverter.Infrastructure]*"

dotnet test test/CurrencyConverter.API.Tests/CurrencyConverter.API.Tests.csproj `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput="./TestResults/" `
    /p:Include="[CurrencyConverter.API]*"

# Generate combined report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator `
    -reports:"test/**/TestResults/coverage.cobertura.xml" `
    -targetdir:"test/coveragereport" `
    -reporttypes:Html_Dark

# Open the report
Start-Process "test/coveragereport/index.html"