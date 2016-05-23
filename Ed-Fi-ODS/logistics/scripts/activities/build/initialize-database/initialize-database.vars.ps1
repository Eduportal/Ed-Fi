. (Get-CorePath "logistics\scripts\activities\build\initialize-database\initialize-database.vars.ps1")
# Path to the Console BulkLoad executable. Default value below assumes it has been copied to a subfolder logistics/bin/ in a post-build event.
#$consoleBulkLoadExe = $folders.base.invoke("logistics/bin/EdFi.Ods.BulkLoad.Console/EdFi.Ods.BulkLoad.Console.exe")
# Note that, because the console bulk loader is copied to this location as a post-build step, this must not use $folders.base.invoke()
$consoleBulkLoadRelPath = "logistics/bin/EdFi.Ods.BulkLoad.Console/EdFi.Ods.BulkLoad.Console.exe"

# Paths to the folder and manifest for Descriptors
$edfiDescriptorsFolder = $folders.base.invoke("$infrastructureDbPath\Data\EdFi\Descriptors")
$edfiDescriptorsManifest = $folders.base.invoke("$infrastructureDbPath\Data\EdFi\Descriptors\Manifest.xml")

# Paths to the folder and manifest for EdOrgs
$edfiEdOrgsFolder = $folders.base.invoke("$infrastructureDbPath\Data\EdFi\EdOrg")
$edfiEdOrgsManifest = $folders.base.invoke("$infrastructureDbPath\Data\EdFi\EdOrg\Manifest.xml")

# The 'EdFi' (aka ODS) dbType has always been transient in EdFi 1.x, but in 2.x it is now considered persistent. 
# For the time being, the -Common repo will still set the 'EdFi' dbType as transient, and we will unset that here. 
# The plan is to change this in -Common after 1.3 comes out
$dbTypeTransient = $dbTypeTransient |? { $_ -ne "EdFi" }

# Allow environments to define a dbTypeTransientOverride variable which can add 'EdFi' back 
if ($dbTypeTransientOverride) {
    $dbTypeTransient += @($dbTypeTransientOverride)
}
