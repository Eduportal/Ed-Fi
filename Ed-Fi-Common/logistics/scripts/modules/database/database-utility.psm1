<#
    .Synopsis
    Generate T-SQL that declares a SQL parameter.
    .Description
    Take in a ParameterName and an optional ParameterValue and return T-SQl
    that can be run on a SQL server to declare that variable. 

    When possible, guess the relevant SQL type for the variable; assume VARCHAR
    in all other cases.
    .Parameter ParameterName
    Define a parameter with the given name
    .Parameter ParameterValue
    Define a parameter with the given value
#>
Function Get-SqlParameter {
    param(
        [parameter(mandatory=$true)] 
        [string] 
        $ParameterName,
        [parameter(mandatory=$true)] 
        [ValidateNotNull()]
        $ParameterValue
    )
    $ParameterName = "@$ParameterName"

    if ($ParameterValue -and ($ParameterValue.Gettype() -match "(?i)Int(\d\d)?")) {
        $ParameterType = "INT"
    }
    elseif ($ParameterValue.Length -gt 7990) {
        $ParameterType = "VARCHAR(MAX)"
        $ParameterValue = "'$ParameterValue'"
    }
    else {
        $ParameterType = "VARCHAR($($ParameterValue.Length + 10))"
        $ParameterValue = "'$ParameterValue'"
    }
    
    $parmLine = @("DECLARE $ParameterName AS $ParameterType","SET $ParameterName = $ParameterValue;")
    return $parmLine
}


function Get-SqlBatchData {
    [cmdletbinding()]
    param(
        [string[]] $batchSql,
        [int] $executionCount
    )
    #Default to 1
    if ($executionCount -lt 1) { $executionCount = 1}
    #Abstraction in case we need to add properties later.
     $batchData = @{
        script         = $batchSql -join [Environment]::NewLine
        executionCount = $executionCount
     }
    return $batchData
}


<#
    .Synopsis
    Convert a sql script that requests variables and metadata into an object
    that contains the metadata and ready-to-run SQL, including definition of
    requested SQL paramters.
    .Description
    Take in the path of a .sql script or a string array of a sql script and 
    return an object that contains its contents. Read specially crafted 
    SQL comments to get additional metadata about how the sql script should 
    be run, and enable use of variables as T-SQL parameters.

    We use a custom format where lines at that begin with 
        --#ps MetadataType:MetadataValue
    are considered to contain metadata pairs. 

    Possible types of metadata, with examples: 

        --#ps Variable:PowershellVariableName

            Declare a SQL parameter called @PowershellVariableName that is set to 
            the value of $PowershellVariableName

        --#ps VariableFileContent:PowershellVariableName

            Delcare a SQL parameter called @PowershellVariableName that is set to 
            the *contents* of a file located at $PowershellVariableName. 

        --#ps Timeout:1800

            Set the timeOut property on the returned object to 1800. Indicating
            a timeout of 1800 seconds.

        --#ps Permission:Admin

            Set the requiresAdmin property on the returned object to $true.
            Indicating that this script should be run with elevated permissions.

    .Parameter ScriptPath
    The location of a .sql script that may contain metadata pairs
    .Parameter ScriptSql
    The script sql to be run that may contain metadata pairs
    .Parameter SqlVariable
    A hashtable of variables the SQL script may request
    .Parameter IgnoreMetadata
    Ignore any of the metadata information, if it is present
#>
function New-SqlScript {
    [cmdletbinding()]
    param(
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='path')]
        [string] 
        $scriptPath,
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='string')]
        [string[]] 
        $scriptSql,
        [Parameter(Position=1, Mandatory=$false)]
        [hashtable] 
        $sqlVariables,
        [Parameter(Position=2, Mandatory=$false)]
        [switch] 
        $ignoreMetadata
    )
    
    #TODO: Add parameter set logic to allow string array in.
    if ($PsCmdlet.ParameterSetName -eq "path") {
        $scriptPath = resolve-path $scriptPath
        [string[]]$originalScriptContents = get-content $scriptPath 
    }
    else {
        #Try to handle if we are given a big string of SQL
        if ($scriptSql.Count -eq 1 -and $scriptSql[0].Contains([Environment]::NewLine)) {
            [string[]]$originalScriptContents = $scriptSql[0] -split [Environment]::NewLine
        }
        else {
            [string[]]$originalScriptContents = $scriptSql
        }
    }
    [string[]]$batchSql = @() #Holds lines in a batch until a go is reached
    $sqlParms = @() #Holds any sql parms that are found
    $SqlScriptProperties = @{
        scriptBatches = @()
        scriptVariables = @()
        permission = ""
        connectionString = $null
        timeOut = $null
        variables = @{}
    }
    
    #Doing the no go logic to enable variable prservation accross batches.
    #The alternative is to use the $server.ConnectionContext
    #However the that would involve string injection after every "GO"
    #Continuing the GO split seems like the lesser "evil"
    #This no go split allows comments on the same line and multiple batch execution
    $noGoRegex = "(?i)^(?:\s*(?<isGo>GO)(?:(?:\s*)|(?:\s+(?<repeat>[1-9]+)\s*)?)(?:--.*)*)|(?:(?<out1>.+\s+)(?<isGo>GO)(?<out2>\s+[^1-9].*)?)$"
    $commentBlockStart = "(?i)(?:(?:.*?)(?:((?<!\*)/\*))(?:.*?))+.*"
    $commentBlockEnd = "(?i)(?:(?:.*?)(?:(\*/))(?:.*?))+.*"
    $outerCommentBlockStart = "(?i)^.*(?:(?<!.*--.*)((?<!\*)/\*)).*$"
    $singleLineCommentBlock = "(?i)^(?<out1>.*)((?:(?<!.*--.*)(?:(?<!\*)/\*)).*(?:\*/))(?<out2>.*)$"
    $sqlScriptPsMetaPrefix = "--#ps"
    $commentBlockCount =0
    foreach ($line in $originalScriptContents) {
         $isFirstCommentLine = $false
         #Remove comment blocks that are all on one line because they make it too hard to process for no go.
         #Note... If for somereason some one nests the closing comment markup on the same line where this lives
         #It will not remove the GO on that line
         $line = $line -replace $singleLineCommentBlock, "`${out1}`${out2}"
         #Doesn't count as a comment block if it has a single line comment before it.
         if($commentBlockCount -eq 0 -and $line -match $outerCommentBlockStart) {$isFirstCommentLine = $true; $commentBlockCount++ }
         #DifferentRegex for nested comment blocks.
         if($commentBlockCount -gt 0 -and $line -match $commentBlockStart) { $commentBlockCount += ($matches[1].Count - [int]$isFirstCommentLine) }
         if($commentBlockCount -gt 0 -and $line -match $commentBlockEnd) { $commentBlockCount -= $matches[1].Count}
         #Aggregate until we hit a go
         if($commentBlockCount -eq 0 -and $line -match $noGoRegex) {
             #Pull the execution count is present, else default to once.
             $executionCount =  if($matches["repeat"]) {[Int32]::Parse($matches["repeat"])}
             #Override the line in this context using comment to suppress any iterations in sql
             #NOTE: Single line inlined Go's with SQL statements after them will result in the subsequent SQL
             #statement not executing as it will be commented out.
             $line = $line -replace $noGoRegex,"`${out1}--`${out2}"
             $batchSql += $line
             
             #add to script batches for provided SQL
             $SqlScriptProperties.scriptBatches += Get-SqlBatchData $batchSql $executionCount

             #Reset for new batch
             [string[]]$batchSql = @()
         }
         else {
            #aggregate
            $batchSql += $line
         }
        
        if (-not $ignoreMetadata.ispresent -and $line.StartsWith($sqlScriptPsMetaPrefix) ) {
            $mdType, $mdValue = $line.TrimStart($sqlScriptPsMetaPrefix).Trim().Split(":")
            write-verbose "Processing metadata: $mdType = $mdValue"
            switch($mdType) {
                "Timeout" {
                    $SqlScriptProperties.timeout = $mdValue
                }
                "Permission" {
                        #This allows the specification of the role the script should run under.
                        #The current model only allows for 'Admin' and default.
                        $SqlScriptProperties.permission = $mdValue
                }

                # If the metadata type is Variable or VariableFileContent, we look
                # in the provided $sqlVariables hashtable for a KEY that is equal to 
                # the $mdValue from the $line found in the provided SQL script.
                # The value in the $sqlVariables hashtable that matches that key, is the value for the variable.
                "Variable" {
                    if ($sqlVariables.Keys -contains $mdValue) {
                        $sqlParm = Get-SqlParameter -ParameterName $mdValue -ParameterValue $sqlVariables[$mdValue]
                        $SqlScriptProperties.scriptVariables += $sqlParm -join [Environment]::NewLine 
                        $SqlScriptProperties.variables += @{ $mdValue = $sqlVariables[$mdValue] }
                    }
                    else {
                        Write-Warning "The $mdType '$mdValue' was requsted by the script metadata but was not found in the `$sqlVariables provided."
                        Write-Verbose "`$sqlVariables contents:"
                        $sqlVariables | Format-Table | Write-Verbose
                    }
                }
                "VariableFileContent" {
                    #make sure this is empty string to start.
                    $variableFileContents = ""
                    if ($sqlVariables.Keys -contains $mdValue) {
                        #For the variable file content this path is specified in configuration in the $sqlVariables hastable as the value for the $mdValue
                        if (Test-Path $sqlVariables[$mdValue]) {
                            #I think there is a reason why we're using ReadAllText here instead of get content.
                            #Maybe line breaks?
                            #The current assumption is that the caller knows what they are getting from the file.
                            #This might be SQL, or Text, etc.
                            $variableFileContents = [IO.File]::ReadAllText($sqlVariables[$mdValue]) -replace "'", "''"
                        }
                        else {
                            Write-Warning "The $mdType '$mdValue' was requsted by the script metadata but the file was not found."
                        }
                    }
                    else {
                        Write-Warning "The $mdType '$mdValue' was requsted by the script metadata but was not found in the `$sqlVariables provided."
                        Write-Verbose "`$sqlVariables contents:"
                        $sqlVariables | Format-Table | Write-Verbose
                    }
                    $sqlParm = Get-SqlParameter -ParameterName $mdValue -ParameterValue $variableFileContents
                    $SqlScriptProperties.scriptVariables += $sqlParm -join [Environment]::NewLine 
                    $SqlScriptProperties.variables += @{ $mdValue = $variableFileContents }
                }
            }
        }
    }
    #Clear SQL buffer if there were no "go's" in the file or the last statement in the file didn't have one.
    if ($batchSql.Count -gt 0) {
         #add to script batches for provided SQL
         $SqlScriptProperties.scriptBatches += Get-SqlBatchData $batchSql
    }

    #Instantiate
    $SqlScriptObject = New-Object PSObject -property $SqlScriptProperties
    return $SqlScriptObject
}


export-modulemember -function Get-SqlParameter, New-SqlScript