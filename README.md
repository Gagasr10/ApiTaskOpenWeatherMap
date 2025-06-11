# Weather API Testing Project (OpenWeatherMap)

This is an automated test suite for validating the OpenWeatherMap Current Weather API.  
The tests are written in C# using .NET 8, NUnit, and RestSharp.  
They cover functional, security, input validation, and performance aspects.

---

## Technologies Used

- .NET 8
- NUnit
- RestSharp
- FluentAssertions
- Microsoft.Extensions.Configuration
- DotNetEnv

---

## Project Structure

ApiTask/
│
├── Services/ # API client logic
├── Models/ # DTOs for response parsing
├── Helpers/ # Configuration and logging
├── Tests/ # Test classes and helpers
│ ├── WeatherApiTests.cs
│ └── Helpers/TestHelpers.cs
├── Logs/ # Log output from test runs
├── Program.cs # Entry point for quick manual testing
├── appsettings.json # Local config (excluded from git)
├── .env # Environment variables (excluded from git)
└── ApiTask.csproj

---

## Configuration

Set the following values in either:

- `.env` file  
- or `appsettings.json`

Example:
OpenWeatherMap__BaseUrl=https://api.openweathermap.org/data/2.5/
OpenWeatherMap__ApiKey=YOUR_API_KEY


These files are ignored via `.gitignore`.

---

## Test Coverage

## Functional Tests
# GetWeatherData_ValidCity_ReturnsSuccess
→ Verifies that the API successfully returns weather data for a valid city.

# CoordinatesQuery_ShouldReturnNoviSad
→ Confirms that specific coordinates return the correct city (Novi Sad).

# UnitsParameter_ShouldAffectTemperatureScale
→ Ensures that changing the units parameter affects the temperature format (metric,    imperial, standard).

## Authentication / Authorization Tests
* MissingApiKey_ShouldReturnUnauthorized
→ Simulates a request without an API key; expects 401 Unauthorized.

# InvalidApiKey_ShouldReturnUnauthorized
→ Checks that a completely wrong API key results in 401 Unauthorized.

# SlightlyAlteredApiKey_ShouldReturnUnauthorized
→ Verifies that even a slightly modified key is rejected.

## Input Validation & Error Handling
# NoParameters_ShouldReturnError
→ Sends a request without required parameters; expects 400 or 401, not 500.

# SpecialCharacters_ShouldReturnNotFound
→ Sends special characters as input; expects 404 Not Found.

# ExcessivelyLongCityName_ShouldNotCauseServerError
→ Validates that an extremely long city name doesn’t cause a server error (500), but returns 400 or 404.

## Security Tests
# SqlInjectionLikeCityName_ShouldReturnNotFoundOrBadRequest
→ Simulates SQL injection input and ensures the API responds safely without crashing.

# XssInjectionLikeInput_ShouldReturnNotFoundOrBadRequest
→ Sends an XSS payload to test frontend/script injection resilience.

## Headers & Performance
ContentType_ShouldBeApplicationJson
→ Ensures that the API returns content with the correct Content-Type: application/json.

# ResponseTime_ShouldBeAcceptable
→ Measures API response time to ensure it's below a reasonable threshold (e.g., <1500 ms).
---

## Logs

Each test writes a log entry to:
Logs/log.txt

This includes timestamp, status code, response body, and test name.

---

## Running Tests

From the project root:
dotnet test

Make sure the `.env` or `appsettings.json` file is correctly configured.


## Author

Dragan Stojilkovic  


