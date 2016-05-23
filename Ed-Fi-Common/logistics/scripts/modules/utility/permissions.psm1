Function Add-DirectoryPermissions ($dirPath, $grantee, $permissionLevel) {
    if(Test-Path $dirPath) {
        $dirAcl = Get-Acl $dirPath
        #set up the access rules
        $allInherit = [system.security.accesscontrol.InheritanceFlags]"ContainerInherit, ObjectInherit"
        $allPropagation = [system.security.accesscontrol.PropagationFlags]"None"
        $accessRule = New-Object system.security.AccessControl.FileSystemAccessRule($grantee, $permissionLevel, $allInherit, $allPropagation, "Allow")
        $dirAcl.AddAccessRule($accessRule)
        Set-Acl $dirPath $dirAcl
        #Make sure it worked
        $dirAcl = Get-Acl $dirPath
        $identitySearch = "*\$grantee"
        $accessList = @()
        $accessList = $accessList + ($dirAcl.Access | where {$_.IdentityReference -like $identitySearch -and $_.FileSystemRights -like "*$($permissionLevel)*" })
        if (($accessList.Length -gt 0) -and ($accessList[0] -ne $null)) {
            Write-Host "$permissionLevel Permissions for $grantee on $dirPath have been established sucessfully!" }
        else {
            Throw "Adding $permissionLevel Permissions for $grantee on $dirPath failed." }
    }
    else {
        Throw "Invalid path specified. Permissions can not be granted."
    }
}
Function Remove-DirectoryPermissions($dirPath, $grantee) {
    if(Test-Path $dirPath) {
        $permissionRemovalCntr = 0
        $dirAcl = Get-Acl $dirPath
        $identitySearch = "*\$grantee"
        $accessesToRemove = @()
        $accessesToRemove = $accessesToRemove + ($dirAcl.Access | where {$_.IdentityReference -like $identitySearch})
        if (($accessesToRemove.Length -gt 0) -and ($accessesToRemove[0] -ne $null)) {
            $inheritedAccess = @()
            $inheritedAccess = $inheritedAccess + ($accessesToRemove | where { $_.IsInherited -eq $true })
            #If we have inherited access we need to remove, we must first protect this object from inheritance.
            if(($inheritedAccess.Length -gt 0) -and ($inheritedAccess[0] -ne $null)){
                $dirAcl.SetAccessRuleProtection($true,$true)
                Set-Acl $dirPath $dirAcl
                Write-Host "Inheritance from parent removed for $dirPath ..."
                #Now that that is done, go around again. to see if we have any more access changes to make.
                Remove-DirectoryPermissions "$dirPath" "$grantee"
            }
            else {
                $accessesToRemove | % {$dirAcl.RemoveAccessRule($_) | Out-Null
                $permissionRemovalCntr++
                }
                Set-Acl $dirPath $dirAcl
                Write-Host "$permissionRemovalCntr permission(s) removed on $dirPath"
            }
        }
        else {
            Write-Host "Nothing to do, $grantee does not individually have rights to this folder."
        }
    }
    else {
        Throw "Invalid path specified. Permissions can not be removed."
    }
}
Export-ModuleMember -Function Remove-DirectoryPermissions, Add-DirectoryPermissions