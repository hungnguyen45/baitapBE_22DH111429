-- Additive schema changes required by checkout and order management.
-- Existing product records receive a demo starting quantity of 10 each.
IF COL_LENGTH('dbo.Product', 'Quantity') IS NULL
    ALTER TABLE dbo.Product ADD Quantity int NOT NULL
        CONSTRAINT DF_Product_Quantity DEFAULT (10) WITH VALUES;
GO
IF COL_LENGTH('dbo.Product', 'SoldQuantity') IS NULL
    ALTER TABLE dbo.Product ADD SoldQuantity int NOT NULL
        CONSTRAINT DF_Product_SoldQuantity DEFAULT (0) WITH VALUES;
GO
IF COL_LENGTH('dbo.[Order]', 'OrderStatus') IS NULL
    ALTER TABLE dbo.[Order] ADD OrderStatus nvarchar(50) NULL
        CONSTRAINT DF_Order_OrderStatus DEFAULT (N'Chờ thanh toán') WITH VALUES;
GO
UPDATE dbo.[Order]
SET OrderStatus = N'Chờ thanh toán'
WHERE OrderStatus IS NULL;
GO
