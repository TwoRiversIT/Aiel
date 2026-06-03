# Permission System Design Review Three

## 1. Target layer and package ownership

`Aiel.Authorization.EntityFrameworkCore`: I am not entirely clear on what `grant entities` are, but if they are Domain entities then they belong in a a separate assembly. If we truly committed to the Clean Architecture pattern, then we should have the following assembly, regardless of how thin they might be.

- Aiel.Authorization.Application
- Aiel.Authorization.Application.Contracts
- Aiel.Authorization.Application.IntegrationTests
- Aiel.Authorization.Domain
- Aiel.Authorization.Domain.Shared
- Aiel.Authorization.Domain.UnitTests
- Aiel.Authorization.EntityFrameworkCore
- Aiel.Authorization.EntityFrameworkCore.IntegrationTests
- Aiel.Authorization.EntityFrameworkCore.PostgreSql
- Aiel.Authorization.Testing

## 2. Generated output

> The generator also emits an explicit registration extension method. The developer must call it from the dependency module; no assembly scan is performed.

`Aiel.Results` uses the ` [ModuleInitializer]` attribute to automatically register the generated code thereby establishing a framework precedent. We could use the same approach here, or is that too "magical" and the Analyzer is sufficient? Having the consumer manually call the registration does provide an extension point for configuration and/or customization:

```csharp
public sealed class AviendhaApplicationContracts : AielDependencyConfigurator
{
    public override Task PreConfigureAsync(
        DependencyConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        context.Services.ConfigurePermissionDefinitions(permissions
            => permissions.AddAviendhaPermissions(options =>
            { 
                options.UsePostgreSql();
                options.ConfigureSomethingHere(...);
            }));

        return Task.CompletedTask;
    }
}
```
## 3. Permission lifecycle and change management

Genuine Question: Could we use EntityFrameworkCore migrations? They are already well established and the underlying operations are essentially CRUD. If the consumer requires changes to their permissions it is likely that they will require a migration anyway on which the permission migration can piggyback. This will be a common scenario over long lived evolving applications so we could consider a DSL to facilitate permission migrations.

## 4. Scope and subject model

I need more explanation around Scope and Subject. I know ABP has scope, but we are being inspired by ABP, not copying it. So what is the justification for having Scope in Aiel permissions?

Assuming we there is a valid use case, can we make scope extensible? `Host`, `Platform`, and `Tenant` are the defaults, but consumers could conceivably need more. For example, in Aviendha the "Organization" plan allows for `Clinics`. Is there value to allowing Aviendha to add a `Clinic` scope?

> `PermissionSubject.Key` should eventually become a value object, not a raw string. The raw string is shown here only to keep the pseudo-code short.

Aiel is a greenfield project with a core value of Zero Technical Debt. Thus, "... should eventually become a value object, not a raw string." should be: "is a value object".

We already have `StrongId` in the framework as a domain primitive. We should consider promoting it to a first class module and have it be the basis for for all identifiers, keys, etc. Again, greenfield project reminder. If this is the right thing to do, we do it now, not later.

## 5. Permission evaluation context

> `HttpContext` may be used by the ASP.NET Core adapter to read the authenticated principal, but it is not part of the permission check. `IPermissionChecker` receives an evaluation context, not an HTTP object.

To be explicit, the intention is to leverage the ASP.NET Core authorization system, not replace it. What I mean is that I do not want to see a `[RequirePermission]` attribute on a controller action.

## 6. Checker, manager, and store split

> Open design question: decide whether IPermissionStore should expose batch methods only. Since the checker may evaluate several scopes and subjects for one permission, a batch-first store contract may avoid chatty database access from day one.

This looks like premature optimzation to me. I am open to it, but I need a stronger use case than "may avoid". Alternatively, the `IPermissionStore` could be decorated with a caching implemencation, or batching implementation, or both.

TODO: GitHub #8 - We need to formalize a Decorator system for the framework.

## 7. EF Core persistence shape

> The EF package should not own application role tables, user tables, or tenant tables. It stores subject keys supplied by the application adapters.

Please justify that statement.

## 8. Pipeline behavior

> The command and query behaviors should run before handler execution and should return PermissionDeniedError through Result, not throw.

The command and query behaviors MUST run before handler execution and MUST return PermissionDeniedError through Result, not throw.

## 9. ASP.NET Core policy bridge

> Open design question: define where an HTTP request's IExecutionContext is created and how it is made available to the authorization handler, endpoint, command dispatcher, and telemetry pipeline.

Pretty sure this will have to be custom middleware that gets inserted into the ASP.NET Core request pipeline and combined with a controller base class which can extract values from the context property bag and provide dispatch API for commands and queries. Maybe `AielControllerBase` or `PipelineControllerBase`.

A source generator could used to generate the controllers, or maybe just ASP.NET Minimal API classes.

TODO: GitHub #9 Souce Generators for HTTP API Controllers and their clients

## 10. Resource authorization seam

Question: Is data encryption a framework feature, or application concern? In the Aviendha design, the encryption strategy is specific to protecting Clients and supporting Law Enforcement requests. It is based on encrypting POCO Documents (not PDFs or .docx) and storing them with metadata. It does NOT leverage row or column encryption features of the underlying database.

`ReasonCode` should be either a `StrongId` or an `enum`.

> Open design question: decide whether TAction should be a permission name, a resource-specific action value object, or a generated action type associated with the permission constants.


## 11. Questions to resolve in the next review

> 1. Should authored permission metadata use attributes on const string fields, a fluent builder, or both?

Not sure yet. I would like to explore the fluent builder idea though. Would it be instead of a source generator? Would a source generator output leverage a fluent builder? We cannot eliminate the `static` classes with `const string` permission names as that is our defense against magic strings.

> 2. Should PermissionScope.Tenant use Guid initially or wait for a framework-level strong TenantId value object?

No technical debt: StrongId

> 3. Should a broad platform/host Prohibited always override tenant/user grants?

> 4. Should IPermissionStore be batch-only to prevent chatty permission checks?

I addressed this earlier in this document.

> 5. How should the ASP.NET Core adapter create and flow IExecutionContext during authorization?

I proposed some options earlier in this document.

> 6. Should resource actions be raw permission names or a separate generated action model?

Id need more context before I can answer this.
