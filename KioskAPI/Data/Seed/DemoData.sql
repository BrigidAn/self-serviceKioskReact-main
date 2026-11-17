-- Demo data for the Tech Kiosk database (kiosk2).
-- All demo users log in with password: Demo1234
-- Admin: admin@kiosk.com | Users: becy@, stacy@, garfield@, marie@kiosk.com
-- Run on an EMPTY database (after `dotnet ef database update`).

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
IF EXISTS (SELECT 1 FROM AspNetUsers) OR EXISTS (SELECT 1 FROM Products)
BEGIN PRINT 'Database already has data - demo seed skipped.'; RETURN; END;

BEGIN TRY
BEGIN TRANSACTION;

-- Users
SET IDENTITY_INSERT AspNetUsers ON;
INSERT INTO AspNetUsers (Id,Name,CreatedAt,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount) VALUES (1,N'Admin User','2026-10-01',N'admin@kiosk.com',N'ADMIN@KIOSK.COM',N'admin@kiosk.com',N'ADMIN@KIOSK.COM',1,N'AQAAAAIAAYagAAAAEI2shvKWEFrp5X1WHe46LcEs7UZVo2x2xM3df/+6e/qvGR2dGrAOG+YiDa+GNz4sIA==',N'FC8296577BAB4571BCC04AC8FCFBDEF9',N'b5748ba6-6a75-4d84-a9c8-c99864cfb095',0,0,1,0);
INSERT INTO AspNetUsers (Id,Name,CreatedAt,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount) VALUES (2,N'Becy Mokoena','2026-10-01',N'becy@kiosk.com',N'BECY@KIOSK.COM',N'becy@kiosk.com',N'BECY@KIOSK.COM',1,N'AQAAAAIAAYagAAAAEFX588lz5cR56rLNN3JLkEzAtAu2W3wW15jAWQccWnaE8KROxBxTqEuOkccYz931tQ==',N'465BD1445BE94AC1AB47A4283856B8BD',N'1b02af34-884a-4720-ba0d-3819ee825abb',0,0,1,0);
INSERT INTO AspNetUsers (Id,Name,CreatedAt,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount) VALUES (3,N'Stacy Naidoo','2026-10-01',N'stacy@kiosk.com',N'STACY@KIOSK.COM',N'stacy@kiosk.com',N'STACY@KIOSK.COM',1,N'AQAAAAIAAYagAAAAEKQHrZcDlyuOVzk/4dpbzwALTyTdWsXWMSMdb5CkylTV1C62tNmOudsk7QhVoAfv2g==',N'06B104706C0A4A67AA4BC2942FE456BB',N'fab74afd-3e94-4150-bb28-8b67bd425ab3',0,0,1,0);
INSERT INTO AspNetUsers (Id,Name,CreatedAt,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount) VALUES (4,N'Garfield Dlamini','2026-10-01',N'garfield@kiosk.com',N'GARFIELD@KIOSK.COM',N'garfield@kiosk.com',N'GARFIELD@KIOSK.COM',1,N'AQAAAAIAAYagAAAAEKDFJrEF/2K+x7DrKosRsflF+hCrryb/u+/VZPEInWhh0iU5a5flfRw5GKQ26pPWjA==',N'AA29E38CB82B41B6BDB792FAC79225F2',N'f05b789f-1e3c-43d0-94c7-8ad528365270',0,0,1,0);
INSERT INTO AspNetUsers (Id,Name,CreatedAt,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount) VALUES (5,N'Marie Botha','2026-10-01',N'marie@kiosk.com',N'MARIE@KIOSK.COM',N'marie@kiosk.com',N'MARIE@KIOSK.COM',1,N'AQAAAAIAAYagAAAAENhqzzQHL/8a1pTEvN918m3lYbn6KsvhqRi0kLtDv+YEHFtFtqEmIlBVV+584KQ94A==',N'8061856C5E534906846EA93DF77925EA',N'd806669d-2582-41b2-8efd-dfbc01df7f5f',0,0,1,0);
SET IDENTITY_INSERT AspNetUsers OFF;

INSERT INTO AspNetUserRoles (UserId,RoleId) VALUES (1,1);
INSERT INTO AspNetUserRoles (UserId,RoleId) VALUES (2,2);
INSERT INTO AspNetUserRoles (UserId,RoleId) VALUES (3,2);
INSERT INTO AspNetUserRoles (UserId,RoleId) VALUES (4,2);
INSERT INTO AspNetUserRoles (UserId,RoleId) VALUES (5,2);

-- Suppliers
SET IDENTITY_INSERT Suppliers ON;
INSERT INTO Suppliers (SupplierId,Name,ContactInfo) VALUES (1,N'TechSource Distributors',N'sales@techsource.example | 011 555 0101');
INSERT INTO Suppliers (SupplierId,Name,ContactInfo) VALUES (2,N'SoundWave Audio',N'orders@soundwave.example | 021 555 0202');
INSERT INTO Suppliers (SupplierId,Name,ContactInfo) VALUES (3,N'FutureBots Ltd',N'hello@futurebots.example | 031 555 0303');
INSERT INTO Suppliers (SupplierId,Name,ContactInfo) VALUES (4,N'Immersive Realities',N'support@immersive.example | 012 555 0404');
SET IDENTITY_INSERT Suppliers OFF;

-- Products
SET IDENTITY_INSERT Products ON;
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (1,N'Wireless Charging Pad',N'15W fast Qi wireless charger with LED indicator.',349.99,N'Accessories',40,1,N'https://placehold.co/600x400?text=Wireless+Charging+Pad',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (2,N'USB-C Hub 7-in-1',N'HDMI 4K, 3x USB 3.0, SD/microSD and 100W PD pass-through.',599.00,N'Accessories',25,1,N'https://placehold.co/600x400?text=USB-C+Hub+7-in-1',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (3,N'Mechanical Keyboard',N'RGB backlit keyboard with hot-swappable brown switches.',1299.00,N'Accessories',15,1,N'https://placehold.co/600x400?text=Mechanical+Keyboard',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (4,N'Laptop Stand',N'Adjustable aluminium stand for 11-17 inch laptops.',449.50,N'Accessories',30,1,N'https://placehold.co/600x400?text=Laptop+Stand',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (5,N'AirBeat Pro Earbuds',N'Active noise cancelling true wireless earbuds, 30h battery.',1899.00,N'Earbuds',35,2,N'https://placehold.co/600x400?text=AirBeat+Pro+Earbuds',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (6,N'SportFit Earbuds',N'Sweat-resistant earbuds with secure ear hooks.',799.00,N'Earbuds',50,2,N'https://placehold.co/600x400?text=SportFit+Earbuds',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (7,N'BassBuds Mini',N'Compact earbuds with deep bass and touch controls.',499.99,N'Earbuds',0,2,N'https://placehold.co/600x400?text=BassBuds+Mini',0);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (8,N'StudioMax Headset',N'Over-ear studio headphones with 40mm drivers.',2499.00,N'Headsets',12,2,N'https://placehold.co/600x400?text=StudioMax+Headset',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (9,N'GamerX 7.1 Headset',N'Surround sound gaming headset with detachable mic.',1599.00,N'Headsets',20,2,N'https://placehold.co/600x400?text=GamerX+7.1+Headset',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (10,N'Office Call Headset',N'USB headset with noise-cancelling boom mic for meetings.',899.00,N'Headsets',18,2,N'https://placehold.co/600x400?text=Office+Call+Headset',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (11,N'CodeBot Junior',N'Programmable robot kit that teaches kids to code.',2299.00,N'Robots',10,3,N'https://placehold.co/600x400?text=CodeBot+Junior',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (12,N'RoboVac S5',N'Smart robot vacuum with app control and auto-docking.',4999.00,N'Robots',6,3,N'https://placehold.co/600x400?text=RoboVac+S5',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (13,N'Mini Drone Explorer',N'Foldable camera drone with 1080p video and 20 min flight.',3499.00,N'Robots',8,3,N'https://placehold.co/600x400?text=Mini+Drone+Explorer',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (14,N'VisionVR One',N'Standalone VR headset, 128GB, 6DoF tracking.',8999.00,N'VR',5,4,N'https://placehold.co/600x400?text=VisionVR+One',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (15,N'VR Controller Grips',N'Silicone grips and knuckle straps for VR controllers.',299.00,N'VR',45,4,N'https://placehold.co/600x400?text=VR+Controller+Grips',1);
INSERT INTO Products (ProductId,Name,Description,Price,Category,Quantity,SupplierId,ImageUrl,IsAvailable) VALUES (16,N'Phone VR Viewer',N'Lightweight VR viewer for 4.7-6.7 inch smartphones.',399.00,N'VR',22,4,N'https://placehold.co/600x400?text=Phone+VR+Viewer',1);
SET IDENTITY_INSERT Products OFF;

-- Orders and order items
SET IDENTITY_INSERT Orders ON;
INSERT INTO Orders (OrderId,UserId,OrderDate,TotalAmount,Status,DeliveryMethod,PaymentStatus,DeliveryFee) VALUES (1,2,'2026-11-02 10:15',2248.99,N'Completed',N'pickup',N'Paid',0.00);
INSERT INTO Orders (OrderId,UserId,OrderDate,TotalAmount,Status,DeliveryMethod,PaymentStatus,DeliveryFee) VALUES (2,3,'2026-11-05 14:40',2277.00,N'Completed',N'delivery',N'Paid',80.00);
INSERT INTO Orders (OrderId,UserId,OrderDate,TotalAmount,Status,DeliveryMethod,PaymentStatus,DeliveryFee) VALUES (3,4,'2026-11-09 09:05',2379.00,N'Completed',N'delivery',N'Paid',80.00);
INSERT INTO Orders (OrderId,UserId,OrderDate,TotalAmount,Status,DeliveryMethod,PaymentStatus,DeliveryFee) VALUES (4,5,'2026-11-12 16:20',1748.50,N'Pending',N'pickup',N'Unpaid',0.00);
INSERT INTO Orders (OrderId,UserId,OrderDate,TotalAmount,Status,DeliveryMethod,PaymentStatus,DeliveryFee) VALUES (5,2,'2026-11-14 11:30',1678.00,N'Pending',N'delivery',N'Unpaid',80.00);
SET IDENTITY_INSERT Orders OFF;
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (1,5,1,1899.00,'2026-11-02 10:15',DATEADD(day,1,'2026-11-02 10:15'));
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (1,1,1,349.99,'2026-11-02 10:15',DATEADD(day,1,'2026-11-02 10:15'));
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (2,9,1,1599.00,'2026-11-05 14:40',DATEADD(day,1,'2026-11-05 14:40'));
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (2,15,2,299.00,'2026-11-05 14:40',DATEADD(day,1,'2026-11-05 14:40'));
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (3,11,1,2299.00,'2026-11-09 09:05',DATEADD(day,1,'2026-11-09 09:05'));
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (4,3,1,1299.00,'2026-11-12 16:20',DATEADD(day,1,'2026-11-12 16:20'));
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (4,4,1,449.50,'2026-11-12 16:20',DATEADD(day,1,'2026-11-12 16:20'));
INSERT INTO OrderItems (OrderId,ProductId,Quantity,PriceAtPurchase,AddedAt,ExpiresAt) VALUES (5,6,2,799.00,'2026-11-14 11:30',DATEADD(day,1,'2026-11-14 11:30'));

-- Wallet accounts and transactions
SET IDENTITY_INSERT Accounts ON;
INSERT INTO Accounts (AccountId,UserId,Balance,LastUpdated,TransactionsSummary) VALUES (1,2,2751.01,'2026-11-14',NULL);
INSERT INTO Accounts (AccountId,UserId,Balance,LastUpdated,TransactionsSummary) VALUES (2,3,1723.00,'2026-11-14',NULL);
INSERT INTO Accounts (AccountId,UserId,Balance,LastUpdated,TransactionsSummary) VALUES (3,4,3621.00,'2026-11-14',NULL);
INSERT INTO Accounts (AccountId,UserId,Balance,LastUpdated,TransactionsSummary) VALUES (4,5,3000.00,'2026-11-14',NULL);
SET IDENTITY_INSERT Accounts OFF;
INSERT INTO Transactions (AccountId,OrderId,Type,TotalAmount,Description,CreatedAt) VALUES (1,NULL,'TopUp',5000.00,N'Wallet top-up','2026-11-01');
INSERT INTO Transactions (AccountId,OrderId,Type,TotalAmount,Description,CreatedAt) VALUES (1,1,'Checkout',2248.99,N'Payment for order #1','2026-11-02 10:15');
INSERT INTO Transactions (AccountId,OrderId,Type,TotalAmount,Description,CreatedAt) VALUES (2,NULL,'TopUp',4000.00,N'Wallet top-up','2026-11-01');
INSERT INTO Transactions (AccountId,OrderId,Type,TotalAmount,Description,CreatedAt) VALUES (2,2,'Checkout',2277.00,N'Payment for order #2','2026-11-05 14:40');
INSERT INTO Transactions (AccountId,OrderId,Type,TotalAmount,Description,CreatedAt) VALUES (3,NULL,'TopUp',6000.00,N'Wallet top-up','2026-11-01');
INSERT INTO Transactions (AccountId,OrderId,Type,TotalAmount,Description,CreatedAt) VALUES (3,3,'Checkout',2379.00,N'Payment for order #3','2026-11-09 09:05');
INSERT INTO Transactions (AccountId,OrderId,Type,TotalAmount,Description,CreatedAt) VALUES (4,NULL,'TopUp',3000.00,N'Wallet top-up','2026-11-01');

-- An open cart for Marie
SET IDENTITY_INSERT Carts ON;
INSERT INTO Carts (CartId,UserId,IsCheckedOut,ExpiresAt) VALUES (1,5,0,DATEADD(day,1,SYSUTCDATETIME()));
SET IDENTITY_INSERT Carts OFF;
INSERT INTO CartItems (CartId,ProductId,Quantity,UnitPrice) VALUES (1,8,1,2499.00);
INSERT INTO CartItems (CartId,ProductId,Quantity,UnitPrice) VALUES (1,16,1,399.00);

COMMIT TRANSACTION;
PRINT 'Demo data inserted.';
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;
