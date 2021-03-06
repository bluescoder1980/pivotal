USE [TIC_Objects_SAM]
GO
/****** Object:  Table [dbo].[pre_contact_nbhd_profile]    Script Date: 09/30/2010 09:31:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[pre_contact_nbhd_profile](
	[CONTACT_PROFILE_NBHD_ID] [binary](8) NULL,
	[SOURCE_SYSTEM] [varchar](3) NULL,
	[NEIGHBORHOOD_LOOKUP] [varchar](40) NULL,
	[DIVISION_LOOKUP] [varchar](40) NULL,
	[TYPE] [varchar](40) NULL,
	[INACTIVE] [tinyint] NULL,
	[PROCESSED] [int] NULL,
	[CONTACT_ID] [binary](8) NULL,
	[BATCH_ID] [varchar](38) NULL,
	[SOURCE_TABLE] [varchar](20) NULL,
	[LEAD_ID] [binary](8) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF