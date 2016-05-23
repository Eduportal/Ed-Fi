# Hack for setting up databases for use during code generation

# All the same as the normal integration environment...
. $PSScriptRoot\EFASharedIntegration.vars.ps1

# ... except for these things
$sqlServerName = "localhost"
foreach ($k in $environmentCSBsByDbType.keys) {
    $environmentCSBsByDbType.$k['Server'] = $sqlServerName
}
