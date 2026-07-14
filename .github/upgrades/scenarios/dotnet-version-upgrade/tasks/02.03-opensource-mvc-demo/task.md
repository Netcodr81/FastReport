# 02.03-opensource-mvc-demo: Upgrade MVC demo project TFM and validate behavioral compatibility

# 02.03-opensource-mvc-demo

## Objective
Upgrade `FastReport.OpenSource.MVC.6.0` to .NET 10 and address any compatibility updates required by the assessment's behavioral-change signal.

## Scope
- Project: `Demos/OpenSource/MVC/FastReport.OpenSource.MVC.6.0/FastReport.OpenSource.MVC.6.0.csproj`
- Assessment signals: `Project.0002` (mandatory), `Api.0003` (potential behavioral change)

## Steps
1. Update the project target framework to net10.0.
2. Build and identify any behavioral/API changes requiring code or configuration updates.
3. Apply required compatibility adjustments in project scope.
4. Rebuild and validate project success.

## Done when
- Project targets net10.0.
- Project builds successfully with required compatibility updates applied.
- Any behavioral-change handling needed for compilation/runtime baseline is documented.
