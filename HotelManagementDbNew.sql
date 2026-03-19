CREATE DATABASE HotelManagement
GO
USE [HotelManagement]
GO

/****** Object:  Table [dbo].[Bookings]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bookings](
	[BookingId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[CheckIn] [datetime] NOT NULL,
	[CheckOut] [datetime] NOT NULL,
	[Deposit] [decimal](18, 2) NULL,
	[NumOfPeople] [int] NOT NULL,
	[Status] [nvarchar](50) NULL,
	[StaffId] [int] NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT ('Chua xác nh?n') FOR [Status]
GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO

/****** Object:  Table [dbo].[BookingServices]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BookingServices](
	[BookingServiceId] [int] IDENTITY(1,1) NOT NULL,
	[BookingId] [int] NULL,
	[ServiceId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Customers]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[CustomerId] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Gender] [nvarchar](10) NULL,
	[IDCard] [nvarchar](20) NULL,
	[Address] [nvarchar](255) NULL,
	[Nationality] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[Phone] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Feedbacks]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedbacks](
	[FeedbackId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[Rating] [int] NULL,
	[Comment] [nvarchar](500) NULL,
	[FeedbackDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[FeedbackId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Feedbacks] ADD  DEFAULT (getdate()) FOR [FeedbackDate]
GO

/****** Object:  Table [dbo].[Invoices]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Invoices](
	[InvoiceId] [int] IDENTITY(1,1) NOT NULL,
	[RentalId] [int] NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[PaymentDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Invoices] ADD  DEFAULT ('Chua thanh toán') FOR [Status]
GO

/****** Object:  Table [dbo].[Rentals]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rentals](
	[RentalId] [int] IDENTITY(1,1) NOT NULL,
	[BookingId] [int] NULL,
	[CheckInActual] [datetime] NOT NULL,
	[CheckOutActual] [datetime] NULL,
	[StaffId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[RentalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[RoomBookings]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoomBookings](
	[RoomBookingId] [int] IDENTITY(1,1) NOT NULL,
	[BookingId] [int] NULL,
	[RoomId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[RoomBookingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Rooms]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rooms](
	[RoomId] [int] IDENTITY(1,1) NOT NULL,
	[RoomTypeId] [int] NULL,
	[RoomNumber] [nvarchar](50) NOT NULL,
	[Image] [nvarchar](255) NULL,
	[Price] [decimal](18, 2) NULL,
	[Status] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[RoomId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Rooms] ADD  DEFAULT ('Tr?ng') FOR [Status]
GO

/****** Object:  Table [dbo].[RoomTypes]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoomTypes](
	[RoomTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Image] [nvarchar](255) NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Capacity] [int] NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[RoomTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[RoomTypes] ADD  DEFAULT ((1)) FOR [IsActive]
GO

/****** Object:  Table [dbo].[Services]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Services](
	[ServiceId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Services] ADD  DEFAULT ((1)) FOR [IsActive]
GO

/****** Object:  Table [dbo].[Staffs]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Staffs](
	[StaffId] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Gender] [nvarchar](10) NULL,
	[DateOfBirth] [date] NULL,
	[Address] [nvarchar](255) NULL,
	[Email] [nvarchar](100) NULL,
	[Phone] [nvarchar](20) NULL,
	[Image] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[StaffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Users]    Script Date: 3/16/2026 1:24:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[Role] [nvarchar](20) NOT NULL,
	[CustomerId] [int] NULL,
	[StaffId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([CustomerId])
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staffs] ([StaffId])
GO
ALTER TABLE [dbo].[BookingServices]  WITH CHECK ADD FOREIGN KEY([BookingId])
REFERENCES [dbo].[Bookings] ([BookingId])
GO
ALTER TABLE [dbo].[BookingServices]  WITH CHECK ADD FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Services] ([ServiceId])
GO
ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([CustomerId])
GO
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD FOREIGN KEY([RentalId])
REFERENCES [dbo].[Rentals] ([RentalId])
GO
ALTER TABLE [dbo].[Rentals]  WITH CHECK ADD FOREIGN KEY([BookingId])
REFERENCES [dbo].[Bookings] ([BookingId])
GO
ALTER TABLE [dbo].[Rentals]  WITH CHECK ADD FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staffs] ([StaffId])
GO
ALTER TABLE [dbo].[RoomBookings]  WITH CHECK ADD FOREIGN KEY([BookingId])
REFERENCES [dbo].[Bookings] ([BookingId])
GO
ALTER TABLE [dbo].[RoomBookings]  WITH CHECK ADD FOREIGN KEY([RoomId])
REFERENCES [dbo].[Rooms] ([RoomId])
GO
ALTER TABLE [dbo].[Rooms]  WITH CHECK ADD FOREIGN KEY([RoomTypeId])
REFERENCES [dbo].[RoomTypes] ([RoomTypeId])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([CustomerId])
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD FOREIGN KEY([StaffId])
REFERENCES [dbo].[Staffs] ([StaffId])
GO

SET IDENTITY_INSERT [dbo].[Customers] ON 

GO
INSERT [dbo].[Customers] ([CustomerId], [FullName], [Gender], [IDCard], [Address], [Nationality], [Email], [Phone]) VALUES (1, N'Nguyễn Văn Khách', N'Nam', NULL, NULL, NULL, N'customer@gmail.com', N'0900000001')
GO
SET IDENTITY_INSERT [dbo].[Customers] OFF
GO

SET IDENTITY_INSERT [dbo].[Rooms] ON 

GO
INSERT [dbo].[Rooms] ([RoomId], [RoomTypeId], [RoomNumber], [Image], [Price], [Status]) VALUES (1, 1, N'1', N'11', CAST(1111.00 AS Decimal(18, 2)), N'Booked')
GO
INSERT [dbo].[Rooms] ([RoomId], [RoomTypeId], [RoomNumber], [Image], [Price], [Status]) VALUES (3, 1, N'2222', NULL, CAST(22222.00 AS Decimal(18, 2)), N'Booked')
GO
INSERT [dbo].[Rooms] ([RoomId], [RoomTypeId], [RoomNumber], [Image], [Price], [Status]) VALUES (4, 2, N'2', NULL, CAST(222222.00 AS Decimal(18, 2)), N'Occupied')
GO
SET IDENTITY_INSERT [dbo].[Rooms] OFF
GO

SET IDENTITY_INSERT [dbo].[RoomTypes] ON 

GO
INSERT [dbo].[RoomTypes] ([RoomTypeId], [Name], [Image], [Price], [Capacity], [Description], [IsActive]) VALUES (1, N'1111', N'11', CAST(1111.00 AS Decimal(18, 2)), 3, N'111', 1)
GO
INSERT [dbo].[RoomTypes] ([RoomTypeId], [Name], [Image], [Price], [Capacity], [Description], [IsActive]) VALUES (2, N'2222', N'aaa', CAST(1111111.00 AS Decimal(18, 2)), 4, N'mama mia', 1)
GO
INSERT [dbo].[RoomTypes] ([RoomTypeId], [Name], [Image], [Price], [Capacity], [Description], [IsActive]) VALUES (3, N'3333', NULL, CAST(3333333.00 AS Decimal(18, 2)), 3, N'4444', 0)
GO
SET IDENTITY_INSERT [dbo].[RoomTypes] OFF
GO

SET IDENTITY_INSERT [dbo].[Services] ON 

GO
INSERT [dbo].[Services] ([ServiceId], [Name], [Price], [IsActive]) VALUES (1, N'111', CAST(1111.00 AS Decimal(18, 2)), 1)
GO
SET IDENTITY_INSERT [dbo].[Services] OFF
GO

SET IDENTITY_INSERT [dbo].[Staffs] ON 

GO
INSERT [dbo].[Staffs] ([StaffId], [FullName], [Gender], [DateOfBirth], [Address], [Email], [Phone], [Image]) VALUES (1, N'Trần Văn Nhân Viên', N'Male', CAST(N'2026-03-17' AS Date), NULL, N'staff@gmail.com', N'0900000002', NULL)
GO
SET IDENTITY_INSERT [dbo].[Staffs] OFF
GO

SET IDENTITY_INSERT [dbo].[Users] ON 

GO
INSERT [dbo].[Users] ([UserId], [Username], [PasswordHash], [Role], [CustomerId], [StaffId]) VALUES (1, N'customer01', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'Customer', 1, NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [PasswordHash], [Role], [CustomerId], [StaffId]) VALUES (2, N'staff01', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'Staff', NULL, 1)
GO
INSERT [dbo].[Users] ([UserId], [Username], [PasswordHash], [Role], [CustomerId], [StaffId]) VALUES (3, N'admin', N'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', N'Admin', NULL, NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [PasswordHash], [Role], [CustomerId], [StaffId]) VALUES (4, N'testingadmin', N'YV7X+xUEsMckopbXpp5sey+eosV8HYIGxa/fOS69/SU=', N'Admin', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Users] OFF
GO

