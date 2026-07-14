# Upgrade Options — FastReport.OpenSource

Assessment: 26 projects, mixed net462/net472/net6.0 targets, 138 affected files, and significant Windows-specific API usage (System.Drawing).

## Strategy

### Upgrade Strategy
.NET Framework projects are present across a multi-project dependency graph, so tier-based migration is required.

| Value | Description |
|-------|-------------|
| **Bottom-Up** (selected) | Upgrade dependency leaves first, then move upward tier by tier with validation at each tier boundary. |

## Project Structure

### Project Approach
The solution contains many .NET Framework class libraries that must continue serving dependents during transition.

| Value | Description |
|-------|-------------|
| **Class Libraries: Multi-targeting** (selected) | Keep existing framework target and add a modern target during migration to support both old and new consumers. |
| Class Libraries: In-place | Replace framework target directly; only safe when all consumers migrate together. |

### Package Management
Framework-to-modern migration with mixed project formats is expected to have temporary package divergence.

| Value | Description |
|-------|-------------|
| **Per-Project (defer CPM to post-migration)** (selected) | Keep package versions in each project during migration; add centralized package management after convergence. |
| Central Package Management (CPM) | Enforce shared package versions now via Directory.Packages.props; adds friction during divergence. |

## Compatibility

### Unsupported API Handling
Assessment reports API incompatibilities and behavioral changes that should be resolved within each migration task.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve both simple and complex API changes in the active task so no stub debt remains. |
| Defer Complex Changes | Use temporary stubs for complex replacements and create follow-up resolution subtasks. |

### Windows Native APIs
The assessment detected extensive GDI+/System.Drawing usage, indicating broad Windows API reliance.

| Value | Description |
|-------|-------------|
| **Windows Compatibility Pack** (selected) | Add Microsoft.Windows.Compatibility to keep Windows APIs functional while migration progresses. |
| No Compatibility Pack | Force immediate cross-platform replacements by surfacing Windows API failures early. |
