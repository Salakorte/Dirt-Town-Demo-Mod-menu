@echo off
call "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" -arch=x64 -host_arch=x64 >nul
if errorlevel 1 exit /b 1
ml64 /nologo /c /Fo"%~dp0..\artifacts\winmm_proxy.obj" "%~dp0winmm_proxy.asm"
if errorlevel 1 exit /b 1
ml64 /nologo /c /Fo"%~dp0..\artifacts\terrain_camera.obj" "%~dp0terrain_camera.asm"
if errorlevel 1 exit /b 1
cl /nologo /std:c++20 /EHsc /O2 /MT /LD "%~dp0bridge.cpp" "%~dp0..\artifacts\winmm_proxy.obj" "%~dp0..\artifacts\terrain_camera.obj" /Fo:"%~dp0..\artifacts\bridge10.obj" /link user32.lib bcrypt.lib /DEF:"%~dp0winmm_proxy.def" /IMPLIB:"%~dp0..\artifacts\winmm.lib" /OUT:"%~dp0..\artifacts\DirtTownMenu-0.6.3\winmm.dll"
exit /b %errorlevel%
