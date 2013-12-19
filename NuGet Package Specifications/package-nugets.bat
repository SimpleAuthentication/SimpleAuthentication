@echo off
set version=%1
shift

nuget.exe pack Glimpse.SimpleAuthentication.nuspec -Version %version%
nuget.exe pack Nancy.SimpleAuthentication.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.Core.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.ExtraProviders.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.Mvc-3.nuspec -Version %version%
nuget.exe pack SimpleAuthentication.Mvc-4.nuspec -Version %version%
