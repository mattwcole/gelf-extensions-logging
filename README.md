# Gelf.Extensions.Logging [![travis](https://img.shields.io/travis/mattwcole/gelf-extensions-logging.svg?style=flat-square)](https://travis-ci.org/mattwcole/gelf-extensions-logging) [![nuget](https://img.shields.io/nuget/v/Gelf.Extensions.Logging.svg?style=flat-square)](https://www.nuget.org/packages/Gelf.Extensions.Logging) [![license](https://img.shields.io/github/license/mattwcole/gelf-extensions-logging.svg?style=flat-square)](https://github.com/mattwcole/gelf-extensions-logging/blob/master/LICENSE.md)

[GELF](http://docs.graylog.org/en/2.3/pages/gelf.html) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging) for sending logs to [Graylog](https://www.graylog.org/), [Logstash](https://www.elastic.co/products/logstash) and more from .NET Standard 1.3+ compatible components.

## Usage

The following examples are for ASP.NET Core. The [samples](/samples) directory contains example console apps with and without ASP.NET Core. For more information on providers and logging in general, see the aspnetcore [logging documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging?tabs=aspnetcore2x#how-to-add-providers).

### ASP.NET Core 2.x

In `Program.cs`, import the `LoggingBuilder.AddGelf()` extension method from `Gelf.Extensions.Logging` and add the following to your `WebHost` configuration.

```csharp
var webHost = WebHost
    .CreateDefaultBuilder(args)
    .UseStartup<Startup>()
    .ConfigureLogging((context, builder) =>
    {
        builder.AddConfiguration(context.Configuration.GetSection("Logging"))
            .AddConsole()
            .AddDebug()
            .AddGelf(options =>
            {
                // Optional customisation applied on top of settings in Logging:GELF configuration section.
                options.LogSource = context.HostingEnvironment.ApplicationName;
                options.AdditionalFields["machine_name"] = Environment.MachineName;
                options.AdditionalFields["app_version"] = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            });
    })
    .Build();
```

Logger options are taken from the "GELF" provider section in `appsettings.json` in the same way as other providers. These can be customised further in code as in the above example.

```json5
{
  "Logging": {
    "LogLevel": {
      "Default": "Error"
    },
    "Console": {
      "LogLevel": {
        "Default": "Debug"
      }
    },
    "GELF": {
      "Host": "localhost",
      "Port": 12201,    // Not required if using default 12201.
      "LogSource": "my-app-name",   // Required if not set in code.
      "AdditionalFields": {     // Optional fields added to all logs.
        "project_name": "my-project-name"
      },
      "LogLevel": {
        "Default": "Information",
        "Some.Namespace": "Debug"
      }
    }
  }
}
```

For a full list of options e.g. UDP/HTTP(S) settings, see [`GelfLoggerOptions`](src/Gelf.Extensions.Logging/GelfLoggerOptions.cs).

### ASP.NET Core 1.x

In `Startup.cs`, import the `LoggerFactory.AddGelf()` extension method from `Gelf.Extensions.Logging` and add the following to your `Configure()` method.

```csharp
public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
{
    loggerFactory
        .AddConsole()
        .AddDebug()
        .AddGelf(new GelfLoggerOptions
        {
            Host = "localhost",
            LogSource = "application-name",
            LogLevel = LogLevel.Information
        });

    ...
}
```

### Additional Fields

By default, `logger` and `exception` fields are included on all messages (the `exception` field is only added when an exception is passed to the logger). There are a number of other ways to attach data to logs.

#### Global Fields

Global fields can be added to all logs by setting them in `GelfLoggerOptions.AdditionalFields`.

```csharp
var options = new GelfLoggerOptions
{
    Host = "graylog-host",
    LogSource = "my-application",
    AdditionalFields =
    {
        ["machine_name"] = Environment.MachineName,
        ["foo"] = "bar"
    }
});
```

#### Scoped Fields

Log scopes can also be used to attach fields to a group of related logs. Create a log scope with a [`ValueTuple<string, string>`](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/), `ValueTuple<string, int/byte/double>` (or any other numeric value) or `Dictionary<string, object>` to do so. _Note that any other types passed to `BeginScope()` will be ignored, including `Dictionary<string, string>`._

```csharp
using (_logger.BeginScope(("correlation_id", correlationId)))
{
    // Field will be added to all logs within this scope (using any ILogger<T> instance).
}

using (_logger.BeginScope(new Dictionary<string, object>
{
    ["order_id"] = orderId,
    ["customer_id"] = customerId
}))
{
    // Fields will be added to all logs within this scope (using any ILogger<T> instance).
}
```

#### Structured/Semantic Logging

[Semantic logging](https://softwareengineering.stackexchange.com/questions/312197/benefits-of-structured-logging-vs-basic-logging) is also supported meaning fields can be extracted from individual log lines.

```csharp
_logger.LogInformation("Order {order_id} took {order_time} seconds to process", orderId, orderTime);
```

In the example above, the message will contain an `order_id` and `order_time`.

### Log Filtering

When using .NET Core 2.x, the log filtering API should be used to filter the "GELF" provider (details [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging?tabs=aspnetcore2x#log-filtering)). In .NET Core 1.x, log filtering can be overridden by setting a custom filter with `GelfLoggerOptions.Filter`, overriding the default filter that uses `GelfLoggerOptions.LogLevel`.

### Testing

This repository contains a Docker Compose file that can be used for creating local a Graylog stack with a single command using the [Graylog Docker image](https://hub.docker.com/r/graylog/graylog/). This can be useful for testing application logs locally. Requires [Docker](https://www.docker.com/get-docker) and Docker Compose.

- `docker-compose up`
- Navigate to [http://localhost:9000](http://localhost:9000)
- Credentials: admin/admin
- Create a UDP input and send logs to localhost:12201

## Contributing

Pull requests welcome! In order to run tests, first run `docker-compose up` to create the Graylog stack. Existing tests log messages and use the Graylog API to assert that they have been sent correctly. A UDP input will be created as part of the test setup (if not already present), so there is no need to create one manually. Build and tests are run on CI in Docker, meaning it is possible to run the build locally under identical conditions using `docker-compose -f docker-compose.ci.build.yml -f docker-compose.yml up --abort-on-container-exit`.
