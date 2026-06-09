-- Script khởi tạo Database cho PBL3
-- Bạn cần tạo Database tên 'EsLandDB' trước khi chạy script này

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        ID INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) UNIQUE NOT NULL,
        Password NVARCHAR(100) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PlayerSaves')
BEGIN
    CREATE TABLE PlayerSaves (
        UserID INT PRIMARY KEY,
        InventoryJSON NVARCHAR(MAX) NOT NULL,
        LastUpdated DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (UserID) REFERENCES Users(ID)
    );
END
GO
