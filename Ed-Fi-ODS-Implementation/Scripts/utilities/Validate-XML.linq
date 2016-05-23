<Query Kind="Statements">
  <Namespace>System.Xml.Schema</Namespace>
</Query>

string schemaFileName = @"C:\work\DoubleLine\TDOE\MyTdoeFork\TNExtensions\Schemas\TDOE-Interchange-Descriptors-Extension.xsd";
string dataFileName = @"c:\var\tmp\xml\EdFi_Ods_Minimal_Template.xml";

// Set the validation settings.
XmlReaderSettings settings = new XmlReaderSettings();
settings.ValidationType = ValidationType.Schema;
settings.Schemas.Add(null, schemaFileName);

settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

int errorCount = 0;

settings.ValidationEventHandler += new ValidationEventHandler((sender, args) =>
{
	errorCount++;
	
	var senderAsReader = sender as IXmlLineInfo;
	string location = " at line " + senderAsReader.LineNumber + ", pos " + senderAsReader.LinePosition;
	
     if (args.Severity==XmlSeverityType.Warning)
       args.Message.Dump("Warning: Matching schema not found.  No validation occurred." + location);
     else
	 {
        args.Message.Dump("Validation error" + location);
	 }
});

// Create the XmlReader object.
using (var reader = XmlReader.Create(dataFileName, settings))
{
	// Parse the file. 
	while (reader.Read()) ;
}

if (errorCount == 0)
	"Validation was successful.".Dump("Validation Completed");
else
	(errorCount + " validation error" + (errorCount == 1 ? "" : "s") + " occured.").Dump("Validation Completed");