@echo off
set version=%1
set key=%2
shift

nuget.exe pack Glimpse.SimpleAuthentication.nuspec -Version %version%
nuget.exe pack Nancy.SimpleAuthentication.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.Core.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.ExtraProviders.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.Mvc-3.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.Mvc-4.nuspec -Version %version%

nuget.exe push Glimpse.SimpleAuthentication.%version%.nupkg %key%
nuget.exe push Nancy.SimpleAuthentication.%version%.nupkg %key%
nuget.exe push SimpleAuthentication.Core.%version%.nupkg %key%
nuget.exe push SimpleAuthentication.ExtraProviders.%version%.nupkg %key%
nuget.exe push SimpleAuthentication.Mvc3.%version%.nupkg %key%
nuget.exe push SimpleAuthentication.Mvc4.%version%.nupkg %key%
