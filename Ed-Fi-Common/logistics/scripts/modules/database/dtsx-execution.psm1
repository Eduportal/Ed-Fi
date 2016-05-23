#Load Assembly
[Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ManagedDTS")
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\modules\path-resolver.psm1
}
$logFileBasePath = "$($folders.base.invoke(''))..\temp\PackageLogs\"
function Invoke-Package { param ([parameter(Mandatory=$true)][string] $packagePath, [Hashtable] $connections, [Hashtable] $variables)
    
    if (Test-Path $packagePath) {
        #create new app
        $app = New-Object ("Microsoft.SqlServer.Dts.Runtime.Application")

        #load the package
        $package = $app.LoadPackage("$packagePath", $null)
        #configure
        foreach ($connection in $connections.Keys) {
            if ($package.Connections.Contains("$connection")) {
                $package.Connections["$connection"].ConnectionString = $connections[$connection]
            }
        }
        foreach ($variable in $variables.Keys) {
            if ($package.Variables.Contains("$variable")) {
                $package.Variables["$variable"].Value = $variables["$variable"]
            }
        }
        #Only enable file logging if we are debugging. 
        if($PSDebugContext) {
            Write-Debug "Enabling Package Logging..."
            $packageName = (dir $packagePath).BaseName
            Remove-OldLogsForPackage $packageName
            if(-not (Test-path $logFileBasePath)){md $logFileBasePath | Out-null}
            $logFilePathName = "$logFileBasePath{0}-{1}.xml" -f $packageName, [guid]::NewGuid()
            
            if(Test-Path $logFilePathName) { Remove-Item $logFilePathName -force }
            
            $logConMgr = $package.Connections.Add("File");
            $logConMgr.ConnectionString = $logFilePathName
            $logConMgr.Name = "XML Logging"
            $packageLogProvider = $package.LogProviders.Add("DTS.LogProviderXMLFile")
            $packageLogProvider.ConfigString = "XML Logging"
            $package.LoggingOptions.EventFilterKind = [Microsoft.SqlServer.Dts.Runtime.DTSEventFilterKind]::Inclusion
            $package.LoggingOptions.EventFilter = @("OnPreExecute","OnPostExecute","OnError","OnWarning")
            $package.LoggingMode = "Enabled"
            $package.LoggingOptions.SelectedLogProviders.Add($packageLogProvider)
        }
        $packageName = Split-Path $packagePath -Leaf
        $packageDir = Split-Path $packagePath
        Write-Host "Before execution of package:" -ForegroundColor Yellow
        Write-Host "    $packageName" -ForegroundColor DarkYellow
        Write-Host "From directory:" -ForegroundColor Yellow
        Write-Host "    $packageDir" -ForegroundColor DarkYellow
        #run
        $result = $package.Execute()
        
        #report
        Write-Host "`nResult of $packageName Execution:" -ForegroundColor Yellow
        Write-Host -ForegroundColor DarkYellow "    $result"
        
        #Only report on the log files if we are debugging and they exist.
        if($PSDebugContext -and (Test-Path $logFilePathName)) {
            Write-Debug "`nPackage Log Information Summary (from $logFilePathName):"
            [xml]$logInfo = Get-Content $logFilePathName
            foreach($dtslog in $logInfo.dtslogs.dtslog) {
                $packageLogNodes += $dtslog.ChildNodes
            }
            $packageLogOut = $packageLogNodes | Format-table -Property @{label="Source";Expression={$_.source};alignment="left"; width=25},
                                                                       @{label="Event";Expression={$_.event};alignment="left"; width=14},
                                                                       @{label="Message";Expression={$_.message};alignment="left";} -wrap | out-string
            Write-Debug "$packageLogOut"
        }
        
        $success = $false
        if ($package.Errors.Count -gt 0 -or ($result -eq [Microsoft.SqlServer.Dts.Runtime.DTSExecResult]::Failure)) {
            $packageErrorOut = @()
            foreach ($packageError in $package.Errors){
                 #Don't count the encrypted password errors.
                 if ($packageError.ErrorCode -ne -2146893813) {
                    $packageErrorOut += "    {0}: {1}" -f $packageError.Source,$packageError.Description
                 }
            }
            if ($packageErrorOut.Count -gt 0) {
                Write-host "The following errors occured during package execution:" -ForegroundColor Yellow
                Write-host "$packageErrorOut" -ForegroundColor DarkRed
            }
            elseif ($result -ne [Microsoft.SqlServer.Dts.Runtime.DTSExecResult]::Failure) {
                $success = $true
            }
        }
        else {$success = $true}
        
        if ($package -ne $null) {
            $package.Dispose()
            $package = $null
        }
        
        #if ( $($package.Warnings.Count) > 0) {
        #   Write-Host "The following warnings were returned: `r`n $($package.Warnings)"
        #}
        
        #Return Success
        $success
    }
    else {
        throw "$packagePath is not valid! A vaild package path must be provided."
    }
}

function Remove-OldLogsForPackage ([parameter(Mandatory=$true)][string]$packageName,[int]$daysToKeep = 15) {
    if(Test-path $logFileBasePath) {
        $keepDate = (Get-Date).ToUniversalTime() - [Timespan]::FromDays($daysToKeep)
        Write-Host "Purging logs for $packageName older than $keepDate (UTC):"
        $logsToPurge = dir $logFileBasePath | where {$_.BaseName.ToLower().StartsWith($packageName.ToLower())} | where {$_.LastAccessTimeUtc -lt $keepDate} 
        foreach ($log in $logsToPurge) {
            $log | Write-Host
            try {
                $log | Remove-Item -force -ErrorAction "Stop"
            } catch {
                Write-Host ("An error occured while purging the log:`r`n {0}" -f $_)
            }
        }
        
        Write-Host "Log purge completed."
    }
    else {
        Write-Host "No logfile directory found."
    }
}
<# This needs updating and testing that is not in scope at this time.
function Invoke-PackageAsync { param ([parameter(Mandatory=$true)][string] $packagePath, [Hashtable] $connections, [Hashtable] $variables)
    #Check package path validity before we make the async call.
    if (-not (Test-Path $packagePath)) { throw "$packagePath is not valid! A vaild package path must be provided." }
    [int]$id = Start-job -scriptBlock { ExecutePackage $packagePath $connections $variables} -InitializationScript { Import-Module $MyInvocation.MyCommand.Path } | % { $_.Id }
    #return Job Id
    $id
}

function ExecutePackagesAsync { param ([parameter(Mandatory=$true)][string[]] $packagePaths, [Hashtable] $connections, [Hashtable] $variables)
    [int]$ids = $()
    $packagePaths | % {
        $ids += ExecutePackageAsync $_ $connections $variables
    }
    #returns job ids
    $ids
}

#SingleFile Execution
function ExecutePackages { param ([parameter(Mandatory=$true)][string[]] $packagePaths, [Hashtable] $connections, [Hashtable] $variables)
    $packagePaths | % {
        Invoke-Package $_ $connections $variables
    }
}
#>
Export-ModuleMember Invoke-Package