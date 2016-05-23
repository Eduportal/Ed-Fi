using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using CsvFile;
using EdFi.Ods.CodeGen.XsdToWebApi;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

namespace EdFi.Ods.XsdToOdsMap
{
    class Program
    {
        private static CsvFileWriter csvWriter = null;

        static void Main(string[] args)
        {
            // Was a path specified?
            if (args.Length == 0)
            {
                ShowExitMessage("Usage error: No path argument specified\r\n\r\nExample: XsdToOdsMap.exe \"c:\\xsds\"");
                return;
            }

            string xsdPath = args[0];

            // Does the path exist?
            if (!Directory.Exists(xsdPath))
            {
                ShowExitMessage("Specified path does not exist: {0}", xsdPath);
                return;
            }

            string[] xsdFiles = Directory.GetFiles(xsdPath, "*.xsd");

            // Are there any .xsd files in the path?
            if (xsdFiles.Length == 0)
            {
                ShowExitMessage("Specified path does not contain any .xsd files");
                return;
            }

            Console.WriteLine("Processing the following XSDs:");
            string outputCSVFilePath;
            if (args.Length > 1)
                outputCSVFilePath = args[1];
            else
                outputCSVFilePath = ConfigurationManager.AppSettings["DefaultOutputFileName"];
            Console.WriteLine("Creating the output CSV: {0}", outputCSVFilePath);

            EdFi.Ods.CodeGen.XsdToWebApi.Process.Process.ConfigureForResources();

            try
            {
                csvWriter = new CsvFileWriter(outputCSVFilePath);
                csvWriter.WriteRow(OutputParsedSchemaObject.ResultHeader());

                var parsedSchemaObjects = new List<ParsedSchemaObject>();
                xsdFiles.ToList().ForEach(delegate(string file)
                {
                    Console.WriteLine(file);
                    parsedSchemaObjects.AddRange(InterchangeLoader.Load(file));
                });

                foreach (var parsedSchemaObject in parsedSchemaObjects)
                    WriteToCSV(parsedSchemaObject);

                csvWriter.Dispose();
                ShowExitMessage("Finished successfully!");
            }
            catch (Exception ex)
            {
                if (csvWriter != null)
                    csvWriter.Dispose();
                Console.WriteLine(ex);
                ShowExitMessage(ex.Message);
            }
        }

        private static void WriteToCSV(ParsedSchemaObject parsedSchemaObject)
        {
            foreach (var childElement in parsedSchemaObject.ChildElements)
            {
                if (IsAcceptableOutputType(childElement))
                {
                    foreach(var resultList in OutputParsedSchemaObject.ResultList(childElement))
                        csvWriter.WriteRow(resultList);
                    if (childElement.ProcessResult.ProcessChildren)
                        WriteToCSV(childElement);
                }
            }
        }

        private static void ShowExitMessage(string message, params object[] arg)
        {
            Console.WriteLine(message, arg);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();            
        }

        private static bool IsAcceptableOutputType(ParsedSchemaObject parsedSchemaObject)
        {
            return parsedSchemaObject.XmlSchemaType.TypeCode != XmlTypeCode.Id &&
                   parsedSchemaObject.XmlSchemaType.TypeCode != XmlTypeCode.Idref;
        }
    }
}
