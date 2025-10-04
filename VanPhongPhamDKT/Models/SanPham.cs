using System;
using System.Collections.Generic;

namespace VanPhongPhamDKT.Models;

public partial class SanPham
{
    public int MaSp { get; set; }

    public string TenSp { get; set; } = null!;

    public decimal Gia { get; set; }

    public int SoLuong { get; set; }

    public string? HinhAnh { get; set; }

    public string? MoTa { get; set; }

    public int MaDm { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual DanhMucSanPham MaDmNavigation { get; set; } = null!;
}
