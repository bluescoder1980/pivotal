USE [TIC_Objects_SAM]
GO
/****** Object:  Table [dbo].[pre_contact]    Script Date: 09/30/2010 09:30:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[pre_contact](
	[LINKED_ID] [binary](8) NULL,
	[SOURCE_SYSTEM] [varchar](3) NULL,
	[FIRST_NAME] [varchar](30) NULL,
	[LAST_NAME] [varchar](30) NULL,
	[TITLE] [varchar](9) NULL,
	[SUFFIX] [varchar](20) NULL,
	[ADDRESS_1] [varchar](50) NULL,
	[CITY] [varchar](40) NULL,
	[STATE_] [varchar](10) NULL,
	[ZIP] [varchar](12) NULL,
	[AREA_CODE] [varchar](10) NULL,
	[COUNTY_ID] [binary](8) NULL,
	[COUNTRY] [varchar](35) NULL,
	[PHONE] [varchar](25) NULL,
	[CELL] [varchar](25) NULL,
	[FAX] [varchar](25) NULL,
	[EMAIL] [varchar](100) NULL,
	[M1_UNSUBSCRIBE] [tinyint] NULL,
	[PROCESSED] [int] NULL,
	[drvBATCH_ID] [varchar](38) NULL,
	[drvSOURCE_TABLE] [varchar](7) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF