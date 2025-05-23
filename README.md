# 项目介绍
这是一个shopify的自定义应用，用来进行商品和订单同步。把已有的商品同步到shopify上售卖，同时把shopify上的订单同步到私有系统。

# 基本环境是
 - .Net8.0  
 - SqlServer2022

# 数据结构
商品有：SysNo、Status等字段，表示主键、状态以及商品的其他属性
> SysNo int
> Status short 状态枚举：0-无效，1-有效，2-停止售卖
订单有信息有：OrderID、ProudctSysNo、Quantity、Status、ShippingAddress等
> OrderID int 订单ID
> ProudctSysNo int 商品ID
> Status short 订单状态

# 项目要求
- 私有系统可以控制商品在shopify上的状态。例如：上线、下线
- shopify下单后自动把订单同步到私有系统
- 私有系统对订单的处理进度，反馈到shopify
- 对接shopify的支付


