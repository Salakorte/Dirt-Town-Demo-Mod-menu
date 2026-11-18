param([switch]$SelfContained,[switch]$SkipNative)
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'src\DirtTownMenu\DirtTownMenu.csproj'
$output = Join-Path $PSScriptRoot 'artifacts\DirtTownMenu-0.6.3'
dotnet publish $project -c Release -r win-x64 --self-contained $SelfContained.IsPresent.ToString().ToLowerInvariant() -o $output
if ($LASTEXITCODE -ne 0) { throw 'No se ha podido compilar el menú.' }
if (!$SkipNative) {
    & (Join-Path $PSScriptRoot 'native\build.cmd')
    if ($LASTEXITCODE -ne 0) { throw 'No se ha podido compilar el puente nativo.' }
} elseif (!(Test-Path (Join-Path $output 'winmm.dll'))) { throw 'Falta el puente previamente compilado.' }
Write-Output "Menú generado: $output\DirtTownMenu.exe"
