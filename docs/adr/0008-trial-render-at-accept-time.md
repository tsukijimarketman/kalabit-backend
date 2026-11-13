# ADR-0008: Validate template data with a trial render at accept time

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Key decision 8 (p.5); Templates (p.10–11); Request processing order (p.9)

## Context

A request names a template and supplies data for its variables. If the data is missing a variable, we want to reject the request immediately with a clear error, rather than accept it and fail later in the worker. The PRD plans a JSON Schema per template version and validates request data against it at accept time. That needs a schema-validation library, a schema authoring step for every template, and a way to keep schemas in sync with template text.

## Decision

At accept time, the Sender API does a **trial render** of the template version with the supplied data, using Fluid with **strict variables** and the same sandbox limits as real sends (maximum steps and a limited output size).

- If the render reports missing variables, the request is rejected with **`422 Unprocessable Content`** and a problem-details body listing the missing names.
- If the render succeeds, the request is accepted. The worker renders again at send time.
- Parsed templates are cached, so each render does not re-parse the template.

## Consequences

**Positive**

- No new dependency and no separate schema to maintain. The template itself is the contract.
- It catches the most common integration mistake (a missing or misspelled variable) before anything is stored.
- It uses the same renderer as the worker, so "accepted" means "this data renders".

**Negative**

- Every accepted request is rendered twice (accept and send). The CPU cost is small with caching and the sandbox limits.
- No type checking. A number where a string was expected still renders.
- Only variables the template actually reaches are checked. Variables in branches that the given data doesn't enter are not caught.

## Revisit when

- We need type or format checks (numbers, dates, email addresses) on template data.
- Rendering at accept time shows up as a meaningful cost in the load-test results.
