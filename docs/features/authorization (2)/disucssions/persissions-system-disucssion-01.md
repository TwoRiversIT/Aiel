# Permissions System Discussion 01

> I just realized that I completely forgot to include Roles in the permission system. Looking at the current design and existing implementation, please draft a plan to add support for Roles to transfrom this from a Permissions System to a full RBAC system.
> Remember, greenfield posture. Breaking changes are allowed

I’m grounding this in the existing permissions design and code so the RBAC plan lands on the actual abstractions already in place. First I’m locating the current permission contracts and any role-adjacent implementation surfaces, then I’ll draft the plan directly into the design context.

I’m checking whether roles already exist as a partial concept before I change the design, because the plan should extend the real seam instead of introducing a parallel model. After that I’ll anchor the RBAC additions in the current permission contracts and milestones.

Searched for regex `\bRole\b|\brole\b|PermissionSubjectType|PermissionGrantRequest|IActionCapabilityService|IPermissionManager` (`**/Aiel/**`), 111 results

Searched for regex `role|RBAC|subject type|PermissionSubject|grant evaluator` (`**/Aiel/docs/features/permissions/**`), 63 results

Read memory

The codebase already has generic subjects, which is promising; the critical check now is whether grant evaluation can expand a user into effective subjects like roles, because that determines whether RBAC is an additive extension or a deeper redesign. Then I’ll write the plan against that seam.

Searched for regex `class .*PermissionGrantEvaluator|sealed class .*GrantEvaluator|IPermissionGrantEvaluator` (`**/Aiel/src/**`), 6 results

Read [](file:///d%3A/source/Aviendha/Aiel/src/Aiel.Permissions.Application.Contracts/Aiel/Permissions/IPermissionGrantEvaluator.cs)

Read [](file:///d%3A/source/Aviendha/Aiel/src/Aiel.Permissions.Application/Aiel/Permissions/DefaultPermissionManager.cs)

Read [](file:///d%3A/source/Aviendha/Aiel/src/Aiel.Permissions.Domain/Aiel/Permissions/PermissionGrant.cs)

The evaluator surface is currently direct subject lookup only, which means roles are not just missing terminology; they’re missing the effective-subject expansion step that RBAC needs. I’m reading that implementation seam and the current design doc next so I can add the plan in the right place rather than bolting on a separate role subsystem.

Searched for regex `EvaluateAsync\(|GetGrantDecision|PermissionGrantDecision\?` (`**/Aiel/src/Aiel.Permissions.Application/**`), no results

Read [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md#1-1), lines 1 to 180

Read [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md#700-700), lines 700 to 980

Read [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md#980-980), lines 980 to 1247

I’ve got the controlling seam now: the store and grants already accept generic subject types, but evaluation and capability surfaces still assume a single subject, so roles need to be introduced as effective-subject resolution and membership management, not as a second unrelated permission path.

Plan:

1. Add an RBAC extension section to the permissions plan that builds on existing `PermissionSubject*` abstractions.
2. Update milestones/open questions to account for role definitions, assignments, and effective-subject evaluation.
3. Sanity-check the markdown for consistency against the current contracts and implementation surface.

The controlling seam is clear now: grants are already generic over subject type, but evaluation is still single-subject and does not expand a user into role memberships. With your greenfield posture, I’m going to update the plan as a breaking-change RBAC design rather than preserving the current evaluator shape.

Created 4 todos

I’m editing the plan now in one pass: elevate roles to a first-class design concern, add the missing runtime seams for effective-subject expansion and role assignment, and update the milestones so this reads as an RBAC plan instead of a permission-only plan.

Generating patch (511 lines) in [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md)

```

```
The doc edit is in place; the only validation failures are markdown table-style diagnostics in this file. I’m normalizing the separator rows and then I’ll rerun file diagnostics so the plan lands cleanly.

Searched for text `|---|---|` (`**/Aiel/docs/features/permissions/permissions-system-plan.md`), 9 results

I found nine separator rows in this file using the older compact table style. I’m fixing just those rows so the markdown diagnostics reflect the RBAC content change instead of formatting noise.

Generating patch (30 lines) in [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md)


```

```


Checked [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md), 1 problem found

One file-local markdown diagnostic remains, also in this document. I’m converting that emphasized status line into normal text and then rerunning diagnostics on the same file.

Generating patch (6 lines) in [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md)


```

```


Checked [](file:///d%3A/source/Aviendha/Aiel/docs/features/permissions/permissions-system-plan.md), no problems found

The plan content is validated cleanly now. I’m pulling the exact section anchors so I can point you to the RBAC additions precisely instead of giving you a vague changelog.

Searched for regex `^## RBAC Upgrade Decision|^## Role Model|^## Effective Subject Resolution|^## Default Roles and Application Composition|^## Updated Implementation Milestones` (`**/Aiel/docs/features/permissions/permissions-system-plan.md`), 5 results

Searched for regex `^# Permission and Role System - Action-Centered RBAC Implementation Plan|^## Goals|^## Target Layer and Package Ownership|^## Analyzer Requirements|^## Working Verdict` (`**/Aiel/docs/features/permissions/permissions-system-plan.md`), 5 results

Completed: *Draft RBAC design additions* (2/4)

I updated the plan in permissions-system-plan.md to turn it into an action-centered RBAC design instead of a permission-only design.

The main design change is a deliberate breaking shift away from single-subject evaluation. The plan now treats roles as first-class framework concepts, adds role definitions and role assignments, and introduces effective-subject resolution as the evaluator seam that expands an actor into direct and role-derived authority before grant evaluation. The core additions are in permissions-system-plan.md, permissions-system-plan.md, and permissions-system-plan.md.

I also extended the plan to cover default role composition, role lifecycle and manifest migration, EF persistence for role definitions and assignments, RBAC-specific analyzers, and a rewritten milestone sequence that puts roles into the implementation path instead of bolting them on later. Those updates are anchored in permissions-system-plan.md, permissions-system-plan.md, and permissions-system-plan.md.

Validation: markdown diagnostics on the edited file are clean.

1. If you want, I can turn this RBAC plan into concrete phase/task briefs matching the existing permissions phase docs.
2. I can also propagate the RBAC changes into SAD.md so the architecture record matches the updated design.
3. If you want to start implementation immediately, I’d begin with the breaking contract slice: `IRoleManager`, `IRoleAssignmentStore`, and the new effective-subject evaluator surface.

Made changes.

> Please explain "subjects" to me with respect to RBAC.

In RBAC, a subject is the thing that can hold authority.

Put more concretely:

- A permission answers what action is allowed.
- A role answers which bundle of permissions is assigned.
- A subject answers who or what receives that assignment.

Typical subjects are:

- a user
- a service account
- a client application
- sometimes a system identity or machine identity

In the Aiel design, “subject” is intentionally generic. It is modeled as a typed identity, not hardcoded to “user”, which is why the plan talks about `PermissionSubjectTypeName` plus `PermissionSubjectKey` in permissions-system-plan.md. That lets the framework say “this grant applies to subject type `User` with key `123`” without baking Aviendha’s identity model into the permissions package.

The important RBAC distinction is this:

- The actor is the caller currently trying to do something.
- The subject is the authorization identity you evaluate.
- In simple systems, actor and subject are effectively the same.
- In real RBAC, one actor often expands into multiple effective subjects.

Example:

- Alice signs in.
- Alice is the actor.
- Alice may evaluate as these effective subjects:
- `User:alice`
- `Role:ClinicAdministrator`
- `Role:Scheduler`

That matters because authorization can succeed in different ways:

- Alice might have a direct permission grant as `User:alice`.
- Or Alice might have no direct grant, but her assigned role `Scheduler` grants the permission.
- Or a deny/prohibit on one subject may override a grant on another, depending on precedence rules.

So when I added RBAC to the plan, the key change was not just “add roles.” It was “replace single-subject evaluation with effective-subject resolution.” Without that, you have a generic permission system with typed assignees, but not a complete RBAC system.

A useful mental model is:

- Subject = assignee
- Role = reusable authority bundle
- Permission = atomic action right

And the evaluation flow becomes:

1. Identify the actor.
2. Resolve that actor into effective subjects for the current scope.
3. Find direct and role-derived grants for those subjects.
4. Apply precedence.
5. Decide whether the action is allowed.

If you want, I can also explain the difference between subjects, roles, groups, and claims, because those get conflated a lot in authorization design.

