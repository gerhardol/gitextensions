@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

REM https://github.com/3F/hMSBuild
set msbuild="hMSBuild.bat"
set project=..\GitExtensions.VS2015.sln
set EnableNuGetPackageRestore=true
..\.nuget\nuget.exe restore %project%
set msbuildparams=/p:Configuration=%Configuration% /t:Clean /nologo /v:m

REM Clean the solution, it is built with the installer
%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

call BuildGitExtNative.cmd %Configuration% Clean
IF ERRORLEVEL 1 EXIT /B 1

rem keep cached packages
rem call DownloadExternals.cmd

call MakeInstallers.cmd %Configuration% Rebuild
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
