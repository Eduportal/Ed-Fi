# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
properties {
	$teamCityNunitPath = $env:teamcity_dotnet_nunitlauncher
    $nUnitExe = if([IntPtr]::size -eq 8) {"nunit-console-x86.exe"} else {"nunit-console.exe"}
    $localNunitPath = "$($folders.base.invoke(`"Application\packages\NUnit*\tools\$nUnitExe`")) /framework:net-4.0 /xml:'$($folders.base.invoke('..\'))TestResults.xml'"
    $isLocal = ($teamCityNunitPath -eq $null)
    
    $nunitPath = if($isLocal){$localNunitPath} Else {$teamCityNunitPath + " v4.0 x64 NUnit-2.5.10"}
    $testDir = "$($folders.base.invoke(`"..\$testType\`"))"
    Import-Module "$($folders.modules.invoke('utility\zip.psm1'))"
}

task default -depends Test

task Test -depends Setup-Test, Test-WebApp


task Setup-Test {
	try {
        Expand-Zip "$testDir\Tests.zip" "$testDir\"
	} catch {
		Write-Host "Error During Test Setup"
		Exit 1;
	}
}

task Test-WebApp { 
    try {
            $testAssemblies = Get-ChildItem $testDir -Filter $testFilter
            
            foreach ($assembly in $testAssemblies) {
				$args = [string] $args + " " + $assembly.FullName
			}
			
			if ($args -ne $null -and $args.Length -gt 0)
			{ $args = $args.Substring(1) }
            Invoke-Expression "$nunitPath $args" | Out-Host
    } 
    catch {
		Write-Host "Error During $testType"
        throw $error
	}
}
