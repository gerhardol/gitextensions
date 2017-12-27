@echo off

cd /d "%~p0"

REM Visual Studio Version is set in Developer Command Prompt, set manually if run externally
IF "%VisualStudioVersion%"=="" SET VisualStudioVersion=14.0

set msbuild="%programfiles(x86)%\MSBuild\%VisualStudioVersion%\Bin\MSBuild.exe"
set project=..\GitExtensions.VS2015.sln
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.VS2015.sln
set projectSshAskPass=..\GitExtSshAskPass\GitExtSshAskPass.VS2015.sln
set SkipShellExtRegistration=1
set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectShellEx% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectShellEx% /p:Platform=x64 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectSshAskPass% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
