# This requires pandoc from <http://johnmacfarlane.net/pandoc>

[cmdletbinding()]
param(
    [ValidateSet("html","docx")] [string[]] $buildFormat = @("html","docx"),
    [switch] $OpenOutput
)

set-alias pandoc "$env:localappdata\pandoc\pandoc.exe" #default install location

$fileBasename = "ConfiguringHigherEnvironments"
$fileDatedBasename = "$fileBasename $(get-date -format yyyyMMdd)"
$infile = "$fileBasename.markdown"
$outfiles = @()

$informat = "markdown_mmd+pandoc_title_block"

function makehtml {
    $outfile = "$fileDatedBasename.html"
    write-verbose "Writing $outfile"
    pandoc -f $informat -t html5 "$infile" -o "$outfile" `
        --smart --normalize --standalone --toc --toc-depth=5 `
        --include-in-header="./css-header.html"
    $outfiles += @($outfile)
}
function makedocx {
    $outfile = "$fileDatedBasename.docx"
    write-verbose "Writing $outfile"
    pandoc -f $informat -t docx "$infile" -o "$outfile" `
        --smart --normalize 
    $outfiles += @($outfile)
}

foreach ($bf in $buildFormat) {
    invoke-expression "make$bf"
}
if ($OpenOutput.IsPresent) {
    foreach ($of in $outfiles) {
        start $of.fullname
    }
}
return $outfiles
