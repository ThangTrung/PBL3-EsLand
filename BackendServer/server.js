const express = require('express');
const sql = require('mssql');
const bodyParser = require('body-parser');

const app = express();
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Cấu hình kết nối SQL Server
const dbConfig = {
    user: 'sa', 
    password: '@Khoa121206',
    server: 'localhost', 
    database: 'EsLandDB',
    options: {
        encrypt: false,
        trustServerCertificate: true
    }
};

// API POST: Lưu hoặc Cập nhật dữ liệu túi đồ (UPSERT)
app.post('/api/savegame', async (req, res) => {
    try {
        const { userID, inventoryJSON } = req.body;
        console.log("Đang lưu dữ liệu cho UserID:", userID);

        // Kết nối SQL Server
        const pool = await sql.connect(dbConfig);
        
        // Chạy câu lệnh SQL đã được bọc chuỗi chuẩn chỉnh
        await pool.request()
            .input('UserID', sql.Int, userID)
            .input('InventoryJSON', sql.NVarChar(sql.MAX), inventoryJSON)
            .query(`
                IF EXISTS (SELECT 1 FROM PlayerSaves WHERE UserID = @UserID)
                BEGIN
                    UPDATE PlayerSaves 
                    SET InventoryJSON = @InventoryJSON, LastUpdated = GETDATE()
                    WHERE UserID = @UserID
                END
                ELSE
                BEGIN
                    INSERT INTO PlayerSaves (UserID, InventoryJSON, LastUpdated) 
                    VALUES (@UserID, @InventoryJSON, GETDATE())
                END
            `);

        res.status(200).send({ success: true, message: "Cloud Save thành công!" });
    } catch (err) {
        console.error("Lỗi Server khi lưu dữ liệu:", err.message);
        res.status(500).send({ success: false, error: err.message });
    }
});

// API GET: Tải dữ liệu Inventory
app.get('/api/loadgame/:userID', async (req, res) => {
    try {
        const userID = req.params.userID;
        console.log("Đang tải dữ liệu cho UserID:", userID);

        let pool = await sql.connect(dbConfig);
        let result = await pool.request()
            .input('UserID', sql.Int, userID)
            .query('SELECT InventoryJSON FROM PlayerSaves WHERE UserID = @UserID');
        
        // Đặt Header để Unity hiểu đây là cục JSON thuần
        res.set('Content-Type', 'application/json');

        if (result.recordset.length > 0) {
            // Trả về đúng cục JSON Data túi đồ
            res.status(200).send(result.recordset[0].InventoryJSON);
        } else {
            // CÚ CHỐT: Nếu người chơi mới, trả về cấu trúc JSON rỗng chuẩn thay vì báo lỗi 404
            res.status(200).send(JSON.stringify({ inventory: { savedItems: [] } }));
        }
    } catch (err) {
        console.error("Lỗi Server:", err);
        res.status(500).send({ error: "Lỗi kết nối Database" });
    }
});

const PORT = 3000;
app.listen(PORT, () => {
    console.log("Backend Server đang chạy tại: http://localhost:" + PORT);
});