# API Thực đơn - Tài liệu Kỹ Thuật

## 🔌 Endpoints

### **1. Lấy Danh sách Thực đơn**
```
GET /ThucDon/Index
```
- **Quyền truy cập**: Bếp trưởng
- **Trả về**: HTML view với danh sách thực đơn
- **Status Code**: 200 OK, 302 Redirect (nếu không đủ quyền)

---

### **2. Lấy Nguyên liệu của Thực đơn**
```
GET /ThucDon/GetItems?id={maThucDon}
```
- **Parameters**:
  - `id` (int): Mã thực đơn
- **Quyền truy cập**: Bếp trưởng
- **Trả về**:
```json
{
  "success": true,
  "data": [
    {
      "maThucDonItem": 1,
      "maNguyenLieu": 5,
      "tenNguyenLieu": "Cơm",
      "soLuong": 100,
      "calo": 130,
      "soLuongTon": 500.50
    },
    {
      "maThucDonItem": 2,
      "maNguyenLieu": 3,
      "tenNguyenLieu": "Gà",
      "soLuong": 150,
      "calo": 247.5,
      "soLuongTon": -250.00
    }
  ]
}
```
- **Status Code**: 200 OK, 401 Unauthorized

---

### **3. Thêm Nguyên liệu vào Thực đơn**
```
POST /ThucDon/AddItem
```
- **Body Form Data**:
  ```
  maThucDon: 1
  maNguyenLieu: 5
  soLuong: 100
  ```
- **Quyền truy cập**: Bếp trưởng
- **Validation**:
  - `soLuong` > 0 (bắt buộc)
  - `maThucDon` phải tồn tại
  - `maNguyenLieu` phải tồn tại
- **Trả về**:
```json
{
  "success": true,
  "item": {
    "maThucDonItem": 1,
    "maThucDon": 1,
    "maNguyenLieu": 5,
    "soLuong": 100
  },
  "kho": {
    "maKho": 2,
    "maNguyenLieu": 5,
    "soLuongTon": 400.50
  },
  "thucDon": {
    "maThucDon": 1,
    "tongCalo": 402.50
  }
}
```
- **Signalr**: Phát `MenuUpdated` với payload
- **Status Code**: 200 OK, 400 Bad Request, 401 Unauthorized, 404 Not Found

---

### **4. Chỉnh sửa Số lượng Nguyên liệu**
```
POST /ThucDon/EditItem
```
- **Body Form Data**:
  ```
  id: 1
  soLuong: 150
  ```
- **Quyền truy cập**: Bếp trưởng
- **Validation**:
  - `soLuong` > 0
  - `id` phải tồn tại
- **Trả về**: (tương tự AddItem)
- **Signalr**: Phát `MenuUpdated`
- **Log**: "Chỉnh sửa số lượng 'Gà' từ 100 thành 150"
- **Status Code**: 200 OK, 400 Bad Request, 401 Unauthorized, 404 Not Found

---

### **5. Xóa Nguyên liệu**
```
POST /ThucDon/DeleteItem
```
- **Body Form Data**:
  ```
  id: 1
  ```
- **Quyền truy cập**: Bếp trưởng
- **Hành động**:
  - Xóa item khỏi thực đơn
  - Trả lại số lượng vào tồn kho
  - Tính toán lại TongCalo
- **Trả về**:
```json
{
  "success": true
}
```
- **Signalr**: Phát `MenuUpdated`
- **Log**: "Xóa nguyên liệu 'Gà' khỏi thực đơn 1"
- **Status Code**: 200 OK, 401 Unauthorized, 404 Not Found

---

### **6. Tạo Thực đơn Mới**
```
POST /ThucDon/Create
```
- **Body**: (không cần)
- **Quyền truy cập**: Bếp trưởng
- **Hành động**:
  - Tạo thực đơn mới với ngày hôm nay
  - `TongCalo = 0`
  - Redirect về trang Index
- **Signalr**: Phát `MenuUpdated`
- **Log**: "Tạo thực đơn mới cho ngày 2026-03-14"
- **Status Code**: 302 Redirect (OK)/400 Bad Request

---

### **7. Chỉnh sửa Ngày Thực đơn**
```
POST /ThucDon/Edit
```
- **Body Form Data**:
  ```
  id: 1
  ngayApDung: 2026-03-15
  ```
- **Quyền truy cập**: Bếp trưởng
- **Validation**:
  - `id` phải tồn tại
- **Trả về**:
```json
{
  "success": true,
  "td": {
    "maThucDon": 1,
    "ngayApDung": "2026-03-15T00:00:00",
    "tongCalo": 402.50
  }
}
```
- **Signalr**: Phát `MenuUpdated`
- **Log**: "Cập nhật ngày thực đơn 1 từ 2026-03-14 thành 2026-03-15"
- **Status Code**: 200 OK, 400 Bad Request, 404 Not Found

---

### **8. Xóa Thực đơn**
```
POST /ThucDon/Delete
```
- **Body Form Data**:
  ```
  id: 1
  ```
- **Quyền truy cập**: Bếp trưởng
- **Hành động**:
  - Xóa thực đơn
  - Xóa tất cả items
  - Trả lại kho cho tất cả nguyên liệu
  - Transaction: Tất cả hoặc không
- **Trả về**:
```json
{
  "success": true
}
```
- **Signalr**: Phát `MenuUpdated`
- **Log**: "Xóa thực đơn 1 (ngày: 2026-03-14) và 8 nguyên liệu"
- **Status Code**: 200 OK, 400 Bad Request, 401 Unauthorized, 404 Not Found

---

### **9. Lấy Danh sách Nguyên liệu (API)**
```
GET /api/thucdon/nguyenlieu
```
- **Quyền truy cập**: (công khai, nhưng thường dùng bởi Bếp trưởng)
- **Trả về**:
```json
[
  {
    "maNguyenLieu": 1,
    "tenNguyenLieu": "Bắp cải",
    "calo": 0.25
  },
  {
    "maNguyenLieu": 3,
    "tenNguyenLieu": "Gà",
    "calo": 1.65
  },
  {
    "maNguyenLieu": 5,
    "tenNguyenLieu": "Cơm",
    "calo": 1.30
  }
]
```
- **Sorting**: Tên nguyên liệu (A-Z)
- **Status Code**: 200 OK, 400 Bad Request

---

## 📡 SignalR Events

### **Event: MenuUpdated**
Phát khi có bất kỳ thay đổi nào về thực đơn

**Client Receive Handler**:
```javascript
connection.on('MenuUpdated', function(payload) {
  // payload = { action, item, kho, thucDon, ... }
  // Tự động reload items nếu modal đang mở
  // Hoặc reload trang nếu không
});
```

**Actions**:
- `"add"`: Thêm item mới
- `"edit"`: Chỉnh sửa item
- `"delete"`: Xóa item
- `"created"`: Tạo thực đơn mới
- `"edited"`: Chỉnh sửa ngày thực đơn
- `"deleted"`: Xóa thực đơn

**Payload Example**:
```json
{
  "action": "add",
  "item": {
    "maThucDonItem": 1,
    "maThucDon": 5,
    "maNguyenLieu": 3,
    "soLuong": 150
  },
  "kho": {
    "maKho": 1,
    "maNguyenLieu": 3,
    "soLuongTon": -250.0
  },
  "thucDon": {
    "maThucDon": 5,
    "tongCalo": 247.5
  }
}
```

---

## 🔐 Authentication & Authorization

### **Headers** (nếu cần Token):
```
Authorization: Bearer <token>
```

### **Session Check**:
- **Quyền Bếp trưởng**: `HttpContext.Session.GetString("Quyen") == "Bếp trưởng"`
- **User ID**: `HttpContext.Session.GetString("MaNguoiDung")`

### **Response nếu không đủ quyền**:
- **302 Redirect**: Chuyển về `/Home/AccessDenied`
- **401 Unauthorized**: `{ "error": "..." }`

---

## 📊 Error Response Format

```json
{
  "error": "Mô tả lỗi cụ thể",
  "details": "Chi tiết bổ sung (nếu có)"
}
```

### **Common Error Types**:
1. **400 Bad Request**: Dữ liệu không hợp lệ
   - Số lượng ≤ 0
   - Thiếu thông tin bắt buộc
   
2. **401 Unauthorized**: Không đủ quyền
   - Không phải "Bếp trưởng"
   - Session hết hạn
   
3. **404 Not Found**: Dữ liệu không tồn tại
   - Thực đơn không tồn tại
   - Nguyên liệu không tồn tại
   - Item không tồn tại

4. **500 Internal Server Error**: Lỗi server
   - Database error
   - Unhandled exception

---

## 🧪 Ví dụ Curl Request

### **Thêm Nguyên liệu**:
```bash
curl -X POST http://localhost:5000/ThucDon/AddItem \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "maThucDon=1&maNguyenLieu=5&soLuong=100"
```

### **Lấy Danh sách Thực đơn**:
```bash
curl -X GET http://localhost:5000/api/thucdon/nguyenlieu
```

### **Xóa Item**:
```bash
curl -X POST http://localhost:5000/ThucDon/DeleteItem \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "id=1"
```

---

## 💾 Database Schema

### **ThucDon**
```sql
CREATE TABLE ThucDon (
  MaThucDon INT IDENTITY(1,1) PRIMARY KEY,
  NgayApDung DATETIME2 NOT NULL,
  TongCalo FLOAT NOT NULL
);
```

### **ThucDonItem**
```sql
CREATE TABLE ThucDonItem (
  MaThucDonItem INT IDENTITY(1,1) PRIMARY KEY,
  MaThucDon INT NOT NULL FOREIGN KEY REFERENCES ThucDon(MaThucDon),
  MaNguyenLieu INT NOT NULL FOREIGN KEY REFERENCES NguyenLieu(MaNguyenLieu),
  SoLuong FLOAT NOT NULL
);
```

### **Kho**
```sql
CREATE TABLE Kho (
  MaKho INT IDENTITY(1,1) PRIMARY KEY,
  MaNguyenLieu INT NOT NULL FOREIGN KEY REFERENCES NguyenLieu(MaNguyenLieu),
  SoLuongTon FLOAT NOT NULL
);
```

### **NguyenLieu**
```sql
CREATE TABLE NguyenLieu (
  MaNguyenLieu INT IDENTITY(1,1) PRIMARY KEY,
  TenNguyenLieu NVARCHAR(MAX) NOT NULL,
  GiaTriDinhDuong FLOAT,
  Calo FLOAT NULL,
  Dam FLOAT NULL,
  Cali FLOAT NULL
);
```

---

**Tài liệu API**: v2.0 (12/03/2026)
