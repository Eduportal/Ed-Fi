<Query Kind="Statements">
  <Namespace>System.Xml.Xsl</Namespace>
</Query>

string scriptDir = Path.GetDirectoryName(Util.CurrentQueryPath);
string xsltFile =  @"..\..\Extensions\Transformations\XSDEnumerationsToTypeInserts.xslt";
string sqlFile = @"..\..\Database\Structure\EdFi\3001-Type-inserts-generated-from-the-xsd.sql";
string schemaFileName = Path.Combine(scriptDir, @"..\..\Extensions\Schemas\Ed-Fi-Core.xsd");

var xslt = new XslCompiledTransform();
xslt.Load(xsltFile);

xslt.Transform(schemaFileName, sqlFile);

File.ReadAllText(sqlFile).Dump("Generated Content (to '" + sqlFile + "'.");
