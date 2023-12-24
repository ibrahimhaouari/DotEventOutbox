# DotEventOutbox

## Overview

DotEventOutbox is a .NET library designed to streamline the implementation of the outbox pattern with MediatR and Entity Framework. It ensures reliable, consistent, and idempotent processing of domain events.

## Features

- **MediatR and Entity Framework Integration**: Seamless interaction for managing and dispatching domain events.
- **Idempotent Processing**: Ensures each event is processed only once, avoiding duplicate handling.
- **Automated Outbox Message Management**: Handles conversion and lifecycle of domain events as outbox messages.
- **Dead Letter Queue**: Manages failed messages for later analysis or reprocessing.
- **Quartz Integration**: Automates and schedules outbox message processing.
- **Configurable Settings**: Customizable behavior through `EventOutboxSettings`.

## Installing DotEventOutbox

You should install [DotEventOutbox with NuGet](https://www.nuget.org/packages/DotEventOutbox):

```bash
Install-Package DotEventOutbox
```

Or via the .NET Core command line interface:

```bash
dotnet add package DotEventOutbox
```

Either commands, from Package Manager Console or .NET Core CLI, will download and install DotEventOutbox and all required dependencies.

## Using Contracts-Only Package

To reference only the contracts for DotEventOutbox, which includes:

- `IEvent`
- `DomainEvent`
- `IDomainEventEmitter`

Add a package reference to [DotEventOutbox.Contracts](https://www.nuget.org/packages/DotEventOutbox.Contracts)

This package is useful in scenarios where your DotEventOutbox contracts are in a separate assembly/project from handlers.

## Registering with `IServiceCollection`

DotEventOutbox supports `Microsoft.Extensions.DependencyInjection.Abstractions` directly. To register various DotEventOutbox services:

```csharp
services.AddDotEventOutbox(configuration,
  options => options.UseNpgsql(configuration.GetConnectionString("AppDb")));
```

## Configuration

Detail the available settings in `DotEventOutbox` and their impact:

- **ProcessingIntervalInSeconds**: Defines the frequency of outbox processing cycles.
- **MaxMessagesProcessedPerBatch**: Limits the number of messages processed in a single batch to optimize performance.
- **RetryIntervalInMilliseconds**: Specifies the delay between retry attempts for failed message processing.
- **MaxRetryAttempts**: Sets the maximum number of retries for each message before moving it to the dead-letter queue.

```json
// appsettings.json
{
  "ConnectionStrings": {
    "AppDb": "Your-Database-Connection-String"
  },
  "DotEventOutbox": {
    "ProcessingIntervalInSeconds": 10,
    "MaxMessagesProcessedPerBatch": 10,
    "RetryIntervalInMilliseconds": 50,
    "MaxRetryAttempts": 3
  }
}
```

## Example Usage

Below is an example demonstrating how to use DotEventOutbox in a console application:

```csharp
// Program.cs
var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Demo.com");
dbContext.Users.Add(user);
var outboxCommitProcessor = scope.ServiceProvider.GetRequiredService<IOutboxCommitProcessor>();
await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);
```

## Contributing

We welcome contributions and suggestions! Please read through our contributing guidelines for more information on how to get started.

## License

This project is licensed under the [MIT License](LICENSE.md).
