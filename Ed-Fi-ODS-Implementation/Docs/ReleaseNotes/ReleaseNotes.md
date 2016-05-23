Release Tag: 2014_07_18
==
* Fixed and re-enabled all acceptance tests ignored due to security implemenation except for the All Resource tests and a single bulk operation end-to-end that will need to be evaluated further.
* Adding Bulk Exception logging to Bulk Load controller errors also. Previously only BulkLoadMaster errors were logged in exception log.
* Change all web api controller actions to return IHttpActionResults for better consistency with web api 2.0
* Introduced IDbDictionary dependency to XmlFileReader
* Added InMemoryDbDictionary as a simple name value dictionary implementation
* Simplified NodeSearch interface to take only an ID and not entities to search in, since ID is unique
* Disabled nhibernate logging from bulkload console
* Added file appender to log4net for bulkload console
* Modified next step referencing in xmlGPS to consider namespace along with typename
* Refactored GetAll Response Tests to use Courses and EducationOrganizationNetwork resources since these are available to every application
* Fixed registration bug for IEventDbContext - was not properly registered
* Change to return Forbidden (403) when requesting unauthorized resources
* Split out GetAll from GetByKey to ensure proper routing
* Added new validator attribute for datetime types to ensure value is within SQL datetime range (January 1, 1753, through December 31, 9999)
* Unify exception handling of bulk upload and web api methods to allow for more consistent reporting of errors
* Added full unit test suite for all actions within controller base


Release Tag: 2014_07_03
==
* Add full acceptance test suite to test authorization scenarios
* Added ability to migrate deployed staging sandbox admin database.
* Added ability to migrate deployed production sandbox admin database.
* Fixed URL generation in controllers for Location header in response to properly include virtual directory, if present
* Optimized use of the log4net LogManager class in the controllers each controller stores the logger into a static field after the first invocation
* Added support for ICacheProvider, and two implementations (AspNetCacheProvider and MemoryCacheProvider)
* Added support for injecting the School Year context into the location headers returned by the controllers after a successful creation.
* Fixed a problem in the authorization context data provider code generation that allows default values (i.e. 0's) introduced by the ASP.NET WebApi model binding process to be copied into the authorization context data rather than converting them to nulls.
* Improved exception messages for currently unsupported relationship-based authorizations for Education Organization Networks and Education Service Centers.
* Modified authorization layer to allow Courses to be read by everyone.
* Added support for authorization GetModelsBySpecification
* Fixed validator extension method to capture exceptions thrown, and turn them into validation results rather than allowing exceptions to cascade out of the API as generalized 500 errors.
* Added authorization support for GetEntitiesByIds...
* Removed direct use of IRepository interface, replacing with specific operation-based interfaces so that authorization is performed correctly.  IRepository should be retired as it represents a hole in authorization.
* Configured web.config to use SimpleMembershipProvider from WebMatrix explicitly.
* Removed Admin site web.config section for performing config file encryption.
* Added support for converting authorizations against "EducationOrganizationId" be performed against the concrete version of the identifier (e.g. LocalEducationAgencyId, SchoolId, etc.) by using an in-memory cache of the full Education Organization data from the ODS (with a default absolute expiration of 30 minutes, controlled by a config file entry).
* Added support for GetByKey pipeline.
* Remove cryptography from connection strings of Admin website.
* Modified Create/Update entity decorators to "clarify" exact action (i.e. overwriting authorization context action with 'Create' or 'Update' actions as opposed to 'Upsert' to provide needed clarity for the authorization decision).
* Integrated 'Establish Claims' branch for issuing claims for the ClaimsPricipal based on OAuth token details.
* Integrated authorization decorators for invoking appropriate authorization decisions on calls to repositories.
* Integrated authorization subsystem for making authorization decisions based on claims issued.
* Added Reset-GeneratedArtifacts script command to automatically re-execute all t4 templates.
* Refactored EdFi.Ods.Common and EdFi.BulkLoad.Core into different assemblies to facilitate code generation.
* Removed preprocessor directives based error details output from Rest web service message provider. This feature was useful when error logging was not available, and so developers would need to see what really went wrong. Now, with error logging being integrated, developers debugging the application will have the same information available through log4net targets.
* Added _log member of type ILog to TestBase, so that it is available to all tests which want to see if their code is logging properly.
* Added call to log4net logging methods in BulkLoad File Exception Handler. Other exception handlers can follow suite when they are created.
* Modified unit tests for the above.
* Included IOC registration for log4net in edfi ods common.
* Bug 73379840 Fix: altered use of Entity Framework DbContext to isolate context to each specific stored procedure.  Searches will result in many simultaneous connections to the EduId database.
* Fix a defect that was resulting in a 500 internal server error for bulk uploads when xml does not contain required values
* Added support for correctly saving, updating and deleting resources that are concrete base classes (e.g. StudentProgramAssociation and its derivatives such as StudentTitleIAProgramAssociation)
* **IMPORTANT:** Must run 4003 sql script against existing databases.
* Introduced correct "Not Found" handling for the GetById and GetByKey pipeline stepsscenarios
* Implemented controller refactoring to an EdFiController base class.
* Added a try/catch during JSON deserialization to report back the underlying content to assist with troubleshooting (in case of an HTML response).
* Added new custom EdFiSecurityException class for authorization failures and deprecated all usage of .NET's SecurityException class
* Added new section in Ed-Fi database scripts for features that explicitly support data warehouse features
* Added t4 template for creating script containing triggers that set the create/update dates using the server-side ``getutcdate()`` function.
* Modified the creation script for the DeletedEvent table to add a column to capture the "Schema" of the table whose event is being captured.
* Added t4 template for creating script containing triggers that insert records into the dbo.DeletedEvent table of the ODS appropriately.
* Incorporated ODS connection string hash code into cache key used for quickly looking up Education Organization details
* Added establishing claims functionality in OAuth Attribute
* Added support for Ed-Fi extension tables (where columns are added by creating a matching table in the 'extension' schema, with a table name suffix of "Extension").
* Update deployment instructions to reflect new Admin database structure
* Added order by clause to GetByEntities so that limit and offset work correctly
* Fixed multiple small issues with the scripts run by the reset-minimaltemplatedatabase scripts
* Added new TN Education Organization files and altered scripts to run new files
* New EdOrg XML.  This one all fits into one file.
* Replace Azure Emulator with SQL Storage using Entity Framework
* Refactored EventStore project to move its Entities and DBContext to appropriate places
* Added new script to add migrations to RestApi database
* When creating a new sandbox, give the client permissions to all the LEAs that are in the sandbox
* Change the sandbox staging services to use a new service bus namespace to avoid collisions with other deployed workers
* __IMPORTANT FOR VENDOR ENGAGEMENT__
  * Subtle changes to quotation mark character-code usage to the following Types (based on Ed-Fi Core schema changes):
    * DisciplineActionLengthDifferenceReasonType
    * EducationalEnvironmentType
  * Added US territories to StateAbbreviationType
* Added registrations for IEnvelopeVisitor to Security components installer
* Added support to specify certain array/collection item types that can be supplied as dependencies as an empty array/collection
* Added support for serializing and deserializing UTC times in JSON, matching format supported in XML.


Release Tag: 2014_06_04c
==
* Added ability to migrate deployed staging sandbox admin database.
* Added ability to migrate deployed production sandbox admin database.
* Update deployment instructions to reflect new Admin database structure
* Fixed multiple small issues with the scripts run by the reset-minimaltemplatedatabase scripts
* Added new TN Education Organization files and altered scripts to run new files
* Removed multiple TN Education Organization files in favor of a smaller file (this undoes the previous change)
* Change the sandbox staging services to use a new service bus namespace to avoid collisions with other deployed workers

Release Tag: 2014_06_02
==
* Bug fix: Fixed api to return 400 code and validation message upon validation error
* Bug fix: Fixed api to return 404 code when attemtping to get person resource by unique id that does not exist
* Bug fix for Discipline Incident requiring Staff Usi even though its optional
* Bug fix: for Bug 71709016 : Interchange controllers held state between file execution (even across operations) and this was manifesting in a couple issues: 1)Operations took more and more time because and memory because of the state, 2)All the exceptions in memory were being logged against each operation.
* Bug fix: an error which did not allow generated SDK to compile.
* Bug fix: Bulk Operation Status too slow - Removed file chunks from bulk operation resource to reduce payload when requesting status
* Bug fix: Keep Azure message locks from timing out while we are processing the message.
* Bug fix: Fixing of bug in Entity Mapper to cater for null string
* Bug fix: issue with test object causing intermittent failure in unit test
* Bug fix: Modified to return an error when bulk load does not encounter an exception, but does not load any resources
* Bug fix: (FERPA) Remove XML files from the working local storage after the data has been loaded.  The XML chunks will remain in the database, but this clears up an issue that would allow XML data to rest on the HDD unencrypted.
* Bug fix: Mulitple fixes for swagger, including improved json generation, new metadata, including school year in route, camelCase, and escaping keyword for ExceptionsLog
* Bug fix: Weight item was being dropped during mapping from one Identity type to another. Fixed this bug.
* Bug fix: Fixed bug where decimal types with zero value were failing validation for attribute RequiredWithNonDefault. 0 is a valid non-default for decimals.
* Added new interchange controller for educational agency calendar
* Converted Identifiers from Generic to Guid
* Decompose Repository into method based interfaces
* Added Base security infrastructure to enforce access rights once authorization components are added
* Added base claims and helpers to create a claim.
* Created and added the EdFi abstractions around connection string providers.
* Added reference implementation of database connection string in EdFi.Ods.WebApi.
* Fim Synch views are changed to the records modifed within the last 24 hours. 
* A new view has been created to return unfiltered records from the StateEducationAgencyUserIdentity table.
* Created IHaveInterchangeSpecificLoaderCollections and implemented tied to Xsd files in the XsdToLoad folder.  This should result in the collections being updated as new Xsd files are added.
* Added the EdFi.Common project with base interfaces and standards.
* Introduced the infrastructure for IoC.
* Converted Admin from EF 5 to 6
* Converted Admin from auto-migration to manual
* Added PS check in solutionscripts to correctly set location of msbuid based on installed VS version
* Addition of new step in Initialize Dev Env to import Minimal Template Ed Orgs XML
* Addition of Unit/Integration tests for Bulk Load of Minimal Template
* Cleaned up the classes around bulk loading to make things more easily testable -- and added some unit tests around many of the changes.
* Changed Route of Upload Controller to move path elements to querystring
* Populate the SchoolYearType table with the values contained in the XSD
* Set the current school year to 2015
* Modified API route to include school year before controller - with tests
* Changed BulkOperation status to string from int.	
* Changed UploadFileStatus to string from int.
* Story 70323710: Add validation to ensure Unique Id matches Id on Person (Staff/Student/Parent) for "put" verb
* Story 70323710: Modified schema to include UniqueId on Staff/Student/Parent tables
* Updated Ed Org XML with additional course information

Release Tag: 2014_05_12
==
* Created IPersistUploadFiles interface and implementations for localonly and azureonly.  AzureOnly uses LocalStorage and saves files to the root folder only.
* Altered ISourceFiles to IValidateAndSourceFiles and moved validation under the implementation.  Now returns an IUploadFileSouringResult that has a collection of (string) error messages if errors occur and a (string) path if the file was successfully created.
* Altered BulkLoadMaster to check for error messages and log them if present.
* Added Base Test class affordance to run integration tests covering BulkLoadMaster (handles StartOperationCommand) down - i.e. the Bulk Load absent the website and messaging.
* Added test of initial Skyward XML - fails due to xml error
* Fixed bug in EdOrgInterchangeController - was attempting to load non-aggregate.
* Added local queue implementation to be used for acceptance testing
* Local, thread-based workers for acceptance testing bulk uploads via HTTP
* Acceptance tests for uploading xml via HTTP
* Services to support HTTP bulk load
* Azure services to support HTTP bulk load
* Current version has xml bulk loads working end-to-end in an Azure environment, and in the local development environment
* removed bad implementation of DeleteListener - added story in backlog to create valid implementation based on the LEA Security work
* Story 69701314: Clean up Identity interface to fully represent Identity as the world viewable abstraction (relegates Person to an internal name in EduId)
* Story 70132772: Added validation for Identity requests to check name, gender, and birthdate values using FluentValidation
* Interchange names are not case sensitive anymore.
* Enhanced The Bulk Operation validation:  Now the interchange names validation occurs on posting a Bulk Operation request.
* Modified SwaggerController to use combined reflection and custom attributes to generate json
* Included static json files, based on generated ones, but with possibility of manual customizations
* Bug fix: Fixed api to return 400 code and validation message upon validation error
* Bug fix: Fixed api to return 404 code when attemtping to get person resource by unique id that does not exist
* Changed BulkOperation status to string from int.	
* Changed UploadFileStatus to string from int.
* Story 70323710: Add validation to ensure Unique Id matches Id on Person (Staff/Student/Parent) for "put" verb
* Story 70323710: Modified schema to include UniqueId on Staff/Student/Parent tables
* Weight item was being dropped during mapping from one Identity type to another. Fixed this bug.


Release Tag: 2014_05_05
==
* Created IPersistUploadFiles interface and implementations for localonly and azureonly.  AzureOnly uses LocalStorage and saves files to the root folder only.
* Altered ISourceFiles to IValidateAndSourceFiles and moved validation under the implementation.  Now returns an IUploadFileSouringResult that has a collection of (string) error messages if errors occur and a (string) path if the file was successfully created.
* Altered BulkLoadMaster to check for error messages and log them if present.
* Added Base Test class affordance to run integration tests covering BulkLoadMaster (handles StartOperationCommand) down - i.e. the Bulk Load absent the website and messaging.
* Added test of initial Skyward XML - fails due to xml error
* Fixed bug in EdOrgInterchangeController - was attempting to load non-aggregate.
* Added local queue implementation to be used for acceptance testing
* Local, thread-based workers for acceptance testing bulk uploads via HTTP
* Acceptance tests for uploading xml via HTTP
* Services to support HTTP bulk load
* Azure services to support HTTP bulk load
* Current version has xml bulk loads working end-to-end in an Azure environment, and in the local development environment
* removed bad implementation of DeleteListener - added story in backlog to create valid implementation based on the LEA Security work


Release Tag: 2014_04_28
==
* Added new interchange controller for educational agency calendar, master schedule, student enrollment and staff
* Added StudentAttendence, StudentGrade, StudentProgram, StudentGradebook, and StudentCohort controllers.  Updated the bootstrapper to load them.
* Task #1342823 : Created Covering tests for GrandBend samples of StudentEnrollment, StudentProgram, and StudentDiscipline
* Fixed bug in code gen that fails to handle all "School Year" type elements (these are in a XXXX-XXXX format but must be converted to short using last 4 characters).  This conversion is now keyed off the ProcessResult.ProcessingRuleName == "School Year" which will be true for any element of this type.
* Task #1343132: Converts existing collections behavior into T4 generated object.
* Task #1344508 : Gender enum belonged to UniqueIdentity interface, but was not moved there in the original pull request for UniqueIdentity, so created a new one.
* Fixed bug where we could not GetAll for GradeLevelDescriptor
* Task #1347975 : Interchange Controllers building load more than once - causing exceptions fixed.
* Task #1347973 : Added remaining controllers:
	* Assessment Metadata
	* Student Assessment
	* Student Discipline
	* Student Intervention
	* Post Secondary Event
	* Student Attendance
	* Student Transcript
	* Finance
* Task #1341970 : Solves for skipping top level xsd objects that should be skipped (e.g. SectionReference in StudentGradebook)
* Refactored t4 code gen templates to make use of mvc pattern to simplify future changes
* Modified rest api to make use of unique identifier instead of usis and to allow for correct internal translations
* Changed schema definitions to support unique identifier


Release Tag: 2014_04_21
==
* Added new interchange controller for educational agency calendar
* Added MasterSchedule, StaffAssociation, And StudentEnrollment interchange controllers
* Bell Schedule Refactor
 * Changed edfi.BellSchedule structure and added edfi.BellScheduleMeetingTime 
 * Had to update the NHibernate Driver to NHibernate.Driver.Sql2008ClientDriver due to Time column in the primary key of edfi.BellScheduleMeetingTime
 * changed BellSchedule / GradeLevel to remove unbounded collection to match model
 * rename BellSchedule / EducationOrganizationReference to SchoolRefence to match model
 * changed BellSchedule / CalendarDateReference to remove unbounded collection to match model
 * Updated T4 generated objects as a result
* Added more descriptive message to exception thrown in BulkLoadExtensionMethods.GetAndRemoveLoaderFor when key not found
* Removed Ed-Fi branding from Swagger
* Generated Factories and AggregateLoaders for the following:
 * MasterSchedule
 * EducationOrganizationCalendar
 * StudentParent
 * StaffAssociation
 * StudentCohort
 * StudentDiscipline
 * StudentAttendance
 * StudentEnrollment
 * StudentGradebook
 * StudentGrade
 * StudentProgram
* task #1334081 : LearningObjective appears in more than one interchange - should only produce one factory and AggregateLoader
* In this release, EduId Interface has been separated out into a separate generic interface. All linkages between EduId.WebAPI to any other EduId project have been removed. Instead, the WebAPI only references the interface class library called EdFi.Identity, and then EduId projects implement that interface. This implementation can be swapped by a different one by hooking up EdFi.Identity.IUniqueIdentity to a different implementation in CastleWindsor.
* Additional changes are adding azure connection strings, modifying some tests, and moving test projects into Test folder.
* Task #1328097: Created IHaveInterchangeSpecificLoaderCollections and implemented tied to Xsd files in the XsdToLoad folder.  This should result in the collections being updated as new Xsd files are added.
* Fixed StaffAssociation Test Xml - was using the wrong file
* Task #1341984 : Fixed FK parser issue when multiple elements of the same type are present in different parts of the Xml (e.g. SchoolId in StaffSectionAssociation appears 3 times)
* Created MasterSchedule CoveringTests against EdFi GrandBend sample xml - passing
* Moved last conversion routines out of CodeGenEngine into extension methods based on the property type - this does incur a little DRY-ish violation but can/should be cleaned up later as part of refactoring to a pure Model/View behavior
* task #1334880 : Could not handle collections whose values could be sourced from multiple element types in the xml (i.e. IdentificationDocuments in three places in Student)
* Task #1333961 : fixed code gen not recognizing nested elements (i.e. <student><name><firstname>Bob</firstname> . . . )
* Fixed minor bugs in XElement Extensions that would cause Null Object Ref exceptions
* Added and fixed StaffAssociation XML from EdFi Sample (namespace was still 0120)
* Task #1339656: Added covering test for StaffAssociation Resource Factory - passing
* Adds StudentParent Interchange controller to load StudentParent Xml files.


Release Tag: UniqueIdInterface (2014.04.14)
==
* 67916896 Expand Size of ShortDescription columns on Type Tables
* Type tables are the tables in the database that are suffixed by "Type"
* Added logic to the 3000 script that changes the length of the ShortDescription field in every Type table and also makes the ShortDescription field Not Null.
* Allow for different types of interchanges to be loaded in single run of bulk load console based on interchange manifest
* Implemented SectionAttendanceTakenEvent; removed EffectiveDate from StudentSchoolAssociation;
In the Descriptor Excel Data load file, added entries for StudentCharacteristicDescriptor and SectionCharacteristicDescriptor
* Fixes Bug #1327208: IsExtendedReference should not return objects that have a ProcessResult of Skip
* Fixes Bug #1327209 : ForeignKeyProperty is returning the wrong Type
* Fixes Bug #1326613 : XPath Map Builder Only follows EdOrg Refs to LEA, ESC, and School
* Added detailed exception handling for bulk load operations and exposed an API to access these exceptions
* Created EducationOrganization Interchange Controller to sequence loading of aggregates to avoid dependency issues (i.e. db foreign key errors)
* Updated Console IoC Bootstrapper to include Standards and EdOrg controllers
* task #1328725 : fixed 'does not recognize non-Extended Reference foreign key relationships' bug
* task #1328725 : fixed 'exception thrown if ProcessResult and/or Expected Rest Object are null' bug
* task #1328607: Enhanced ParsedObjectExtensions to recognize and support Extended Descriptor References and Extended Reference Collections correctly
* task #1328607: Created test proving defect and fixed EntityMetadataManager to correctly return Extended Reference Collections as Entity Collections
* task #1328612 : created tests showing defect and fixed XPathMapBuilder to build maps for Extended Descriptor References.
* task #1328608: created tests showing defect and fixed XPathMapBuilder to build maps for an Identity Object.
* Added initial Messaging Infrastructure for Azure Service Bus
* Added Support for Sending Commands (necessary for upload controller's commit method and Bulk Operation File Validator's StartOperation behavior)
* Added Support for Receiving Commands (necessary for Bulk Load Processing)
* Extracted a base controller for interchange controllers to call attention to the order in which the aggregates are loaded within an interchange
* Stopped displaying the connection string when running the spreadsheet and bulk loaders.
* Apply proper loading sequence to self referenced aggregates 
* Story 67887226 - Creates a synchronous, one interchange at a time, sequenced master controller that will load all supplied interchanges (if controllers exist)
* Story 67887226 - Created method in IoC registration class for the console app that allows Interchange Controllers to be added as they are developed.
* Story 66298302 - Created Handler for CommitUpload Command that will verify the upload file state and set the appropriate status on the file and operation (if operation can be started it will send the StartOperation Command)
* Story 66298302 - Altered Master controller to update both file and operation status based on the results of executing the operation.  This should result in all status being appropriately reflected in the resources except for Expired (still need to solve)
* Refactored ResourceFactory Code Generation to be more maintainable
* Added unit tests for refactored behaviors
* Added ability to handle foreign key (aggregate) references in xsd and xml
* Refactored ClientAuthorization classes to use UTC DateTime when determining expiration.  This does not preclude errors related to DateTime from occurring but minimizes their likelihood.
* Updates to bulk load console app and initialization scripts to load database descriptor info from XML
Upgraded Coypu to latest version in Nuget.  This was required for Coypu to work with the current version of Firefox (28)
* task #1328607 : fixed 'Foreign Key object collections not properly recognized' bug
* task #1328700 : fixed 'Descriptor Foreign Keys not being generated' bug
* task #1328612 : fixed 'map to Descriptor Foreign Keys does not find Xml Elements in valid Xml file' bug
* task #1328696 : fixed 'Descriptor Foreign Key Xsd is being ignored' bug
* task #1328608 : fixed 'map to elements contained only in an Identity (i.e. as part of a Foreign Key object collection) are not being generated' bug

**Development Environment Prerequisites required for this release**

* This release includes a new Visual Studio Database project, for which SQL Server Data Tools for Visual Studio 2012 need to be installed. Please follow this link to install it: [SQL Server Data Tools for Visual Studio 2012](http://go.microsoft.com/fwlink/?LinkID=239644&clcid=0x409)


Release Tag:  BulkUploadApi (03.24.2014)
==
This release includes the following functionality:

* Developed an api to support posting a Bulk Operation
* Added API controllers to create and upload bulk operation files
* Added file builder to assemble upload file chunks and create file locally
* Created new Rest API DB and added PS scripts to auto build DB for deployments
* Fixed bugs in AggregateLoaderBase and DescriptorsController that could result in defects or anomalous behavior.  New code has additional unit tests to verify correctness
* Correct the spelling of HomelessPrimaryNightResidenceDescriptor to match the REST API (HomelessResidenceDescriptor)
* Moved bulk operations and file uploads to new Rest API DB built using "Code first" entity framework migrations
* Add powershell script enhancements to allow sandbox templates to be uploaded directly to the production SQL Azure server.  The old method involved pushing the templates to the development server then a painful Azure backup and restore process to move the templates from the development server to the production server.
* Added swagger attributes to UploadsController. (Known Issues: Swagger UI doesn't show them properly. Try the raw view)
* Change the support link from http://www.ed-fi.org/licensing/ to http://www.ed-fi.org/license/
* Upgraded VS projects to reference Azure tools 2.2
* Created api/uploads/{id}/chunk/{offset:long}/{size:long} POST behavior - allows chunks of data to be uploaded to the api for processing.  Guards against chunks being sent for upload files that have been processed or have expired.  Also guards against chunks that would exceed the expected total file size.
* Created api/uploads/{id}/commit POST behavior - signals the api to create a commitUpload message for a given file.  Guards against message being sent for upload files that have been processed or have expired.


Release Tag:  XML-Descriptors
==
This release includes the following functionality:

* XML: Export Descriptor information from an ODS database into EdFi XML vNext format
* XML: Import Descriptors from EdFi XML vNext via a console application.  This includes establishing the core architecture and framework for processing XML data.
* REST API: Multiple bug fixes related to HTTP response code in the 
* REST API: Response Code regression testing
* REST API: Complete semantics for asynchronous behavior
