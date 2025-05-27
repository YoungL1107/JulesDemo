namespace Core
{
    public class Order
    {
        public int SysNo { get; set; }
        public int ProductSysNo { get; set; }
        public short Status { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }
        public string ShippingAddress { get; set; }
        public int PaymentStatus { get; set; }
    }
}
