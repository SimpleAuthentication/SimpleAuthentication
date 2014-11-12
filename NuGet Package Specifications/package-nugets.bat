@echo off
set version=%1
set key=%2
shift

CLS
@ECHO .
@ECHO .
@ECHO Packing the SimpleAuthentication NuSpec's into nupkg files.
@ECHO .
nuget.exe pack SimpleAuthentication.Core.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.ExtraProviders.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.Mvc.nuspec -Version %version%
nuget.exe pack Nancy.SimpleAuthentication.nuspec -Version %version%
REM nuget.exe pack Glimpse.SimpleAuthentication.nuspec -Version %version%

REM nuget.exe push Glimpse.SimpleAuthentication.%version%.nupkg %key%
REM nuget.exe push Nancy.SimpleAuthentication.%version%.nupkg %key%
REM nuget.exe push SimpleAuthentication.Core.%version%.nupkg %key%
REM nuget.exe push SimpleAuthentication.ExtraProviders.%version%.nupkg %key%
REM nuget.exe push SimpleAuthentication.Mvc3.%version%.nupkg %key%
REM nuget.exe push SimpleAuthentication.Mvc4.%version%.nupkg %key%
