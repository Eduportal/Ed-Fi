import-module $PSScriptRoot\..\path-resolver.psm1
import-module $folders.modules.invoke('utility\build-utility.psm1')


<#
    .synopsis
    Get all Web.Config files that a given Visual Studio project file relies on
    .description
    Visual Studio projects can include other projects with a <ProjectReferece Include=""> directive.
    Return all Web*.Config files in the main project or any included projects.
    .parameter ProjectFile
    A path to a .ccproj or .csproj file 
    .parameter ConfigType
    The type of configuration file, such as App for App.Config or Web for Web.Config
#>
function Get-ConfigFile {
    param(
        [parameter(mandatory=$true)] [string] $projectFile,
        [string[]] $ConfigType = @("App","Web")
    )
    $projectFile = resolve-path $projectFile
    $projectDir = split-path $projectFile
    # Find all included projects from the projectFile, so you can get at all the .config files
    $projectXml = New-Object XML
    $projectXml.Load($projectFile)
    $includeDir = @($projectDir)
    foreach ($include in $projectXml.project.itemgroup.ProjectReference.Include) {
        $includeDir += @(split-path (resolve-path "$projectDir\$include"))
    }
    $configs = @()
    foreach ($type in $ConfigType) {
        gci -path $includeDir -filter "$type*.config"
    }
    return $webconfig
}

<#
    .synopsis
    Invoke a web request on a URL and ignore the response, unless it's an error. 
    .description
    Invoke a web request on a URL. If it returns an error - even a server error - throw. If not, ignore the content at the URL. 
    This is useful in e.g. Azure, where after deploying for the first time, a site is not set up until it receives its first request.
#>
function Initialize-URL {
    [cmdletbinding()]
    param(
        [string] $url,
        [int]$TimeoutSeconds = 60,
        [switch] $ignoreSslErrors,
        [switch] $ignoreInternalServerErrors
    )
    if ($timeoutSeconds -lt 1) {
        write-verbose "TimeoutSeconds set to '$timeoutSeconds'; resetting to default of '60'."
        $timeoutSeconds = 60
    }
    <#
    WHY are we doing this as a job? Because I can't figure out a better way to make sure that
    ServerCertificateValidationCallback goes back to normal, ssl-validating mode after this is finished. 
    #>
    $invokeWebRequestSB = {
        param(
            [string] $callerScriptRoot,
            [string] $url,
            [int] $timeoutSeconds = 60,
            [bool] $ignoreSslErrors = $false,
            [bool] $ignoreInternalServerErrors = $false
        )
        if ($ignoreSslErrors) {
            $sslLog = "ignoring any SSL errors found"
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
        }
        else {
            $sslLog = "throwing any SSL errors found"
        }
        $HttpReq = [System.Net.HttpWebRequest]::Create($url)
        $HttpReq.Timeout = $timeoutSeconds * 1000
        write-host "Warm up URL '$Url' with $($HttpReq.Timeout) millisecond timeout and $sslLog."
        try {
            $HttpReq.GetResponse()
        }
        catch [System.Net.WebException] {
            #write-host "Error status: $($_.Exception.status)"
            if ($_.Exception.status -eq [System.Net.WebExceptionStatus]::TrustFailure) {
                write-host "SSL validation error"
                write-error $_
            }
            elseif ($ignoreInternalServerErrors) {
                write-host "Caught and ignored an internal server error"
            }
            else {
                write-host "Non-SSL error (possibly an internal server error of some kind?)"
                write-error $_
            }
        }
        return $HttpReq
    }
    $webReqJobArgs = @($PSScriptRoot,$url,$timeoutSeconds,$ignoreSslErrors.ispresent,$ignoreInternalServerErrors.ispresent)
    $webReqJob = start-job -scriptblock $invokeWebRequestSB -argumentList $webReqJobArgs
    while ($webReqJob.state -eq "Running") {
        sleep 1
    }
    $HttpResponse = receive-job $webReqJob
}

export-modulemember -function Get-ConfigFile,Initialize-URL