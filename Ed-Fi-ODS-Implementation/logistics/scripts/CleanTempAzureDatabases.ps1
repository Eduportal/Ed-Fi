<#
	.SYNOPSIS
		Get-ISqlDBUsingSMO
	.DESCRIPTION
		Show all databases using SMO for a given server instance
	.PARAMETER serverInstance
		SQL Server instance
	.EXAMPLE
		.\Get-ISqlDBUsingSMO -serverInstance MyServer\SQL2012
	.INPUTS
	.OUTPUTS
		Database IDs and Names
	.NOTES
	.LINK
#>
param (
	[string]$serverInstance = "$(Read-Host 'Server Instance [e.g. Server01\sql2012]')"
)

begin {
	[void][reflection.assembly]::LoadWithPartialName( "Microsoft.SqlServer.Smo" )
}
process {
	try {	
		if(-not($serverInstance)){
			$serverInstance = "tcp:krdgqpm8ad.database.windows.net"
		}
	
		Write-Verbose "Show all databases using SMO for a given server instance..."

		$smoServer = new-object Microsoft.SqlServer.Management.Smo.Server $serverInstance	
		$smoServer.ConnectionContext.DatabaseName = "Master"
		$smoServer.ConnectionContext.LoginSecure = false
		$smoServer.ConnectionContext.Login = "admin@krdgqpm8ad"
		$smoServer.ConnectionContext.Password = "***REMOVED***"
		
		$dbs = $smoServer.databases | foreach { $_ }

		foreach ($database in $dbs) {
			if(($database.Name.Length -eq 32) -or (($database.Name.StartsWith("EdFi_Ods_") -and  $database.Name.Length -eq 41))){
				$database.Drop()
				Write-Output "Dropped: " $database.Name
			}
		}
	}
	catch [Exception] {
		Write-Error $Error[0]
		$err = $_.Exception
		while ( $err.InnerException ) {
			$err = $err.InnerException
			Write-Output $err.Message
		}
	}
}