@echo off
set version=%1
set push=%2
set key=%3

SET DestinationDirectory="Release %version%"

cls

echo .
echo .
ECHO Command: package-nugets 'push-up-to-nuget.org' 'key'
ECHO      eg: package-nugets 0.3.1 yes
ECHO .
ECHO    NOTE: the nuget Key is OPTIONAL.
ECHO          push to nuget is OPTIONAL. (and can be -any- value to represent pushing the package up. Leave blank for 'no').
ECHO .
ECHO .
ECHO .



rem shift

IF [%version%]==[] (
    ECHO  No version parameter specified. PLEASE SPECIFY ONE. Thanks.
)ELSE (
    IF EXIST %DestinationDirectory% (
        ECHO Directory %DestinationDirectory% exists - so deleting it and all contents...
        RD /s /q %DestinationDirectory%
        MKDIR %DestinationDirectory%
    ) ELSE (
        Directory $DestinationDirectory% doesn't exist, so we'll create it.
        MKDIR %DestinationDirectory%
    )

    nuget.exe pack SimpleAuthentication.Core.nuspec -Version %version%
    nuget.exe pack SimpleAuthentication.ExtraProviders.nuspec -Version %version%
    nuget.exe pack Nancy.SimpleAuthentication.nuspec -Version %version%    
    nuget.exe pack SimpleAuthentication.Mvc-3.nuspec -Version %version%
    nuget.exe pack SimpleAuthentication.Mvc-4.nuspec -Version %version%
    nuget.exe pack Glimpse.SimpleAuthentication.nuspec -Version %version%
    
    MOVE *.nupkg %DestinationDirectory%/
)

IF NOT [%push%]==[] (
    ECHO .
    ECHO .
    ECHO PUSHING UP PACKAGES ......
    ECHO -----
    
    nuget.exe push %DestinationDirectory%/SimpleAuthentication.Core.%version%.nupkg %key%
    nuget.exe push %DestinationDirectory%/SimpleAuthentication.ExtraProviders.%version%.nupkg %key%
    nuget.exe push %DestinationDirectory%/Nancy.SimpleAuthentication.%version%.nupkg %key%
    nuget.exe push %DestinationDirectory%/SimpleAuthentication.Mvc3.%version%.nupkg %key%
    nuget.exe push %DestinationDirectory%/SimpleAuthentication.Mvc4.%version%.nupkg %key%
    nuget.exe push %DestinationDirectory%/Glimpse.SimpleAuthentication.%version%.nupkg %key%

) ELSE (
    ECHO .
    ECHO .
    ECHO **NOT** pushing the packages up to NuGet.org.
)

ECHO .
ECHO  -- END OF bat command.