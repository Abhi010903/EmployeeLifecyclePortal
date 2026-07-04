# Employee Lifecycle Portal
# Enterprise Coding Standards

Version: 1.0

---

# Purpose

This document defines the coding standards, development rules, naming conventions, architecture guidelines, and engineering principles that every contributor and AI coding assistant must follow while working on the Employee Lifecycle Portal.

These rules are mandatory and ensure that the codebase remains maintainable, scalable, secure, and production-ready.

---

# Core Principles

The project follows:

- Clean Architecture
- SOLID Principles
- DRY (Don't Repeat Yourself)
- KISS (Keep It Simple)
- Separation of Concerns
- CQRS
- Repository Pattern
- Unit of Work
- Dependency Injection
- Enterprise Security Practices

No code should violate these principles.

---

# Development Workflow

Every sprint follows this sequence:

1. Analyze the current architecture.
2. Identify required folders.
3. Identify required files.
4. Create all folders in one batch.
5. Create all files in one batch.
6. Replace complete file contents only.
7. Build the solution.
8. Resolve compile errors.
9. Test all implemented features.
10. Commit changes.
11. Push to GitHub.
12. Begin the next sprint.

Never perform partial implementations.

Never leave the repository in a broken state.

The main branch must always build successfully.

---

# General Coding Rules

Always write production-quality code.

Never write temporary code.

Never write placeholder implementations.

Never comment out unfinished code.

Never duplicate logic.

Never expose internal implementation details.

Always prefer readability over cleverness.

Always prefer maintainability over short code.

---

# Naming Conventions

## Classes

PascalCase

Examples

Employee

EmployeeRepository

CreateEmployeeCommand

UpdateEmployeeCommandHandler

---

## Interfaces

Always begin with I

Examples

IEmployeeRepository

IUnitOfWork

ICurrentUserService

---

## Methods

PascalCase

Examples

CreateEmployee()

AssignRole()

CommitAsync()

---

## Variables

camelCase

Examples

employee

department

userRepository

---

## Private Fields

_prefixCamelCase

Examples

_employeeRepository

_unitOfWork

_passwordHasher

---

## Constants

PascalCase

Example

DefaultPageSize

---

## Enums

PascalCase

Enum values also use PascalCase.

---

# Folder Naming

Folders use PascalCase.

Correct

Commands

Queries

Repositories

Validators

Middleware

Incorrect

commands

repositories

middlewares

---

# File Naming

One class per file.

The filename must match the class name.

Correct

EmployeeRepository.cs

Employee.cs

Role.cs

---

# Project Structure Rules

API

Contains only:

Controllers

Middleware

Configuration

Program.cs

No business logic.

---

Application

Contains:

Commands

Queries

Handlers

Validators

DTOs

Interfaces

Authorization

Business workflows

No Entity Framework.

---

Domain

Contains:

Entities

Enums

Value Objects

Business Rules

No infrastructure references.

---

Infrastructure

Contains:

Repositories

DbContext

JWT

Logging

Email

Caching

Persistence

External Services

No controller code.

---

Shared

Contains reusable components only.

---

# Controller Rules

Controllers must remain thin.

Controllers must never:

Contain business logic.

Access DbContext.

Call repositories directly.

Perform validation.

Hash passwords.

Generate JWT tokens.

Controllers only communicate with MediatR.

---

# Handler Rules

Handlers contain business logic.

Handlers should:

Perform one use case.

Use repositories through interfaces.

Use Unit Of Work.

Use CancellationToken.

Return DTOs.

Never access HttpContext directly.

---

# Repository Rules

Repositories handle persistence only.

Repositories must never:

Contain business logic.

Perform validation.

Generate DTOs.

Return IActionResult.

Repositories communicate only with DbContext.

---

# Entity Rules

Entities represent business objects.

Entities should:

Protect invariants.

Validate constructor inputs.

Expose behavior.

Avoid public setters.

Avoid anemic models.

---

# DTO Rules

DTOs exist only for communication.

Never expose entities through API.

Every request and response should use DTOs.

DTOs should contain only required fields.

---

# Validation Rules

Every request must be validated.

FluentValidation is mandatory.

Validation belongs before handlers execute.

Business validation belongs inside handlers.

---

# Exception Handling

Never catch exceptions unnecessarily.

Unhandled exceptions are processed by ApiExceptionMiddleware.

Never expose stack traces.

Always return meaningful HTTP responses.

---

# Authentication Rules

Use JWT.

Passwords must always be hashed.

Never store plain text passwords.

Always validate credentials.

Use BCrypt.

---

# Authorization Rules

Use policy-based authorization.

Avoid hardcoded role checks.

Controllers should use authorization attributes.

Business logic should not depend on UI authorization.

---

# Dependency Injection

Every service must be injected.

Avoid static services.

Avoid service locator patterns.

Never instantiate repositories manually.

---

# Async Rules

Use asynchronous programming.

Prefer:

async

await

CancellationToken

Avoid synchronous database operations.

---

# Entity Framework Rules

Use DbContext only inside Infrastructure.

Application never references EF Core.

Always use migrations.

Never disable tracking without reason.

Use Include() only when necessary.

---

# Logging Rules

Use structured logging.

Never log passwords.

Never log secrets.

Log:

Request

Response

Execution Time

Exceptions

Correlation Id

---

# Security Rules

Always validate input.

Never trust client input.

Prevent SQL Injection.

Prevent XSS.

Use HTTPS.

Store secrets securely.

Rotate keys when required.

---

# API Standards

RESTful endpoints.

Correct HTTP verbs.

Correct status codes.

Pagination for collections.

Filtering support.

Sorting support.

Version APIs.

---

# Git Standards

Commit after every completed sprint.

Commit messages should be meaningful.

Examples

Sprint 17 - Logging Infrastructure

Sprint 18 - Refresh Token Implementation

Sprint 19 - Audit Trail

Never commit broken code.

---

# Documentation Rules

Every major feature should have documentation.

Update:

Architecture

Database

API

Security

Sprint Plan

Roadmap

when changes occur.

---

# Performance Guidelines

Avoid unnecessary allocations.

Avoid N+1 queries.

Use pagination.

Use indexing.

Prefer async operations.

Measure before optimizing.

---

# Testing Standards

Every important feature should have:

Unit Tests

Integration Tests

API Tests

Authentication Tests

Authorization Tests

Regression Tests

---

# Code Review Checklist

Before committing ensure:

Solution builds successfully.

No warnings of concern.

No duplicate logic.

Validation exists.

Authorization exists.

Logging exists where appropriate.

DTOs are used.

Architecture rules are respected.

Tests pass.

Documentation is updated.

---

# AI Assistant Guidelines

When using Amazon Q, GitHub Copilot, Claude, ChatGPT, or any AI coding assistant:

Always analyze the existing architecture before generating code.

Never generate partial implementations.

Never insert code snippets into existing files.

Always replace complete files.

Follow the sprint workflow.

Do not introduce new architectural patterns without justification.

Respect the dependency rules.

Ensure generated code builds successfully.

Prioritize production-quality code over quick fixes.

---

# Final Engineering Philosophy

The Employee Lifecycle Portal is intended to demonstrate enterprise backend engineering.

Every design decision should prioritize:

- Maintainability
- Security
- Scalability
- Testability
- Readability
- Extensibility
- Production Readiness

Shortcuts that compromise these goals are not acceptable.

The codebase should be suitable for real-world deployment and serve as a professional portfolio project that reflects enterprise software engineering practices.