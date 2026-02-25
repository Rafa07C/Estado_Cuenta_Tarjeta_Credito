-- Create DB
IF DB_ID('CreditCardStatementDb') IS NULL
BEGIN
    CREATE DATABASE CreditCardStatementDb;
END
GO

USE CreditCardStatementDb;
GO

-- Tables (drop if you want a clean recreate during dev)
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
    InterestRate          DECIMAL(9,4) NOT NULL, -- e.g. 0.25 = 25%
    MinimumPaymentRate    DECIMAL(9,4) NOT NULL, -- e.g. 0.05 = 5%
    UpdatedAt             DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Config_Rates CHECK (InterestRate >= 0 AND MinimumPaymentRate >= 0)
);
GO

/* =========================
   Seed data (demo)
   ========================= */

-- Config singleton row (ConfigId=1) - idempotent upsert style (run once per script)
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

-- Insert CardHolder
INSERT INTO dbo.CardHolders(FullName)
VALUES (N'Iván Patiño');

-- Get the identity value generated in THIS scope
SET @CardHolderId = CAST(SCOPE_IDENTITY() AS INT);

-- Insert CreditCard using the real CardHolderId
INSERT INTO dbo.CreditCards(CardHolderId, CardNumberLast4, CreditLimit)
VALUES (@CardHolderId, '1234', 10000.00);
GO

/* =========================
   Stored Procedures
   ========================= */

-- Add Purchase
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

    -- Basic validations
    IF @Amount <= 0
        THROW 50001, 'Amount must be greater than 0.', 1;

    IF @Description IS NULL OR LTRIM(RTRIM(@Description)) = ''
        THROW 50002, 'Description is required for purchases.', 1;

    -- Card must exist and be active
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50020, 'CardId does not exist.', 1;

    IF EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId AND IsActive = 0)
        THROW 50021, 'Card is inactive.', 1;

    -- Optional (recommended): prevent exceeding credit limit
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
        THROW 50022, 'Purchase exceeds credit limit.', 1;

    INSERT INTO dbo.Transactions(CardId, TxDate, Description, Amount, TxType)
    VALUES (@CardId, @TxDate, @Description, @Amount, 'PURCHASE');
END
GO

-- Add Payment
IF OBJECT_ID('dbo.sp_AddPayment', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AddPayment;
GO
CREATE PROCEDURE dbo.sp_AddPayment
    @CardId INT,
    @TxDate DATE,
    @Amount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Amount <= 0
        THROW 50003, 'Amount must be greater than 0.', 1;

    -- Card must exist and be active
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50030, 'CardId does not exist.', 1;

    IF EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId AND IsActive = 0)
        THROW 50031, 'Card is inactive.', 1;

    INSERT INTO dbo.Transactions(CardId, TxDate, Description, Amount, TxType)
    VALUES (@CardId, @TxDate, NULL, @Amount, 'PAYMENT');
END
GO

-- Get Month Transactions (purchases + payments, descending)
IF OBJECT_ID('dbo.sp_GetMonthTransactions', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetMonthTransactions;
GO
CREATE PROCEDURE dbo.sp_GetMonthTransactions
    @CardId INT,
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate card exists
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50040, 'CardId does not exist.', 1;

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

-- Get Statement (includes calculated fields)
IF OBJECT_ID('dbo.sp_GetStatement', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetStatement;
GO
CREATE PROCEDURE dbo.sp_GetStatement
    @CardId INT,
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate card exists and active
    IF NOT EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId)
        THROW 50011, 'CardId does not exist.', 1;

    IF EXISTS (SELECT 1 FROM dbo.CreditCards WHERE CardId = @CardId AND IsActive = 0)
        THROW 50012, 'Card is inactive.', 1;

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

    -- Validate config exists
    IF @InterestRate IS NULL OR @MinPayRate IS NULL
        THROW 50010, 'StatementConfig (ConfigId=1) is missing. Seed or insert config first.', 1;

    -- Current balance: purchases - payments (all time)
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

    -- If balance is negative (overpayment), clamp to 0 for interest/min payment calculations
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