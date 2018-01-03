@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

REM https://github.com/3F/hMSBuild
set msbuild="hMSBuild.bat"
set project=..\GitExtensions.VS2015.sln
set EnableNuGetPackageRestore=true
..\.nuget\nuget.exe restore %project%
set msbuildparams=/p:Configuration=%Configuration% /t:restore /t:Rebuild /nologo /v:m

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

call DownloadExternals.cmd %Configuration% 
call BuildGitExtNative.cmd %Configuration% Rebuild

call MakeInstallers.cmd
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause