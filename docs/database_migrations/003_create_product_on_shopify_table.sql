-- Create ProductOnShopify table
CREATE TABLE ProductOnShopify (
    SysNo INT PRIMARY KEY IDENTITY(1,1),
    ProductSysNo INT NOT NULL,
    ShopifyProductId NVARCHAR(255) NOT NULL,
    Version NVARCHAR(50) NOT NULL,
    LastSyncedAt DATETIME NULL,
    CONSTRAINT FK_ProductOnShopify_Products FOREIGN KEY (ProductSysNo) REFERENCES Products(SysNo)
);

-- Create index on ProductSysNo
CREATE INDEX IX_ProductOnShopify_ProductSysNo ON ProductOnShopify (ProductSysNo);

-- Create unique index on ShopifyProductId
CREATE UNIQUE INDEX UQ_ProductOnShopify_ShopifyProductId ON ProductOnShopify (ShopifyProductId);
