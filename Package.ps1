$ErrorActionPreference='Stop'
$version='0.4.0'
$publish=Join-Path $PSScriptRoot "artifacts\DirtTownMenu-$version"
$stage=Join-Path $PSScriptRoot "artifacts\package-$version\DirtTownMenu-$version"
$zip=Join-Path $PSScriptRoot "artifacts\DirtTownMenu-$version-coop.zip"
foreach($name in @('DirtTownMenu.exe','DirtTownMenu.dll','DirtTownBridge8.dll','coreclr.dll','System.Windows.Forms.dll')) {
    if(!(Test-Path (Join-Path $publish $name))){throw "Falta $name. Ejecuta Build.ps1 -SelfContained."}
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null
foreach($file in Get-ChildItem -LiteralPath $publish -File -Recurse) {
    $relative=[IO.Path]::GetRelativePath($publish,$file.FullName)
    if($relative -match '^backups[\\/]' -or $file.Extension -eq '.pdb'){continue}
    if($file.Name -in @('dirttown.exe','version.dll','steam_api64.dll')){throw "Archivo del juego excluido: $relative"}
    $target=Join-Path $stage $relative
    New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $target -Force
}
foreach($name in @('README.md','COOP_COMPATIBILITY.md')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $stage -Force}
$runtimeVersion=(Get-Content -LiteralPath (Join-Path $publish 'DirtTownMenu.runtimeconfig.json') -Raw | ConvertFrom-Json).runtimeOptions.includedFrameworks[0].version
$runtimePackage=Join-Path $env:USERPROFILE ".nuget\packages\microsoft.netcore.app.runtime.win-x64\$runtimeVersion"
Copy-Item -LiteralPath (Join-Path $runtimePackage 'LICENSE.TXT') -Destination (Join-Path $stage 'DOTNET-LICENSE.TXT') -Force
Copy-Item -LiteralPath (Join-Path $runtimePackage 'THIRD-PARTY-NOTICES.TXT') -Destination (Join-Path $stage 'DOTNET-THIRD-PARTY-NOTICES.TXT') -Force
Copy-Item -LiteralPath (Join-Path $env:USERPROFILE ".nuget\packages\microsoft.windowsdesktop.app.runtime.win-x64\$runtimeVersion\LICENSE") -Destination (Join-Path $stage 'DOTNET-DESKTOP-LICENSE.TXT') -Force
Set-Content -LiteralPath (Join-Path $stage 'Abrir-ModMenu.cmd') -Value "@echo off`r`nstart `"`" `"%~dp0DirtTownMenu.exe`"" -Encoding ascii
Compress-Archive -Path $stage -DestinationPath $zip -Force
Get-FileHash -LiteralPath $zip -Algorithm SHA256
Write-Output "Paquete: $zip"
