-- Скрипт создания базы данных для PostgreSQL
-- Имя БД: ComputerPartsSales

-- Удаление таблиц, если они существуют (в обратном порядке зависимостей)
DROP TABLE IF EXISTS OrderDetails;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS Employees;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS Suppliers;
DROP TABLE IF EXISTS Categories;

-- 1. Таблица Категорий
CREATE TABLE Categories (
    CategoryID SERIAL PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(255)
);

-- 2. Таблица Поставщиков
CREATE TABLE Suppliers (
    SupplierID SERIAL PRIMARY KEY,
    CompanyName VARCHAR(100) NOT NULL,
    ContactName VARCHAR(100),
    Phone VARCHAR(20),
    Email VARCHAR(100),
    Address VARCHAR(255)
);

-- 3. Таблица Клиентов
CREATE TABLE Customers (
    CustomerID SERIAL PRIMARY KEY,
    FullName VARCHAR(150) NOT NULL,
    Phone VARCHAR(20),
    Email VARCHAR(100),
    Address VARCHAR(255),
    RegistrationDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 4. Таблица Сотрудников
CREATE TABLE Employees (
    EmployeeID SERIAL PRIMARY KEY,
    FullName VARCHAR(150) NOT NULL,
    Position VARCHAR(100),
    Login VARCHAR(50),
    PasswordHash VARCHAR(255)
);

-- 5. Таблица Товаров
CREATE TABLE Products (
    ProductID SERIAL PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Price DECIMAL(18, 2) NOT NULL,
    StockQuantity INT DEFAULT 0,
    CategoryID INT REFERENCES Categories(CategoryID),
    SupplierID INT REFERENCES Suppliers(SupplierID),
    WarrantyPeriodMonth INT,
    Image VARCHAR(255) -- Путь к файлу картинки (относительный или URL)
);

-- 6. Таблица Заказов
CREATE TABLE Orders (
    OrderID SERIAL PRIMARY KEY,
    Date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CustomerID INT REFERENCES Customers(CustomerID),
    EmployeeID INT REFERENCES Employees(EmployeeID),
    TotalAmount DECIMAL(18, 2) DEFAULT 0,
    Status VARCHAR(50) DEFAULT 'Новый' -- Новый, Оплачен, Завершен, Отменен
);

-- 7. Таблица Деталей Заказа
CREATE TABLE OrderDetails (
    OrderDetailID SERIAL PRIMARY KEY,
    OrderID INT REFERENCES Orders(OrderID),
    ProductID INT REFERENCES Products(ProductID),
    Quantity INT NOT NULL,
    PriceAtSale DECIMAL(18, 2) NOT NULL
);

-- Тестовые данные
INSERT INTO Categories (CategoryName, Description) VALUES 
('Процессоры', 'CPU для настольных ПК и серверов'),
('Видеокарты', 'GPU для игр и работы'),
('Оперативная память', 'Модули RAM DDR4/DDR5'),
('Материнские платы', 'Основа для сборки ПК');

INSERT INTO Suppliers (CompanyName, ContactName, Phone, Email, Address) VALUES
('TechGlobal', 'John Doe', '+123456789', 'contact@techglobal.com', 'NY, 5th Avenue'),
('ChinaComponents', 'Li Wei', '+987654321', 'sales@chinacomp.cn', 'Shenzhen, Tech Park');

INSERT INTO Customers (FullName, Phone, Email, Address) VALUES
('Иванов Иван Иванович', '+79001112233', 'ivanov@mail.ru', 'г. Москва, ул. Ленина 10'),
('Петров Петр Петрович', '+79004445566', 'petrov@gmail.com', 'г. Санкт-Петербург, Невский пр. 25');

INSERT INTO Employees (FullName, Position, Login) VALUES
('Сидоров Сидор', 'Менеджер', 'manager1'),
('Админов Админ', 'Администратор', 'admin');

INSERT INTO Products (Name, Price, StockQuantity, CategoryID, SupplierID, WarrantyPeriodMonth) VALUES
('Intel Core i5-12400F', 12500.00, 15, 1, 1, 12),
('AMD Ryzen 5 5600X', 14000.00, 10, 1, 1, 12),
('NVIDIA GeForce RTX 3060', 35000.00, 5, 2, 2, 24),
('Kingston Fury Beast 16GB', 4500.00, 40, 3, 1, 60),
('ASUS Prime B660M-K', 9800.00, 8, 4, 2, 36);

INSERT INTO Orders (CustomerID, EmployeeID, TotalAmount, Status) VALUES
(1, 1, 47500.00, 'Завершен'),
(2, 1, 14000.00, 'Новый');

INSERT INTO OrderDetails (OrderID, ProductID, Quantity, PriceAtSale) VALUES
(1, 1, 1, 12500.00),
(1, 3, 1, 35000.00),
(2, 2, 1, 14000.00);
