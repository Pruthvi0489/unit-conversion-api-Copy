# Unit Conversion API

A simple web API that converts a number from one unit to another.
For example: meters to feet, Celsius to Fahrenheit, or kilograms to pounds.

It is built with ASP.NET Core (.NET 10) and comes with Swagger, so you can try every endpoint in your browser.

## What it supports

There are 24 units in 4 categories. More can be added while the app is running (see "Adding new units" below).

| Category    | Units                                                            |
| ----------- | ---------------------------------------------------------------- |
| Length      | meter, kilometer, centimeter, millimeter, mile, yard, foot, inch |
| Mass        | kilogram, gram, milligram, tonne, pound, ounce, stone            |
| Temperature | celsius, fahrenheit, kelvin                                      |
| Volume      | liter, milliliter, gallon, quart, cup, fluid ounce               |

You can write a unit as its name, its symbol or a common alias, in any letter case.
So `meter`, `meters` and `m` all mean the same thing.

## How to run it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/UnitConversion.Api
```

Then open **http://localhost:5080/swagger** in your browser.
Swagger lists all endpoints and lets you test them with a button click.

To run the tests:

```bash
dotnet test
```

See "How to test the API" below for step-by-step examples.

## How to test the API

Start the API first (`dotnet run --project src/UnitConversion.Api`), then use any of the options below.

### Option 1: Swagger (easiest)

1. Open **http://localhost:5080/swagger** in your browser.
2. Click `POST /api/v1/conversions` to expand it.
3. Click **Try it out**.
4. Edit the request body. It is already filled with an example (100 meters to feet).
5. Click **Execute**.
6. Look under **Server response** for the status code and the result.

The same steps work for every other endpoint.

### Option 2: curl

```bash
curl -X POST http://localhost:5080/api/v1/conversions \
  -H "Content-Type: application/json" \
  -d '{ "category": "length", "fromUnit": "meters", "toUnit": "feet", "value": 100 }'
```

On Windows PowerShell, use this instead:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/v1/conversions `
  -ContentType "application/json" `
  -Body '{ "category": "length", "fromUnit": "meters", "toUnit": "feet", "value": 100 }'
```

### Test cases to try

Send each one to `POST /api/v1/conversions`. The values below are the request body fields.

**Successful conversions**

| category    | fromUnit   | toUnit     | value | Expected result |
| ----------- | ---------- | ---------- | ----- | --------------- |
| length      | meters     | feet       | 100   | 328.083989501   |
| length      | mile       | km         | 1     | 1.609344        |
| length      | km         | m          | 5     | 5000            |
| mass        | kg         | lb         | 10    | 22.0462262185   |
| mass        | pound      | ounce      | 1     | 16              |
| temperature | celsius    | fahrenheit | 100   | 212             |
| temperature | F          | C          | 32    | 0               |
| temperature | celsius    | kelvin     | 0     | 273.15          |
| volume      | gallon     | liter      | 1     | 3.785411784     |

**Requests that should fail**

| Request body                                                                  | Expected status | Why                           |
| ----------------------------------------------------------------------------- | --------------- | ----------------------------- |
| `{ "category": "length", "fromUnit": "meters", "toUnit": "parsecs", "value": 1 }` | 400         | Unknown unit                  |
| `{ "category": "speed", "fromUnit": "m", "toUnit": "ft", "value": 1 }`         | 400             | Unknown category              |
| `{ "category": "length", "fromUnit": "meters", "toUnit": "kilograms", "value": 1 }` | 400        | Unit is from another category |
| `{ "category": "length" }`                                                    | 400             | Required fields are missing   |

An error response looks like this:

```json
{
  "title": "Invalid request",
  "status": 400,
  "detail": "The unit 'parsecs' is not supported in the 'length' category."
}
```

### Test the catalog endpoints

```bash
# List all categories
curl http://localhost:5080/api/v1/categories

# List the units in the length category
curl http://localhost:5080/api/v1/categories/length/units

# Unknown category (expect 404)
curl -i http://localhost:5080/api/v1/categories/nonsense/units
```

### Test adding a new unit

1. Add a furlong to the length category (expect `201 Created`):

   ```bash
   curl -X POST http://localhost:5080/api/v1/categories/length/units \
     -H "Content-Type: application/json" \
     -d '{ "name": "furlong", "symbol": "fur", "toBaseFactor": 201.168, "aliases": ["furlongs"] }'
   ```

2. Use it in a conversion. One furlong should be `201.168` meters:

   ```bash
   curl -X POST http://localhost:5080/api/v1/conversions \
     -H "Content-Type: application/json" \
     -d '{ "category": "length", "fromUnit": "furlong", "toUnit": "meter", "value": 1 }'
   ```

3. Send the first request again. This time you should get `409 Conflict`,
   because the name `furlong` is already taken.

### Run the automated tests

```bash
dotnet test
```

This runs both the unit tests and the integration tests. All tests should pass.
To run only one group, filter by name:

```bash
dotnet test --filter "FullyQualifiedName~Integration"
dotnet test --filter "FullyQualifiedName!~Integration"
```

## Endpoints

| Method | URL                                   | What it does                       |
| ------ | ------------------------------------- | ---------------------------------- |
| POST   | `/api/v1/conversions`                 | Convert a value                    |
| GET    | `/api/v1/categories`                  | List all categories                |
| GET    | `/api/v1/categories/{category}/units` | List the units of one category     |
| POST   | `/api/v1/categories`                  | Create a new category              |
| POST   | `/api/v1/categories/{category}/units` | Add a unit to an existing category |

### Example: convert a value

Request:

```json
POST /api/v1/conversions

{
  "category": "temperature",
  "fromUnit": "celsius",
  "toUnit": "fahrenheit",
  "value": 100
}
```

Response:

```json
{
  "category": "temperature",
  "fromUnit": "celsius",
  "toUnit": "fahrenheit",
  "inputValue": 100,
  "result": 212
}
```

### Example: error response

If something is wrong, the API returns a clear message:

```json
{
  "title": "Invalid request",
  "status": 400,
  "detail": "The unit 'parsecs' is not supported in the 'length' category."
}
```

| Status | Meaning                                             |
| ------ | --------------------------------------------------- |
| 200    | Success                                             |
| 201    | Unit or category was created                        |
| 400    | Bad input (unknown unit, missing field, and so on)  |
| 404    | Category not found                                  |
| 409    | The name, symbol or alias already exists            |

## Adding new units

### While the app is running

Add a unit to an existing category:

```bash
curl -X POST http://localhost:5080/api/v1/categories/length/units \
  -H "Content-Type: application/json" \
  -d '{ "name": "furlong", "symbol": "fur", "toBaseFactor": 201.168, "aliases": ["furlongs"] }'
```

`toBaseFactor` is how many base units one of your unit is worth.
The base units are: meter (length), kilogram (mass), celsius (temperature) and liter (volume).
One furlong is 201.168 meters, so the factor is 201.168.

Create a whole new category. Choose one unit as the base and give it a factor of 1:

```bash
curl -X POST http://localhost:5080/api/v1/categories \
  -H "Content-Type: application/json" \
  -d '{ "name": "speed", "units": [
        { "name": "meter per second", "symbol": "mps", "toBaseFactor": 1 },
        { "name": "kilometer per hour", "symbol": "kph", "toBaseFactor": 0.2777777778 } ] }'
```

Note: units added this way are kept in memory, so they disappear when the app restarts.

### Permanently

To make a unit part of the app, add one line in
`src/UnitConversion.Infrastructure/Repositories/UnitCatalogSeed.cs`:

```csharp
Linear("furlong", "fur", 201.168, "furlongs")
```

## Project structure

The code is split into layers. Each layer has one job.

```
src/
  UnitConversion.Domain          Core rules: units, categories, exceptions
  UnitConversion.Application     Business logic: services and request/response models
  UnitConversion.Infrastructure  Where the units are stored (in memory)
  UnitConversion.Api             Controllers, error handling, Swagger, Program.cs
tests/
  UnitConversion.Tests           Unit tests and integration tests
```

How the layers depend on each other:

```
Api  ->  Application  ->  Domain
Infrastructure  ->  Application  ->  Domain
```

The inner layers never depend on the outer ones. Services and repositories are connected
with dependency injection, so each class only knows about interfaces.

## Design decisions

- **One formula for every unit.** Each unit stores a factor (and sometimes an offset) compared to
  the base unit of its category. A conversion goes `from unit -> base unit -> to unit`.
  This means adding a unit is just one line of data, and the app can grow to hundreds of units
  without writing a formula for each pair.

- **Temperature uses the same approach.** Temperature scales start at different zero points, so
  the unit definition also has an offset. No special case is needed for Celsius, Fahrenheit or Kelvin.

- **Storage is hidden behind an interface.** Right now units live in memory (`ICategoryRepository`).
  To use a database later, write a new repository class and change one line in the setup code.

- **The category is required in every request.** This avoids confusion between units that look alike
  and stops invalid requests such as meters to kilograms.

- **Numbers use `double`.** Results are rounded to 12 significant digits, so you get `212`
  and not `212.00000000000003`. If exact decimal precision is ever needed, `decimal` can be used instead.

- **One place for error handling.** A single exception handler turns errors into a consistent
  JSON response, and unexpected errors are logged without showing internal details to the caller.

- **Versioned URLs.** Routes start with `/api/v1`, so future changes will not break existing users.

- **Thread-safe updates.** Adding units at runtime is protected by a lock, and a unit is added
  only if all of its names are free. A category is never left half-updated.

## Tests

- **Unit tests** check the conversion math, the unit lookup and the services.
- **Integration tests** start the real API in memory and call it over HTTP,
  including success cases and error cases (400, 404, 409).

## Ideas for the future

- Save added units in a database
- Add authentication to the write endpoints
- Add caching and rate limiting
