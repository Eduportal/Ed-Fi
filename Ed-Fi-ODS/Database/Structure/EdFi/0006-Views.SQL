SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaff_Assignment]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW auth.EducationOrganizationToStaff_Assignment
WITH SCHEMABINDING
AS
-- EdOrg to Staff (through assignment)
select	edorg.EducationOrganizationId, s.Id as StaffGuid, s.StaffUniqueId, assgn.BeginDate, assgn.EndDate, COUNT_BIG(*) as Count
from	edfi.EducationOrganization edorg
		inner join edfi.StaffEducationOrganizationAssignmentAssociation assgn
			on edorg.EducationOrganizationId = assgn.EducationOrganizationId
		inner join edfi.Staff s
			on assgn.StaffUSI = s.StaffUSI
group by s.Id, s.StaffUniqueId, edorg.EducationOrganizationId, BeginDate, EndDate

--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaff_Assignment]') AND name = N'UCIX_EducationOrganizationToStaff_Assignment')
CREATE UNIQUE CLUSTERED INDEX [UCIX_EducationOrganizationToStaff_Assignment] ON [auth].[EducationOrganizationToStaff_Assignment]
(
	[StaffGuid] ASC,
	[EducationOrganizationId] ASC,
	[StaffUniqueId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaff_Assignment]') AND name = N'IX_EducationOrganizationToStaff_Assignment_StaffUniqueId')
CREATE NONCLUSTERED INDEX [IX_EducationOrganizationToStaff_Assignment_StaffUniqueId] ON [auth].[EducationOrganizationToStaff_Assignment]
(
	[StaffUniqueId] ASC,
	[EducationOrganizationId] ASC,
	[StaffGuid] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaff_Employment]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW auth.EducationOrganizationToStaff_Employment
WITH SCHEMABINDING
AS
-- EdOrg to Staff (through Employment)
select	edorg.EducationOrganizationId, s.Id as StaffGuid, s.StaffUniqueId, empl.HireDate as BeginDate, empl.EndDate, COUNT_BIG(*) as Count
from	edfi.EducationOrganization edorg
		inner join edfi.StaffEducationOrganizationEmploymentAssociation empl
			on edorg.EducationOrganizationId = empl.EducationOrganizationId
		inner join edfi.Staff s
			on empl.StaffUSI = s.StaffUSI
group by s.Id, s.StaffUniqueId, edorg.EducationOrganizationId, HireDate, EndDate

--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaff_Employment]') AND name = N'UCIX_EducationOrganizationToStaff_Employment')
CREATE UNIQUE CLUSTERED INDEX [UCIX_EducationOrganizationToStaff_Employment] ON [auth].[EducationOrganizationToStaff_Employment]
(
	[StaffGuid] ASC,
	[EducationOrganizationId] ASC,
	[StaffUniqueId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaff_Employment]') AND name = N'IX_EducationOrganizationToStaff_Employment')
CREATE NONCLUSTERED INDEX [IX_EducationOrganizationToStaff_Employment] ON [auth].[EducationOrganizationToStaff_Employment]
(
	[StaffUniqueId] ASC,
	[EducationOrganizationId] ASC,
	[StaffGuid] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaffUSI_Assignment]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW auth.EducationOrganizationToStaffUSI_Assignment
WITH SCHEMABINDING
AS
-- EdOrg to Staff (through assignment)
select	edorg.EducationOrganizationId, assgn.StaffUSI, assgn.BeginDate, assgn.EndDate, COUNT_BIG(*) as Count
from	edfi.EducationOrganization edorg
		inner join edfi.StaffEducationOrganizationAssignmentAssociation assgn
			on edorg.EducationOrganizationId = assgn.EducationOrganizationId
group by assgn.StaffUSI, edorg.EducationOrganizationId, BeginDate, EndDate

--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaffUSI_Assignment]') AND name = N'UCIX_EducationOrganizationToStaffUSI_Assignment')
CREATE UNIQUE CLUSTERED INDEX [UCIX_EducationOrganizationToStaffUSI_Assignment] ON [auth].[EducationOrganizationToStaffUSI_Assignment]
(
	[StaffUSI] ASC,
	[EducationOrganizationId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaffUSI_Employment]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW auth.EducationOrganizationToStaffUSI_Employment
WITH SCHEMABINDING
AS
-- EdOrg to Staff (through Employment)
select	edorg.EducationOrganizationId, empl.StaffUSI, empl.HireDate as BeginDate, empl.EndDate, COUNT_BIG(*) as Count
from	edfi.EducationOrganization edorg
		inner join edfi.StaffEducationOrganizationEmploymentAssociation empl
			on edorg.EducationOrganizationId = empl.EducationOrganizationId
group by empl.StaffUSI, edorg.EducationOrganizationId, HireDate, EndDate

--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationToStaffUSI_Employment]') AND name = N'UCIX_EducationOrganizationToStaffUSI_Employment')
CREATE UNIQUE CLUSTERED INDEX [UCIX_EducationOrganizationToStaffUSI_Employment] ON [auth].[EducationOrganizationToStaffUSI_Employment]
(
	[StaffUSI] ASC,
	[EducationOrganizationId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgency]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.LocalEducationAgency
WITH SCHEMABINDING
AS
	select	Id, LocalEducationAgencyId 
	from	edfi.LocalEducationAgency lea
			inner join edfi.EducationOrganization edorg
				on edorg.EducationOrganizationId = lea.LocalEducationAgencyId
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgency]') AND name = N'UCIX_LocalEducationAgency')
CREATE UNIQUE CLUSTERED INDEX [UCIX_LocalEducationAgency] ON [auth].[LocalEducationAgency]
(
	[Id] ASC,
	[LocalEducationAgencyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgency]') AND name = N'IX_LocalEducationAgency_LocalEducationAgencyId')
CREATE NONCLUSTERED INDEX [IX_LocalEducationAgency_LocalEducationAgencyId] ON [auth].[LocalEducationAgency]
(
	[LocalEducationAgencyId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToParent]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.LocalEducationAgencyIdToParent
WITH SCHEMABINDING
AS
-- LEA to Parent Guid
select	sch.LocalEducationAgencyId, p.Id as ParentGuid, p.ParentUniqueId, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate, COUNT_BIG(*) as Count 
from	edfi.School sch
		inner join edfi.StudentSchoolAssociation ssa
			on sch.SchoolId = ssa.SchoolId
		inner join edfi.Student s
			on ssa.StudentUSI = s.StudentUSI
		inner join edfi.StudentParentAssociation spa
			on ssa.StudentUSI = spa.StudentUSI
		inner join edfi.Parent p
			on spa.ParentUSI = p.ParentUSI
group by p.Id, LocalEducationAgencyId, p.ParentUniqueId, EntryDate, ExitWithdrawDate
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToParent]') AND name = N'UCIX_LocalEducationAgencyIdToParent')
CREATE UNIQUE CLUSTERED INDEX [UCIX_LocalEducationAgencyIdToParent] ON [auth].[LocalEducationAgencyIdToParent]
(
	[ParentGuid] ASC,
	[LocalEducationAgencyId] ASC,
	[ParentUniqueId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToParent]') AND name = N'IX_LocalEducationAgencyIdToParent')
CREATE NONCLUSTERED INDEX [IX_LocalEducationAgencyIdToParent] ON [auth].[LocalEducationAgencyIdToParent]
(
	[ParentUniqueId] ASC,
	[LocalEducationAgencyId] ASC,
	[ParentGuid] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToParentUSI]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.LocalEducationAgencyIdToParentUSI
WITH SCHEMABINDING
AS
-- LEA to Parent USI
select	sch.LocalEducationAgencyId, spa.ParentUSI, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate, COUNT_BIG(*) as Count
from	edfi.School sch
		inner join edfi.StudentSchoolAssociation ssa
			on sch.SchoolId = ssa.SchoolId
		inner join edfi.Student s
			on ssa.StudentUSI = s.StudentUSI
		inner join edfi.StudentParentAssociation spa
			on ssa.StudentUSI = spa.StudentUSI
group by spa.ParentUSI, LocalEducationAgencyId, ssa.EntryDate, ssa.ExitWithdrawDate 
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToParentUSI]') AND name = N'UCIX_LocalEducationAgencyIdToParentUSI')
CREATE UNIQUE CLUSTERED INDEX [UCIX_LocalEducationAgencyIdToParentUSI] ON [auth].[LocalEducationAgencyIdToParentUSI]
(
	[ParentUSI] ASC,
	[LocalEducationAgencyId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToSchoolGuid]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.LocalEducationAgencyIdToSchoolGuid
WITH SCHEMABINDING
AS
-- LEA to School Guid (SchoolId could be included here, but requires a join and is less direct than the view below)
select	sch.LocalEducationAgencyId, edorg_sch.Id as SchoolGuid --, sch.SchoolId
from	edfi.School sch
		inner join edfi.EducationOrganization edorg_sch
			on sch.SchoolId = edorg_sch.EducationOrganizationId
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToSchoolGuid]') AND name = N'UCIX_LocalEducationAgencyIdToSchoolGuid')
CREATE UNIQUE CLUSTERED INDEX [UCIX_LocalEducationAgencyIdToSchoolGuid] ON [auth].[LocalEducationAgencyIdToSchoolGuid]
(
	[SchoolGuid] ASC,
	[LocalEducationAgencyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToSchoolId]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.LocalEducationAgencyIdToSchoolId
--WITH SCHEMABINDING
AS
-- LEA to School
select	LocalEducationAgencyId, SchoolId 
from	edfi.School sch
--' 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToStaff]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW auth.LocalEducationAgencyIdToStaff
AS
-- LEA to Staff (through employment)
select	emp.EducationOrganizationId as LocalEducationAgencyId, null as SchoolId, emp.StaffGuid, emp.StaffUniqueId, BeginDate, EndDate
from	edfi.LocalEducationAgency lea
		inner join auth.EducationOrganizationToStaff_Employment emp
			on lea.LocalEducationAgencyId = emp.EducationOrganizationId
UNION ALL
-- LEA to Staff (through assignment)
select	assgn.EducationOrganizationId as LocalEducationAgencyId, null as SchoolId, assgn.StaffGuid, assgn.StaffUniqueId, BeginDate, EndDate
from	edfi.LocalEducationAgency lea
		inner join auth.EducationOrganizationToStaff_Assignment assgn
			on lea.LocalEducationAgencyId = assgn.EducationOrganizationId
UNION ALL
-- School to Staff (through employment)
select	sch.LocalEducationAgencyId, emp.EducationOrganizationId as SchoolId, emp.StaffGuid, emp.StaffUniqueId, BeginDate, EndDate
from	edfi.School sch
		inner join auth.EducationOrganizationToStaff_Employment emp
			on sch.SchoolId = emp.EducationOrganizationId
UNION ALL
-- School to Staff (through assignment)
select	sch.LocalEducationAgencyId, assgn.EducationOrganizationId as SchoolId, assgn.StaffGuid, assgn.StaffUniqueId, BeginDate, EndDate
from	edfi.School sch
		inner join auth.EducationOrganizationToStaff_Assignment assgn
			on sch.SchoolId = assgn.EducationOrganizationId
--' 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToStaffUSI]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.LocalEducationAgencyIdToStaffUSI
AS
-- LEA to Staff (through employment)
select	emp.EducationOrganizationId as LocalEducationAgencyId, null as SchoolId, emp.StaffUSI, BeginDate, EndDate
from	edfi.LocalEducationAgency lea
		inner join auth.EducationOrganizationToStaffUSI_Employment emp
			on lea.LocalEducationAgencyId = emp.EducationOrganizationId
UNION ALL
-- LEA to Staff (through assignment)
select	assgn.EducationOrganizationId as LocalEducationAgencyId, null as SchoolId, assgn.StaffUSI, BeginDate, EndDate
from	edfi.LocalEducationAgency lea
		inner join auth.EducationOrganizationToStaffUSI_Assignment assgn
			on lea.LocalEducationAgencyId = assgn.EducationOrganizationId
UNION ALL
-- School to Staff (through employment)
select	sch.LocalEducationAgencyId, emp.EducationOrganizationId as SchoolId, emp.StaffUSI, BeginDate, EndDate
from	edfi.School sch
		inner join auth.EducationOrganizationToStaffUSI_Employment emp
			on sch.SchoolId = emp.EducationOrganizationId
UNION ALL
-- School to Staff (through assignment)
select	sch.LocalEducationAgencyId, assgn.EducationOrganizationId as SchoolId, assgn.StaffUSI, BeginDate, EndDate
from	edfi.School sch
		inner join auth.EducationOrganizationToStaffUSI_Assignment assgn
			on sch.SchoolId = assgn.EducationOrganizationId
--' 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToStudent]'))
EXEC dbo.sp_executesql @statement = N'

-- LEA to Student
CREATE VIEW [auth].[LocalEducationAgencyIdToStudent]
WITH SCHEMABINDING
AS
select    LocalEducationAgencyId, s.Id as StudentGuid, s.StudentUniqueId, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate, COUNT_BIG(*) as Ignored
from    edfi.School sch
        inner join edfi.StudentSchoolAssociation ssa
            on sch.SchoolId = ssa.SchoolId
        inner join edfi.Student s
            on ssa.StudentUSI = s.StudentUSI
group by LocalEducationAgencyId, s.Id, s.StudentUniqueId, ssa.EntryDate, ssa.ExitWithdrawDate
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToStudent]') AND name = N'UCIX_LocalEducationAgencyIdToStudent')
CREATE UNIQUE CLUSTERED INDEX [UCIX_LocalEducationAgencyIdToStudent] ON [auth].[LocalEducationAgencyIdToStudent]
(
	[StudentGuid] ASC,
	[LocalEducationAgencyId] ASC,
	[StudentUniqueId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToStudent]') AND name = N'UIX_LocalEducationAgencyIdToStudent_StudentUniqueId')
CREATE NONCLUSTERED INDEX [UIX_LocalEducationAgencyIdToStudent_StudentUniqueId] ON [auth].[LocalEducationAgencyIdToStudent]
(
	[StudentUniqueId] ASC,
	[LocalEducationAgencyId] ASC,
	[StudentGuid] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToStudentUSI]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.LocalEducationAgencyIdToStudentUSI
WITH SCHEMABINDING
AS
-- LEA to Student GUID
select sch.LocalEducationAgencyId, ssa.StudentUSI, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate, COUNT_BIG(*) as Ignored
from   edfi.School sch
              inner join edfi.StudentSchoolAssociation ssa
                     on sch.SchoolId = ssa.SchoolId
group by sch.LocalEducationAgencyId, ssa.StudentUSI, ssa.EntryDate, ssa.ExitWithdrawDate
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[LocalEducationAgencyIdToStudentUSI]') AND name = N'UCIX_LocalEducationAgencyToStudentUSI')
CREATE UNIQUE CLUSTERED INDEX [UCIX_LocalEducationAgencyToStudentUSI] ON [auth].[LocalEducationAgencyIdToStudentUSI]
(
	[StudentUSI] ASC,
	[LocalEducationAgencyId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[ParentToStudent]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW auth.ParentToStudent
WITH SCHEMABINDING
AS
select	s.Id as StudentGuid, s.StudentUniqueId, p.Id as ParentGuid, p.ParentUniqueId
from	edfi.Student s
		inner join edfi.StudentParentAssociation spa
			on s.StudentUSI = spa.StudentUSI
		inner join edfi.Parent p
			on spa.ParentUSI = p.ParentUSI
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[ParentToStudent]') AND name = N'UCIX_ParentToStudent')
CREATE UNIQUE CLUSTERED INDEX [UCIX_ParentToStudent] ON [auth].[ParentToStudent]
(
	[StudentUniqueId] ASC,
	[ParentUniqueId] ASC,
	[StudentGuid] ASC,
	[ParentGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[ParentToStudent]') AND name = N'IX_ParentToStudent')
CREATE NONCLUSTERED INDEX [IX_ParentToStudent] ON [auth].[ParentToStudent]
(
	[ParentUniqueId] ASC,
	[StudentUniqueId] ASC,
	[StudentGuid] ASC,
	[ParentGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[ParentUSIToStudentUSI]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.ParentUSIToStudentUSI
WITH SCHEMABINDING
AS
select	spa.StudentUSI, spa.ParentUSI
from	edfi.StudentParentAssociation spa

--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[ParentUSIToStudentUSI]') AND name = N'UCIX_ParentUSIToStudentUSI')
CREATE UNIQUE CLUSTERED INDEX [UCIX_ParentUSIToStudentUSI] ON [auth].[ParentUSIToStudentUSI]
(
	[StudentUSI] ASC,
	[ParentUSI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[ParentUSIToStudentUSI]') AND name = N'IX_ParentUSIToStudentUSI')
CREATE NONCLUSTERED INDEX [IX_ParentUSIToStudentUSI] ON [auth].[ParentUSIToStudentUSI]
(
	[ParentUSI] ASC,
	[StudentUSI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[School]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.School
WITH SCHEMABINDING
AS
	select	Id, SchoolId, LocalEducationAgencyId
	from	edfi.School sch
			inner join edfi.EducationOrganization edorg
				on edorg.EducationOrganizationId = sch.SchoolId
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[School]') AND name = N'UCIX_School')
CREATE UNIQUE CLUSTERED INDEX [UCIX_School] ON [auth].[School]
(
	[Id] ASC,
	[SchoolId] ASC,
	[LocalEducationAgencyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[School]') AND name = N'IX_School_LocalEducationAgencyId')
CREATE NONCLUSTERED INDEX [IX_School_LocalEducationAgencyId] ON [auth].[School]
(
	[LocalEducationAgencyId] ASC,
	[SchoolId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[School]') AND name = N'IX_School_SchoolId')
CREATE NONCLUSTERED INDEX [IX_School_SchoolId] ON [auth].[School]
(
	[SchoolId] ASC,
	[LocalEducationAgencyId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[SchoolIdToStaff]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.SchoolIdToStaff
AS
-- School to Staff (through employment)
select	sch.SchoolId, emp.StaffGuid, emp.StaffUniqueId, BeginDate, EndDate
from	edfi.School sch
		inner join auth.EducationOrganizationToStaff_Employment emp
			on sch.SchoolId = emp.EducationOrganizationId
UNION ALL
-- School to Staff (through assignment)
select	sch.SchoolId, assgn.StaffGuid, assgn.StaffUniqueId, BeginDate, EndDate
from	edfi.School sch
		inner join auth.EducationOrganizationToStaff_Assignment assgn
			on sch.SchoolId = assgn.EducationOrganizationId
--' 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[SchoolIdToStaffUSI]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.SchoolIdToStaffUSI
AS
-- School to Staff (through employment)
select	sch.SchoolId, seo_empl.StaffUSI, HireDate as BeginDate, EndDate
from	edfi.School sch
		inner join edfi.StaffEducationOrganizationEmploymentAssociation seo_empl
			on sch.SchoolId = seo_empl.EducationOrganizationId
UNION ALL
-- School to Staff (through assignment)
select	sch.SchoolId, seo_assgn.StaffUSI, BeginDate, EndDate
from	edfi.School sch
		inner join edfi.StaffEducationOrganizationAssignmentAssociation seo_assgn
			on sch.SchoolId = seo_assgn.EducationOrganizationId
--' 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[SchoolIdToStudent]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW auth.SchoolIdToStudent
WITH SCHEMABINDING
AS
-- LEA to Student
select	SchoolId, s.Id as StudentGuid, s.StudentUniqueId, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate
from	edfi.StudentSchoolAssociation ssa
		inner join edfi.Student s
			on ssa.StudentUSI = s.StudentUSI
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[SchoolIdToStudent]') AND name = N'UCIX_SchoolIdToStudent')
CREATE UNIQUE CLUSTERED INDEX [UCIX_SchoolIdToStudent] ON [auth].[SchoolIdToStudent]
(
	[StudentGuid] ASC,
	[SchoolId] ASC,
	[StudentUniqueId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[SchoolIdToStudent]') AND name = N'UIX_SchoolIdToStudent_StudentUniqueId')
CREATE NONCLUSTERED INDEX [UIX_SchoolIdToStudent_StudentUniqueId] ON [auth].[SchoolIdToStudent]
(
	[StudentUniqueId] ASC,
	[SchoolId] ASC,
	[StudentGuid] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[SchoolIdToStudentUSI]'))
EXEC dbo.sp_executesql @statement = N'

CREATE VIEW auth.SchoolIdToStudentUSI
WITH SCHEMABINDING
AS
-- LEA to Student GUID
select	ssa.SchoolId, ssa.StudentUSI, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate
from	edfi.StudentSchoolAssociation ssa
--' 
GO
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[auth].[SchoolIdToStudentUSI]') AND name = N'UCIX_SchoolIdToStudentUSI')
CREATE UNIQUE CLUSTERED INDEX [UCIX_SchoolIdToStudentUSI] ON [auth].[SchoolIdToStudentUSI]
(
	[StudentUSI] ASC,
	[SchoolId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW auth.EducationOrganizationIdToStaffUSI
AS
	SELECT SchoolId As EducationOrganizationId, StaffUSI, BeginDate, EndDate
		FROM auth.SchoolIdToStaffUSI
UNION ALL		
	SELECT LocalEducationAgencyId As EducationOrganizationId, StaffUSI, BeginDate, EndDate
		FROM auth.LocalEducationAgencyIdToStaffUSI
--
GO

CREATE VIEW auth.EducationOrganizationIdToStudentUSI
AS
	SELECT SchoolId As EducationOrganizationId, StudentUSI, BeginDate, EndDate
		FROM auth.SchoolIdToStudentUSI

UNION ALL 	
	SELECT LocalEducationAgencyId As EducationOrganizationId, StudentUSI, BeginDate, EndDate
		FROM auth.LocalEducationAgencyIdToStudentUSI
--
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationIdentifiers]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [auth].[EducationOrganizationIdentifiers]
AS
-- NOTE: Multiple results for a single Education Organization are possible if they are a part of multiple Education Organization Networks
SELECT	edorg.EducationOrganizationId,
		CASE 
			WHEN sea.StateEducationAgencyId IS NOT NULL THEN ''StateEducationAgency''
			WHEN esc.EducationServiceCenterId IS NOT NULL THEN ''EducationServiceCenter''
			WHEN lea.LocalEducationAgencyId IS NOT NULL THEN ''LocalEducationAgency''
			WHEN sch.SchoolId IS NOT NULL THEN ''School''
		END AS EducationOrganizationType,
		COALESCE(sea.StateEducationAgencyId, esc.StateEducationAgencyId, lea.StateEducationAgencyId, lea_sch.StateEducationAgencyId) AS StateEducationAgencyId, 
		COALESCE(esc.EducationServiceCenterId, lea.EducationServiceCenterId, lea_sch.EducationServiceCenterId) AS EducationServiceCenterId,
		COALESCE(lea.LocalEducationAgencyId, sch.LocalEducationAgencyId) AS LocalEducationAgencyId, 
		sch.SchoolId
FROM	edfi.EducationOrganization edorg
		LEFT JOIN edfi.StateEducationAgency sea
			ON edorg.EducationOrganizationId = sea.StateEducationAgencyId
		LEFT JOIN edfi.EducationServiceCenter esc
			ON edorg.EducationOrganizationId = esc.EducationServiceCenterId
		LEFT JOIN edfi.LocalEducationAgency lea
			ON edorg.EducationOrganizationId = lea.LocalEducationAgencyId
		LEFT JOIN edfi.School sch
			ON edorg.EducationOrganizationId = sch.SchoolId
		LEFT JOIN edfi.LocalEducationAgency lea_sch
			ON sch.LocalEducationAgencyId = lea_sch.LocalEducationAgencyId
WHERE	--Use same CASE as above to eliminate non-institutions (e.g. Networks)
		CASE 
			WHEN sea.StateEducationAgencyId IS NOT NULL THEN ''StateEducationAgency''
			WHEN esc.EducationServiceCenterId IS NOT NULL THEN ''EducationServiceCenter''
			WHEN lea.LocalEducationAgencyId IS NOT NULL THEN ''LocalEducationAgency''
			WHEN sch.SchoolId IS NOT NULL THEN ''School''
		END IS NOT NULL'
GO



IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationIdToSchoolId]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [auth].[EducationOrganizationIdToSchoolId]
AS
	-- School-level claims only can access the school
	SELECT	SchoolId, SchoolId as EducationOrganizationId
	FROM	edfi.School'
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationIdToLocalEducationAgencyId]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [auth].[EducationOrganizationIdToLocalEducationAgencyId]
AS
	-- Schools within the LEA are accessible to LEA-level claims
	SELECT	LocalEducationAgencyId, SchoolId AS EducationOrganizationId
	FROM	edfi.School
	UNION	
	-- LEA-level claims also can access the LEA
	SELECT	LocalEducationAgencyId, LocalEducationAgencyId AS EducationOrganizationId
	FROM	edfi.LocalEducationAgency'
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationIdToEducationServiceCenterId]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [auth].[EducationOrganizationIdToEducationServiceCenterId]
AS
	SELECT	DISTINCT EducationServiceCenterId, EdOrgId as EducationOrganizationId
	FROM
			(SELECT	* FROM auth.EducationOrganizationIdentifiers) As Source
	UNPIVOT (EdOrgId FOR IdType IN (LocalEducationAgencyId, SchoolId)) -- Only LEAs and Schools are accessible to ESC-level claims
			As UnpivotedSource
	UNION	
	-- ESC-level claims also can access the ESC
	SELECT	EducationServiceCenterId, EducationServiceCenterId AS EducationOrganizationId
	FROM	edfi.EducationServiceCenter'
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[auth].[EducationOrganizationIdToStateAgencyId]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [auth].[EducationOrganizationIdToStateAgencyId]
AS
	SELECT	DISTINCT StateEducationAgencyId, EdOrgId as EducationOrganizationId
	FROM
			(SELECT	* FROM auth.EducationOrganizationIdentifiers) As Source
	UNPIVOT (EdOrgId FOR IdType IN (EducationServiceCenterId, LocalEducationAgencyId, SchoolId)) -- Only ESCs, LEAs and Schools are accessible to State-level claims
			As UnpivotedSource
	UNION	
	-- State-level claims also can access the State
	SELECT	StateEducationAgencyId, StateEducationAgencyId AS EducationOrganizationId
	FROM	edfi.StateEducationAgency'
GO

CREATE VIEW [auth].[ParentToSchoolId]
WITH SCHEMABINDING
AS
-- School to Parent
select	ssa.SchoolId, p.Id as ParentGuid, p.ParentUniqueId, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate, COUNT_BIG(*) as Count 
from	edfi.StudentSchoolAssociation ssa
		inner join edfi.Student s
			on ssa.StudentUSI = s.StudentUSI
		inner join edfi.StudentParentAssociation spa
			on ssa.StudentUSI = spa.StudentUSI
		inner join edfi.Parent p
			on spa.ParentUSI = p.ParentUSI
group by p.Id, SchoolId, p.ParentUniqueId, EntryDate, ExitWithdrawDate
--
GO

CREATE UNIQUE CLUSTERED INDEX [UCIX_ParentToSchoolId] ON [auth].[ParentToSchoolId]
(
	[ParentGuid] ASC,
	[SchoolId] ASC,
	[ParentUniqueId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_ParentToSchoolId] ON [auth].[ParentToSchoolId]
(
	[ParentUniqueId] ASC,
	[SchoolId] ASC,
	[ParentGuid] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)
GO

CREATE SYNONYM [auth].[ParentGuidToSchoolIdId] FOR [auth].[ParentToSchoolId]
GO

CREATE SYNONYM [auth].[ParentUniqueIdToSchoolIdId] FOR [auth].[ParentToSchoolId]
GO

CREATE VIEW [auth].[ParentUSIToSchoolId]
WITH SCHEMABINDING
AS
-- School to Parent USI
select	ssa.SchoolId, spa.ParentUSI, ssa.EntryDate as BeginDate, ssa.ExitWithdrawDate as EndDate, COUNT_BIG(*) as Count
from	edfi.StudentSchoolAssociation ssa
		inner join edfi.Student s
			on ssa.StudentUSI = s.StudentUSI
		inner join edfi.StudentParentAssociation spa
			on ssa.StudentUSI = spa.StudentUSI
group by spa.ParentUSI, SchoolId, ssa.EntryDate, ssa.ExitWithdrawDate 
--
GO

CREATE UNIQUE CLUSTERED INDEX [UCIX_ParentUSIToSchoolId] ON [auth].[ParentUSIToSchoolId]
(
	[ParentUSI] ASC,
	[SchoolId] ASC,
	[BeginDate] ASC,
	[EndDate] ASC
)
GO
