using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // 👈 thêm using này

namespace VanPhongPhamDKT.Models
{
    public partial class SanPham
    {
        [Key]
        public int MaSp { get; set; }

        [Required, StringLength(200)]
        public string TenSp { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999999)]
        public decimal Gia { get; set; }

        [Range(0, int.MaxValue)]
        public int SoLuong { get; set; }

        public string? HinhAnh { get; set; }

        [StringLength(4000)]
        public string? MoTa { get; set; }

        [Required]
        public int MaDm { get; set; }

        [ValidateNever] // 👈 KHÔNG validate thuộc tính navigation
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } =
            new List<ChiTietDonHang>();

        [ValidateNever] // 👈 KHÔNG validate navigation
        public virtual DanhMucSanPham? MaDmNavigation { get; set; } // 👈 cho phép nullable
    }
}
