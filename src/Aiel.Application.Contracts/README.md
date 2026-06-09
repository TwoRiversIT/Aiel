# Aiel.Application.Contracts

This project contains the application layer contracts for the Aiel application. It defines the interfaces and data transfer objects (DTOs) that are used to communicate between the application layer and other layers of the application, such as the domain layer and the presentation layer.

## Key Features

- **Actions**: Defines the `IAction` interface which is the basis for all actions: Commands and Queries
- **Commands**: Defines the `ICommand` interface which represents a command that can be executed to perform an action that changes the state of the application.
- **Domain**: Contains the basic domain entities and value object base classes that represent the core business logic of an application.
- **Execution**: Provides the context for executing actions, including command handlers and query handlers.
- **Queries**: Defines the `IQuery` interface which represents a action that can be executed to retrieve data without changing the state of the application.

