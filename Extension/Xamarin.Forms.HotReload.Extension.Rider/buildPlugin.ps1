Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. ".\settings.ps1"

Invoke-Exe $MSBuildPath "/t:Restore;Rebuild" "$SolutionPath" "/v:minimal"