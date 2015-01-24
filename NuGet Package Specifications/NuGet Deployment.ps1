

############################################################################
###                                                                      ###
###                    NUGET  PACKAGE and PUBLISH                        ###
###                                                                      ###
############################################################################



param (
  [string]$version = "",
  [string]$apiKey = "",
  [string]$source = $PSScriptRoot,
  [string]$destination = $PSScriptRoot,
  [string]$pushSource = "https://nuget.org",
  [string]$nuget = ""
)


function DisplayCommandLineArgs()
{
    "Options provided:"
    "    => version: $version"
    "    => source: $source"
    "    => destination: $destination"
    "    => nuget: $nuget"
    "    => api key: $apiKey"

    ""
    "eg. NuGetPackageAndPublish.ps1 -version 0.1-alpha"
    "eg. NuGetPackageAndPublish.ps1 -version 0.1-alpha -destination C:\temp\TempNuGetPackages"
    "eg. NuGetPackageAndPublish.ps1 -version 0.1-alpha -source ../nugetspecs/ -destination C:\temp\TempNuGetPackages"
    "eg. NuGetPackageAndPublish.ps1 -version 0.1-alpha -nuget c:\temp\nuget.exe"
    "eg. NuGetPackageAndPublish.ps1 -version 0.1-alpha -nuget c:\temp\nuget.exe -apiKey ABCD-EFG..."
    ""

    if (-Not $version)
    {
        ""
        "**** The version of this NuGet package is required."
        "**** Eg. ./NuGetPackageAndPublish.ps1 -version 0.1-alpha"
        ""
        ""
        throw;
    }

    if ($source -eq "")
    {
        ""
        "**** A source parameter provided cannot be an empty string."
        ""
        ""
        throw;
    }

    if ($destination -eq "")
    {
        ""
        "**** A destination parameter provided cannot be an empty string."
        ""
        ""
        throw;
    }

    if ($pushSource -eq "")
    {
        ""
        "**** The NuGet push source parameter provided cannot be an empty string."
        ""
        ""
        throw;
    }

    # Setup the nuget path.
    if (-Not $nuget -eq "")
    {
        $global:nugetExe = $nuget
    }
    else
    {
        # Assumption, nuget.exe is the current folder where this file is.
        $global:nugetExe = Join-Path $source "nuget" 
    }

    $global:nugetExe

    if (!(Test-Path $global:nugetExe -PathType leaf))
    {
        ""
        "**** Nuget file was not found. Please provide the -nuget parameter with the nuget.exe path -or- copy the nuget.exe to the current folder, side-by-side to this powershell file."
        ""
        ""
        throw;
    }
}


function CleanUp()
{
    $nupkgFiles = @(Get-ChildItem $destination -Filter *.nupkg)

    if ($nupkgFiles.Length -gt 0)
    {
        "Found " + $nupkgFiles.Length + " *.nupkg files. Lets delete these first..."

        foreach($nupkgFile in $nupkgFiles)
        {
            $combined = Join-Path $destination $nupkgFile
            "... Removing $combined."
            Remove-Item $combined
        }
        
        "... Done!"
    }
}


function PackageTheSpecifications()
{
    ""
    "Getting all *.nuspec files to package in directory: $source"

    $files = Get-ChildItem $source -Filter *.nuspec

    if ($files.Count -eq 0)
    {
        ""
        "**** No nuspec files found in the directory: $source"
        "Terminating process."
        throw;
    }

    "Found: " + $files.Length + " files :)"

    foreach($file in $files)
    {
        &$nugetExe pack $file -Version $version -OutputDirectory $destination

        ""
    }
}


function PushThePackagesToNuGet()
{
    if ($apiKey -eq "")
    {
        "@@ No NuGet server api key provided - so not pushing anything up."
        return;
    }


    ""
    "Getting all *.nupkg's files to push to : $pushSource"

    $files = Get-ChildItem $destination -Filter *.nupkg

    if ($files.Count -eq 0)
    {
        ""
        "**** No nupkg files found in the directory: $destination"
        "Terminating process."
        throw;
    }

    "Found: " + $files.Length + " files :)"

    foreach($file in $files)
    {
        &$nugetExe push ($file.FullName) -Source $pushSource -apiKey $apiKey

        ""
    }
}

##############################################################################
##############################################################################

$ErrorActionPreference = "Stop"
$global:nugetExe = ""

cls

""
" ---------------------- start script ----------------------"
""
""
"  Starting NuGet packing/publishing script -  (╯°□°）╯︵ ┻━┻"
""
"  This script will look for -all- *.nuspec files in a source directory,"
"  then paackage them up to *.nupack files. Finally, it can publish"
"  them to a NuGet server, if an api key was provided."
""

DisplayCommandLineArgs

CleanUp

PackageTheSpecifications

PushThePackagesToNuGet

""
""
" ---------------------- end of script ----------------------"
""
""
