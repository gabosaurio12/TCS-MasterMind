CREATE TABLE [Player] (
  [player_id] int PRIMARY KEY IDENTITY(1, 1),
  [email] nvarchar(255) UNIQUE,
  [number_of_reports] int,
  [username] nvarchar(255) UNIQUE,
  [password] nvarchar(255)
)
GO

CREATE TABLE [VerificationCode] (
  [verification_code_id] int PRIMARY KEY IDENTITY(1, 1),
  [verification_code] nvarchar(255),
  [player_id] int UNIQUE NOT NULL
)
GO

CREATE TABLE [Friendship] (
  [friendship_id] int PRIMARY KEY IDENTITY(1, 1),
  [requester_id] int NOT NULL,
  [addressee_id] int NOT NULL,
  [status_id] int NOT NULL
)
GO

CREATE TABLE [GameInvitation] (
  [game_invitation_id] int PRIMARY KEY IDENTITY(1, 1),
  [requester_id] int NOT NULL,
  [addressee_id] int NOT NULL,
  [match_room_id] int NOT NULL,
  [status_id] int NOT NULL
)
GO

CREATE TABLE [RequestStatusCatalog] (
  [request_status_id] int PRIMARY KEY IDENTITY(1, 1),
  [status] nvarchar(255) UNIQUE NOT NULL
)
GO

CREATE TABLE [PlayerReport] (
  [report_id] int PRIMARY KEY IDENTITY(1, 1),
  [reported_player_id] int NOT NULL,
  [reported_by_player_id] int NOT NULL,
  [reason_id] int NOT NULL
)
GO

CREATE TABLE [ReportReasonCatalog] (
  [report_reason_id] int PRIMARY KEY IDENTITY(1, 1),
  [reason] nvarchar(255) UNIQUE NOT NULL
)
GO

CREATE TABLE [PlayerTimeTrialRecords] (
  [records_id] int PRIMARY KEY IDENTITY(1, 1),
  [player_id] int NOT NULL,
  [time_record] int,
  [rank] int NOT NULL CHECK ([rank] BETWEEN 1 AND 3),
  UNIQUE ([player_id], [rank])
)
GO

CREATE TABLE [PlayerTriesRecords] (
  [records_id] int PRIMARY KEY IDENTITY(1, 1),
  [player_id] int NOT NULL,
  [tries_record] int,
  [rank] int NOT NULL CHECK ([rank] BETWEEN 1 AND 3),
  UNIQUE ([player_id], [rank])
)
GO

CREATE TABLE [DecipherTry] (
  [decipher_try_id] int PRIMARY KEY IDENTITY(1, 1),
  [match_room_id] int NOT NULL,
  [player_id] int NOT NULL,
  [attempt_number] int
)
GO

CREATE TABLE [DecipherTryCode] (
  [decipher_try_id] int NOT NULL,
  [position] int,
  [color_id] int NOT NULL,
  PRIMARY KEY ([decipher_try_id], [position])
)
GO

CREATE TABLE [CorrectPositionAndColors] (
  [decipher_try_id] int PRIMARY KEY
)
GO

CREATE TABLE [CorrectPositionAndColorsClue] (
  [correct_position_and_colors_id] int NOT NULL,
  [position] int,
  [clue_color_id] int NOT NULL,
  PRIMARY KEY ([correct_position_and_colors_id], [position])
)
GO

CREATE TABLE [ClueColorsCatalog] (
  [clue_color_id] int PRIMARY KEY IDENTITY(1, 1),
  [color] nvarchar(255) UNIQUE NOT NULL
)
GO

CREATE TABLE [GameModesCatalog] (
  [gamemode_id] int PRIMARY KEY IDENTITY(1, 1),
  [gamemode] nvarchar(255) UNIQUE NOT NULL
)
GO

CREATE TABLE [MatchRoom] (
  [match_room_id] int PRIMARY KEY IDENTITY(1, 1),
  [gamemode_id] int NOT NULL,
  [player_one_id] int NOT NULL,
  [player_two_id] int NOT NULL,
  [private_room_code] nvarchar(255)
)
GO

CREATE TABLE [TimeTrialConfig] (
  [match_room_id] int PRIMARY KEY,
  [max_time_of_seconds] int
)
GO

CREATE TABLE [TriesConfig] (
  [match_room_id] int PRIMARY KEY,
  [max_of_tries] int
)
GO

CREATE TABLE [CodeColorsCatalog] (
  [code_color_id] int PRIMARY KEY IDENTITY(1, 1),
  [color] nvarchar(255) UNIQUE NOT NULL
)
GO

CREATE TABLE [MatchRoomSecretCode] (
  [position] int,
  [match_room_id] int NOT NULL,
  [color_id] int NOT NULL,
  PRIMARY KEY ([match_room_id], [position])
)
GO

CREATE TABLE [PlayerInMatch] (
  [player_in_match_id] int PRIMARY KEY IDENTITY(1, 1),
  [player_id] int NOT NULL,
  [best_time] int,
  [least_tries] int,
  [rounds_won] int
)
GO

CREATE TABLE [MatchResults] (
  [match_results_id] int PRIMARY KEY IDENTITY(1, 1),
  [match_room_id] int UNIQUE,
  [player_one_results_id] int NOT NULL,
  [player_two_results_id] int NOT NULL
)
GO

CREATE TABLE [RoundsTimeResults] (
  [rounds_time_results_id] int PRIMARY KEY IDENTITY(1, 1),
  [player_in_match_id] int NOT NULL,
  [round] int,
  [time] int
)
GO

CREATE TABLE [RoundsTriesResults] (
  [rounds_tries_results_id] int PRIMARY KEY IDENTITY(1, 1),
  [player_in_match_id] int NOT NULL,
  [round] int,
  [tries] int
)
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Cached counter. Updated via trigger_IncrementReportCount (AFTER INSERT ON PlayerReport) and trigger_DecreaseReportCount (AFTER DELETE ON PlayerReport)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Player',
@level2type = N'Column', @level2name = 'number_of_reports';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CHECK (rank BETWEEN 1 and 3)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PlayerTimeTrialRecords',
@level2type = N'Column', @level2name = 'rank';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CHECK (rank BETWEEN 1 and 3)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PlayerTriesRecords',
@level2type = N'Column', @level2name = 'rank';
GO

ALTER TABLE [VerificationCode] ADD FOREIGN KEY ([player_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [Friendship] ADD FOREIGN KEY ([requester_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [Friendship] ADD FOREIGN KEY ([addressee_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [Friendship] ADD FOREIGN KEY ([status_id]) REFERENCES [RequestStatusCatalog] ([request_status_id])
GO

ALTER TABLE [GameInvitation] ADD FOREIGN KEY ([requester_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [GameInvitation] ADD FOREIGN KEY ([addressee_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [GameInvitation] ADD FOREIGN KEY ([match_room_id]) REFERENCES [MatchRoom] ([match_room_id])
GO

ALTER TABLE [GameInvitation] ADD FOREIGN KEY ([status_id]) REFERENCES [RequestStatusCatalog] ([request_status_id])
GO

ALTER TABLE [PlayerReport] ADD FOREIGN KEY ([reported_player_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [PlayerReport] ADD FOREIGN KEY ([reported_by_player_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [PlayerReport] ADD FOREIGN KEY ([reason_id]) REFERENCES [ReportReasonCatalog] ([report_reason_id])
GO

ALTER TABLE [PlayerTimeTrialRecords] ADD FOREIGN KEY ([player_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [PlayerTriesRecords] ADD FOREIGN KEY ([player_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [DecipherTry] ADD FOREIGN KEY ([match_room_id]) REFERENCES [MatchRoom] ([match_room_id])
GO

ALTER TABLE [DecipherTry] ADD FOREIGN KEY ([player_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [DecipherTryCode] ADD FOREIGN KEY ([decipher_try_id]) REFERENCES [DecipherTry] ([decipher_try_id])
GO

ALTER TABLE [DecipherTryCode] ADD FOREIGN KEY ([color_id]) REFERENCES [CodeColorsCatalog] ([code_color_id])
GO

ALTER TABLE [CorrectPositionAndColors] ADD FOREIGN KEY ([decipher_try_id]) REFERENCES [DecipherTry] ([decipher_try_id])
GO

ALTER TABLE [CorrectPositionAndColorsClue] ADD FOREIGN KEY ([correct_position_and_colors_id]) REFERENCES [CorrectPositionAndColors] ([decipher_try_id])
GO

ALTER TABLE [CorrectPositionAndColorsClue] ADD FOREIGN KEY ([clue_color_id]) REFERENCES [ClueColorsCatalog] ([clue_color_id])
GO

ALTER TABLE [MatchRoom] ADD FOREIGN KEY ([gamemode_id]) REFERENCES [GameModesCatalog] ([gamemode_id])
GO

ALTER TABLE [MatchRoom] ADD FOREIGN KEY ([player_one_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [MatchRoom] ADD FOREIGN KEY ([player_two_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [TimeTrialConfig] ADD FOREIGN KEY ([match_room_id]) REFERENCES [MatchRoom] ([match_room_id])
GO

ALTER TABLE [TriesConfig] ADD FOREIGN KEY ([match_room_id]) REFERENCES [MatchRoom] ([match_room_id])
GO

ALTER TABLE [MatchRoomSecretCode] ADD FOREIGN KEY ([match_room_id]) REFERENCES [MatchRoom] ([match_room_id])
GO

ALTER TABLE [MatchRoomSecretCode] ADD FOREIGN KEY ([color_id]) REFERENCES [CodeColorsCatalog] ([code_color_id])
GO

ALTER TABLE [PlayerInMatch] ADD FOREIGN KEY ([player_id]) REFERENCES [Player] ([player_id])
GO

ALTER TABLE [MatchResults] ADD FOREIGN KEY ([match_room_id]) REFERENCES [MatchRoom] ([match_room_id])
GO

ALTER TABLE [MatchResults] ADD FOREIGN KEY ([player_one_results_id]) REFERENCES [PlayerInMatch] ([player_in_match_id])
GO

ALTER TABLE [MatchResults] ADD FOREIGN KEY ([player_two_results_id]) REFERENCES [PlayerInMatch] ([player_in_match_id])
GO

ALTER TABLE [RoundsTimeResults] ADD FOREIGN KEY ([player_in_match_id]) REFERENCES [PlayerInMatch] ([player_in_match_id])
GO

ALTER TABLE [RoundsTriesResults] ADD FOREIGN KEY ([player_in_match_id]) REFERENCES [PlayerInMatch] ([player_in_match_id])
GO

-- Triggers

CREATE TRIGGER trigger_IncrementReportCount
ON [PlayerReport]
AFTER INSERT
AS
BEGIN
  UPDATE p
  SET p.number_of_reports = p.number_of_reports + 1
  FROM [Player] p
  INNER JOIN inserted i ON p.player_id = i.reported_player_id
END
GO

CREATE TRIGGER trigger_DecreaseReportCount
ON [PlayerReport]
AFTER DELETE
AS
BEGIN
  UPDATE p
  SET p.number_of_reports = p.number_of_reports - 1
  FROM [Player] p
  INNER JOIN deleted i ON p.player_id = i.reported_player_id
END
GO

-- Seed catalogs

INSERT INTO [RequestStatusCatalog] ([status])
VALUES
  ('Pending'),
  ('Accepted'),
  ('Rejected');
GO
INSERT INTO [ReportReasonCatalog] ([reason])
VALUES
  ('InappropriateLanguage'),
  ('Cheating');
GO
INSERT INTO [ClueColorsCatalog] ([color])
VALUES
  ('Black'),
  ('White');
GO
INSERT INTO [GameModesCatalog] ([gamemode])
VALUES
  ('TimeTrial'),
  ('Tries');
GO
INSERT INTO [CodeColorsCatalog] ([color])
VALUES
  ('Blue'),
  ('Green'),
  ('Red'),
  ('Yellow'),
  ('Orange'),
  ('White');
GO