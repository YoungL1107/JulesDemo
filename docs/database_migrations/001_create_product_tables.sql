-- Create Products Table
CREATE TABLE Products (
    SysNo INT PRIMARY KEY IDENTITY(1,1), -- Assuming SysNo is an auto-incrementing primary key
    ProductName NVARCHAR(255) NOT NULL,
    Status SMALLINT NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    Inventory INT NOT NULL
);

-- Create Product_Colors Table
CREATE TABLE Product_Colors (
    ProductSysNo INT NOT NULL,
    Color NVARCHAR(50) NOT NULL,
    PRIMARY KEY (ProductSysNo, Color), -- Composite primary key
    FOREIGN KEY (ProductSysNo) REFERENCES Products(SysNo) ON DELETE CASCADE -- Optional: ON DELETE CASCADE if colors should be removed when a product is deleted
);

-- Optional: Add indexes for performance
CREATE INDEX IX_Product_Colors_ProductSysNo ON Product_Colors (ProductSysNo);
-- CREATE INDEX IX_Products_ProductName ON Products (ProductName); -- If you frequently query by name
