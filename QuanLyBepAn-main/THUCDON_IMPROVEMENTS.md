# Cải tiến Chức năng Thực đơn (ThucDon) - Hướng dẫn Sử dụng

## ✅ Những cải tiến đã triển khai

### 1. **Tính toán Calo Tự động**
- **Tính năng**: Mỗi khi thêm/chỉnh sửa/xóa nguyên liệu, hệ thống tự động cập nhật `TongCalo`
- **Cách thức**: 
  - Tính = Số lượng × Calo (của mỗi nguyên liệu)
  - Ví dụ: Nếu 10 gam lương mạch (có 3.5 kcal/gam) = 35 kcal
- **Hiển thị**: Giá trị tổng calo được cập nhật ngay trong bảng danh sách thực đơn

### 2. **Quản lý Kho Tốt hơn**
- **Kiểm soát Tồn kho**: Thêm nguyên liệu → Tự động giảm tồn kho
- **Trả lại Kho**: Xóa nguyên liệu → Tự động trả lại số lượng vào tồn kho
- **Xoá Thực đơn**: Trả lại toàn bộ nguyên liệu vào kho của từng loại
- **Hiển thị Trạng thái**: 
  - 🟢 Tồn kho dương (xanh lá) = Còn hàng
  - 🔴 Tồn kho âm (đỏ) = Thiếu hàng

### 3. **Xử lý Lỗi & Kiểm Định**
- ✓ Kiểm tra quyền truy cập (chỉ "Bếp trưởng")
- ✓ Kiểm tra dữ liệu (số lượng > 0)
- ✓ Kiểm tra tồn tại (thực đơn, nguyên liệu)
- ✓ Thông báo lỗi rõ ràng cho người dùng
- ✓ Ghi nhật ký (Log) mọi hành động quan trọng

### 4. **Giao Diện Cải tiến (Bootstrap 5)**
- **Biểu tượng**: Font Awesome icons cho các nút hành động
- **Màu sắc & Badges**: 
  - 🟢 Nút Thêm (xanh) - tạo thực đơn
  - 🟡 Nút Chỉnh sửa (vàng)
  - 🔴 Nút Xóa (đỏ)
  - 💙 Nút BMI (xanh dương)
- **Bảng Responsive**: Tối ưu cho thiết bị khác nhau
- **Modal Cải tiến**: 
  - Header có icon & màu
  - Input phải có nhãn (label) rõ ràng
  - Hiển thị Calo và Trạng thái tồn kho

### 5. **API RESTful Mới**
- **Endpoint**: `/api/thucdon/nguyenlieu` (GET)
- **Chức năng**: Lấy danh sách nguyên liệu với thông tin calo
- **Trả về**: JSON array với `maNguyenLieu`, `tenNguyenLieu`, `calo`
- **Sắp xếp**: Tên nguyên liệu (A-Z)

### 6. **Real-time (SignalR) - Cập nhật Tức thì**
- Khi bất kỳ người dùng nào thêm/chỉnh sửa/xóa nguyên liệu
- Tất cả người dùng khác sẽ thấy thay đổi ngay lập tức
- không cần F5 (reload)

### 7. **Ghi Nhật ký (Audit Trail)**
- **Mọi hành động**:
  - Thêm nguyên liệu → "Thêm nguyên liệu 'Cơm' vào thực đơn 5"
  - Chỉnh sửa → "Chỉnh sửa số lượng 'Gà' từ 2.5 thành 3.0"
  - Xóa → "Xóa nguyên liệu 'Rau cải' khỏi thực đơn 5"
  - Xóa thực đơn → "Xóa thực đơn 5 (ngày: 2026-03-12) và 8 nguyên liệu"
- **Mục đích**: Track ai làm gì khi nào

---

## 📋 Hướng dẫn Sử dụng

### **A. Tạo Thực đơn Mới**
1. Nhấn nút **"🟢 Tạo thực đơn (ngày hiện tại)"**
2. Thực đơn sẽ được tạo cho ngày hôm nay
3. Nó sẽ xuất hiện cùng với các thực đơn khác trong bảng

### **B. Thêm Nguyên liệu vào Thực đơn**
1. Nhấn **"🟡 Chỉnh sửa"** trên dòng thực đơn cần sửa
2. Cửa sổ **"Nguyên liệu"** sẽ mở ra
3. Chọn nguyên liệu từ dropdown (ví dụ: "Cơm (3.10 kcal)")
4. Nhập **"Số lượng"** (ví dụ: 100)
5. Nhấn **"🟢 Thêm"**
6. Nguyên liệu sẽ được thêm vào bảng
7. TongCalo sẽ tự động cập nhật

#### **Ví dụ Tính toán**:
| Nguyên liệu | Số lượng | Calo/đơn vị | Tổng Calo |
|--|--|--|--|
| Cơm | 100 | 1.3 | 130 |
| Gà | 150 | 1.65 | 247.5 |
| Rau cải | 50 | 0.5 | 25 |
| **Tổng** | | | **402.5 kcal** |

### **C. Chỉnh sửa Số lượng Nguyên liệu**
1. Mở modal **"Nguyên liệu"** (nhấn Chỉnh sửa)
2. Thay đổi giá trị trong cột **"Số lượng yêu cầu"**
3. Khi rời khỏi ô input, hệ thống tự động:
   - Cập nhật số lượng
   - Điều chỉnh kho (tồn kho)
   - Tính toán lại TongCalo
4. Bạn sẽ nhận được thông báo nếu có lỗi

### **D. Xóa Nguyên liệu**
1. Mở modal **"Nguyên liệu"**
2. Nhấn **"🔴 Xóa"** (biểu tượng thùng rác) ở hàng cần xóa
3. Xác nhận lệnh xóa
4. Hệ thống sẽ:
   - Xóa khỏi thực đơn
   - Trả lại số lượng vào tồn kho
   - Tính toán lại TongCalo

### **E. Xóa Thực đơn**
1. Nhấn **"🔴 Xóa"** trên dòng thực đơn
2. Xác nhận: "Bạn có chắc muốn xóa thực đơn này?"
3. Hệ thống sẽ:
   - Xóa thực đơn
   - Xóa tất cả nguyên liệu của nó
   - Trả lại toàn bộ kho
   - Ghi nhật ký (số lượng nguyên liệu đã xóa)

### **F. Tính Chỉ số BMI**
1. Nhấn nút **"💙 Tính BMI"**
2. Cửa sổ **"Tính chỉ số BMI"** sẽ hiển thị
3. Nhập:
   - **Chiều cao** (cm): ví dụ 170
   - **Cân nặng** (kg): ví dụ 70
4. Nhấn **"🟢 Tính"**
5. Kết quả hiển thị:
   - **BMI = 24.22**
   - **Phân loại**: Bình thường ✅
6. Nếu chọn, nhấn **"Chuyển sang tạo thực đơn"** để chuyển hướng đến trang Bếp trưởng

#### **Phân loại BMI**:
| BMI | Phân loại | Màu |
|--|--|--|
| < 18.5 | Gầy (thiếu cân) | 🟡 Vàng |
| 18.5 - 25 | Bình thường | 🟢 Xanh |
| 25 - 30 | Thừa cân | 🟡 Vàng |
| ≥ 30 | Béo phì | 🔴 Đỏ |

---

## 🔐 Bảo mật & Quyền

- ✅ Chỉ **"Bếp trưởng"** mới có quyền truy cập hệ thống thực đơn
- ✅ Người dùng khác sẽ bị chuyển đến trang **"Bạn không có quyền truy cập"**
- ✅ Session timeout tự động nếu giữa nguyên

---

## 📊 Hiển thị Dữ liệu

### **Bảng Thực đơn**:
```
Ngày áp dụng  | Tổng calo        | Hành động
14/03/2026    | 🔵 402.50 kcal  | [Chỉnh sửa] [Xóa]
13/03/2026    | 🔵 350.00 kcal  | [Chỉnh sửa] [Xóa]
```

### **Bảng Nguyên liệu (trong Modal)**:
```
Nguyên liệu | Số lượng | Calo/đơn vị | Tồn kho        | Hành động
Cơm        | 100      | 1.30       | 🟢 500.00      | [Xóa]
Gà         | 150      | 1.65       | 🔴 -250.00     | [Xóa]
Rau        | 50       | 0.50       | 🟢 1000.00     | [Xóa]
```

---

## 🚀 Tính Năng Nâng Cao

### **Real-time Sync**
- Khi bạn thêm nguyên liệu, ngay lập tức tất cả các tab/người dùng khác sẽ cập nhật
- Cơ chế: **SignalR Hub** (`/thucdonHub`)

### **Ghi Nhật ký Đầy đủ**
- Mỗi hành động đều được ghi lại trong bảng `NhatKyHeThong`
- Thông tin ghi: MaNguoiDung, HanhDong, ThoiGian
- Dùng để kiểm soát ai làm gì khi nào

### **Xử lý Giao Dịch (Transaction)**
- Khi xóa thực đơn, nếu có lỗi → **ROLLBACK** (hoàn tác)
- Đảm bảo dữ liệu không bị inconsistent

---

## ⚠️ Lưu Ý Quan Trọng

1. **Số lượng phải > 0**: Hệ thống không cho phép nhập 0 hoặc số âm
2. **Chọn nguyên liệu**: Phải chọn từ dropdown, không thể nhập tự do
3. **Tồn kho có thể âm**: Điều này có nghĩa là "thiếu hàng" (được theo dõi)
4. **Session & Timeout**: 
   - Nếu session hết hạn, bạn sẽ bị yêu cầu đăng nhập lại
   - Thực đơn vẫn tồn tại trong database (không mất dữ liệu)

---

## 🐛 Xử Lý Lỗi

### **Lỗi "Lỗi: Không tìm thấy item"**
- Nguyên liệu không tồn tại
- Cách giải quyết: Reload lại trang (F5)

### **Lỗi "Lỗi: Thực đơn không tồn tại"**
- Thực đơn đã bị xóa (có thể bởi người khác)
- Cách giải quyết: Reload lại trang (F5)

### **Lỗi "Xóa thất bại: ..."**
- Có vấn đề khi xóa thực đơn
- Cách giải quyết: Kiểm tra console (F12) xem thông báo chi tiết

---

## 📱 Hỗ Trợ Thiết Bị

- ✅ **Desktop** (PC, Laptop)
- ✅ **Tablet** (iPad, Android Tablet)
- ✅ **Mobile** (điện thoại)
- ✅ **Responsive Design** (tự điều chỉnh kích thước)

---

## 🔄 Workflow Tiêu Biểu

```
1. [Bài toán] Cần tạo menu phục vụ 500 kcal cho bữa trưa
   ↓
2. [Hành động] Bếp trưởng tạo thực đơn mới (ngày 14/03)
   ↓
3. [Thêm nguyên liệu]:
   - 100g Cơm → 130 kcal
   - 150g Gà → 247.5 kcal
   - 50g Rau → 25 kcal
   ↓
4. [Kiểm tra]: Tổng = 402.5 kcal (chúa đủ 500)
   ↓
5. [Điều chỉnh]: Thêm 200g Rau mầm → +100 kcal
   ↓
6. [Kết quả]: Tổng = 502.5 kcal ✅ (đạt yêu cầu)
   ↓
7. [Log]: Ghi nhật ký tất cả hành động
```

---

## 📞 Hỗ Trợ

Nếu gặp vấn đề:
1. Kiểm tra Console (F12 → Console tab) xem có lỗi JavaScript không
2. Kiểm tra quyền: Bạn có phải "Bếp trưởng" không?
3. Reload trang (Ctrl+F5) - làm sạch cache
4. Liên hệ admin để kiểm tra logs server

---

**Phiên bản**: 2.0 (Cập nhật 12/03/2026)  
**Trạng thái**: ✅ Hoàn thành & Kiểm thử
