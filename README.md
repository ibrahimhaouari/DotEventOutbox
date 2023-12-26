# DotEventOutbox

## Overview

DotEventOutbox is a powerful .NET library created to enhance the implementation of the outbox pattern, integrating seamlessly with MediatR and Entity Framework. This library is key in ensuring that domain events are processed in a reliable, consistent, and idempotent manner.

## Key Features

- **Seamless Integration with MediatR and Entity Framework**: DotEventOutbox provides an effortless and smooth integration for managing and dispatching domain events using MediatR and Entity Framework.
- **Idempotent Processing**: Guarantees that each event is processed exactly once, thereby preventing any possibility of duplicate event handling.
- **Advanced Outbox Message Management**: Automates the conversion, storage, and lifecycle management of domain events into outbox messages.
- **Robust Dead Letter Queue**: Implements a system to manage failed messages, allowing for their later analysis or reprocessing.
- **Quartz Integration**: Provides an automated and scheduled approach to outbox message processing using Quartz.
- **Configurable Settings**: Offers a wide range of customizable settings through `EventOutboxSettings` to tailor the outbox behavior to your specific needs.

## Installation

Install DotEventOutbox via NuGet:

```bash
Install-Package DotEventOutbox
```

Or through the .NET Core CLI:

```bash
dotnet add package DotEventOutbox
```

Both commands will download and install DotEventOutbox along with all necessary dependencies.

## Contracts-Only Package

For projects needing only the contracts of DotEventOutbox, such as `IEvent`, `DomainEvent`, and `IDomainEventEmitter`, use the DotEventOutbox.Contracts package:

```bash
dotnet add package DotEventOutbox.Contracts
```

This is ideal for separating DotEventOutbox contracts from their handlers in different assemblies or projects.

## Service Registration

Register DotEventOutbox services easily with `IServiceCollection`:

```csharp
services.AddDotEventOutbox(configuration,
  options => options.UseNpgsql(configuration.GetConnectionString("AppDb")));
```

## Database Migration

Create necessary tables by executing:

```csharp
await app.MigrateDotEventOutbox();
```

This will create `OutboxMessages`, `OutboxMessageConsumers`, and `DeadLetterMessages` tables.

## Configuration Details

Customize DotEventOutbox using these settings:

- **ProcessingIntervalInSeconds**: Time interval for processing outbox messages.
- **MaxMessagesProcessedPerBatch**: Maximum number of messages processed per batch.
- **RetryIntervalInMilliseconds**: Time delay between retry attempts for failed messages.
- **MaxRetryAttempts**: Maximum retry attempts before moving a message to the dead-letter queue.

Example `appsettings.json`:

```json
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

Here's a basic example in a console application:

```csharp
// Program.cs
// Create a new user instance
var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Demo.com");

// Raise the UserCreatedDomainEvent
user.RaiseEvent(new UserCreatedDomainEvent(user.Name, user.Email));

// Add the new user to the DbContext
dbContext.Users.Add(user);

// Save changes and process domain events using OutboxCommitProcessor
var outboxCommitProcessor = scope.ServiceProvider.GetRequiredService<IOutboxCommitProcessor>();
await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);
```

For a comprehensive example, visit the [Demo Repository](https://github.com/ibrahimhaouari/DotEventOutbox/tree/main/src/DotEventOutbox.Demo).

## Contributing

Contributions and suggestions are highly appreciated. Please review our [Contributing Guidelines](CONTRIBUTING.md) for detailed information on how to participate.

## License

DotEventOutbox is open-sourced under the [MIT License](LICENSE.md).
