Function global:Remove-Sandboxes() {
	$parms = @{
		 environment = "Development"
	 }
	 . $folders.activities.invoke('build/remove-sandboxen/remove-sandboxen.ps1') @parms
	 if ($error) { throw $error }
 }