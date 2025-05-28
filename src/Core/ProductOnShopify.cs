using System;

namespace Core
{
    public class ProductOnShopify
    {
        public int SysNo { get; set; }
        public int ProductSysNo { get; set; }
        public string ShopifyProductId { get; set; }
        public string Version { get; set; }
        public DateTime? LastSyncedAt { get; set; }
    }
}
