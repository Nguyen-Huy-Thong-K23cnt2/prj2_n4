using System;
using System.Collections.Generic;

namespace VanPhongPhamDKT.Models;

public partial class ChiTietDonHang
{
    public int MaCtdh { get; set; }

    public int MaDh { get; set; }

    public int MaSp { get; set; }

    public int SoLuong { get; set; }

    public decimal Gia { get; set; }

    public virtual DonHang MaDhNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
