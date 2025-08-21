-- SQL Script to Insert C25 (OMXC25) Danish Stock Exchange Stocks
-- These are the 25 most traded stocks on Nasdaq Copenhagen
-- Stock codes are formatted for Yahoo Finance API (.CO suffix for Copenhagen)

USE [Aktier];
GO

-- Insert C25 stocks into the Stocks table
-- Using MERGE to avoid duplicates if script is run multiple times
MERGE dbo.Stocks AS target
USING (
    VALUES 
        ('NOVO-B.CO', 'Novo Nordisk B'),
        ('ORSTED.CO', 'Ørsted'),
        ('CARL-B.CO', 'Carlsberg B'),
        ('MAERSK-B.CO', 'A.P. Møller - Mærsk B'),
        ('DSV.CO', 'DSV'),
        ('GMAB.CO', 'Genmab'),
        ('NZYM-B.CO', 'Novozymes B'),
        ('PNDORA.CO', 'Pandora'),
        ('VWS.CO', 'Vestas Wind Systems'),
        ('AMBU-B.CO', 'Ambu B'),
        ('DEMANT.CO', 'Demant'),
        ('CHR.CO', 'Chr. Hansen Holding'),
        ('COLB.CO', 'Coloplast B'),
        ('TRYG.CO', 'Tryg'),
        ('FLS.CO', 'FLSmidth'),
        ('DANSKE.CO', 'Danske Bank'),
        ('ROCK-B.CO', 'Rockwool International B'),
        ('ISS.CO', 'ISS'),
        ('JYSK.CO', 'Jyske Bank'),
        ('GN.CO', 'GN Store Nord'),
        ('NETC.CO', 'Netcompany Group'),
        ('RILBA.CO', 'Ringkjøbing Landbobank'),
        ('RBREW.CO', 'Royal Unibrew'),
        ('SIM.CO', 'SimCorp'),
        ('DKIGI.CO', 'Dankse Bank Indices')
) AS source (StockCode, FriendlyName)
ON target.StockCode = source.StockCode
WHEN MATCHED THEN
    UPDATE SET 
        FriendlyName = source.FriendlyName,
        UpdatedDate = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (StockCode, FriendlyName, CreatedDate, UpdatedDate)
    VALUES (source.StockCode, source.FriendlyName, GETUTCDATE(), GETUTCDATE());

-- Display the results
SELECT 
    StockCode,
    FriendlyName,
    CreatedDate,
    UpdatedDate
FROM dbo.Stocks 
WHERE StockCode LIKE '%.CO'
ORDER BY FriendlyName;

PRINT 'C25 Danish stocks have been inserted/updated successfully.';
PRINT 'Total C25 stocks: 25';

-- Optional: Show count of total stocks in database
SELECT COUNT(*) AS TotalStocks FROM dbo.Stocks;