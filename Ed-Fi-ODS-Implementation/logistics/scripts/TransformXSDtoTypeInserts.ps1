# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
param(
	[string] $xsltFile,
	[string] $xsdFile,
	[string] $sqlDestinationFile
)

	$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
	$rootDir = (get-item $scriptPath ).parent.parent.FullName

	if(-Not($xsltFile)) {
		$xsltFile = Join-Path -Path $rootDir -ChildPath "\TNExtensions\Transformations\XSDEnumerationsToTypeInserts.xslt"
	}
		
	if(-Not($xsdFile)){
		$xsdFile = Join-Path -Path $rootDir -ChildPath "\TNExtensions\Schemas\Ed-Fi-Core.xsd"
	}
	
	if(-Not($sqlDestinationFile)){
		$sqlDestinationFile = Join-Path -Path $rootDir -ChildPath "\Database\Structure\EdFi\3001-Type-inserts-generated-from-the-xsd.sql"
	}
	
	$xslt = New-Object System.Xml.Xsl.XslCompiledTransform
	$xslt.Load($xsltFile)
	$xslt.Transform($xsdFile, $sqlDestinationFile)


