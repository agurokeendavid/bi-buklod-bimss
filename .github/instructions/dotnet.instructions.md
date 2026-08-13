---
applyTo: "**/*.cs,**/*.csproj"
---

# .NET / C# Instructions

- Target .NET 10.
- Enable nullable reference types.
- Prefer clear domain types and explicit intent over clever abstractions.
- Use async I/O APIs.
- Propagate `CancellationToken` where practical.
- Do not block asynchronous code with `.Result` or `.Wait()`.
- Controllers should be thin.
- Do not expose EF Core entities as web/API contracts.
- Keep validation and authorization server-side.
- Prefer dependency injection over static service locators.
- Add XML/API documentation only where it materially improves maintainability.
- Keep public APIs small.
- Update/add tests with behavior changes.
