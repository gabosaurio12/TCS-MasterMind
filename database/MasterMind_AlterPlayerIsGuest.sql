USE [MasterMind]
GO

IF COL_LENGTH('dbo.Player', 'is_guest') IS NULL
BEGIN
    ALTER TABLE [dbo].[Player] ADD [is_guest] bit NOT NULL CONSTRAINT [DF_Player_is_guest] DEFAULT 0
END
GO
