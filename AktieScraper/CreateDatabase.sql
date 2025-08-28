USE [Stocks]
GO

/****** Object:  Table [dbo].[Stocks]    Script Date: 21/08/2025 21.48.26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Stocks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StockCode] [nvarchar](20) NOT NULL,
	[FriendlyName] [nvarchar](255) NULL,
	[Currency] [nvarchar](10) NULL,
	[ExchangeName] [nvarchar](100) NULL,
	[CreatedDate] [datetime2](7) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[NumberOfStocksOwned] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[StockCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Stocks] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[Stocks] ADD  DEFAULT (getutcdate()) FOR [UpdatedDate]
GO



/****** Object:  Table [dbo].[StockValues]    Script Date: 21/08/2025 21.46.54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StockValues](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[StockId] [int] NOT NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Open] [decimal](18, 6) NULL,
	[High] [decimal](18, 6) NULL,
	[Low] [decimal](18, 6) NULL,
	[Close] [decimal](18, 6) NULL,
	[Volume] [bigint] NULL,
	[CreatedDate] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[StockValues] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[StockValues]  WITH CHECK ADD  CONSTRAINT [FK_StockValues_Stocks] FOREIGN KEY([StockId])
REFERENCES [dbo].[Stocks] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[StockValues] CHECK CONSTRAINT [FK_StockValues_Stocks]
GO


