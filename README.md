# DotEventOutbox Library

## Overview

`DotEventOutbox` is a .NET library designed to facilitate robust and efficient handling of domain events using the Outbox Pattern. It provides a suite of tools and services to ensure reliable and scalable event processing, particularly in distributed systems or microservices architectures.

## Features

- **Domain Event Handling**: Define and handle domain events in your application.
- **Outbox Pattern Implementation**: Reliably handle event dispatch to ensure consistency and fault tolerance.
- **Idempotent Processing**: Ensure each event is processed only once.
- **Configurable and Extendable**: Tailor the library to fit the specific needs of your application.

## Getting Started

### Installation

You can install the `DotEventOutbox` library via NuGet package manager. Run the following command:

```bash
dotnet add package DotEventOutbox
```

### Basic Setup

1. **Configure Services**: Add `DotEventOutbox` to your service collection in the `Startup.cs` or wherever you configure services.

   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
   services.AddOutbox(Configuration, options =>
   options.UseSqlServer(Configuration.GetConnectionString("YourConnectionString")));
   }
   ```

2. **Applying Migrations**: Apply migrations to set up the necessary database tables.
   - For development, you can apply migrations automatically during application startup.
   - For production, it's recommended to apply migrations manually or as part of your CI/CD pipeline.

### Usage

- Define domain events implementing `IEvent`.
- Implement `IDomainEventEmitter` in your entities.
- Use `OutboxMessageProcessingJob` for processing and dispatching events.

## Advanced Configuration

- Customize the `OutboxMessage` entity configurations as needed.
- Configure Quartz jobs for event processing schedules.

## Contributing

Contributions are welcome! If you have a feature request, bug report, or pull request, please open an issue or submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE.md).

## Acknowledgements

Special thanks to the contributors and users of `DotEventOutbox`. Your feedback and support are greatly appreciated!
