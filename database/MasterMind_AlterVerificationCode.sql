USE [MasterMind]
GO

IF COL_LENGTH('dbo.VerificationCode', 'created_at') IS NULL
BEGIN
    ALTER TABLE [dbo][VerificationCode]
        ADD [created_at] datetime2 NOT NULL
        CONSTRAINT [DF_VerificationCode_created_at] DEFAULT SYSUTCDATETIME()
END
GO
