# Phase 04a Contract-Boundary Validation Brief

- **Date:** 2026-05-25T16:14:59.732-07:00
- **Author:** Verin
- **Scope:** First atomic 04a slice = Task 1 plus Task 2 together. Move the public CQRS and execution contracts into `Aiel.Application.Contracts`, then delete the old definitions in the same change.
- **Baseline observed in this worktree:**
  - `Aiel.Application.Contracts.UnitTests`: 1/1 passing
  - `Aiel.Application.UnitTests`: 58/58 passing
  - `Aiel.Analyzers.UnitTests`: 9/9 passing
- **Non-collision rule:** Do not mix permission runtime, generator, EF Core, ASP.NET Core, or Aviendha adoption work into this slice.

## First failing-test path

### Primary red gate — `Aiel.Application.Contracts.UnitTests`

- **Target project:** `Aiel/tests/Aiel.Application.Contracts.UnitTests/Aiel.Application.Contracts.UnitTests.csproj`
- **Add file:** `Aiel/tests/Aiel.Application.Contracts.UnitTests/Aiel/Contracts/ApplicationContractSurfaceTests.cs`
- **Make it fail first by asserting that the contract package exposes:**
  - `ICommand : IAction`
  - `IQuery<TResult> : IAction`
  - `ICommandHandler<TCommand>`
  - `IQueryHandler<TQuery, TResult>`
  - `ICommandDispatcher`
  - `IQueryDispatcher`
  - `ICommandPipelineBehavior<TCommand>`
  - `IQueryPipelineBehavior<TQuery, TResult>`
  - `IExecutionContext`, `IActionExecutionContext<TAction>`, and `ActionExecutionContext<TAction>`
- **Expected red reason today:** the CQRS interfaces still live under `Aiel.Application`, and `IExecutionContext` is still split across `Aiel.Application.Contracts` and `Aiel.Domain`.

### Secondary red gate — `Aiel.Application.UnitTests`

- **Target project:** `Aiel/tests/Aiel.Application.UnitTests/Aiel.Application.UnitTests.csproj`
- **Add file:** `Aiel/tests/Aiel.Application.UnitTests/Aiel/Contracts/ContractBoundaryAssemblyTests.cs`
- **Retarget these existing tests to the new contract boundary before implementation:**
  - `Aiel/tests/Aiel.Application.UnitTests/Aiel/Commands/DefaultCommandDispatcherTests.cs`
  - `Aiel/tests/Aiel.Application.UnitTests/Aiel/Commands/CommandLoggingPipelineBehaviorTests.cs`
  - `Aiel/tests/Aiel.Application.UnitTests/Aiel/Queries/DefaultQueryDispatcherTests.cs`
  - `Aiel/tests/Aiel.Application.UnitTests/Aiel/Queries/QueryLoggingPipelineBehaviorTests.cs`
  - `Aiel/tests/Aiel.Application.UnitTests/Aiel/DependencyInjection/AddAielCqrsTests.cs`
  - `Aiel/tests/Aiel.Application.UnitTests/Aiel/Domain/DefaultDomainEventDispatcherTests.cs`
- **Green assertions:**
  - Moved contracts load from `Aiel.Application.Contracts`.
  - `Aiel.Application` no longer defines `Aiel.Commands.ICommand`, `Aiel.Queries.IQuery\`1`, or the moved handler, dispatcher, and pipeline interfaces.
  - `Aiel.Domain` no longer defines `Aiel.Execution.IExecutionContext`.
  - Dispatcher ordering and child-context behavior still pass.
- **Keep and extend:** `Aiel/tests/Aiel.Application.Contracts.UnitTests/Aiel/Execution/ActionExecutionContextTests.cs` should continue to prove actor, correlation, causation, properties, payload, and `ClientInstanceId` preservation.

## Focused preflight gate

Treat this command pair as authoritative for the first 04a slice:

1. `dotnet test .\Aiel\tests\Aiel.Application.Contracts.UnitTests\Aiel.Application.Contracts.UnitTests.csproj --nologo --verbosity minimal`
2. `dotnet test .\Aiel\tests\Aiel.Application.UnitTests\Aiel.Application.UnitTests.csproj --nologo --verbosity minimal`

Use `Aiel.Analyzers.UnitTests` only as a secondary smoke check for this slice. It is green today, but it does not prove the move-delete boundary is complete.

## Verin rejection cases

I will reject the first 04a attempt if it leaves duplicate contract definitions, adds shims or `[Obsolete]` wrappers, weakens `IExecutionContext` to avoid fixing the actor mismatch, keeps tests on the old namespaces, or bundles permission/runtime work into the same PR. I will also reject a change that passes only by leaving `Aiel.Application` or `Aiel.Domain` as parallel contract owners instead of proving the old files are gone.
