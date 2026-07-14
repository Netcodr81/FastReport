$ErrorActionPreference = 'Stop'

Write-Host 'Running modernization guard checks...'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Set-Location $root

$forbiddenTfmPattern = 'net6\.0|net462|net472|net10\.0-windows'
$removedConnectorPattern = 'FastReport\.OpenSource\.Data\.(Couchbase|Cassandra|ClickHouse|Firebird|Ignite)'

$csprojFiles = Get-ChildItem -Path . -Recurse -Filter *.csproj | Select-Object -ExpandProperty FullName
$forbiddenTfmHits = @()

foreach ($file in $csprojFiles) {
	$matches = Select-String -Path $file -Pattern $forbiddenTfmPattern -AllMatches
	if ($matches) { $forbiddenTfmHits += $matches }
}

if ($forbiddenTfmHits.Count -gt 0) {
	Write-Host 'Forbidden target frameworks detected:' -ForegroundColor Red
	$forbiddenTfmHits | ForEach-Object { Write-Host " - $($_.Path):$($_.LineNumber) => $($_.Line.Trim())" }
	throw 'Modernization guard failed: forbidden TFMs found.'
}

$scanFiles = Get-ChildItem -Path . -Recurse -File |
	Where-Object {
		$_.FullName -notmatch '\\.git\\' -and
		$_.FullName -notmatch '\\.vs\\' -and
		$_.FullName -notmatch '\\.github\\upgrades\\' -and
		$_.FullName -notmatch '\\bin\\' -and
		$_.FullName -notmatch '\\obj\\'
	} |
	Select-Object -ExpandProperty FullName

$removedConnectorHits = @()
foreach ($file in $scanFiles) {
	$matches = Select-String -Path $file -Pattern $removedConnectorPattern -AllMatches
	if ($matches) { $removedConnectorHits += $matches }
}

if ($removedConnectorHits.Count -gt 0) {
	Write-Host 'Removed connector references detected in active files:' -ForegroundColor Red
	$removedConnectorHits | ForEach-Object { Write-Host " - $($_.Path):$($_.LineNumber) => $($_.Line.Trim())" }
	throw 'Modernization guard failed: removed connector references found.'
}

Write-Host 'Modernization guard checks passed.' -ForegroundColor Green
