Function global:Reset-GeneratedArtifacts() {
    function private:ProcessSwaggerProject() {
        # Modify csproj files for new/removed files
        $webApiProj = "$($srcPath)EdFi.Ods.WebApi\EdFi.Ods.WebApi.csproj"
        $webApiProjFolder = [IO.Path]::GetDirectoryName($webApiProj)

        Write-RunMessage "Rewriting '$webApiProj' for multiple-outputs generated Swagger metadata..."

        [xml] $projDoc = Get-Content $webApiProj

        # Remove all JSON content files
        $projDoc.Project.ItemGroup.Content | ? {$_.Include -and $_.Include.StartsWith("Metadata") -and $_.Include.EndsWith(".json")} | % {$_.ParentNode.RemoveChild($_)} | out-null

        # Get XmlElement references needed to modify the XML document
        $swaggerTemplateElt = $projDoc.Project.ItemGroup.Content | ? {$_.Include -and $_.Include.EndsWith("SwaggerMetadata.tt")}
        $itemGroupElt = $swaggerTemplateElt.ParentNode

        # Find all the includes
        $metadataPath = [IO.Path]::Combine($webApiProjFolder, "Metadata")
        $includes = dir *.json -recurse -path $metadataPath | sort FullName -Descending | % { $_.FullName.Replace($webApiProjFolder, "").Trim('\\') }

        foreach ($include in $includes) {
	        $newElt = $swaggerTemplateElt.OwnerDocument.CreateElement("Content", $projDoc.DocumentElement.NamespaceURI);
	        $newElt.SetAttribute("Include", $include);
	        $itemGroupElt.InsertAfter($newElt, $swaggerTemplateElt) | out-null
        }

        # Save the modified project file using UTF-8 encoding
        $sw = new-object System.IO.StreamWriter($webApiProj, $false, [System.Text.Encoding]::UTF8)
        $projDoc.Save($sw);
        $sw.Dispose();
    }

    function private:ProcessNHibernateMappingsProject() {
        # Modify csproj files for new/removed files
        $mappingsProj = "$($srcPath)EdFi.Ods.Entities.NHibernate.Mappings.SqlServer\EdFi.Ods.Entities.NHibernate.Mappings.SqlServer.csproj"
        $mappingsProjFolder = [IO.Path]::GetDirectoryName($mappingsProj)

        Write-RunMessage "Rewriting '$mappingsProj' for multiple-outputs generated NHibernate mappings..."

        [xml] $projDoc = Get-Content $mappingsProj

        # Remove all hbm embedded resources
        $projDoc.Project.ItemGroup.EmbeddedResource | ? {$_.Include -and $_.Include.EndsWith(".hbm.xml") } | % {$_.ParentNode.RemoveChild($_)} | out-null

        # Get XmlElement references needed to modify the XML document
        $mappingsTemplateElt = $projDoc.Project.ItemGroup.None | ? {$_.Include -and $_.Include.EndsWith("_EntityOrmMappings.tt")}
        $itemGroupElt = $mappingsTemplateElt.ParentNode

        # Find all the includes
        $includes = dir *.hbm.xml -recurse -path $mappingsProjFolder | sort FullName -Descending | % { $_.FullName.Replace($mappingsProjFolder, "").Trim('\\') }

        foreach ($include in $includes) {
	        $newElt = $mappingsTemplateElt.OwnerDocument.CreateElement("EmbeddedResource", $projDoc.DocumentElement.NamespaceURI);
	        $newElt.SetAttribute("Include", $include);
	        $itemGroupElt.InsertAfter($newElt, $mappingsTemplateElt) | out-null
        }

        # Save the modified project file using UTF-8 encoding
        $sw = new-object System.IO.StreamWriter($mappingsProj, $false, [System.Text.Encoding]::UTF8)
        $projDoc.Save($sw);
        $sw.Dispose();
    }

    function private:GenerateTemplate([string] $templateFilename) {
	    $templateContent = [IO.File]::ReadAllText($templateFilename)
	    $extensionRegex = [regex]"<#@\s+output\s+extension=`"(?<Extension>[\.\w]+)`"\s*.*?#>"
	    $match = $extensionRegex.Match($templateContent)

	    if ($match.Success) {
		    $extension = $match.Groups["Extension"].Value;
	    } else {
		    $extension = ".cs";  # Output to C# by default
	    }

	    $templateOutputFilename = [IO.Path]::Combine([IO.Path]::GetDirectoryName($templateFilename), [IO.Path]::GetFileName($templateFilename).Replace("External.tt", $extension))

	    # Execute transformation
	    Write-RunMessage "Generating template $templateFilename..."
	    $result = Invoke-TextTransform "-r $commonAssembly -r $odsCommonAssembly -r $commonCodeGenAssembly -r $bulkLoadCommonAssembly -r $xmlShreddingCodeGenAssembly -r $xsdParsingAssembly $templateFilename -out $templateOutputFilename"

        # return the output filename
        return $templateOutputFilename
    }

	Write-CommandMessage "Reset-GeneratedArtifacts"

    Write-RunMessage "Resetting generated artifacts..."

    $srcPath = "$($solutionPaths.repositoryRoot)Application\"

    # Set environment variables needed by templates (in lieu of having access to EnvDTE)
    $env:EdFiProjectFileName = "$($srcPath)EdFi.Ods.Entities.Common\EdFi.Ods.Entities.Common.csproj"
    $env:EdFiSolutionPath = "$($srcPath)"


    # Build the code generation support assemblies (use /p:OutputDir=bin\Debug to redirect output binaries)
    $commonProj              = "$($srcPath)EdFi.Common\EdFi.Common.csproj"
    $odsCommonProj           = "$($srcPath)EdFi.Ods.Common\EdFi.Ods.Common.csproj"
    $bulkLoadCommonProj      = "$($srcPath)EdFi.Ods.BulkLoad.Common\EdFi.Ods.BulkLoad.Common.csproj"
    $commonCodeGenProj       = "$($srcPath)EdFi.Ods.Common.CodeGen\EdFi.Ods.Common.CodeGen.csproj"
    $xmlShreddingCodeGenProj = "$($srcPath)EdFi.Ods.XmlShredding.CodeGen\EdFi.Ods.XmlShredding.CodeGen.csproj"
    $xsdParsingProj          = "$($srcPath)EdFi.Ods.XsdToWebApi\EdFi.Ods.XsdToWebApi.csproj"

    Write-RunMessage "Compiling supporting assemblies..."
    $result = Invoke-MsBuild "$commonProj /v:q"
    $result = Invoke-MsBuild "$odsCommonProj /v:q"
    $result = Invoke-MsBuild "$commonCodeGenProj /v:q"
    $result = Invoke-MsBuild "$xmlShreddingCodeGenProj /v:q"
    $result = Invoke-MsBuild "$xsdParsingProj /v:q"

    # CodeGen support assembly paths
    $commonAssembly              = "$($srcPath)EdFi.Common\bin\Debug\EdFi.Common.dll"
    $odsCommonAssembly           = "$($srcPath)EdFi.Ods.Common\bin\Debug\EdFi.Ods.Common.dll"
    $commonCodeGenAssembly       = "$($srcPath)EdFi.Ods.Common.CodeGen\bin\Debug\EdFi.Ods.Common.CodeGen.dll"
    $bulkLoadCommonAssembly      = "$($srcPath)EdFi.Ods.BulkLoad.Common\bin\Debug\EdFi.Ods.BulkLoad.Common.dll"
    $xmlShreddingCodeGenAssembly = "$($srcPath)EdFi.Ods.XmlShredding.CodeGen\bin\Debug\EdFi.Ods.XmlShredding.CodeGen.dll"
    $xsdParsingAssembly          = "$($srcPath)EdFi.Ods.XsdToWebApi\bin\Debug\EdFi.Ods.XsdParsing.dll"

    # TransformText exe path
    #$t4 = "C:\Program Files (x86)\Common Files\microsoft shared\TextTemplating\12.0\TextTransform.exe"

    # Get all external T4 template file paths
    $templateFilenames = dir *External.tt -path $srcPath -recurse | % { $_.FullName }

    # Codify known template dependencies (this template depends on an assembly that depends on the Resources generated by another template)
    $postResourceGenerationTemplates = @("ResourceFactoriesExternal.tt")

    $postponedTemplates = @()

    foreach ($templateFilename in $templateFilenames) {
	    if ($postResourceGenerationTemplates.Contains([IO.Path]::GetFileName($templateFilename))) {
		    Write-Host "Postponing processing of $templateFilename..." -Fore DarkCyan
		    $postponedTemplates += $templateFilename
		    continue;
	    }
	
	    $outputFilename = GenerateTemplate $templateFilename

        # If the file start with a number, copy it to the Ed-Fi structure folder
        if ([IO.Path]::GetFileName($templateFilename) -match "^[0-9]{4}.*") {
            Write-RunMessage "Copying $outputFilename to $($solutionPaths.repositoryRoot)Database\Structure\EdFi ..."
            copy $outputFilename "$($solutionPaths.repositoryRoot)Database\Structure\EdFi"
        }
    }

    # Process dependent templates
    $result = Invoke-MsBuild "$bulkLoadCommonProj /v:q"

    foreach ($templateFilename in $postponedTemplates) {
	    $outputFilename = GenerateTemplate $templateFilename
    
        # If the file start with a number, copy it to the Ed-Fi structure folder
        if ([IO.Path]::GetFileName($templateFilename) -match "^[0-9]{4}.*") {
            Write-RunMessage "Copying $outputFilename to $($solutionPaths.repositoryRoot)Database\Structure\EdFi ..."
            copy $outputFilename "$($solutionPaths.repositoryRoot)Database\Structure\EdFi"
        }
    }

    ProcessSwaggerProject
    ProcessNHibernateMappingsProject
}

