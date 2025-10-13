# ![Logo](https://raw.githubusercontent.com/ceronus/medius/master/doc/icons/icon-64x64.png) Medius

[![NuGet version (Medius)](https://img.shields.io/nuget/v/medius?style=for-the-badge)](https://www.nuget.org/packages/medius/)  
[![Build](https://github.com/ceronus/medius/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/ceronus/medius/actions/workflows/ci.yml)

- [NuGet Package](https://www.nuget.org/packages/medius)
- [Release Notes](https://github.com/ceronus/medius/releases)
- [License](LICENSE.md)

---

## Overview
**Medius** is a lightweight operation dispatcher for .NET.  
It follows an **ACQRS** (Actions–Commands–Queries–Responsibility Segregation) model,
providing a simple, type-safe way to execute operations through dedicated handlers.

### In Plain Terms
**Medius** helps keep software organized.  
It makes sure that when an application needs to **do something** or **ask for something**,  
the right piece of code handles it — clearly and efficiently.  
It’s built to make applications simpler, more predictable, and easier to maintain.

---

### Features
- Supports Commands, Queries, and Actions  
- Scoped lifetime handling for safe DI use  
- Simple and automatic handler discovery  
- Zero external dependencies 

---

## Quick Start
```csharp
// Register Medius
services.AddMedius();

// Define an operation
public sealed class GetTimeQuery : IMediusQuery<DateTime> { }

// Create its handler
public sealed class GetTimeHandler : MediusQueryHandler<GetTimeQuery, DateTime>
{
    public override Task<DateTime?> HandleQueryAsync(GetTimeQuery query, CancellationToken ct)
        => Task.FromResult<DateTime?>(DateTime.UtcNow);
}

// Invoke the operation
var result = await medius.InvokeAsync(new GetTimeQuery());
```

---

## Concepts

ACQRS stands for **Actions, Commands, and Queries with Responsibility Segregation**.  
It extends traditional CQRS principles by explicitly defining “Actions” as first-class operations 
that modify state *and* return results — providing a balanced approach between commands and queries.

| Operation Type | Base Interface | Purpose | Typical Return Type | Example Handler |
|----------------|----------------|----------|----------------------|-----------------|
| **Command** | `IMediusCommand` | Performs an action or side effect; does **not** return a result. | `MediusUndefinedType` | `MediusCommandHandler<TCommand>` |
| **Query** | `IMediusQuery<TQueryResult>` | Retrieves or computes data without modifying state. | `TQueryResult` | `MediusQueryHandler<TQuery, TQueryResult>` |
| **Action** | `IMediusAction<TActionResult>` | Performs an operation and **returns a result** (e.g., create, update, or process something). | `TActionResult` | `MediusActionHandler<TAction, TActionResult>` |

Each operation has a matching handler that defines its execution logic.  
All handlers are automatically discovered and registered by **Medius** during startup.

---

## 💡 **Highlights**
- Zero configuration — handlers are auto-discovered via reflection  
- DI-friendly (handlers can depend on scoped or transient services)  
- No runtime reflection overhead after startup  
- Fully documented API with XML summaries  

---

## Project Goals
- **Lightweight by design** — focuses on operation dispatching without external dependencies or pipelines.  
- **Framework-agnostic** — integrates cleanly into any .NET project using standard dependency injection.  
- **Explicit, not opinionated** — encourages simple, typed operations over hidden behaviors or conventions.  
- **Built around ACQRS** — unifies **Actions**, **Commands**, and **Queries** into a clear, consistent execution model.  
- **Not a MediatR clone** — built independently to provide a smaller, faster, and clearer alternative for internal dispatching needs.  

### Comparison Note

> **Medius** and **MediatR** share the broad idea of dispatching operations,  
> but they differ fundamentally in purpose and design philosophy.

**Medius**
- Self-contained — no external dependencies  
- Automatically discovers and registers handlers  
- Explicitly manages dependency-injection scopes  
- No pipeline or decorator infrastructure  
- ~300–400 lines of code — simple, lean, and transparent  

**MediatR**
- Includes a full behavioral pipeline with pre/post processors  
- Requires explicit registration for notifications  
- Introduces its own abstraction layers and decorators  
- Focuses on extensibility and pattern orchestration  

Medius is not a MediatR clone.  
It was built independently as a smaller, faster, and clearer alternative  
for projects that only need clean **operation dispatching**,  
not full mediator-pattern orchestration.

---

## Requirements
- .NET 8.0 or later  


© 2025 ceronus — MIT License
