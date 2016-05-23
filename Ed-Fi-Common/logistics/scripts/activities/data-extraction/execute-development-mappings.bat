@Echo off
cd %~dp0
Echo Enter the Local Education Agency Name to preform development data extaction for: (e.g. Glenadale ISD,  Grand Bend ISD, etc.)
Set /P districtName=
@Echo on
%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -noexit -ExecutionPolicy unrestricted -Command "& { $target = [System.IO.Path]::Combine((resolve-path .), 'execute-lea-data-extraction.ps1'); . $target '%districtName%' -developmentOnlyNoPreExtract; }"