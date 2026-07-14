# .NET Version Upgrade

## Preferences
- **Flow Mode**: Guided
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: migrate_to_netcore
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: Bottom-Up

### Project Structure
- Project Approach: Class Libraries: Multi-targeting
- Package Management: Per-Project (defer CPM to post-migration)

### Compatibility
- Unsupported API Handling: Fix Inline
- Windows Native APIs: Windows Compatibility Pack

## Strategy
**Selected**: Bottom-Up (Dependency-First)
**Rationale**: 26 projects with a 4-level dependency graph and mixed .NET Framework/.NET 6 targets require strict dependency-first migration ordering.

### Execution Constraints
- Complete dependency levels in order; do not begin a higher level until the current level validates.
- Validate each level with restore/build/test before advancing to the next level.
- Keep project package management per-project during migration; defer CPM until post-migration cleanup.
- Resolve API compatibility issues inside each migration task (no deferred stub backlog).
- Use Windows Compatibility Pack during migration to stabilize widespread System.Drawing usage before cross-platform replacement work.

## User Preferences
### Execution Style
- Do not implement code/project changes until explicit user approval.
- Avoid preview package versions; prefer stable releases only for package upgrades.

### Documentation Preferences
- Create and use a dedicated migration docs folder for migration-related documents.
- If a context reset/window change is needed, maintain a Markdown handoff file to resume work cleanly.

## Key Decisions Log
- 2026-07-14: User approved starting the .NET 10 migration workflow in guided mode with a documentation-first approach.
- 2026-07-14: User approved the assessment and requested progression to planning.
- 2026-07-14: User confirmed upgrade-options.md selections for strategy and migration constraints.
- 2026-07-14: User approved execution stage start after reviewing plan and tasks.
- 2026-07-14: During 02.02, user chose to install Node.js and retry build instead of skipping debug validation or marking blocked.
- 2026-07-14: User resumed after restart; Node.js was detected and 02.02 debug build succeeded.
