@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

REM Visual Studio Version is set in Developer Command Prompt, set manually if run externally
IF "%VisualStudioVersion%"=="" SET VisualStudioVersion=14.0

set msbuild="%programfiles(x86)%\MSBuild\%VisualStudioVersion%\Bin\MSBuild.exe"
set project=..\GitExtensions.VS2015.sln
set EnableNuGetPackageRestore=true
..\.nuget\nuget.exe restore %project%
set msbuildparams=/p:Configuration=%Configuration% /t:Rebuild /nologo /v:m

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

call DownloadExternals.cmd
call BuildGitExtNative.cmd %Configuration% Rebuild

call MakeInstallers.cmd %Configuration%
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
