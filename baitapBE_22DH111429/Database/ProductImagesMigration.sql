-- Apply the local product image paths after importing the demo products.
-- This only changes ProductImage for the five demo products.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE dbo.Product SET ProductImage = N'~/Content/Images/iphone16.png' WHERE ProductID = 1;
UPDATE dbo.Product SET ProductImage = N'~/Content/Images/samsung-s25.jpg' WHERE ProductID = 2;
UPDATE dbo.Product SET ProductImage = N'~/Content/Images/asus-vivobook.jpg' WHERE ProductID = 3;
UPDATE dbo.Product SET ProductImage = N'~/Content/Images/tai-nghe.png' WHERE ProductID = 4;
UPDATE dbo.Product SET ProductImage = N'~/Content/Images/noi-chien.png' WHERE ProductID = 5;

COMMIT TRANSACTION;