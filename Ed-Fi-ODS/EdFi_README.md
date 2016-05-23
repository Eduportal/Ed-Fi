# Automatic Generation of T4 Templates
This solution uses T4 templates to generate code from metadata and databases. None of the generated files are kept (by default) in the repository. Therefore, they are generated as needed, and when the solution is built. This introduces several changes to the directory structure and project settings in the solution.

Because the generators use projects contained in this solution, build dependencies have been created between the assemblies providing the code generation logic and the projects containing assemblies using those assemblies.

The Microsoft supported mechanism for running T4 templates during compiles is documented [here](http://msdn.microsoft.com/en-us/library/ee847423.aspx). Consult this documentation to identify the files required by your build server.

You must install the [Microsoft Visual Studio 2013 SDK](http://www.microsoft.com/en-us/download/details.aspx?id=40758) as well as the [Modeling SDK for Microsoft Visual Studio 2013](http://www.microsoft.com/en-us/download/details.aspx?id=40754) to use the automatic transformation.

For more information about this process you can read [this blog post](http://www.olegsych.com/2010/04/understanding-t4-msbuild-integration)

Generating the templates [Inside Visual Studio](http://blogs.clariusconsulting.net/kzu/how-to-transform-t4-templates-on-build-without-installing-a-visual-studio-sdk/) without the VS SDK

Additional documentation regarding configuration is available [here](http://www.olegsych.com/2010/04/understanding-t4-msbuild-integration)

The `<#@ cleanupbehavior processor="T4VSHost" cleanupafterprocessingtemplate="true" #>` statement has been removed from the t4 templates because it does not work with msbuild. See the [Microsoft Documentation](http://msdn.microsoft.com/en-us/library/dn495329.aspx) and [Rowan Miller's Blog](http://romiller.com/2013/03/21/processor-named-t4vshost-could-not-be-found-for-the-directive-named-cleanupbehavior/) for more details.

The problem here is that the msbuild process is not completely cleaned up of references to modules that were used in previous builds. When projects are rebuilt and their target libraries are still in use, the builds fail. The problem addressed by the cleanup behavior processor in visual studio may be addressed in msbuild by setting an environment variable (MSBUILDDISABLENODEREUSE=1) either globally, or in the Developer Command Prompt prior to launching visual studio (devenv). If invoking msbuild directly, add the /nr:false flag to the msbuild command line. This setting will turn off the default behavior of reusing msbuild processes. 

## Targets
Each project that generates t4 templates should include a reference to the T4TextTemplating.Targets file located in the /src directory. This file is used to add automatic T4 generation to the project.

## Templates Directory
A global reference to the Templates directory has been added to the available parameters for t4 template generation, and this directory is referenced within each t4 script. 
We have copied __EF.Utility.CS.ttinclude__ into this directory. It is normally included as part of the Entity Framework Tools and is used to generate multiple files under one template. If this file is updated by microsoft (it is open source as part of Entity Framework), it should be found in the 
    __C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\Extensions\Microsoft\Entity Framework Tools\Templates\Includes__ directory.

## Lib Directory
There is a lib directory created in the Visual Studio Solution directory where DLLs required for downstream t4 generation are copied after successful build. This approach replaces the practice of finding the /bin/debug or similar directory for dlls within each script and removes the requirement for each script to have relative path information to each upstream library.

## MSBuild Settings
The following build switches are recommended for MSBuild

msbuild /nr:false /t:clean;build 