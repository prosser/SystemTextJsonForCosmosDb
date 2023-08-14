# SystemTextJsonFoCosmosDb

[![.NET Build and Package](https://github.com/prosser/SystemTextJsonForCosmosDb/actions/workflows/dotnet.yml/badge.svg)](https://github.com/prosser/SystemTextJsonForCosmosDb/actions/workflows/dotnet.yml)

A tiny package for using System.Text.Json with Cosmos DB.

## Installing

Add the package to your project:

### Using Package Manager
```powershell
Install-Package SystemTextJsonForCosmosDb
```

### PackageReference in your project file
```xml
<PackageReference Include="SystemTextJsonForCosmosDb" Version="0.1.0" />
```

## Quick start (ASP.NET Core using Microsoft.Extensions.DependencyInjection)

Add the following code to your services registration code (e.g., in Startup's `ConfigureServices`, or anywhere you're adding to your `IServiceCollection`):

```csharp
using System.Text.Json;
using SystemTextJsonForCosmosDb;

// replace with your options
JsonSerializerOptions mySerializerOptions = new()
{
    MaxDepth = 64,
    PropertyNamingPolicy = NamingPolicy.CamelCase,
};

services
    .AddSystemTextJsonForCosmosDb()
    .AddSingleton<IJsonSerializerOptionsProvider>(new JsonSerializerOptionsProvider(mySerializerOptions));
```

When you want to initialize a connection to Cosmos DB, you can inject the `ICosmosClientOptionsProvider` like this:

```csharp
namespace MyNamespace;

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SystemTextJsonForCosmosDb;

internal class MyRepository
{
    private CosmosClient client;

    public MyRepository(ICosmosClientOptionsProvider clientOptionsProvider)
    {
        config = options.Value;
        CosmosClientOptions clientOptions = clientOptionsProvider.GetCosmosClientOptions();
        client = new(
            // you should definitely be getting this from configuration, not hardcoding it...
            "https://myaccount.documents.azure.com",
            // You're using RBAC, not secret credentials, right?
            new DefaultAzureCredential(),
            clientOptions);
    }

    // ... methods that use the client in fun and interesting ways
}
```

## Customizing

### ICosmosSerializer

If you prefer to have full control over the serialization, perhaps because you already have a serializer implemented, you can implement the
`ICosmosSerializer` interface and provide that instead:

```csharp
services
    .AddSystemTextJsonForCosmosDb()
    .AddSingleton<ICosmosSerializer, MySerializer>();
```
> NOTE: If both `ICosmosSerializer` **AND** `IJsonSerializerOptionsProvider` are registered, `ICosmosSerializer` will be used.

The following converters are highly recommended to be used if you implement your own serializer:

    CosmosDateTimeOffsetJsonConverter
    CosmosDateTimeJsonConverter
    JsonStringEnumConverter
    CosmosNullableDateTimeOffsetJsonConverter
    CosmosNullableDateTimeJsonConverter
    PatchOperationJsonConverter

### IJsonSerializerOptionsProvider

This interface exposes just one method: `GetJsonSerializerOptions()`, which returns `System.Text.Json.JsonSerializerOptions`.

When using `IJsonSerializerOptionsProvider`, the library guarantees to:
- Not mutate your options. A new copy is created and used internally.
- Honor naming policies, except for two cases: the `id` and `etag`/`_etag` properties (case-insensitive) of any object being (de)serialized will be converted to `"id"` and `"_etag"`, respectively.
- Preempt certain data type conversions:
    - `DateTime`, `DateTimeOffset`, and nullable variants will always use the format `yyyy-MM-ddTHH:mm:ss.fffffffZ` to conform to Cosmos DB standards.
    - `Microsoft.Azure.Cosmos.PatchOperation` will be converted using Cosmos DB naming, according to the RFC standard.
