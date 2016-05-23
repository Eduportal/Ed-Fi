<#
DO NOT ADD FUNCTIONS TO THIS FILE. 

It is included as a stop-gap. 

All functionality belongs in the build-utility.psm1 *module* file. 
#>

import-module $PSScriptRoot\build-utility.psm1 -force

<#
What remains are functions which have scope problems when they are defined in a module. 

**PLEASE CONSIDER THESE FUNCTIONS DEPRECATED**

It is our intention to replace them over time, but doing so is not a high priority. 
#>

function Test-Function { 
    param (
        [string] $functionName
    )
    $values = @(get-command -all | where {$_.Name -eq "$functionName" -and $_.CommandType -eq "Function"})

    #Return True or False
    $values.Count -gt 0
}

Function Test-Variables {
    param (
        [string[]] $names
    )
    $missingNames = $names | where { (Test-Path "variable:\$_") -eq $false }

    $missingNames -eq 0
}

function Assert-Variables([String[]]$names) {
    [String[]] $missingNames = $names | where { (Test-Path "variable:\$_") -eq $false }

    if ($missingNames.Count -gt 0) {
        $msg = [string]::Join("`n`t", $missingNames)
        throw @"
Missing required script arguments:
    $msg
"@
    }
}

# TODO: Confirm no usage under new script organziation, and delete
function Assert-PSakeArguments([String[]]$names) {
    [String[]] $missingNames = $names | where { ($scriptArgs.Contains($_) -eq $false) }

    Write-Host "This version of the function is obsolete. Please use Assert-Variables." -Fore Blue
    
    if ($missingNames.Count -gt 0) {
        $msg = [string]::Join("`n`t", $missingNames)
        throw @"
Missing required script arguments:
    $msg
"@
    }
}
set-alias Assert-Psake-Arguments Assert-PsakeArguments

function Test-DefaultTask {
    $script:context.Peek().tasks.ContainsKey("default")
}

