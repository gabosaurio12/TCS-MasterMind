USE MasterMind;
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