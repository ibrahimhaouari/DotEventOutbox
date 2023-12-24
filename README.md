# DotEventOutbox

## Overview

DotEventOutbox is a .NET library designed to streamline the implementation of the outbox pattern with MediatR and Entity Framework. It provides a robust mechanism to manage domain events, ensuring their consistency, reliability, and idempotent processing.

## Features

- **Integration with MediatR and Entity Framework**: Seamless interaction with these frameworks.
- **Idempotent Event Processing**: Guarantees that each event is processed only once.
- **Automated Outbox Message Handling**: Converts domain events into outbox messages and manages their lifecycle.
- **Dead Letter Handling**: Moves failed messages to a dead-letter queue for later analysis or reprocessing.
- **Quartz Integration**: Schedules and automates the processing of outbox messages.
- **Configurable**: Offers customization options via `EventOutboxSettings`.

## Getting Started

### Installation

To install DotEventOutbox, use the following NuGet command:

```
Install-Package DotEventOutbox
```

### Configuration

Configure the library in your application's startup by calling `AddOutbox`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
services.AddOutbox(Configuration, options =>
{
options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
});
}
```

### Usage

Simply raise domain events in your application. The library handles the rest, ensuring that these events are processed and managed effectively.

## Documentation

### Key Components

- `IEvent`: Interface for domain events.
- `OutboxCommitProcessor`: Processes and saves domain events as outbox messages.
- `OutboxMessageProcessingJob`: Quartz job for processing outbox messages.
- `IdempotencyDomainEventHandlerDecorator`: Ensures idempotent processing of domain events.

### Entity Framework Configurations

Entity configurations for `OutboxMessage`, `OutboxMessageConsumer`, and `DeadLetterMessage` are provided to manage the underlying database schema effectively.

## Contributing

Contributions are welcome! Please read our contributing guidelines for more information.

## License

This project is licensed under the [MIT License](LICENSE.md).
