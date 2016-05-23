properties { 
    Import-Module $folders.modules.invoke('database\ssis-management.psm1')
    Import-Module $folders.modules.invoke('database\database-management.psm1')
}

task default -depends PrepareServer,Deploy,SyncronizeSSISEnvironmentVariables

task PrepareServer {
    Enable-CLR $ssisDeploymentCSB.DataSource
    New-SsisCatalog $ssisDeploymentCSB.DataSource -ssisCatalogPassword $ssisCatalogPassword
    New-SsisFolder $ssisDeploymentCSB.DataSource -ssisFolderName $ssisCatalogFolderName
}

task Deploy {
    Publish-SsisProject $ssisDeploymentCSB.DataSource -ssisFolderName $ssisCatalogFolderName -packageFilePath $ssisPackageFilePath -ssisProjectName $ssisProjectName
}

task SyncronizeSSISEnvironmentVariables {
    #Reset the SSIS environment with this name in this SSIS folder to empty.
    Sync-SsisEnvironment $ssisDeploymentCSB.DataSource -ssisFolderName $ssisCatalogFolderName -ssisEnvironmentName $ssisEnvironment
    #Establish all of the variable sets in the SSIS environment
    Set-SsisEnvironmentVariables $ssisDeploymentCSB.DataSource -ssisFolderName $ssisCatalogFolderName -ssisEnvironmentName $ssisEnvironment -variables $ssisEnvironmentVariables
    #Set the SSIS environment for the named project to the named SSIS environment that was just reset and repopulated.
    Set-SsisProjectEnvironment $ssisDeploymentCSB.DataSource -ssisFolderName $ssisCatalogFolderName -ssisEnvironmentName $ssisEnvironment -ssisProjectName $ssisProjectName
    #Align the project's parameter values to be the environment variables with the same name.
    Sync-SsisProjectParmatersToEnvironmentVariables $ssisDeploymentCSB.DataSource -ssisFolderName $ssisCatalogFolderName -ssisEnvironmentName $ssisEnvironment -ssisProjectName $ssisProjectName
}