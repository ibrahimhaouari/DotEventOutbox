# DotEventOutbox.Contracts

## Overview

`DotEventOutbox.Contracts` is part of the `DotEventOutbox` suite, a library designed for effective domain event handling in .NET applications using the Outbox Pattern. This project contains the core contracts and interfaces that define the fundamental behaviors and structures for domain event management.

## Key Components

- `IEvent`: Interface for domain events, extending MediatR's `INotification`.
- `DomainEvent`: An abstract record providing a base implementation for domain events with a unique identifier and a timestamp.
- `IDomainEventEmitter`: Interface defining the contract for an emitter of domain events, capable of tracking and managing domain events.

## Usage

To use these contracts in your application, implement these interfaces and abstract records according to your domain requirements.
