USE [MasterMind]
GO

IF OBJECT_ID('dbo.MatchRoundState', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MatchRoundState] (
        [match_room_id] int NOT NULL PRIMARY KEY,
        [round_number] int NOT NULL CONSTRAINT [DF_MatchRoundState_round_number] DEFAULT 1,
        [player_one_points] int NOT NULL CONSTRAINT [DF_MatchRoundState_p1_points] DEFAULT 0,
        [player_two_points] int NOT NULL CONSTRAINT [DF_MatchRoundState_p2_points] DEFAULT 0,
        [started_at_utc] datetime2 NULL,
        [deadline_utc] datetime2 NULL,
        [finished_username] nvarchar(255) NULL,
        [finished_solved] bit NULL,
        [finished_tries] int NULL,
        CONSTRAINT [FK_MatchRoundState_MatchRoom] FOREIGN KEY ([match_room_id])
            REFERENCES [dbo].[MatchRoom] ([match_room_id]) ON DELETE CASCADE
    )
END
GO
