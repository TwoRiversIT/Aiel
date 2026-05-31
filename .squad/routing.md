# Work Routing

How to decide who handles what.

## Routing Table

| Work Type                                              | Route To | Examples                                                                                                                              |
| ------------------------------------------------------ | -------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Architecture, boundaries, shared decisions             | Moiraine | Aiel/Aviendha seams, ADRs, integration patterns, platform direction                                                                   |
| Aiel framework and core services                       | Perrin   | Hosting model, composition, primitives, extension points, shared infrastructure                                                       |
| Aviendha SaaS flows and UX                             | Elayne   | Tenant journeys, feature delivery, user workflows, application behavior on top of Aiel                                                |
| Accessibility, inclusive design                        | Egwene   | Accessibility diagnostics, inclusive design guidance, WCAG conformance, assistive technology testing                                  |
| Reliability, diagnostics, operations                   | Nynaeve  | Logging, health checks, resilience, incident debugging, operational readiness                                                         |
| Documentation, runbooks, onboarding                    | Loial    | READMEs, ADR polish, architecture docs, runbooks, migration guides, developer onboarding                                              |
| Code review                                            | Moiraine | Review PRs, check quality, suggest improvements on architectural or cross-cutting work                                                |
| Component testing — unit, contract, edge cases         | Verin    | Behavioural unit tests, contract tests, edge-case tests, regression tests, invariant tests, seam validation between Aiel and Aviendha |
| System validation — E2E, acceptance, smoke             | Faile    | Playwright E2E suite, system acceptance criteria, cross-component boundary verification, tenant isolation, PHI-safety at boundary     |
| Scope & priorities                                     | Moiraine | What to build next, trade-offs, decisions                                                                                             |
| Session logging                                        | Scribe   | Automatic — never needs routing                                                                                                       |

## Issue Routing

| Label            | Action                                               | Who             |
| ---------------- | ---------------------------------------------------- | --------------- |
| `squad`          | Triage: analyze issue, assign `squad:{member}` label | Moiraine (Lead) |
| `squad:moiraine` | Pick up issue and complete the work                  | Moiraine        |
| `squad:perrin`   | Pick up issue and complete the work                  | Perrin          |
| `squad:elayne`   | Pick up issue and complete the work                  | Elayne          |
| `squad:egwene`   | Pick up issue and complete the work                  | Egwene          |
| `squad:nynaeve`  | Pick up issue and complete the work                  | Nynaeve         |
| `squad:verin`    | Pick up issue and complete the work                  | Verin           |
| `squad:faile`    | Pick up issue and complete the work                  | Faile           |
| `squad:loial`    | Pick up issue and complete the work                  | Loial           |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, the **Lead** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. The Lead handles all `squad` (base label) triage.
8. **QA triage** — testing work routes to Verin by default. Route to Faile only when the task is explicitly system-level: E2E via Playwright, acceptance criteria from a user perspective, cross-process or cross-network boundary validation, or boundary guarantees such as tenant isolation and PHI-safety. When the scope is ambiguous, prefer Verin first — Faile picks up system-level validation once component behaviour is established. When both agents have valid interests at a seam, the Coordinator confirms the split before either proceeds.

## Work Type → Agent

| Work Type                                                    | Primary  | Secondary |
| ------------------------------------------------------------ | -------- | --------- |
| Architecture, decisions, Aiel/Aviendha integration           | Moiraine | —         |
| Aiel framework, abstractions, core services                  | Perrin   | —         |
| Aviendha SaaS flows, tenants, UX                             | Elayne   | —         |
| Accessibility, inclusive design, ARIA, WCAG                  | Egwene   | Verin     |
| Reliability, diagnostics, operations                         | Nynaeve  | —         |
| Behavioural unit, contract, edge-case, and invariant tests   | Verin    | Faile     |
| E2E, acceptance, cross-component boundary, and smoke tests   | Faile    | Verin     |
| Documentation, onboarding, architecture narratives           | Loial    | Moiraine  |

## Module Ownership

| Module                                          | Primary  | Secondary |
| ----------------------------------------------- | -------- | --------- |
| `Aiel/src`                                      | Perrin   | Moiraine  |
| `Aiel/src/Aiel.Analyzers`                       | Perrin   | Verin     |
| `Aiel/src/Aiel.Generators`                      | Perrin   | Moiraine  |
| `Aiel/tests`                                    | Verin    | Perrin    |
| `Aviendha/src`                                  | Elayne   | Perrin    |
| `Aviendha/aspire`                               | Nynaeve  | Elayne    |
| `Aviendha/tests/Aviendha.E2E.Tests`             | Faile    | Verin     |
| `Aviendha/docs`                                 | Loial    | Moiraine  |
| `Aiel/docs`                                     | Loial    | Moiraine  |
| `docs`                                          | Loial    | Moiraine  |
