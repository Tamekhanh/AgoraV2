USE MASTER

DROP DATABASE Agora

CREATE DATABASE Agora

use Agora

CREATE TABLE [OutboxMessages] (
    [Id] uniqueidentifier NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [OccurredOn] datetime2 NOT NULL,
    [ProcessedOn] datetime2 NULL,
    [Error] nvarchar(max) NULL,
	  [ErrorTime] int,
    CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id])
);

CREATE TABLE [Product] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [name] nvarchar(255),
  [barcode] int,
  [description] text,
  [categoryId] int,
  [shopId] int,
  [costPrice] int,
  [retailPrice] int,
  [stockQty] int,
  [discountPercent] int,
  [guaranteeMonths] int,
  [imageId] int,
  [createdAt] datetime2,
  [updatedAt] datetime2,
  [note] text,
  [soldQty] int,
  [status] int
)
GO

CREATE TABLE [ImageFile] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [imageFile] binary
)
GO

CREATE TABLE [Category] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [name] nvarchar(255),
  [description] text,
  [createdAt] datetime2,
  [updatedAt] datetime2
)
GO

CREATE TABLE [Shop] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [name] nvarchar(255),
  [contactName] nvarchar(255),
  [phone] nvarchar(255),
  [email] nvarchar(255),
  [taxCode] nvarchar(255),
  [address] text,
  [userId] int,
  [imageId] int,
  [status] int,
  [createdAt] datetime2
)
GO

CREATE TABLE [User] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [name] nvarchar(255),
  [phone] nvarchar(255),
  [email] nvarchar(255),
  [address] text,
  [taxCode] nvarchar(255),
  [username] nvarchar(255),
  [password] nvarchar(255),
  [role] int,
  [imageId] int,
  [createdAt] datetime2
)
GO

CREATE TABLE [Cart] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [userId] int,
  [createdAt] datetime2,
  [updatedAt] datetime2
)
GO

CREATE TABLE [CartItem] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [cartId] int,
  [productId] int,
  [quantity] int,
  [status] int
)
GO

CREATE TABLE [Order] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [userId] int,
  [orderDate] datetime,
  [totalAmount] int,
  [discount] int,
  [tax] int,
  [shippingFee] int,
  [paymentMethod] nvarchar(255),
  [paymentStatus] nvarchar(255),
  [orderStatus] int,
  [note] text,
  [createdAt] datetime2,
  [updatedAt] datetime2
)
GO

CREATE TABLE [OrderItem] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [orderId] int,
  [productId] int,
  [quantity] int,
  [unitPrice] int,
  [discount] int,
  [total] int
)
GO

CREATE TABLE [Payment] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [orderId] int,
  [amount] int,
  [method] nvarchar(255),
  [status] nvarchar(255),
  [transactionId] nvarchar(255),
  [paymentDate] datetime2
)
GO

ALTER TABLE [Product] ADD FOREIGN KEY ([categoryId]) REFERENCES [Category] ([id])
GO

ALTER TABLE [Product] ADD FOREIGN KEY ([shopId]) REFERENCES [Shop] ([id])
GO

ALTER TABLE [Product] ADD FOREIGN KEY ([imageId]) REFERENCES [ImageFile] ([id])
GO

ALTER TABLE [Shop] ADD FOREIGN KEY ([userId]) REFERENCES [User] ([id])
GO

ALTER TABLE [Shop] ADD FOREIGN KEY ([imageId]) REFERENCES [ImageFile] ([id])
GO

ALTER TABLE [User] ADD FOREIGN KEY ([imageId]) REFERENCES [ImageFile] ([id])
GO

ALTER TABLE [Cart] ADD FOREIGN KEY ([userId]) REFERENCES [User] ([id])
GO

ALTER TABLE [CartItem] ADD FOREIGN KEY ([cartId]) REFERENCES [Cart] ([id])
GO

ALTER TABLE [CartItem] ADD FOREIGN KEY ([productId]) REFERENCES [Product] ([id])
GO

ALTER TABLE [Order] ADD FOREIGN KEY ([userId]) REFERENCES [User] ([id])
GO

ALTER TABLE [OrderItem] ADD FOREIGN KEY ([orderId]) REFERENCES [Order] ([id])
GO

ALTER TABLE [OrderItem] ADD FOREIGN KEY ([productId]) REFERENCES [Product] ([id])
GO

ALTER TABLE [Payment] ADD FOREIGN KEY ([orderId]) REFERENCES [Order] ([id])
GO
