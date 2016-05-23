# NOTE: To use these in NPM Console/SolutionScripts, they must be set to global!

$appTypePrefix = "EdFi"
$sqlServerName = "localhost"
$defaultOleDbProvider = "SQLNCLI11"

# Always drop all databases before creating them in Development
# [ODS-433] Required to be Rest_Api due to client dependencies on folder name, rename later to EdFi_Bulk
$dbTypeTransient += @("EdFi","EdFiAdmin","EduId","SSO_Integration","EdFiPopulated", "Rest_Api", "EdFiSecurity")

#This is the pattern that matched for script $specificEntitySubTypeRegExs to indicate that it is an LEA $specificEntitySubTypeRegEx for additonal filtering.
$specificEntitySubTypeRegEx = "\d+\.\w+ISD"

# $env:certPrefix = "Development"

