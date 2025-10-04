namespace VanPhongPhamDKT.Models
{
    public class CartItem
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }

        public decimal ThanhTien => Gia * SoLuong;
    }
}
