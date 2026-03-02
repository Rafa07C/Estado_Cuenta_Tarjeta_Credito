-- =============================================
-- CreditCardStatementDb - Script de base de datos
-- =============================================

-- Crear base de datos
IF DB_ID('CreditCardStatementDb') IS NULL
BEGIN
    CREATE DATABASE CreditCardStatementDb;
END
GO

USE CreditCardStatementDb;
GO

-- =============================================
-- Tablas
-- =============================================

IF OBJECT_ID('dbo.Transactions', 'U') IS NOT NULL DROP TABLE dbo.Transactions;
IF OBJECT_ID('dbo.CreditCards', 'U') IS NOT NULL DROP TABLE dbo.CreditCards;
IF OBJECT_ID('dbo.CardHolders', 'U') IS NOT NULL DROP TABLE dbo.CardHolders;
IF OBJECT_ID('dbo.StatementConfig', 'U') IS NOT NULL DROP TABLE dbo.StatementConfig;
GO

CREATE TABLE dbo.CardHolders
(
    CardHolderId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName     NVARCHAR(150) NOT NULL,
    CreatedAt    DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.CreditCards
(
    CardId           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CardHolderId     INT NOT NULL,
    CardNumberLast4  CHAR(4) NOT NULL,
    CreditLimit      DECIMAL(18,2) NOT NULL,
    IsActive         BIT NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CreditCards_CardHolders
        FOREIGN KEY (CardHolderId) REFERENCES dbo.CardHolders(CardHolderId)
);

CREATE TABLE dbo.Transactions
(
    TransactionId    BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CardId           INT NOT NULL,
    TxDate           DATE NOT NULL,
    Description      NVARCHAR(200) NULL,
    Amount           DECIMAL(18,2) NOT NULL,
    TxType           VARCHAR(10) NOT NULL,  -- 'PURCHASE' or 'PAYMENT'
    CreatedAt        DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Transactions_CreditCards
        FOREIGN KEY (CardId) REFERENCES dbo.CreditCards(CardId),
    CONSTRAINT CK_Transactions_Amount_Positive CHECK (Amount > 0),
    CONSTRAINT CK_Transactions_TxType CHECK (TxType IN ('PURCHASE','PAYMENT'))
);
GO

CREATE INDEX IX_Transactions_CardId_TxDate
ON dbo.Transactions(CardId, TxDate);
GO

CREATE TABLE dbo.StatementConfig
(
    ConfigId              INT NOT NULL PRIMARY KEY,
    InterestRate          DECIMAL(9,4) NOT NULL, -- ej. 0.25 = 25%
    MinimumPaymentRate    DECIMAL(9,4) NOT NULL, -- ej. 0.05 = 5%
    UpdatedAt             DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Config_Rates CHECK (InterestRate >= 0 AND MinimumPaymentRate >= 0)
);
GO

-- =============================================
-- Datos de prueba
-- =============================================

-- Configuración (fila única ConfigId=1)
IF EXISTS (SELECT 1 FROM dbo.StatementConfig WHERE ConfigId = 1)
BEGIN
    UPDATE dbo.StatementConfig
    SET InterestRate = 0.25,
        MinimumPaymentRate = 0.05,
        UpdatedAt = SYSUTCDATETIME()
    WHERE ConfigId = 1;
END
ELSE
BEGIN
    INSERT INTO dbo.StatementConfig(ConfigId, InterestRate, MinimumPaymentRate)
    VALUES (1, 0.25, 0.05);
END
GO

DECLARE @CardHolderId INT;

INSERT INTO dbo.CardHolders(FullName)
VALUES (N'Iván Patiño');

SET @CardHolderId = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.CreditCards(CardHolderId, CardNumberLast4, CreditLimit)
VALUES (@CardHolderId, '1234', 10000.00);
GO

-- =============================================
-- Procedimientos Almacenados
-- =============================================

-- Agregar Compra
IF OBJECT_ID('dbo.sp_AddPurchase', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AddPurchase;
GO
CREATE PROCEDURE dbo.sp_AddPurchase
    @CardId INT,
    @TxDate DATE,
    @Description NVARCHAR(200),
    @Amount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validaciones básicas
    IF @Amount <= 0
        THROW 50001, 'El monto debe ser mayor a 0.', 1;

    IF @Description IS NULL OR LTRIM(RTRIM(@Description)) = ''
        THROW 50002, 'La descripción es requerida para las compras.', 1;

    -- La tarjeta debe existir y estar activa
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50020, 'El CardId no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId AND IsActive = 0)
        THROW 50021, 'La tarjeta está inactiva.', 1;

    -- La fecha no puede ser futura
    IF @TxDate > CAST(GETDATE() AS DATE)
        THROW 50023, 'La fecha de la compra no puede ser futura.', 1;

    -- Validar que la compra no exceda el límite de crédito
    DECLARE @CurrentBalance DECIMAL(18,2) =
    (
        SELECT ISNULL(SUM(CASE WHEN TxType='PURCHASE' THEN Amount ELSE -Amount END), 0.00)
        FROM dbo.Transactions
        WHERE CardId = @CardId
    );

    DECLARE @CreditLimit DECIMAL(18,2) =
    (
        SELECT CreditLimit FROM dbo.CreditCards WHERE CardId = @CardId
    );

    IF (@CurrentBalance + @Amount) > @CreditLimit
        THROW 50022, 'La compra excede el límite de crédito disponible.', 1;

    INSERT INTO dbo.Transactions(CardId, TxDate, Description, Amount, TxType)
    VALUES (@CardId, @TxDate, @Description, @Amount, 'PURCHASE');
END
GO

-- Agregar Pago
IF OBJECT_ID('dbo.sp_AddPayment', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AddPayment;
GO
CREATE PROCEDURE dbo.sp_AddPayment
    @CardId INT,
    @TxDate DATE,
    @Amount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validaciones básicas
    IF @Amount <= 0
        THROW 50003, 'El monto debe ser mayor a 0.', 1;

    -- La tarjeta debe existir y estar activa
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50030, 'El CardId no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId AND IsActive = 0)
        THROW 50031, 'La tarjeta está inactiva.', 1;

    -- Validar que el pago no exceda el saldo actual
    DECLARE @CurrentBalance DECIMAL(18,2) =
    (
        SELECT ISNULL(SUM(CASE WHEN TxType='PURCHASE' THEN Amount ELSE -Amount END), 0.00)
        FROM dbo.Transactions
        WHERE CardId = @CardId
    );

    IF @Amount > @CurrentBalance
        THROW 50032, 'El monto del pago no puede exceder el saldo actual.', 1;

    -- La fecha no puede ser futura
    IF @TxDate > CAST(GETDATE() AS DATE)
        THROW 50033, 'La fecha del pago no puede ser futura.', 1;

    INSERT INTO dbo.Transactions(CardId, TxDate, Description, Amount, TxType)
    VALUES (@CardId, @TxDate, NULL, @Amount, 'PAYMENT');
END
GO

-- Obtener Transacciones del Mes (compras y pagos, orden descendente)
IF OBJECT_ID('dbo.sp_GetMonthTransactions', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetMonthTransactions;
GO
CREATE PROCEDURE dbo.sp_GetMonthTransactions
    @CardId INT,
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar que la tarjeta existe
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50040, 'El CardId no existe.', 1;

    DECLARE @Start DATE = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @End   DATE = DATEADD(MONTH, 1, @Start);

    SELECT
        TransactionId,
        CardId,
        TxDate,
        Description,
        Amount,
        TxType
    FROM dbo.Transactions
    WHERE CardId = @CardId
      AND TxDate >= @Start AND TxDate < @End
    ORDER BY TxDate DESC, TransactionId DESC;
END
GO

-- Obtener Estado de Cuenta (incluye campos calculados)
IF OBJECT_ID('dbo.sp_GetStatement', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetStatement;
GO
CREATE PROCEDURE dbo.sp_GetStatement
    @CardId INT,
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar que la tarjeta existe y está activa
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50011, 'El CardId no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId AND IsActive = 0)
        THROW 50012, 'La tarjeta está inactiva.', 1;

    DECLARE @Start DATE = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @End   DATE = DATEADD(MONTH, 1, @Start);

    DECLARE @PrevStart DATE = DATEADD(MONTH, -1, @Start);
    DECLARE @PrevEnd   DATE = @Start;

    DECLARE @InterestRate DECIMAL(9,4);
    DECLARE @MinPayRate   DECIMAL(9,4);

    SELECT
        @InterestRate = InterestRate,
        @MinPayRate   = MinimumPaymentRate
    FROM dbo.StatementConfig
    WHERE ConfigId = 1;

    -- Validar que existe la configuración
    IF @InterestRate IS NULL OR @MinPayRate IS NULL
        THROW 50010, 'La configuración del estado de cuenta (ConfigId=1) no existe.', 1;

    -- Saldo actual: compras menos pagos (histórico total)
    DECLARE @CurrentBalance DECIMAL(18,2) =
    (
        SELECT ISNULL(SUM(CASE WHEN TxType='PURCHASE' THEN Amount ELSE -Amount END), 0.00)
        FROM dbo.Transactions
        WHERE CardId = @CardId
    );

    DECLARE @CreditLimit DECIMAL(18,2) =
    (
        SELECT CreditLimit FROM dbo.CreditCards WHERE CardId = @CardId
    );

    DECLARE @AvailableBalance DECIMAL(18,2) = @CreditLimit - @CurrentBalance;

    DECLARE @PurchasesThisMonth DECIMAL(18,2) =
    (
        SELECT ISNULL(SUM(Amount), 0.00)
        FROM dbo.Transactions
        WHERE CardId = @CardId
          AND TxType = 'PURCHASE'
          AND TxDate >= @Start AND TxDate < @End
    );

    DECLARE @PurchasesPrevMonth DECIMAL(18,2) =
    (
        SELECT ISNULL(SUM(Amount), 0.00)
        FROM dbo.Transactions
        WHERE CardId = @CardId
          AND TxType = 'PURCHASE'
          AND TxDate >= @PrevStart AND TxDate < @PrevEnd
    );

    -- Si el saldo es negativo (sobrepago), se usa 0 para los cálculos
    DECLARE @BalanceForCalc DECIMAL(18,2) = CASE WHEN @CurrentBalance > 0 THEN @CurrentBalance ELSE 0 END;

    DECLARE @InterestBonificable DECIMAL(18,2) = ROUND(@BalanceForCalc * @InterestRate, 2);
    DECLARE @MinimumPayment      DECIMAL(18,2) = ROUND(@BalanceForCalc * @MinPayRate, 2);
    DECLARE @TotalWithInterest   DECIMAL(18,2) = @BalanceForCalc + @InterestBonificable;

    SELECT
        ch.FullName                         AS CardHolderName,
        cc.CardId                           AS CardId,
        cc.CardNumberLast4                  AS CardNumberLast4,
        cc.CreditLimit                      AS CreditLimit,
        @CurrentBalance                     AS CurrentBalance,
        @AvailableBalance                   AS AvailableBalance,
        @PurchasesThisMonth                 AS PurchasesThisMonth,
        @PurchasesPrevMonth                 AS PurchasesPreviousMonth,
        @InterestRate                       AS InterestRate,
        @MinPayRate                         AS MinimumPaymentRate,
        @InterestBonificable                AS InterestBonificable,
        @MinimumPayment                     AS MinimumPayment,
        @BalanceForCalc                     AS TotalToPay,
        @TotalWithInterest                  AS TotalToPayWithInterest
    FROM dbo.CreditCards cc
    INNER JOIN dbo.CardHolders ch ON ch.CardHolderId = cc.CardHolderId
    WHERE cc.CardId = @CardId;
END
GO
