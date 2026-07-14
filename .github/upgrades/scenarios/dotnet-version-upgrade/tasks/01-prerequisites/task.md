# 01-prerequisites: Validate toolchain and migration constraints

Confirm the migration environment before touching project files: verify .NET 10 SDK availability, confirm global.json compatibility, and lock execution constraints from scenario-instructions.md (guided flow, bottom-up ordering, and no implementation changes without explicit approval). This task establishes the baseline guardrails for all subsequent tasks.

## Research Findings

### Scope Inventory
- Projects affected: none (prerequisites-only task)
- Distinct concerns: SDK availability validation, global.json compatibility validation, execution-constraint confirmation
- Change signals: no code/package migration changes in this task

### Validations Performed
- `validate_dotnet_sdk_installation(targetFramework=net10.0)` returned **Compatible SDK found**.
- `validate_dotnet_sdk_in_globaljson(targetFramework=net10.0)` returned **Success: no global.json config found, nothing to validate or fix**.

### Baseline Constraints Confirmed
- Flow mode is Guided.
- Upgrade strategy is Bottom-Up.
- Package management is per-project during migration.
- User requested no implementation changes without explicit approval.

**Done when**: .NET 10 SDK readiness is verified, baseline constraints are documented in task context, and prerequisite blockers are either resolved or explicitly recorded.
