# *************************************************************************
# ©2014 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************

#Root folder name for DB scripts
$infrastructureDbPath = "Database\"
    
#changed this to plus equals because there are times when you'd want to add more database types to it before initialize-database is called and not have it overwritten.
#this should be fine because it should all be happening in the same scope and the next time we call it psake should clear the variable.
$dbTypeTransient += @("Dashboard", "EdFi", "Acceptance")
#matches the edfiStandardVersion supplied
$edfiStandardVersionRegEx = "(.*)(?:\\$edfiStandardVersion)"
#matches ALL edfi Legacy Version folders
$edfiLegacyVersionsRegEx = "(.*)(?:\\\d+\.\d+)"
$datascriptSqlTimeout=30
$connectionStrings = @{}
if (-not (test-path variable:leaSubTypeRegex)) {
    $leaSubTypeRegex = $specificEntitySubTypeRegex
}

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
