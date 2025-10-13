using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VanPhongPhamDKT.Models;

public partial class DonHang
{
    public int MaDh { get; set; }

    public int MaKh { get; set; }

    public DateTime? NgayDat { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal TongTien { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

}
