$ErrorActionPreference = 'Stop';

$packageName = "Syringe"
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$serviceZip = "$toolsDir\Syringe.Service.$version.0.zip"
$websiteZip = "$toolsDir\Syringe.Web.$version.0.zip"

$serviceDir = "$toolsDir\Syringe.Service"
$websiteDir = "$toolsDir\Syringe.Web"

$serviceExe = "$toolsDir\Syringe.Service\Syringe.Service.exe"
$websiteSetupScript = "$toolsDir\Syringe.Web\bin\iis.ps1"

$syringeUsername = "SyringeUser"
$syringePassword = "Passw0rd123"

$version = "{{VERSION}}"
$url = "https://yetanotherchris.blob.core.windows.net/syringe/Syringe-$version.zip"
$url64 = $url

. "$toolsDir\common.ps1"

if ((Test-IisInstalled) -eq $False)
{
    throw "IIS is not installed, please install it before continuing."
}

# Parse command line arguments - this function is required because of the context Chocolatey runs in, e.g.
# choco install syringe -packageParameters "/websitePort:82 /websiteDomain:'www.example.com' /restoreConfigs:true"
$arguments = @{}
$arguments["websitePort"] = 80;
$arguments["websiteDomain"] = "localhost";
$arguments["websiteDir"] = $websiteDir;
$arguments["restoreConfigs"] = "false";
Parse-Parameters($arguments);

# Stop the service and ignore if it fails
& sc.exe stop Syringe 2>&1

# Uninstall the service if it exists
if (test-path $serviceExe)
{
  Write-Host "Service found - stopping and uninstalling the service."
  & $serviceExe stop 2>&1
  & $serviceExe uninstall
}

# Backup the configs
if (test-path "$serviceDir\configuration.json")
{
    cp "$serviceDir\configuration.json" "$serviceDir\configuration.bak.json" -Force -ErrorAction Ignore
    cp "$serviceDir\environments.json" "$serviceDir\environments.bak.json" -Force -ErrorAction Ignore
    cp "$websiteDir\configuration.json" "$websiteDir\configuration.bak.json" -Force -ErrorAction Ignore
}

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'EXE' #only one of these: exe, msi, msu
  url           = $url
  url64bit      = $url64
}

# Download
Install-ChocolateyZipPackage $packageName $url $toolsDir

# Unzip the service + website (overwrites existing files when upgrading)
Get-ChocolateyUnzip  $serviceZip $serviceDir "" $packageName
Get-ChocolateyUnzip  $websiteZip $websiteDir "" $packageName

# Create a default config if none exists
if (!(test-path "$serviceDir\configuration.json"))
{
    $json = '{
        "ServiceUrl": "http://*:1981",
        "WebsiteUrl": "http://{WEBSITEURL}",
        "TestFilesBaseDirectory": "{TESTFILES-DIR}",
        "TestFileFormat": "Xml",
        "MongoDbDatabaseName": "Syringe",
        "ReadonlyMode": false,
        "OAuthConfiguration": {
            "GithubAuthClientId": "",
            "GithubAuthClientSecret": "",
            "GoogleAuthClientId": "",
            "GoogleAuthClientSecret": "",
            "MicrosoftAuthClientId": "",
            "MicrosoftAuthClientSecret": ""
        },
        "OctopusConfiguration": {
            "OctopusUrl": "",
            "OctopusApiKey": ""
        }
    }';

    $websiteUrl = $arguments["websiteDomain"] + ":" + $arguments["websitePort"];

    $testFilesDir = $arguments["websiteDir"];
    $testFilesDir = $xmlDir.Replace("\", "\\");
    $testFilesDir += "\\TestFiles"

    if (!(test-path $testFilesDir))
    {
        md $testFilesDir -ErrorAction Ignore
    }

    $json = $json.Replace("{WEBSITEURL}", $websiteUrl);
    $json = $json.Replace("{TESTFILES-DIR}", $testFilesDir); 

    [System.IO.File]::WriteAllText("$serviceDir\configuration.json", $json);
}

# Restore the configs if it's set
if ($arguments["restoreConfigs"] -eq "true")
{
    Write-Host "Restoring configs." -ForegroundColor Green
    cp "$serviceDir\configuration.bak.json" "$serviceDir\configuration.json" -Force -ErrorAction Ignore
    cp "$serviceDir\environments.bak.json" "$serviceDir\environments.json" -Force -ErrorAction Ignore
    cp "$websiteDir\configuration.bak.json" "$websiteDir\configuration.json" -Force -ErrorAction Ignore
}

# Add the user "SyringeUser" for the service
. "$toolsDir\adduser.ps1"
AddUser $syringeUsername $syringePassword

# Install and start the service
Write-Host "Installing the Syringe service." -ForegroundColor Green
& $serviceExe install --autostart -username=".\$syringeUsername" -password="$syringePassword"

Write-Host "Starting the Syringe service." -ForegroundColor Green
& $serviceExe start

# Run the website installer
Write-Host "Setting up IIS site." -ForegroundColor Green
Invoke-Expression "$websiteSetupScript -websitePath $($arguments.websiteDir) -websiteDomain $($arguments.websiteDomain) -websitePort $($arguments.websitePort)"

# Info
Write-Host ""
Write-host "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" -ForegroundColor DarkYellow
Write-Host "Setup complete." -ForegroundColor Green
Write-host "- MVC site          : http://$($arguments.websiteDomain):$($arguments.websitePort)/"
Write-Host "- REST api          : http://localhost:1981/swagger/"
Write-Host ""
Write-Host "Remember to remove the default website in IIS, if you are using port 80."
Write-host "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" -ForegroundColor DarkYellow