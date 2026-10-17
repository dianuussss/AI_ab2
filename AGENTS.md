# Project Instructions

This repository is a .NET 9 library-management lab. Follow the existing layered architecture instead of putting all logic in one place.

## Architecture

- Domain code lives in `src/Lab1/Itmo.ObjectOrientedProgramming.Lab1.Domain`.
- Application use cases live in `src/Lab1/Itmo.ObjectOrientedProgramming.Lab1.Application`.
- Repository interfaces live in `src/Lab1/Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions`.
- In-memory repository implementations live in `src/Lab1/Itmo.ObjectOrientedProgramming.Lab1.Infrastructure`.
- Controller-facing code lives in `src/Lab1/Itmo.ObjectOrientedProgramming.Lab1.Presentation`.
- Tests live in `tests/Lab1.Tests`.

## Layer Rules

- Do not import `Application`, `Infrastructure`, or `Presentation` from `Domain`.
- Do not import `Presentation` or `Infrastructure.InMemory` from `Application`.
- Do not import repositories or `Infrastructure` from controllers.
- Controllers delegate to `ILibraryService`; they only do simple argument validation.
- Business rules belong in `LibraryService` or domain models, not in repositories or controllers.
- Repositories store and return entities. They must not approve requests, change user roles, or enforce borrowing rules.

## Errors

- Use `ArgumentException` for invalid primitive input such as empty names, invalid titles, or non-positive quantities.
- Use `BusinessRuleViolationException` for domain/business conflicts: duplicate users, missing users, wrong roles, already borrowed books, unavailable copies, or non-pending requests.
- Do not throw `InvalidOperationException` from application workflows for expected business failures.

## Feature Changes

When adding a library feature:

1. Update the public abstraction first, usually `ILibraryService` and, if exposed to callers, `ILibraryController`.
2. Put the workflow in `LibraryService`.
3. Add or update domain model methods only for entity state changes.
4. Add repository methods only when data access cannot be expressed by the existing interfaces.
5. Add tests for success, validation errors, business-rule errors, and auto-approval behavior if roles are involved.

For detailed steps, use `skills/implement-library-feature/SKILL.md`.

## Testing And Checks

Run these commands before handing off:

```bash
dotnet test Itmo.ObjectOrientedProgramming.sln
python3 scripts/check-agent-conventions.py
```

The check script must report `Violations: 0`.

## Checkable Convention Map

These instructions are intentionally written so they can be checked automatically:

- Layer dependencies are checked by `ARCH001`, `ARCH002`, and `ARCH003` in `scripts/check-agent-conventions.py`.
- Controller-to-repository access is checked by `ARCH004`.
- Service construction of concrete in-memory repositories is checked by `ARCH005`.
- Wrong exception choice in application workflows is checked by `ERR002`.
- Unfinished source code is checked by `SRC001`.
- Missing tests for public service methods are checked by `TEST001`.

If a rule cannot be checked by the script or by `dotnet test`, rewrite the rule until it becomes concrete.

## Do Not

- Do not add console apps or UI entry points unless the task explicitly asks for one.
- Do not bypass `ILibraryService` from presentation code.
- Do not put `InMemory*Repository` construction inside application services.
- Do not use `NotImplementedException` in committed source code.
- Do not weaken `Directory.Build.props`, `.editorconfig`, or test assertions to make checks pass.
