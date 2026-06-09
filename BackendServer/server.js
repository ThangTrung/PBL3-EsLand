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

// API POST: Đăng nhập hoặc Đăng ký tự động
app.post('/api/auth/login', async (req, res) => {
    try {
        const { username, password } = req.body;
        console.log("Yêu cầu đăng nhập:", username);

        const pool = await sql.connect(dbConfig);
        
        // Kiểm tra user tồn tại
        let userResult = await pool.request()
            .input('Username', sql.NVarChar, username)
            .query('SELECT ID, Password FROM Users WHERE Username = @Username');

        if (userResult.recordset.length > 0) {
            const user = userResult.recordset[0];
            // So sánh mật khẩu (demo dùng text thuần, thực tế nên dùng bcrypt)
            if (user.Password === password) {
                res.status(200).send({ success: true, userID: user.ID, message: "Đăng nhập thành công!" });
            } else {
                res.status(401).send({ success: false, message: "Sai mật khẩu!" });
            }
        } else {
            // Tự động đăng ký nếu chưa có
            let insertResult = await pool.request()
                .input('Username', sql.NVarChar, username)
                .input('Password', sql.NVarChar, password)
                .query('INSERT INTO Users (Username, Password) OUTPUT INSERTED.ID VALUES (@Username, @Password)');
            
            const newUserID = insertResult.recordset[0].ID;
            res.status(201).send({ success: true, userID: newUserID, message: "Tài khoản mới đã được tạo!" });
        }
    } catch (err) {
        console.error("Lỗi Auth Server:", err.message);
        res.status(500).send({ success: false, error: err.message });
    }
});

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
            // Trả về cấu trúc GameData rỗng chuẩn để Unity JsonUtility có thể parse
            res.status(200).send(JSON.stringify({ 
                _inventories: [], 
                _equippedItems: [],
                _resourceNodes: [],
                _droppedItems: [],
                _openedGates: [],
                _destroyedEntityIDs: [],
                _activeEnemies: [],
                playerHealth: 100,
                playerPosition: { x: 0, y: 0, z: 0 },
                respawnPoint: { x: 0, y: 0, z: 0 }
            }));
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