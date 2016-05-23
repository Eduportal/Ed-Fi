function global:Initialize-BaseQueueDirectoryWithinRepository() {
	$queuePath = Join-Path -Path $solutionPaths.repositoryRoot -ChildPath MessagingQueueData 
	if (Test-Path $queuePath) 
	{
		Remove-Item -Recurse -Force $queuePath
	}
	New-Item -ItemType directory -Path $queuePath
}

function global:Initialize-QueuesForApplication($appName) {
    function Initialize-QueuePath($queuePath) {			
		if (-not (Test-Path $queuePath)) {
			Write-Host "Creating directory for file system queues at $queuePath"
			md $queuePath
		}

		Add-DirectoryPermissions "$queuePath" "IIS_IUSRS" "FullControl"
		Add-DirectoryPermissions "$queuePath" "Users" "FullControl"
	}

	$queueSettings = $localDeploymentSettings.applications[$appName].FileSystemQueues
	$baseQueuePath = Join-Path -Path (Get-ApplicationPhysicalAddress($appName)) -ChildPath ($queueSettings.BaseDirectory)

	Initialize-QueuePath($baseQueuePath)

	foreach($queue in $queueSettings.Queues) {
		$queuePath = Join-Path -Path $baseQueuePath -ChildPath $queue
		Initialize-QueuePath($queuePath)
	}
}
