# Squad Team

> TwoRivers — Aiel framework repo
>
> **Note:** This squad home base lives in `TwoRiversIT/Aviendha`. This file exists only to support GitHub workflow automation in this repo. Operational data (decisions, history, sessions) is tracked in the Aviendha repo.

## Coordinator

| Name   | Role        | Notes                                              |
| ------ | ----------- | -------------------------------------------------- |
| Squad  | Coordinator | Routes work, enforces handoffs and reviewer gates. |

## Members

| Name     | Pronouns   | Role                           | Charter                             | Status      |
| -------- | ---------- | ------------------------------ | ----------------------------------- | ----------- |
| Moiraine | she / her  | Lead                           | `.squad/agents/moiraine/charter.md` | ✅ Active   |
| Perrin   | he / him   | Platform Dev                   | `.squad/agents/perrin/charter.md`   | ✅ Active   |
| Elayne   | she / her  | Application Dev                | `.squad/agents/elayne/charter.md`   | ✅ Active   |
| Egwene   | she / her  | Accessibility Lead             | `.squad/agents/egwene/charter.md`   | ✅ Active   |
| Nynaeve  | she / her  | Reliability Engineer           | `.squad/agents/nynaeve/charter.md`  | ✅ Active   |
| Verin    | she / her  | QA Strategist                  | `.squad/agents/verin/charter.md`    | ✅ Active   |
| Faile    | she / her  | Systems Validation Specialist  | `.squad/agents/faile/charter.md`    | ✅ Active   |
| Loial    | he / him   | Tech Writer                    | `.squad/agents/loial/charter.md`    | ✅ Active   |
| Scribe   | xe / xey   | Session Logger                 | `.squad/agents/scribe/charter.md`   | 📋 Silent   |
| Ralph    | he / him   | Work Monitor                   | `.squad/agents/ralph/charter.md`    | 🔄 Monitor  |

## Project Context

- **Project:** TwoRivers
- **Framework:** `Aiel` — greenfield application framework
- **Application:** `Aviendha` — greenfield SaaS application built on Aiel
- **Operating model:** Perrin drives framework work; Elayne drives SaaS application work; Moiraine owns system architecture, Aiel/Aviendha integration boundaries, and cross-cutting decisions; Nynaeve owns reliability, diagnostics, and operational readiness; Egwene owns accessibility conformance and inclusive design across both layers; Verin owns test strategy and component-level validation (behavioural unit tests, contract tests, edge cases, regression, and invariant tests across Aiel, Aviendha, and the seams between them); Faile owns system-level validation (E2E via Playwright, system acceptance criteria, cross-process boundary contracts, tenant isolation, and PHI-safety guarantees at the system boundary); Loial owns documentation quality and developer-facing narratives
- **QA split:** Verin tests components from the inside — through public APIs, in-process, no browser. Faile tests the system from the outside — through the user-facing boundary, with a real browser and full stack. Work flows to Verin first; Faile takes over when the scenario requires a running deployment, a browser, or cross-tenant or PHI-safety boundary validation.
- **Squad home base:** `TwoRiversIT/Aviendha` — decisions, history, and operational data live there.
- **Created:** 2026-04-14
