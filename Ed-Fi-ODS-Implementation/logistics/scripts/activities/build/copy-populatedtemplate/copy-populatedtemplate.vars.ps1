# NOTE: \NuGet\EdFi.RestApi.Databases\prep-package.ps1 also relies on this file

# Make sure this matches what is in NuGet/EdFi.Samples.Ods/EdFi.Samples.Ods.nuspec
# Note: the version is taken from nuget's packages.config
$SamplesOdsPackageName = "EdFi.Samples.Ods"
$SamplesOdsFilename = "EdFi.Samples.Ods.bak"

# Calculate this for use later
$SamplesOdsDirectory = "$($folders.base.invoke('..'))\$SamplesOdsPackageName"
$SamplesOdsFullPath = "$SamplesOdsDirectory\$SamplesOdsFilename"

# Currently requires an open feed 
$SamplesOdsNugetSourceName = "Ed-Fi Alliance MyGet Feed"
$SamplesOdsNugetSourceUri = "https://www.myget.org/F/d10f0142ad50421a8938b3a9e49977b4/"

