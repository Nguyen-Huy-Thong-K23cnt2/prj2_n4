CREATE DATABASE VanPhongPham;
GO
USE VanPhongPham;
GO

-- 1. Bảng User
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    DiaChi NVARCHAR(255),
    SoDienThoai NVARCHAR(20),
    NgayTao DATETIME DEFAULT GETDATE()
	ChucVu NVARCHAR (200),
);
GO

-- 2. Bảng NhanVien
CREATE TABLE Admin (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    SoDienThoai NVARCHAR(20),
    ChucVu NVARCHAR(50),
    NgayTao DATETIME DEFAULT GETDATE()
	TrangThai NVARCHAR (50),
);
GO

-- 3. Bảng DanhMucSanPham
CREATE TABLE DanhMucSanPham (
    MaDM INT IDENTITY(1,1) PRIMARY KEY,
    TenDM NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255)
);
GO

-- 4. Bảng SanPham
CREATE TABLE SanPham (
    MaSP INT IDENTITY(1,1) PRIMARY KEY,
    TenSP NVARCHAR(200) NOT NULL,
    Gia DECIMAL(18,2) NOT NULL,
    SoLuong INT NOT NULL,
    HinhAnh NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    MaDM INT NOT NULL,
    FOREIGN KEY (MaDM) REFERENCES DanhMucSanPham(MaDM)
);
GO

-- 5. Bảng DonHang
CREATE TABLE DonHang (
    MaDH INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NOT NULL,
    NgayDat DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(50) DEFAULT N'Chờ duyệt',
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
);
GO

-- 6. Bảng ChiTietDonHang
CREATE TABLE ChiTietDonHang (
    MaCTDH INT IDENTITY(1,1) PRIMARY KEY,
    MaDH INT NOT NULL,
    MaSP INT NOT NULL,
    SoLuong INT NOT NULL,
    Gia DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

SELECT * FROM KhachHang;

-- Thêm sản phẩm mẫu vào bảng SanPham
INSERT INTO SanPham (TenSP, Gia, SoLuong, HinhAnh, MoTa, MaDM)
VALUES 
(N'Bìa kẹp tài liệu', 57000, 50, 'bia-kep-tai-lieu.jpg', N'Bìa kẹp tài liệu nhiều màu sắc, bền đẹp', 1),
(N'Vở viết kẻ ngang', 15000, 200, 'vo-viet-ke-ngang.jpg', N'Vở kẻ ngang với hình ảnh dễ thương', 1),
(N'Hộp đựng bút nhựa', 50000, 100, 'hop-dung-but-nhua.jpg', N'Hộp nhựa đựng bút nhiều ngăn', 2),
(N'Sổ tay cá nhân', 28000, 150, 'so-tay-ca-nhan.jpg', N'Sổ tay nhỏ gọn, tiện lợi cho việc ghi chú', 1),
(N'Bút highlight dạ quang', 25000, 300, 'but-highlight.jpg', N'Bút highlight nhiều màu dạ quang nổi bật', 2),
(N'Túi đựng mỹ phẩm', 120000, 80, 'tui-dung-my-pham.jpg', N'Túi đựng mỹ phẩm nhỏ gọn, dễ thương', 3),
(N'Túi đựng đồ văn phòng', 90000, 120, 'tui-dung-van-phong.jpg', N'Túi đựng văn phòng phẩm đa năng', 3),
(N'Máy tính mini gấu cute', 700000, 30, 'may-tinh-mini-gau.jpg', N'Máy tính bỏ túi hình gấu dễ thương', 4);

SELECT * FROM SanPham;

-- Thêm khách hàng mẫu
INSERT INTO KhachHang (HoTen, Email, MatKhau, DiaChi, SoDienThoai)
VALUES
(N'Nguyễn Văn A', 'nguyenvana@example.com', '123456', N'Hà Nội', '0912345678');



