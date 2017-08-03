# Gelf.Extensions.Logging [![travis](https://img.shields.io/travis/mattwcole/gelf-extensions-logging.svg?style=flat-square)](https://travis-ci.org/mattwcole/gelf-extensions-logging) [![nuget](https://img.shields.io/nuget/v/Gelf.Extensions.Logging.svg?style=flat-square)](https://www.nuget.org/packages/Gelf.Extensions.Logging) [![license](https://img.shields.io/github/license/mattwcole/gelf-extensions-logging.svg?style=flat-square)](https://github.com/mattwcole/gelf-extensions-logging/blob/master/LICENSE.md)

GELF provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging).

## Usage

Add the GELF logger using the `ILoggerFactory` extension method.

```csharp
var loggerFactory = new  LoggerFactory();
loggerFactory.AddGelf(new GelfLoggerOptions
{
    Host = "graylog-hostname",
    LogSource = "my-application",
    LogLevel = LogLevel.Information
});
```

In addition to the above, there are a number of other settings on `GelfLoggerOptions` including port, additional fields and log filter.

### Additional Fields

Global fields that will be present on all messages can be added when configuring the logger.

```csharp
var loggerFactory = new  LoggerFactory();
loggerFactory.AddGelf(new GelfLoggerOptions
{
    Host = "graylog-host",
    LogSource = "my-application",
    LogLevel = LogLevel.Information,
    AdditionalFields =
    {
        ["machine_name"] = Environment.MachineName,
        ["foo"] = "bar"
    }
});
```

To add a context specific field, create a log scope with a [`ValueTuple<string, string>`](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/).

```csharp
using (_logger.BeginScope(("correlation_id", correlationId)))
{
    // Field will be added to all logs within this scope (using any ILogger<T> instance).
}
```

To add multiple fields at once, use a `Dictionary<string, string>`.

```csharp
using (_logger.BeginScope(new Dictionary<string, string>
{
    ["order_id"] = orderId,
    ["customer_id"] = customerId
}))
{
    // Fields will be added to all logs within this scope (using any ILogger<T> instance).
}
```

### Log Filtering

By default, all logs greater or equal in level to `GelfLoggerOptions.LogLevel` will be sent. This behavior can be overridden by setting a custom filter. The example below is from an ASP.NET Core application that ignores logging on requests to the `/healthcheck` endpoint.

```csharp
var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
var gelfOptions = new GelfLoggerOptions
{
    Host = logSettings.Host,
    Port = logSettings.Port,
    LogSource = Environment.MachineName,
    AdditionalFields = {{"facility", logSettings.Facility}},
    Filter = (name, logLevel) => logLevel >= logSettings.LogLevel &&
        httpContextAccessor.HttpContext?.Request.Path.Equals("/healthcheck") != true
};
```

### Testing

This repository contains a Docker Compose file that can be used for creating local a Graylog stack with a single command. This can be useful for testing application logs locally. Requires Docker and Docker Compose.

- `docker-compose up`
- Navigate to [http://localhost:9000](http://localhost:9000)
- Credentials: admin/admin
- Create a UDP input and send logs to localhost:12201

## Contributing

Pull requests welcome! The build is done in Docker, but this is not required for local development.
