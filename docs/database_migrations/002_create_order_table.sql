-- Create Order Table
CREATE TABLE [Order] (
    SysNo INT PRIMARY KEY IDENTITY(1,1),
    ProductSysNo INT,
    Status SMALLINT,
    Price DECIMAL(18, 2),
    Quantity INT,
    Color NVARCHAR(50),
    ShippingAddress NVARCHAR(500),
    PaymentStatus INT,
    CONSTRAINT FK_Order_Product FOREIGN KEY (ProductSysNo) REFERENCES Products(SysNo)
);

-- Create Index on ProductSysNo
CREATE INDEX IX_Order_ProductSysNo ON [Order](ProductSysNo);
