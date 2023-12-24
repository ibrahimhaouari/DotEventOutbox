# DotEventOutbox.Contracts

`DotEventOutbox.Contracts` is an integral part of the DotEventOutbox library, designed to aid in the implementation of the outbox pattern within .NET applications using MediatR and Entity Framework. It consists of essential interfaces and classes that define and manage domain events.

## Key Components

### `IEvent` Interface

- **Purpose**: Serves as the foundational interface for all domain events in your application.
- **Features**:
  - Inherits from `MediatR.INotification`, enabling compatibility with MediatR's messaging and handler patterns.
  - Includes properties for a unique event identifier (`Id`) and the timestamp of the event's occurrence (`OccurredOnUtc`).

### `DomainEvent` Record

- **Purpose**: Provides an abstract base record for domain events.
- **Features**:
  - Offers default implementations for the `IEvent` interface.
  - Automatically generates a unique identifier (`Id`) and sets the occurrence timestamp to the current UTC time, simplifying domain event creation.

### `IDomainEventEmitter` Interface

- **Purpose**: Defines the contract for objects that can emit domain events.
- **Features**:
  - Contains a property to access a read-only collection of domain events (`Events`).
  - Includes a method to clear these events (`ClearEvents`), facilitating event tracking and lifecycle management.

These components collectively provide the foundation for effective domain event management in applications, ensuring consistency, idempotency, and seamless integration with MediatR and Entity Framework.
