<Query Kind="Program">
  <Connection>
    <ID>7e6a7ca2-d38a-4b55-b0c1-1258c4d50577</ID>
    <Server>(local)</Server>
    <Database>EdFi_Ods_Empty</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

// ===================================================================================================
// If you need to regenerate the descriptor design and data migration template, set this flag to true
// ---------------------------------------------------------------------------------------------------
static bool stopProcessingMigrationsForT4TemplateGeneration = false;
static bool stopProcessingExtensionMigrations = false;
static int stopAtScriptNumber = int.MaxValue;
// ===================================================================================================

static string basePath = Path.GetDirectoryName(Path.GetDirectoryName(Util.CurrentQueryPath));
static string metadataFilename = Path.GetDirectoryName(basePath) + @"\Ed-Fi-ODS\Application\Templates\DomainMetadata.xml";
static string outputFileName = basePath + @"\Database\Structure\EdFi\4000-Ed-Fi-Updates-for-REST-API.sql";
static string migrationsPath = basePath + @"\Database\Structure\EdFi\";

void Main()
{
	var migrationScripts = 
		from f in Directory.GetFiles(migrationsPath, "*.sql")
		let numberText = f.Substring(f.LastIndexOf("\\") + 1,4)
		where Regex.IsMatch(numberText, "[0-9]{4}")
		let number = int.Parse(numberText)
		where (stopProcessingMigrationsForT4TemplateGeneration && number >= 1000 && number < 1500) ||
			  (stopProcessingExtensionMigrations && ((number >= 1000 && number < 2000) || (number >= 3000 && number < 4000))) ||
			  (!stopProcessingMigrationsForT4TemplateGeneration && !stopProcessingExtensionMigrations && number >= 1000 && number < 4000)
		orderby f
		select new { FileName = f, ScriptNumber = number };
	
	//migrationScriptFileNames.Dump();

	var sb = new StringBuilder();
	
	foreach (var migrationScript in migrationScripts)
	{
		string migrationScriptFileName = migrationScript.FileName;
		
		// Execute migrations
		string migrationScriptSql = File.ReadAllText(migrationScriptFileName);
		string[] migrationScriptParts = migrationScriptSql.Split(new[]{"GO\r\n", "GO\n"}, StringSplitOptions.RemoveEmptyEntries);
				
		if (migrationScript.ScriptNumber >= stopAtScriptNumber)
		{
			("Script execution has been terminated at " + stopAtScriptNumber + " (based on value set in the 'stopAtScriptNumber' constant in this Linqpad script).")
				.Dump("Script Execution Terminated");
			
			return;
		}
		
		Console.WriteLine("Executing script '{0}'...", migrationScriptFileName);
		
		if (migrationScript.ScriptNumber == 1500)
		{
			"Running Descriptor migration script.  To regenerate this script, set the 'stopProcessingMigrationsForT4TemplateGeneration' in this Linqpad script to true, and re-run the 'Restore-ODS-Database' Linqpad script, followed by this one.  Then follow the instructions to regenerate the '1500' script."
				.Dump("NOTE");
		}
		
		foreach (string sql in migrationScriptParts)
		{
			try { ExecuteCommand(sql); }
			catch (Exception ex)
			{
				sql.Dump("Failed SQL Statement");
				throw;
			}
		}
	}
	
	if (stopProcessingMigrationsForT4TemplateGeneration)
	{
		"Script processing has been terminated so that the database is ready for the t4 template generation of 'DescriptorMigration.tt' from the Visual Studio solution.  Go run that template now, copy the generated contents to the '1500-Ed-Fi-Descriptor-design-and-data-migration.sql' script file, and then flip the 'stopProcessingMigrationsForT4TemplateGeneration' variable in this script back to false, and re-run the 'Restore-ODS-Database' Linqpad script, followed by this one."
			.Dump("Go Generate the t4 template in Visual Studio now!");
			
		return;
	}
	
	// Execute migrations
	/*
	int statementNumber = 1;
	foreach (string migrationSql in GetMigrationScripts())
	{
		Console.WriteLine("Executing statement number {0}...", statementNumber++);
		sb.AppendLine(migrationSql);
		ExecuteCommand(migrationSql);
	}
	*/
	
	var doc = GetDomainMetadata(metadataFilename);
		
	var nonDerivedAggregateRoots = 
		(from e in doc.Descendants("Aggregate")
		let root = e.Attribute("root").Value
		let rootEntity = e.Elements("Entity").Where(x => x.Attribute("table").Value == root)
		let schema = rootEntity.Attributes("schema").SingleOrDefault() == null ? "edfi" : rootEntity.Attributes("schema").SingleOrDefault().Value
		let isDerived = rootEntity.Attributes("isA").Any()
		where !isDerived && (!stopProcessingExtensionMigrations || schema == "edfi")
		orderby root
		select new { Root = root, Schema = schema }).ToList();
		
	var addGuidsScriptLines = 
		from x in nonDerivedAggregateRoots
		//where !((x.Root.EndsWith("Type") || x.Root.EndsWith("Descriptor")) && x.Schema == "extension") // Skip TN types/descriptor extensions, but do process new domain aggregates
		select string.Format(
	@"
	ALTER TABLE {1}.{0} ADD 
		Id uniqueidentifier NOT NULL CONSTRAINT {0}_DF_Id DEFAULT newid(),
		LastModifiedDate datetime NOT NULL CONSTRAINT {0}_DF_LastModifiedDate DEFAULT getdate();
	
	CREATE UNIQUE NONCLUSTERED INDEX GUID_{0} ON {1}.{0} (Id);
	", x.Root, x.Schema);
	
	string addGuidsScript = string.Join("\r\n", addGuidsScriptLines.ToArray());
	//addGuidsScript.Dump();
	"Executing script to add GUIDs to model...".Dump("Applying changes for REST API");
	sb.AppendLine(addGuidsScript);
	ExecuteCommand(addGuidsScript);
	
	var fks = ExecuteQuery<FK>(@"
	SELECT
        ChildTable  = FK.TABLE_NAME,
        ChildColumn = CU.COLUMN_NAME,
        ParentTable  = PK.TABLE_NAME,
        ParentColumn = PT.COLUMN_NAME, 
        Constraint_Name = C.CONSTRAINT_NAME,
        ChildOwner = FK.TABLE_SCHEMA,
		ParentOwner = PK.TABLE_SCHEMA,
		OrdinalPosition = CU.ORDINAL_POSITION
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
    INNER JOIN
        (	
            SELECT i1.TABLE_NAME, i2.COLUMN_NAME, i2.ORDINAL_POSITION
            FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
            WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
        ) 
    PT ON PT.TABLE_NAME = PK.TABLE_NAME AND PT.ORDINAL_POSITION = CU.ORDINAL_POSITION
	ORDER BY FK.TABLE_NAME, CU.CONSTRAINT_NAME, CU.ORDINAL_POSITION");
	
	var allTables =
		(from t in ExecuteQuery<SysTable>("select * from sys.tables")
		from e in doc.Descendants("Entity")
		let tableName = e.Attribute("table").Value
		let isDerived = e.Attributes("isA").Any()
		let schema = e.Attributes("schema").SingleOrDefault() == null ? "edfi" : e.Attributes("schema").SingleOrDefault().Value
		where t.Name == tableName && !isDerived		// Don't add CreateDate to derived tables.
		orderby tableName
		select new { TableName = tableName, Schema = schema })
		.ToList();
		
	var lastModifiedDateScriptLines =
		(from t in allTables
		//where !((t.TableName.EndsWith("Type") || t.TableName.EndsWith("Descriptor")) && t.Schema == "extension") // Skip TN types/descriptor extensions, but do process new domain aggregates
		select string.Format(@"
ALTER TABLE {1}.{0} ADD		
	CreateDate datetime NOT NULL CONSTRAINT {0}_DF_CreateDate DEFAULT getdate();", t.TableName, t.Schema));
	
	string lastModifiedDateScript = "BEGIN TRANSACTION;" + string.Join("\r\n", lastModifiedDateScriptLines.ToArray()) + "\r\nCOMMIT TRANSACTION;";
	//lastModifiedDateScript.Dump();
	sb.AppendLine(lastModifiedDateScript);
	ExecuteCommand(lastModifiedDateScript);

	File.WriteAllText(outputFileName, sb.ToString());
	("Script file written to '" + outputFileName + "'.").Dump("SUCCESS!");
}


XDocument GetDomainMetadata(string domainMetadata)
{
	var mainDoc = XDocument.Load(domainMetadata);
	var mainDocAggregates = mainDoc.Element("Aggregates");
	
	string metadataFolder = Path.GetDirectoryName(domainMetadata);
	
	var extensions = Directory.GetFiles(metadataFolder, "DomainMetadataExtensions*.xml");
	
	if (extensions.Length == 0)
		return mainDoc;
	
	if (extensions.Length > 1)
		throw new Exception(string.Format("Multiple metadata extensions files were found in '{0}' (only one was expected).", metadataFolder));
	
	var extensionDoc = XDocument.Load(extensions[0]);
	
	// Process aggregate extensions first
	var aggregateExtensions = extensionDoc.Descendants("AggregateExtension");
	
	foreach (var aggregateExtension in aggregateExtensions)
	{
		string rootTableName = (string)aggregateExtension.Attribute("root");
		
		// Find the corresponding Aggregate in the mainDoc
		var targetAggregate = mainDoc.Descendants("Aggregate").SingleOrDefault(e => ((string)e.Attribute("root")).Equals(rootTableName));
		
		if (targetAggregate == null)
			throw new Exception(string.Format("Unable to find target aggregate with root '{0}'.  Cannot apply aggregate extension metadata.", rootTableName));
		
		targetAggregate.Add(from e in aggregateExtension.Elements("Entity")
							select new XElement("Entity",
							new XAttribute("table", (string)e.Attribute("table")),
							new XAttribute("schema", "extension")));
	}
	
	// Now merge/replace the aggregates defined in the extension metadata
	var extensionAggregates = extensionDoc.Descendants("Aggregate");
	
	foreach (var extensionAggregate in extensionAggregates)
	{
		string rootTableName = (string)extensionAggregate.Attribute("root");
		
		var existingAggregate = mainDoc.Descendants("Aggregate").SingleOrDefault(e => e.Attribute("root").Equals(rootTableName));
		
		if (existingAggregate != null)
			existingAggregate.Remove();
		
		mainDocAggregates.Add(extensionAggregate);
	}
	
	return mainDoc;
}

// Names to discuss
// Actuals, Budgets ... see DomainMetadata.xml
// ContractedStaff --> ContractedStaffers ?? ContractedStaffMembers ??
// LEACategoryType --> LocalEducationAgencyCategoryType? (resource is "/lEACategoryTypes")
// LearningResourceMetadata --> LearningResourceMetadatum ?
// Staff --> StaffMembers??

// Other Tables to consider renaming
// AcademicHonorsType --> AcademicHonorType, and AcademicHonorsTypeId --> AcademicHonorTypeId

public class SysTable
{
	public string Name;
}

// Define other methods and classes here
public class FK
{
   public string ChildTable;
   public string ChildColumn;
   public string ParentTable;
   public string ParentColumn;
   public string Constraint_Name;
   public string ChildOwner;
   public string ParentOwner;
   public int OrdinalPosition;
}