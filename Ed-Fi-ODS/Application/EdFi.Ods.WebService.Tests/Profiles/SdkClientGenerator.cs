using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using EdFi.Common.Extensions;
using EdFi.Ods.Common.Utils.Extensions;
using log4net;
using Microsoft.Win32;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public static class SdkClientGenerator
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SdkClientGenerator));

        public class SdkConfig
        {
            public string BaseUrl = @"http://localhost:4445";
            public string ApiMetadataUrl = @"http://localhost:4445/metadata";
            public string SdkProjectName = "EdFi";
            public string SdkAssemblyName = "EdFi.Ods.Sdk.dll";
            public string SwaggerJarPath = "sdk-generate.jar"; 
        }

        public static void EnsureClientSdkSourceCodeGenerated(
            string apiMetadataUrl, List<string> metadataSections, 
            string tempMetadataETagPath, SdkConfig sdkConfig)
        {
            string sdkAssemblyFilePath = Path.Combine(
                tempMetadataETagPath,
                sdkConfig.SdkProjectName,
                sdkConfig.SdkAssemblyName);

            if (File.Exists(sdkAssemblyFilePath))
            {
                Console.WriteLine("SDK assembly '{0}' already exists.  No code generation will be performed.",
                    sdkAssemblyFilePath);

                return;
            }

            // Clean up all the existing SDK folders, unless configured otherwise
            if (!ConfigurationManager.AppSettings["PreserveAllTestSdks"].EqualsIgnoreCase("true"))
            {
                try
                {
                    string sdksParentFolder = Path.GetDirectoryName(tempMetadataETagPath);

                    Directory.GetDirectories(sdksParentFolder)
                             .ForEach(
                                 directory =>
                                 {
                                     try { Directory.Delete(directory, true); }
                                     catch { }
                                 });
                }
                catch { }
            }
            // Close all listeners to mitigate an intense slowdown incurred from 
            // httpConfig.EnableSystemDiagnosticsTracing() during swagger metadata generation.
            Trace.Close();

            string javaPath = GetJavaPath();

            foreach (var metadataSection in metadataSections)
            {
                var metadataSectionReplaced = metadataSection.Replace('-', '_');
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                var urlArgument = apiMetadataUrl + "/" + metadataSection.ToLower() + "/api-docs";
                startInfo.FileName = javaPath;
                startInfo.Arguments =
                    String.Format(
                        "-jar {0} csharp --url {1} --baseDir {2} --projectName {3} --apiPackage {4} --modelPackage {5} --helperPackage {6}",
                        sdkConfig.SwaggerJarPath, urlArgument, tempMetadataETagPath, sdkConfig.SdkProjectName,
                        String.Format("{0}.Apis.{1}", sdkConfig.SdkProjectName, metadataSectionReplaced),
                        String.Format("{0}.Models.{1}", sdkConfig.SdkProjectName, metadataSectionReplaced), sdkConfig.SdkProjectName);

                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                process.ErrorDataReceived += (sender, args) => Console.WriteLine("Error: " + args.Data);
                process.StartInfo = startInfo;

                try
                {
                    process.Start();
                }
                catch (Win32Exception ex)
                {
                    // Handle the exception that occurs when "java" cannot be found on the PATH with a more helpful message.
                    if (ex.Message.Contains("The system cannot find the file specified"))
                        throw new Exception("Unable to generate .NET SDK client for testing purposes because the attempt to launch the code generation process failed.  Please ensure that the folder containing 'java.exe' is in your PATH environment variable.", ex);

                    throw new Exception("Unable to generate .NET SDK client for testing purposes because the attempt to launch the code generation process failed.", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to generate .NET SDK client for testing purposes because the attempt to launch the code generation process failed.", ex);
                }

                process.BeginOutputReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception(string.Format(
                        "java.exe returned an exit code of '{0}'.\r\nFile name: {1}\r\nArguments: {2}", 
                        process.ExitCode,
                        startInfo.FileName,
                        startInfo.Arguments));
            }

            // Refresh all attached listeners once metadata generation is complete.
            Trace.Refresh();
        }

        private static string GetJavaPath()
        {
            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");

            if (string.IsNullOrEmpty(javaHome))
            {
                _logger.Debug("JAVA_HOME environment variable does not have a value, so falling back to using 'java' available on the PATH.");
                return "java";
            }

            string javaPath = Path.Combine(javaHome, "bin", "java.exe");

            // If java couldn't be found on the explicitly set JAVA_HOME, log a warning and fall back to the PATH
            if (!File.Exists(javaPath))
            {
                _logger.WarnFormat(
                    "JAVA_HOME environment variable was set, but Java could not be found at '{0}'.  Falling back to using the 'java' on the PATH.",
                    javaPath);

                return "java";
            }

            return javaPath;
        }

        public static void EnsureDotNetSdkAssemblyCompiled(string tempSdkAssemblyPath)
        {
            if (File.Exists(tempSdkAssemblyPath))
            {
                return;
            }

            string programCsFilePath = Path.GetDirectoryName(tempSdkAssemblyPath) + "\\Program.cs";

            if (File.Exists(programCsFilePath))
                File.Delete(programCsFilePath);

            string containingDirectory = Path.GetDirectoryName(tempSdkAssemblyPath);

            if (!Directory.Exists(containingDirectory))
            {
                throw new DirectoryNotFoundException(
                    String.Format(
                        "Unable to compile .NET SDK client for testing purposes because the generated source code could not be found at '{0}'.",
                        containingDirectory));
            }

            string projectBinPath = Environment.CurrentDirectory;

            string newtonsoftJsonAssemblyPath = projectBinPath + @"\Newtonsoft.Json.dll";

            if (!File.Exists(newtonsoftJsonAssemblyPath))
            {
                throw new FileNotFoundException(String.Format(
                    "Unable to compile .NET SDK client for testing purposes because JSON.NET assembly could not be found at '{0}'.", 
                    newtonsoftJsonAssemblyPath));
            }

            string restSharpAssemblyPath = projectBinPath + @"\RestSharp.dll";

            if (!File.Exists(restSharpAssemblyPath))
            {
                throw new FileNotFoundException(String.Format(
                    "Unable to compile .NET SDK client for testing purposes because RestSharp assembly could not be found at '{0}'.",
                    restSharpAssemblyPath));
            }

            string cscPath = GetCscPath();
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = cscPath,
                Arguments = String.Format("/target:library /out:{0} /reference:{1} /reference:{2} /nologo /warn:0 /recurse:{3}\\*.cs",
                    tempSdkAssemblyPath,
                    newtonsoftJsonAssemblyPath,
                    restSharpAssemblyPath,
                    containingDirectory),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.ErrorDataReceived += (sender, args) => Console.WriteLine("Error: " + args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception(string.Format("csc.exe returned an exit code of '{0}'.", process.ExitCode));
        }

        public static string GetCscPath()
        {
            const string DefaultCscInstallFolder = "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\";

            // Try to read the path from the registry key
            string cscFolderPath = (string) Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full",
                "InstallPath",
                DefaultCscInstallFolder);

            string cscPath = Path.Combine(cscFolderPath, "csc.exe");

            if (!File.Exists(cscPath))
                throw new FileNotFoundException(
                    String.Format("Unable to compile .NET SDK client for testing purposes because C# compiler could not be found at '{0}'.", cscPath));

            return cscPath;
        }
    }
}
