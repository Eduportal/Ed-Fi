Function Get-ProjectSpecificDatabaseName {
    param(
        [parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$projectType,

        [alias("leaName")]
        [string]$specificEntityTypeName,

        [string]$databaseType, 

        [string]$buildConfigurationTypeName,

        [string]$schoolYear, 

        $versionSuffix
    ) 

    #Building up db name
    #Pattern: Project_SET_DbType_BuildConfig_ProdSuffix
    #Ex. EdFi_GrandBendISD_Dashboard_IntegrationTemp_Prd
    $dbName = $projectType
    if (-not [string]::IsNullOrWhiteSpace($specificEntityTypeName)) {
        $normSpecificEntityTypeName = $specificEntityTypeName.Replace(" ", "")
        $dbName += "_$normSpecificEntityTypeName"
    }
    if (-not [string]::IsNullOrWhiteSpace($databaseType)) {
        $dbName += "_$databaseType"
    }
    if (-not [string]::IsNullOrWhiteSpace($buildConfigurationTypeName)) {
        $dbName += "_$buildConfigurationTypeName"
    }
    if (-not [string]::IsNullOrWhiteSpace($schoolYear)) {
        $dbName += "_$schoolYear"
    }
    if (-not [string]::IsNullOrWhiteSpace("$versionSuffix")) {
        $dbName += "_$versionSuffix"
    } 
    return $dbName
}

Export-ModuleMember Get-ProjectSpecificDatabaseName