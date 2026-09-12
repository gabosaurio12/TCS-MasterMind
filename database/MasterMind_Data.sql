-- =====================================================================
-- MasterMind - Script de datos de prueba
-- =====================================================================
-- Supuestos:
--   1) Este script se ejecuta sobre una base recién creada, por lo que los
--      IDENTITY inician en 1 y son predecibles (se usan para
--      referenciar filas entre INSERTs).
--   2) Los catálogos ya fueron sembrados por MasterMind_Tables.sql con estos
--      IDs (documentado aquí para lectura del script, no se re-insertan):
--        RequestStatusCatalog: 1=Pending, 2=Accepted, 3=Rejected
--        ReportReasonCatalog:  1=InappropriateLanguage, 2=Cheating
--        ClueColorsCatalog:    1=Black, 2=White
--        GameModesCatalog:     1=TimeTrial, 2=Tries
--        CodeColorsCatalog:    1=Blue, 2=Green, 3=Red, 4=Yellow,
--                               5=Orange, 6=White
-- =====================================================================

USE MasterMind;
GO

-- ---------------------------------------------------------------------
-- 1. Player  (6 registros)
--    Caso normal: 6 jugadores. Dos de ellos (Elena, Fabian) simulan
--    estar aún en proceso de verificación (ver paso 2).
-- ---------------------------------------------------------------------
INSERT INTO [Player] ([email], [username], [password])
VALUES
  ('ana@mastermind.test',    'ana_t',    'hash_placeholder_1'),
  ('beto@mastermind.test',   'beto_r',   'hash_placeholder_2'),
  ('caro@mastermind.test',   'caro_d',   'hash_placeholder_3'),
  ('dario@mastermind.test',  'dario_l',  'hash_placeholder_4'),
  ('elena@mastermind.test',  'elena_v',  'hash_placeholder_5'),
  ('fabian@mastermind.test', 'fabian_s', 'hash_placeholder_6');
GO
-- IDs esperados: Ana=1, Beto=2, Caro=3, Dario=4, Elena=5, Fabian=6

-- ---------------------------------------------------------------------
-- 2. VerificationCode  (2 registros)
--    Caso: cuentas nuevas que aún no confirman su email (Elena, Fabian).
--    Ana, Beto, Caro y Dario ya están verificados, por eso no tienen fila.
-- ---------------------------------------------------------------------
INSERT INTO [VerificationCode] ([verification_code], [player_id])
VALUES
  ('482913', 5), -- Elena
  ('117650', 6); -- Fabian
GO

-- ---------------------------------------------------------------------
-- 3. MatchRoom  (3 registros)
--    Variación de gamemode (TimeTrial y Tries) y de room_code
--    (sala pública sin código privado en la #2).
-- ---------------------------------------------------------------------
INSERT INTO [MatchRoom] ([gamemode_id], [player_one_id], [player_two_id], [private_room_code])
VALUES
  (1, 1, 2, 'ABC123'), -- Sala 1: TimeTrial, Ana vs Beto
  (2, 3, 4, NULL),     -- Sala 2: Tries, Caro vs Dario, sala pública
  (1, 5, 6, 'XYZ789'); -- Sala 3: TimeTrial, Elena vs Fabian
GO
-- IDs esperados: Sala1=1, Sala2=2, Sala3=3

-- ---------------------------------------------------------------------
-- 4. TimeTrialConfig / TriesConfig  (2 + 1 registros)
--    PK = FK a MatchRoom (patrón de extensión 1:1, sin IDENTITY).
--    Variación de límites entre salas del mismo modo (120s vs 90s).
-- ---------------------------------------------------------------------
INSERT INTO [TimeTrialConfig] ([match_room_id], [max_time_of_seconds])
VALUES
  (1, 120),
  (3, 90);
GO

INSERT INTO [TriesConfig] ([match_room_id], [max_of_tries])
VALUES
  (2, 10);
GO

-- ---------------------------------------------------------------------
-- 5. MatchRoomSecretCode  (12 registros)
--    Códigos secretos posicionales. Sala 2 incluye un color repetido
--    (Red, Red) para probar que el modelo permite duplicados dentro
--    del mismo código, tal como en el juego original.
-- ---------------------------------------------------------------------
INSERT INTO [MatchRoomSecretCode] ([match_room_id], [position], [color_id])
VALUES
  -- Sala 1: Blue, Green, Red, Yellow
  (1, 1, 1), (1, 2, 2), (1, 3, 3), (1, 4, 4),
  -- Sala 2: Red, Red, Blue, White  (color repetido)
  (2, 1, 3), (2, 2, 3), (2, 3, 1), (2, 4, 6),
  -- Sala 3: Yellow, Orange, Green, Blue
  (3, 1, 4), (3, 2, 5), (3, 3, 2), (3, 4, 1);
GO

-- ---------------------------------------------------------------------
-- 6. Friendship  (4 registros)
--    Cubre los 3 estados del catálogo: Accepted, Pending, Rejected.
-- ---------------------------------------------------------------------
INSERT INTO [Friendship] ([requester_id], [addressee_id], [status_id])
VALUES
  (1, 2, 2), -- Ana -> Beto, Accepted
  (1, 3, 1), -- Ana -> Caro, Pending
  (4, 1, 3), -- Dario -> Ana, Rejected
  (2, 5, 2); -- Beto -> Elena, Accepted
GO

-- ---------------------------------------------------------------------
-- 7. GameInvitation  (4 registros)
--    Cubre los 3 estados. La invitación #3 queda en Pending para
--    representar una sala ya creada pero cuya partida aún no arranca
--    (por eso Sala 3 no tendrá DecipherTry más adelante).
-- ---------------------------------------------------------------------
INSERT INTO [GameInvitation] ([requester_id], [addressee_id], [match_room_id], [status_id])
VALUES
  (1, 2, 1, 2), -- Ana invita a Beto a Sala 1, Accepted (partida ya jugada)
  (3, 4, 2, 2), -- Caro invita a Dario a Sala 2, Accepted (partida ya jugada)
  (5, 6, 3, 1), -- Elena invita a Fabian a Sala 3, Pending (sin iniciar)
  (1, 4, 1, 3); -- Ana invita a Dario (distinto intento), Rejected
GO

-- ---------------------------------------------------------------------
-- 8. PlayerReport  (3 registros)
--    Prueba el trigger trigger_IncrementReportCount con las 2 razones
--    del catálogo. Dario recibe 2 reportes, Fabian recibe 1.
--    Esperado tras este bloque: Dario.number_of_reports = 2,
--    Fabian.number_of_reports = 1.
-- ---------------------------------------------------------------------
INSERT INTO [PlayerReport] ([reported_player_id], [reported_by_player_id], [reason_id])
VALUES
  (4, 1, 1), -- Dario reportado por Ana, InappropriateLanguage
  (4, 3, 2), -- Dario reportado por Caro, Cheating
  (6, 5, 1); -- Fabian reportado por Elena, InappropriateLanguage
GO

-- ---------------------------------------------------------------------
-- 9. Retracto de un reporte -> prueba trigger_DecreaseReportCount
--    Se elimina el reporte de Cheating sobre Dario (report_id = 2).
--    Esperado tras este DELETE: Dario.number_of_reports = 1.
-- ---------------------------------------------------------------------
DELETE FROM [PlayerReport] WHERE [report_id] = 2;
GO

-- Verificación opcional de los triggers (comentado; descomentar para probar):
-- SELECT player_id, username, number_of_reports FROM [Player] WHERE player_id IN (4, 6);
-- Resultado esperado: Dario (4) = 1, Fabian (6) = 1

-- ---------------------------------------------------------------------
-- 10. DecipherTry  (6 registros)
--     Sala 1 y 2 tienen intentos (partida jugada). Sala 3 NO tiene
--     intentos porque su invitación sigue Pending (paso 7).
-- ---------------------------------------------------------------------
INSERT INTO [DecipherTry] ([match_room_id], [player_id], [attempt_number])
VALUES
  (1, 1, 1), -- id=1: Ana, intento 1
  (1, 1, 2), -- id=2: Ana, intento 2 (acierto total)
  (1, 2, 1), -- id=3: Beto, intento 1
  (2, 3, 1), -- id=4: Caro, intento 1
  (2, 4, 1), -- id=5: Dario, intento 1
  (2, 4, 2); -- id=6: Dario, intento 2 (acierto total)
GO

-- ---------------------------------------------------------------------
-- 11. DecipherTryCode  (24 registros)
--     Incluye un caso de acierto exacto (try 2 y try 6, idénticos al
--     código secreto de su sala) y casos parciales/sin acierto.
-- ---------------------------------------------------------------------
INSERT INTO [DecipherTryCode] ([decipher_try_id], [position], [color_id])
VALUES
  -- try 1 (Ana, parcial): Red, Green, Blue, Yellow
  (1, 1, 3), (1, 2, 2), (1, 3, 1), (1, 4, 4),
  -- try 2 (Ana, acierto exacto = secreto Sala 1: Blue,Green,Red,Yellow)
  (2, 1, 1), (2, 2, 2), (2, 3, 3), (2, 4, 4),
  -- try 3 (Beto, todo desplazado): Green, Blue, Yellow, Red
  (3, 1, 2), (3, 2, 1), (3, 3, 4), (3, 4, 3),
  -- try 4 (Caro, parcial): White, Red, Blue, Red
  (4, 1, 6), (4, 2, 3), (4, 3, 1), (4, 4, 3),
  -- try 5 (Dario, parcial): Red, Blue, Red, White
  (5, 1, 3), (5, 2, 1), (5, 3, 3), (5, 4, 6),
  -- try 6 (Dario, acierto exacto = secreto Sala 2: Red,Red,Blue,White)
  (6, 1, 3), (6, 2, 3), (6, 3, 1), (6, 4, 6);
GO

-- ---------------------------------------------------------------------
-- 12. CorrectPositionAndColors  (6 registros)
--     Una fila por intento evaluado (todos los de este set de prueba).
-- ---------------------------------------------------------------------
INSERT INTO [CorrectPositionAndColors] ([decipher_try_id])
VALUES (1), (2), (3), (4), (5), (6);
GO

-- ---------------------------------------------------------------------
-- 13. CorrectPositionAndColorsClue  (24 registros)
--     Pistas Black/White por intento. try 2 y try 6 muestran el caso
--     de acierto total (4 Black, 0 White).
-- ---------------------------------------------------------------------
INSERT INTO [CorrectPositionAndColorsClue] ([correct_position_and_colors_id], [position], [clue_color_id])
VALUES
  -- try 1: 2 Black, 2 White
  (1, 1, 1), (1, 2, 1), (1, 3, 2), (1, 4, 2),
  -- try 2: acierto total -> 4 Black
  (2, 1, 1), (2, 2, 1), (2, 3, 1), (2, 4, 1),
  -- try 3: ningún color en su posición -> 0 Black, 4 White
  (3, 1, 2), (3, 2, 2), (3, 3, 2), (3, 4, 2),
  -- try 4: 2 Black, 2 White
  (4, 1, 1), (4, 2, 1), (4, 3, 2), (4, 4, 2),
  -- try 5: 2 Black, 2 White
  (5, 1, 1), (5, 2, 1), (5, 3, 2), (5, 4, 2),
  -- try 6: acierto total -> 4 Black
  (6, 1, 1), (6, 2, 1), (6, 3, 1), (6, 4, 1);
GO

-- ---------------------------------------------------------------------
-- 14. PlayerInMatch  (4 registros)
--     Estadísticas agregadas por jugador. Ana/Beto usan best_time
--     (TimeTrial); Caro/Dario usan least_tries (Tries) -- el campo no
--     aplicable queda NULL a propósito, mostrando el uso condicional
--     de columnas según el modo de la sala jugada.
-- ---------------------------------------------------------------------
INSERT INTO [PlayerInMatch] ([player_id], [best_time], [least_tries], [rounds_won])
VALUES
  (1, 45,   NULL, 1), -- id=1: Ana
  (2, 52,   NULL, 0), -- id=2: Beto
  (3, NULL, 4,    0), -- id=3: Caro
  (4, NULL, 2,    1); -- id=4: Dario
GO

-- ---------------------------------------------------------------------
-- 15. MatchResults  (2 registros)
-- ---------------------------------------------------------------------
INSERT INTO [MatchResults] ([match_room_id], [player_one_results_id], [player_two_results_id])
VALUES
  (1, 1, 2), -- Sala 1: Ana vs Beto
  (2, 3, 4); -- Sala 2: Caro vs Dario
GO

-- ---------------------------------------------------------------------
-- 16. RoundsTimeResults  (3 registros, solo aplica a las salas TimeTrial)
--     Ana con 2 rondas registradas (variación de desempeño entre
--     rondas); Beto con 1 ronda.
-- ---------------------------------------------------------------------
INSERT INTO [RoundsTimeResults] ([player_in_match_id], [round], [time])
VALUES
  (1, 1, 45),
  (1, 2, 50),
  (2, 1, 52);
GO

-- ---------------------------------------------------------------------
-- 17. RoundsTriesResults  (2 registros, solo aplica a la sala Tries)
-- ---------------------------------------------------------------------
INSERT INTO [RoundsTriesResults] ([player_in_match_id], [round], [tries])
VALUES
  (3, 1, 4),
  (4, 1, 2);
GO

-- ---------------------------------------------------------------------
-- 18. PlayerTimeTrialRecords  (3 registros)
--     Ana con historial de 2 posiciones (rank 1 y 2); Beto con solo
--     rank 1, probando que el top-3 no exige tener las 3 filas.
-- ---------------------------------------------------------------------
INSERT INTO [PlayerTimeTrialRecords] ([player_id], [time_record], [rank])
VALUES
  (1, 45, 1), -- Ana, mejor tiempo
  (1, 50, 2), -- Ana, segundo mejor
  (2, 52, 1); -- Beto, único récord registrado hasta ahora
GO

-- ---------------------------------------------------------------------
-- 19. PlayerTriesRecords  (3 registros)
--     Dario con 2 posiciones; Caro con 1. Misma lógica que el bloque
--     anterior, para el otro modo de juego.
-- ---------------------------------------------------------------------
INSERT INTO [PlayerTriesRecords] ([player_id], [tries_record], [rank])
VALUES
  (4, 2, 1), -- Dario, mejor marca (menos intentos)
  (4, 5, 2), -- Dario, segunda mejor marca
  (3, 4, 1); -- Caro, único récord registrado hasta ahora
GO

-- =====================================================================
-- Total de filas insertadas (netas, tras el DELETE del paso 9): 108
-- Distribuidas en las 19 tablas del esquema con datos de prueba,
-- superando ampliamente el mínimo de 20 registros solicitado.
-- =====================================================================