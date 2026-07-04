# Employee Lifecycle Portal
# Contributing Guide

Version: 1.0

---

# Purpose

Defines contribution standards.

---

# Branch Strategy

main

↓

develop

↓

feature/*

↓

bugfix/*

↓

hotfix/*

---

# Commit Messages

Examples

Sprint 17 - Logging Infrastructure

Sprint 18 - Audit Trail

Sprint 19 - Attendance Module

---

# Pull Requests

Every PR must:

- Build Successfully
- Pass Tests
- Follow Architecture
- Update Documentation

---

# Coding Rules

Follow

- Clean Architecture
- SOLID
- CQRS
- Repository Pattern

---

# Review Checklist

Controllers Thin

Business Logic in Handlers

Validation Exists

Logging Exists

Authorization Applied

DTOs Used

Tests Added

Documentation Updated

---

# AI Contribution Rules

Amazon Q

GitHub Copilot

Claude

ChatGPT

must always

- analyze architecture
- replace complete files
- never bypass layers
- always build successfully

---

# Repository Rules

Never commit broken builds.

Never commit secrets.

Never commit connection strings.

Always update sprint documentation.

Always push working code.