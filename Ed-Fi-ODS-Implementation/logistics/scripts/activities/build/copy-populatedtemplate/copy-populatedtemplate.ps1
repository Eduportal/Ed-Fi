Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1 -force
import-module $folders.modules.invoke('packaging')
import-module $folders.modules.invoke('utility/build-utility.psm1')

. $folders.activities.invoke('build/copy-populatedtemplate/copy-populatedtemplate.vars.ps1')

$packagesDir = $folders.base.invoke('Application\packages')
$NuGetPackagesConfig = $folders.base.invoke('Application\.nuget\packages.config')
$SamplesOdsPackageVersion = get-xpath -targetFilePath $NuGetPackagesConfig -xpath "/packages/package[@id='$SamplesOdsPackageName']/@version"
$PopDbBackupPath = resolve-path "$packagesDir\$SamplesOdsPackageName.$SamplesOdsPackageVersion\$SamplesOdsFilename"

mkdir -force $SamplesOdsDirectory | out-null
write-host "Copy '$PopDbBackupPath' to '$SamplesOdsDirectory'..."
cp $PopDbBackupPath $SamplesOdsDirectory

if ($Error) {
    write-host "ERROR: $error" -fore RED
    exit $error.Count
}
