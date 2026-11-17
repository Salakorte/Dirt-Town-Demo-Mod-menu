@echo off
call "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" -arch=x64 -host_arch=x64 >nul
if errorlevel 1 exit /b 1
cl /nologo /std:c++20 /EHsc /O2 /MT /LD "%~dp0bridge.cpp" /Fo:"%~dp0..\artifacts\bridge8.obj" /link /OUT:"%~dp0..\artifacts\DirtTownMenu-0.4.0\DirtTownBridge8.dll"
exit /b %errorlevel%
