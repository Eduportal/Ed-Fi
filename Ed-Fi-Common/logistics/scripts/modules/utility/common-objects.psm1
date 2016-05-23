$nestedHastableKeyComparerText = @"
public class NestedHastableKeyComparer : System.Collections.IEqualityComparer
{
    bool System.Collections.IEqualityComparer.Equals(object x, object y)
    {	
		var xHash = x as System.Collections.Hashtable;
		var yHash = y as System.Collections.Hashtable;
		//Check keys from first hash
		foreach (var key in xHash.Keys) {
			if (yHash.ContainsKey(key) && yHash[key].ToString().ToUpper() == xHash[key].ToString().ToUpper()) 
			{
				continue;
			}
			return false;
		}
		//Check keys from second hash
		foreach (var key in yHash.Keys) {
			if (xHash.ContainsKey(key) && yHash[key].ToString().ToUpper() == xHash[key].ToString().ToUpper()) 
			{
				continue;
			}
			return false;
		}
		return true;
    }

    public int GetHashCode(object obj)
    {
		//This isn't perfect, hash collisions definitely possible.
		//http://msdn.microsoft.com/en-us/library/system.object.gethashcode(v=vs.110).aspx
		var objHash = obj as System.Collections.Hashtable;
		double hashHolder = 0;
		foreach (var key in objHash.Keys) {
			hashHolder += key.ToString().ToLower().GetHashCode();
			hashHolder += objHash[key].ToString().ToLower().GetHashCode();
		}
		return hashHolder.GetHashCode();
    }
}
"@
if (-not ("NestedHastableKeyComparer" -as [type])) {
    Add-Type -TypeDefinition $nestedHastableKeyComparerText
}
<#
    .Synopsis
    Return a new hashtable with a custom equality comparer for hashtable keys.

    .Description 
    The hashtable returned only allows other hashtables as the key. At this time it does not support other types.
    The hashtables used as keys should themsleves have keys and values that are strings or integers. This implementation
    does not support recursive nesting.
    
    .Example
        $HashtableKeyedHashtable = New-HashtableKeyedHashtable
        $HashtableKeyedHashtable[@{marry = "potter"; larry = "moe"}] = "Funny"
        $HashtableKeyedHashtable[@{marry = "potter"; larry = "moe"}] = "Funny2"
        $HashtableKeyedHashtable
    
        The output from this is a single entry with a value of "Funny2"
        
    .Example
        $HashtableKeyedHashtable = New-HashtableKeyedHashtable
        $HashtableKeyedHashtable[@{marry = "potter"; larry = "moe"}] = "Funny"
        $HashtableKeyedHashtable[@{marry = "potter2"; larry = "moe2"}] = "Funny2"
        $HashtableKeyedHashtable
    
        The output from this is two entries.
        This demonstartates that all of the contents (keys and values) of the hashtable keys must be equal for the hashtable keys to be treated
        as equal.
#>
Function New-HashtableKeyedHashtable {
    return New-Object HashTable -ArgumentList @(New-Object NestedHastableKeyComparer)
}

Export-ModuleMember -Function "New-HashtableKeyedHashtable"