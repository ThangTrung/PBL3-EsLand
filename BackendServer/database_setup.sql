-- Script khởi tạo Database cho PBL3
-- Bạn cần tạo Database tên 'PBL3_EsLand' trước khi chạy script này

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PlayerSaves')
BEGIN
    CREATE TABLE PlayerSaves (
        UserID INT PRIMARY KEY,
        InventoryJSON NVARCHAR(MAX) NOT NULL,
        LastUpdated DATETIME DEFAULT GETDATE()
    );
END
GO
