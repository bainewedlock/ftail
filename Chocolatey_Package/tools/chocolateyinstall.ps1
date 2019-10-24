$packageName = 'ftail'
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$fileLocation = Join-Path $toolsDir 'FTailSetup.msi'

$packageArgs = @{
  packageName   = $packageName
  fileType      = 'msi'
  file          = $fileLocation
  silentArgs    = "/qn /norestart /l*v c:\FTail_msi_install.log"
  validExitCodes= @(0, 3010, 1641)
  softwareName  = 'FTail'
}

Install-ChocolateyInstallPackage @packageArgs
