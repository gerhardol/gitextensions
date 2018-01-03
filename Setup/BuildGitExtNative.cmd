@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

SET BuildType=%2
IF "%BuildType%"=="" SET BuildType=Rebuild

REM https://github.com/3F/hMSBuild
set msbuild="hMSBuild.bat"
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.VS2015.sln
set projectSshAskPass=..\GitExtSshAskPass\GitExtSshAskPass.VS2015.sln
set SkipShellExtRegistration=1
set msbuildparams=/p:Configuration=%Configuration% /t:%BuildType% /nologo /v:m

%msbuild% %projectShellEx% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectShellEx% /p:Platform=x64 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectSshAskPass% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
