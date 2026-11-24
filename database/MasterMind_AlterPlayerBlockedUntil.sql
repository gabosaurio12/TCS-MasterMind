USE [MasterMind]
GO

IF COL_LENGTH('dbo.Player', 'blocked_until') IS NULL
BEGIN
    ALTER TABLE [dbo].[Player] ADD [blocked_until] datetime2 NULL
END
GO
