---
name: implement-library-feature
description: Implement library-management features in this .NET layered project. Use when adding workflows such as soft delete, export, filtering, request processing, borrowing, returning, or role-based behavior.
disable-model-invocation: true
---

# Implement Library Feature

Use this workflow for features that touch service, controller, repository, domain, or tests.

## 1. Locate The Layer Boundary

- Public use case: add or update `ILibraryService`.
- Caller-facing wrapper: add or update `ILibraryController`.
- Business workflow: implement in `LibraryService`.
- Entity state change: add a method to `User`, `Book`, or `LibraryRequest`.
- Storage-only operation: add method to repository abstraction and in-memory implementation.

Do not put business rules in controllers or repositories.

## 2. Add Behavior In This Order

1. Add interface method signatures.
2. Implement service workflow.
3. Add domain methods for state mutation.
4. Extend repositories only if existing `Find`, `Add`, or `Remove` is not enough.
5. Forward controller calls to service.
6. Add focused tests.

## 3. Error Rules

- `ArgumentException`: invalid primitive input, for example empty title or non-positive quantity.
- `BusinessRuleViolationException`: invalid business state, for example missing user, wrong role, duplicate name, unavailable copy, or already processed request.
- Never use `InvalidOperationException` for expected user/business failures in `LibraryService`.

## 4. Test Checklist

For every new feature, add tests under `tests/Lab1.Tests`:

- success path;
- invalid input path;
- business-rule failure path;
- controller forwarding/validation path if the controller surface changed;
- repository test if repository behavior changed;
- role-combination path if librarians can auto-approve the workflow.

Use existing test naming style: `Method_WhenCondition_ShouldResult` where possible.

## 5. Required Commands

Run:

```bash
dotnet test Itmo.ObjectOrientedProgramming.sln
python3 scripts/check-agent-conventions.py
```

Fix all failures before finishing.
