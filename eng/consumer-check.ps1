#Requires -Version 5.1
<#
.SYNOPSIS
    Builds a strict stand-alone consumer app against the packages in artifacts/packages.

.DESCRIPTION
    Run after 'dotnet pack eng/PayGate.Packages.slnf -o artifacts/packages'. Restores into an isolated
    package folder (so a re-packed package with the same version is never served from cache), then
    builds with warnings as errors. Fails if any PayGate package would break a strict consumer.
#>
[CmdletBinding()]
param(
    [string] $PackagesDirectory
)

$ErrorActionPreference = 'Stop'
# Resolved here rather than as a parameter default: Windows PowerShell 5.1 has no $PSScriptRoot there.
if (-not $PackagesDirectory) { $PackagesDirectory = [IO.Path]::Combine($PSScriptRoot, '..', 'artifacts', 'packages') }
$project = [IO.Path]::Combine($PSScriptRoot, 'consumer-check', 'ConsumerCheck.csproj')
$isolatedPackages = [IO.Path]::Combine($PSScriptRoot, 'consumer-check', 'obj', 'packages')

$marker = Get-ChildItem -Path $PackagesDirectory -Filter 'PayGate.Abstractions.*.nupkg' -ErrorAction SilentlyContinue |
    Select-Object -First 1
if (-not $marker) {
    throw "No PayGate packages found in '$PackagesDirectory'. Run 'dotnet pack eng/PayGate.Packages.slnf -c Release -o artifacts/packages' first."
}
$version = $marker.Name -replace '^PayGate\.Abstractions\.', '' -replace '\.nupkg$', ''
Write-Host "Consumer check against PayGate $version"

if (Test-Path $isolatedPackages) { Remove-Item $isolatedPackages -Recurse -Force }

dotnet restore $project -p:PayGateVersion=$version --packages $isolatedPackages
if ($LASTEXITCODE -ne 0) { throw 'Consumer restore failed.' }

dotnet build $project -c Release --no-restore -p:PayGateVersion=$version
if ($LASTEXITCODE -ne 0) { throw 'Consumer build failed.' }

Write-Host 'Consumer check passed.'
