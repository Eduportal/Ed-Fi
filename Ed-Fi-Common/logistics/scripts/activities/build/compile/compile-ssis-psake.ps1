# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
#Assumptions:
# 1) SSIS solution must contain a DTPROJ of the same name.
# 2) A version of SSDT that is less than or equal to the project major ProductVersion must be installed.
# 3) SSDT and the Visual Studio shell it uses will have the same major version number.
properties {
    if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
        Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
    }
	Import-Module $folders.modules.Invoke('database\ssis-management.psm1')
    #$solutionName          #Parameter Passed in.
    #Reminder: passed in properties (not parameters) override these variables.
    $solutionPath              = "$($folders.base.invoke(`"ETL\$solutionName\$solutionName.sln`"))"
    $solutionConfigurationName = ""
}

task default -depends Compile

task Compile {  
    New-Ispac $solutionPath $solutionConfigurationName
}