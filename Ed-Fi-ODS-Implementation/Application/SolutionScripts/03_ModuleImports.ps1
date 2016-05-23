# Can't use the invocation path because the path is not set on the invocation when called from solution scripts

import-module $folders.modules.invoke('deployment') -force
import-module $folders.modules.invoke('azure-tools') -force
import-module $folders.modules.invoke('packaging') -force
import-module $folders.modules.invoke('database\database-management.psm1') -force
import-module $folders.modules.invoke('database\database-migrations.psm1') -force
Import-Module $folders.modules.invoke("utility\permissions.psm1") -force
import-module $folders.modules.invoke('psake\psake.psm1') -force
. $folders.modules.invoke("utility\build-utility.ps1")

# Reimplementation of Update-SolutionScripts from the SolutionScripts NuGet package
# So it can be used from outside of VS Nuget Package Manager
if (-not (test-path function:Update-SolutionScripts)) {
    $solutionScriptsContainer = $folders.base.invoke('Application\SolutionScripts')
    Function Update-SolutionScripts() {
        foreach ($file in (gci $solutionScriptsContainer)) {
            if ($file.extension -eq '.ps1') {
                Write-Host "        Sourcing: $file"            
                . $file.fullname
            }
            if ($file.extension -eq '.psm1')
            {
                Write-Host "Importing Module: $file"
                Import-Module $file.fullname -Force
            }
        }
    }
}
set-alias Reset-SolutionScripts Update-SolutionScripts
set-alias -scope global initscript Update-SolutionScripts

. $folders.activities.invoke('build/Development.vars.ps1')


Function global:Remove-CustomModules() {
	Remove-Module deployment,database-management,database-migrations,build-utility,permissions,psake
}

