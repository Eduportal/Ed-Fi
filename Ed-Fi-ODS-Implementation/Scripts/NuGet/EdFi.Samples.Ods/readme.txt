A NuGet package for the populated template database backup

NOTE: Using a database backup (and this NuGet package to version it) is a temporary stopgap. Soon, this populated template must be built from scratch as part of an automated build process. 

The populated template is different per organization. Customize the name of this directory, the .nuspec file, and the text inside the .nuspec file, before creating the NuGet package. Also decide on a versioning scheme and set the package version and release notes in the NuSpec file. 

Make sure to set the following variables in the variables file (logistics\scripts\activities\build\copy-populatedtemplate\copy-populatedtemplate.vars.ps1):

- $SamplesOdsPackageName = "EdFi.Samples.Ods"
- $SamplesOdsFilename = "EdFi_GrandBendISD_EdFi_IntegrationTemp.bak"

