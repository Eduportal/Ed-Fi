Function Get-ClearFilename { param($dirtyFileName)
    [string]$cleanFileName = $dirtyFileName;
    $badChars = @("`\", "`/", "`:", "`*", "`?", "`"", "`<", "`>", "`|", "`;") #"
    foreach ( $char in $badChars) {
       $cleanFileName =  $cleanFileName.Replace($char, "_")
    }
    $cleanFileName.Trim()
}

Function Get-XpathQuery ($xPathBase,$andValues = @{},$orValues = @{}){
    #First create the xpath with all required fields
    foreach ($name in $andValues.Keys){
        $values = @()
        $values += $andValues[$name]
        foreach ($value in $values) {
            $expression = "[$name = `'$value`']"
            #Map *'s to expected matching style.
            if ($value -match "\*(.*)\*") { $expression = "[contains($name, `'$($matches[1])`')]" }
            elseif ($value -match "(.*)\*") { $expression = "[starts-with($name, `'$($matches[1])`')]" }
            elseif ($value -match "\*(.*)") { $expression = "[ends-with($name, `'$($matches[1])`')]" }
            $xPathBase += "$expression"
        }
    }
    #Write-Host "Base Xpath with 'and' values: $xPathBase"
    $xPath = @()
    foreach ($name in $orValues.Keys){
        $values = @()
        $values += $orValues[$name]
        $startingCount = $xPath.Count
        foreach ($value in $values) {
            $expression = "[$name = `'$value`']"
            #Map *'s to expected matching style.
            if ($value -match "\*(.*)\*") { $expression = "[contains($name, `'$($matches[1])`')]" }
            elseif ($value -match "(.*)\*") { $expression = "[starts-with($name, `'$($matches[1])`')]" }
            elseif ($value -match "\*(.*)") { $expression = "[ends-with($name, `'$($matches[1])`')]" }
            
            #Just add the first or to the list
            if ($startingCount -eq 0) {
                $xPath += "$xPathBase$expression"
            }
            #after that just add it to each of the existing ones.
            for ($i =0; $i -lt $startingCount;$i++) {
                $xPath += "$($xPath[$i])$expression"
            }
        }
        #reset the list so that the original set is gone.
        $xPath = $xPath[$($startingCount)..$($xPath.Length - 1)]
    }
    
    #Write-Host "Base Xpath with 'and' and 'or' values: $xPath"
    if ($xPath.Count -eq 0) {
        $xPathBase
    } 
    else {
        $xPathOut = "$($xPath[0])"
        for ($i =1; $i -lt $xPath.Count;$i++) {
            $xPathOut += " | $($xPath[$i])"
        }
        $xPathOut
    }
	
}
