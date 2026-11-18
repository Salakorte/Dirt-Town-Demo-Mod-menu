$ErrorActionPreference='Stop'
$version='0.6.3'
$publish=Join-Path $PSScriptRoot "artifacts\DirtTownMenu-$version"
$stage=Join-Path $PSScriptRoot "artifacts\package-$version\DirtTownMenu-$version"
$zip=Join-Path $PSScriptRoot "artifacts\DirtTownMenu-$version-coop.zip"
foreach($name in @('DirtTownMenu.exe','DirtTownMenu.dll','winmm.dll','coreclr.dll','System.Windows.Forms.dll')) {
    if(!(Test-Path (Join-Path $publish $name))){throw "Falta $name. Ejecuta Build.ps1 -SelfContained."}
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null
foreach($file in Get-ChildItem -LiteralPath $publish -File -Recurse) {
    $relative=[IO.Path]::GetRelativePath($publish,$file.FullName)
    if($relative -match '^backups[\\/]' -or $file.Extension -eq '.pdb'){continue}
    if($file.Name -match '^DirtTownBridge[0-9]+\.dll$'){continue}
    if($file.Name -in @('dirttown.exe','version.dll','steam_api64.dll')){throw "Archivo del juego excluido: $relative"}
    $target=Join-Path $stage $relative
    New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $target -Force
}
foreach($name in @('README.md','README.es.md','COOP_COMPATIBILITY.md','TERRAIN_BRUSH_RESEARCH.md')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $stage -Force}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'native\vendor\imgui\LICENSE.txt') -Destination (Join-Path $stage 'IMGUI-LICENSE.txt') -Force
$runtimeVersion=(Get-Content -LiteralPath (Join-Path $publish 'DirtTownMenu.runtimeconfig.json') -Raw | ConvertFrom-Json).runtimeOptions.includedFrameworks[0].version
$runtimePackage=Join-Path $env:USERPROFILE ".nuget\packages\microsoft.netcore.app.runtime.win-x64\$runtimeVersion"
Copy-Item -LiteralPath (Join-Path $runtimePackage 'LICENSE.TXT') -Destination (Join-Path $stage 'DOTNET-LICENSE.TXT') -Force
Copy-Item -LiteralPath (Join-Path $runtimePackage 'THIRD-PARTY-NOTICES.TXT') -Destination (Join-Path $stage 'DOTNET-THIRD-PARTY-NOTICES.TXT') -Force
Copy-Item -LiteralPath (Join-Path $env:USERPROFILE ".nuget\packages\microsoft.windowsdesktop.app.runtime.win-x64\$runtimeVersion\LICENSE") -Destination (Join-Path $stage 'DOTNET-DESKTOP-LICENSE.TXT') -Force
Set-Content -LiteralPath (Join-Path $stage 'Abrir-ModMenu.cmd') -Value "@echo off`r`nstart `"`" `"steam://rungameid/5471710`"" -Encoding ascii
Set-Content -LiteralPath (Join-Path $stage 'Abrir-Coordinador-Coop.cmd') -Value "@echo off`r`nstart `"`" `"%~dp0DirtTownMenu.exe`"" -Encoding ascii
Compress-Archive -Path $stage -DestinationPath $zip -Force
Get-FileHash -LiteralPath $zip -Algorithm SHA256
Write-Output "Paquete: $zip"
$integrated=Join-Path $PSScriptRoot "artifacts\integrated-$version\DirtTownMenu-$version"
New-Item -ItemType Directory -Path $integrated -Force | Out-Null
foreach($name in @('winmm.dll','README.md','README.es.md','COOP_COMPATIBILITY.md','TERRAIN_BRUSH_RESEARCH.md','IMGUI-LICENSE.txt','Abrir-ModMenu.cmd')){Copy-Item -LiteralPath (Join-Path $stage $name) -Destination $integrated -Force}
$integratedZip=Join-Path $PSScriptRoot "artifacts\DirtTownMenu-$version-integrado.zip"
Compress-Archive -Path $integrated -DestinationPath $integratedZip -Force
Get-FileHash -LiteralPath $integratedZip -Algorithm SHA256
Write-Output "Menu sin coordinador: $integratedZip"
