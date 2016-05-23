# *************************************************************************
# Copyright (C) 2010, Michael & Susan Dell Foundation. All Rights Reserved.
# *************************************************************************
properties {
    if ("$leaName" -ne "") {$normalizedLeaName = $leaName.Replace(" ", "")}
    
    Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
    . "$($folders.modules.invoke('utility\build-utility.ps1'))"
    if ("$normalizedLeaName" -ne "" -and (Test-Path "$($folders.activities.invoke(`"adhoc-processes\generate-mapping-src\`"))$normalizedLeaName\vars.ps1")) {
        Write-Host "Importing $normalizedLeaName specific variables."
        . "$($folders.activities.invoke(`"adhoc-processes\generate-mapping-src\`"))$normalizedLeaName\vars.ps1"
    }
	if (Test-Path "$($folders.activities.invoke(`"adhoc-processes\generate-mapping-src\`"))$normalizedLeaName\credentials.ps1") {
        . "$($folders.activities.invoke(`"adhoc-processes\generate-mapping-src\`"))$normalizedLeaName\credentials.ps1"
    }
    
    . "$($folders.activities.invoke('adhoc-processes\generate-mapping-src\generate-mapping-src.vars.ps1'))"
    if (-not (Test-Path $mapforcePath)) {
        throw "MapForce executable was not found at $mapforcePath."
    }
}

task default -depends EstablishVpnConnection,GenerateSourceCodeForMappings,DisconnectVpn

task EstablishVpnConnection -precondition {if ("$vpnProfileName" -eq "") { Write-Host "Skipping VPN connection because no profile name was provided."; return $false} else {return $true} } {
    #currently this is the only credential use so putting it here:
    Initialize-Credentials "$($folders.activities.invoke('adhoc-processes\'))"
    
	# Credentials used in this script
	$vpnUser                      = Get-Username          "$normalizedLeaName VPN"
	$vpnPassword                  = Get-PlaintextPassword "$normalizedLeaName VPN"
	
    #This section should be parameterized into a fleixble method in the LEA specific vars file if multiple VPN client support is needed.
	#region VPN Client Specific
    if (-not(Test-Path $vpnClientPath)) {
		throw "VPN client was not found at the expected location: $vpnClientPath ..." #-Fore Red
	}
	
	Write-Host "Establishing VPN connection..."
	&$vpnClientPath connect "$vpnProfileName" user $vpnUser pwd $vpnPassword
	
	# Check to see if command returned a failure code
	if ($LASTEXITCODE -ne 200 -and $LASTEXITCODE -ne 14) { # 14 = Connection already established.
		throw "VPN connection unsuccessful.  Exit code: $LASTEXITCODE"
	}
	#endregion
	Write-Host "Connection established."
}

task DisconnectVpn -precondition {if ("$vpnProfileName" -eq "") { Write-Host "Skipping VPN disconnection because no profile name was provided."; return $false} else {return $true} } {
	if (-not(Test-Path $vpnClientPath)) {
		throw "VPN client was not found at the expected location: $vpnClientPath ..." #-Fore Red
	}

	Write-Host "Disconnecting VPN connection..."
	&$vpnClientPath disconnect

	# Check to see if command returned a failure code
	if ($LASTEXITCODE -ne 201) {
		throw "VPN disconnect unsuccessful."
	}

	Write-Host "VPN disconnected."
}

task CopyCodeTemplateCustomizations {
	# Make sure template has been customized to allow DB connection to stay open for extended periods of time.
	$codeTemplateBasePath = $mapforcePath.Substring(0, $mapforcePath.IndexOf("Altova"))

	dir $codeTemplateCustomizationsPath -Recurse -Include *.* | % {
		$relativePath = $_.FullName.Substring($_.FullName.IndexOf("Altova"))
		$targetPath = [IO.Path]::Combine($codeTemplateBasePath, $relativePath)
		if(Test-Path $targetPath) {
			copy "$($_.FullName)" "$targetPath";
		}
	}
}

task GenerateSourceCodeForMappings -depends CopyCodeTemplateCustomizations {
    if ("$mappingToRegenerate" -ne "") {
        $searchPath = [IO.Path]::Combine($mappingSearchDirectory, $mappingToRegenerate)
    }
    else {
        $searchPath = $mappingSearchDirectory
    }
    
    GenerateSourceCodeForMappingsUnderFolder "$searchPath"
}

Function GenerateSourceCodeForMappingsUnderFolder {
	Param (
	    [Parameter(Mandatory=$True)]
        [string]$folderPath
	)
    
    $mappings = dir *.mfd -Path "$folderPath" -Recurse
	# Apply filter to single mapping, if provided
	if ("$mappingToRegenerate" -ne "") {
		$mappings = $mappings | where {$_.Name -eq "$mappingToRegenerate.mfd"} # -and $_.FullName -notmatch "\\PriorYear\\"}
	}
    #Apply Subtype filter
    if ("$mappingSubType" -ne "") {
        $mappings = $mappings | where {$_.Directory -match ".*\\$mappingSubType"}
    }
	# Nothing to process? Exit now.
	if ($mappings -eq $null) {
		return
	}
	
	foreach ($mapping in $mappings) {
		try {
            $srcDirectory = [IO.Path]::Combine($mapping.DirectoryName, "src")
            #if (-not (Test-Path $srcDirectory)) { md $srcDirectory }
            Write-Host "Generating code for $($mapping.Name)..."
			
			[xml]$mappingXml = Get-Content $mapping.FullName
			$mappingVersion = $mappingXml.mapping.version
            
			# If the mapping file is marked as version 21, then use altova 2013 to do the codegen
			if($mappingVersion -eq 21) {
				$mapforceProcess = Start-Process $mapforce2013Path -ArgumentList @("$($mapping.FullName)", "/CSharp $srcDirectory", "/log $codeGenLogFile") -PassThru -NoNewWindow -Wait
			}
			else {
			# Otherwise it's still on the old version of makforce, so use the old version to do the codegen
				$mapforceProcess = Start-Process $mapforcePath -ArgumentList @("$($mapping.FullName)", "/CSharp $srcDirectory", "/log $codeGenLogFile") -PassThru -NoNewWindow -Wait
			}
            
            cat "$codeGenLogFile"
            del "$codeGenLogFile"
            
            If ($mapforceProcess.ExitCode -gt 0) {
                throw "Failure while genenerating mapping: $($mapping.FullName)"
            }
            
            # Purge credentials embedded in source code artifacts
            $mappingConsolePath = [IO.Path]::Combine($srcDirectory, "Mapping\MappingConsole.cs")
            $mappingFormPath = [IO.Path]::Combine($srcDirectory, "Mapping\MappingForm.cs")
            
            $mappingConsoleText = [IO.File]::ReadAllText($mappingConsolePath)
            [IO.File]::WriteAllText($mappingConsolePath, ($mappingConsoleText -replace "DSN=(?<DSN>[^;]*);SERVER=(?<Server>[^;]*);UID=[^;]*;Pwd=[^;]*;", 'DSN=${DSN};SERVER=${Server};UID=;Pwd=;'))

            $mappingFormText = [IO.File]::ReadAllText($mappingFormPath)
            [IO.File]::WriteAllText($mappingFormPath, ($mappingFormText -replace "DSN=(?<DSN>[^;]*);SERVER=(?<Server>[^;]*);UID=[^;]*;Pwd=[^;]*;", 'DSN=${DSN};SERVER=${Server};UID=;Pwd=;'))
            
			# Post process the code for query optimizations, only for 2010, in 2013, we have beter ways of doing the same thing the optimizations provided us.
			if($mappingVersion -ne 21) {
				PostProcessSourceCodeForQueryOptimizations "$srcDirectory"
			}
        }
        catch {}
	}
}

Function PostProcessSourceCodeForQueryOptimizations {
	Param (
		[Parameter(Mandatory=$True)]
		[string]$srcPath
	)
	
	$mappingMapSourceFiles = dir MappingMap*.cs -Path $srcPath -Recurse
	
	foreach ($sourceFile in $mappingMapSourceFiles) {
		$text = [IO.File]::ReadAllText($sourceFile.FullName)
		
		# Remove all line breaks in code
		$text = $text -replace "(`"\s*\+\s*\`")", ""
		
		$parmsMatches = ([regex]"<parms>(<parm\s*parentColumn=\\`"(?<Column>[^`"]*)\\`"\s*/>\s*)+</parms>").Matches($text) 	#"
		
		if ($parmsMatches.Count -eq 0) {
			break
		}

		$selectExpression = [regex] "(?s)`\(Altova`\.Mapforce`\.Db`\.Select`\(`"(?<SelectSql>.*?)(--\s*<parms|GetEnumerator\(\))" #"
		$selectMatches = $selectExpression.Matches($text)

		$changes = 0

		for ($i = $parmsMatches.Count - 1; $i -ge 0; $i--) {
			$parmsMatch = $parmsMatches[$i]
		
			[string[]]$parentColumnNames = $parmsMatch.Groups["Column"].Captures | %{$_.Value}

			# Iterate through Select statements in reverse order of appearance so we can modify string without affecting positions of subsequently modified SELECTs
			for ($j = $selectMatches.Count - 1; $j -ge 0; $j--) {
				$selectMatch = $selectMatches[$j]
				
				# Skip Select matches that occur after the current parms match
				if ($selectMatch.Index -gt $parmsMatch.Index) {
					continue
				}
				
				# Get text up to the point where the SQL statement starts
				$leadingText = $text.Substring(0, $selectMatch.Index)

				# Look for last instance of class inheriting from IEnumerable
				$lastIEnumerablePos = $leadingText.LastIndexOf(" : IEnumerable")
				
				$closureSearchText = $leadingText.Substring($lastIEnumerablePos)
				$closureVariableRegex = [regex] "this.(?<ClosureVariableName>var[0-9]+_bv) = var[0-9]+_bv;"
				$closureMatches = $closureVariableRegex.Matches($closureSearchText)
				
				if ($closureMatches.Count -eq 0) {
					# Just use a default name, because we couldn't find one
					$closureVariableName = "var1_bv"
				} else {
					$closureVariableName = $closureMatches[0].Groups["ClosureVariableName"].Value;
				}
				
				$selectSql = $selectMatch.Groups["SelectSql"].Value
				$selectModifiedCount = 0
				
				foreach ($parentColumnName in $parentColumnNames) {
				#	$contextClosureExpression = "`" + (closure.var1_bv).Select(Altova.Mapforce.MFQueryKind.AttributeByQName, new System.Xml.XmlQualifiedName(`"{0}`", `"`")) + `"" -f $parentColumnName
					$contextClosureExpression = "`" + Altova.Functions.Core.First((closure.$closureVariableName).Select(Altova.Mapforce.MFQueryKind.AttributeByQName, new System.Xml.XmlQualifiedName(`"{0}`", `"`"))) + `"" -f $parentColumnName

					$parmPos = $selectSql.IndexOf("?")
					
					if ($parmPos -ge 0) {
						$newSelectSql1 = $selectSql.Substring(0, $parmPos)
						$newSelectSql2 = $selectSql.Substring($parmPos + 1)
						$selectSql = $newSelectSql1 + $contextClosureExpression + $newSelectSql2

						$selectModifiedCount++
					}
				}

				if ($selectModifiedCount -gt 0) {
                    #strips out the parameter marker first so it doesn't step on the select replacement.
					$text = $text.Substring(0, $parmsMatch.Index) + $text.Substring($parmsMatch.Index + $parmsMatch.Length)
					#this is so only the part of the file we're working with is impacted, in case the sql statement is in more than one place.
                    $text = $text.SubString(0,$selectMatch.Index) + $text.SubString($selectMatch.Index).Replace($selectMatch.Groups["SelectSql"].Value, $selectSql)
					$changes = $changes + $selectModifiedCount
				}

				break
			}
		}

		if ($changes -gt 0) {
			Write-Host "Writing $changes change(s) to $($sourceFile.FullName) ... "  -ForegroundColor Cyan
			[IO.File]::Copy($sourceFile.FullName, [IO.Path]::ChangeExtension($sourceFile.FullName, ".cs.orig"), $true)
			[IO.File]::WriteAllText($sourceFile.FullName, $text)
			#[IO.File]::WriteAllText([IO.Path]::ChangeExtension($sourceFile.FullName, ".cs.new"), $text)
		}
	}
}