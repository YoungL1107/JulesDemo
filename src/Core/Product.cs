namespace Core
{
    public class Product
    {
        public int SysNo { get; set; }
        public string ProductName { get; set; }
        public short Status { get; set; }
        public decimal Price { get; set; }
        public int Inventory { get; set; } // Changed from 'inventory' to 'Inventory' to follow C# naming conventions
    }
}
