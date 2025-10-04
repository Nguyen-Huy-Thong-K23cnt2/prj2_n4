using System;
using System.Collections.Generic;

namespace VanPhongPhamDKT.Models;

public partial class DanhMucSanPham
{
    public int MaDm { get; set; }

    public string TenDm { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
