Param(  $AdminURLRoot = "",
		$AdminEmail = "",
        $AdminPassword = ""
     )

# ignore self-signed cert errors 
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# login	 
$credentials= @{emailaddress="$AdminEmail";password="$AdminPassword";rememberme="false";Provider="credentials"}
Invoke-RestMethod $AdminURLRoot/account/login -Body $credentials -Method Post -SessionVariable session

# issue sandbox reset command
$reset_command = '{"Command":"reset"}'
Invoke-RestMethod $AdminURLRoot/api/sandbox -Body $reset_command -Method Put -ContentType "application/json" -WebSession $session
