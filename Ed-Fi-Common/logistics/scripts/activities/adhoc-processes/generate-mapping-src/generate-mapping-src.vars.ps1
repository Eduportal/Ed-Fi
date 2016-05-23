$vpnClientPath = "$(Get-ProgramFiles-x86-Path)\Cisco Systems\VPN Client\vpnclient.exe"
$mapforcePath  = "$(Get-ProgramFiles-x86-Path)\Altova\MapForce2010\mapforce.exe"
$mapforce2013Path  = "$(Get-ProgramFiles-x86-Path)\Altova\MapForce2013\mapforce.exe"

$codeTemplateCustomizationsPath = Resolve-Path "$($folders.base.invoke('Etl\EdFi.Etl.Xml\CodeGenerationCustomizations'))"
$codeGenLogFile                 = "$(Resolve-Path "$mappingSearchDirectory\..\")" + "MapForceLog.txt"