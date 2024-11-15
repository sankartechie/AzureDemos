-- Create Players Table
CREATE TABLE Players (
    player_id UNIQUEIDENTIFIER PRIMARY KEY,
    name NVARCHAR(50),
    address NVARCHAR(100),
    nationality NVARCHAR(50),
    gender NVARCHAR(10),
    total_matches_played INT,
    won INT,
    lost INT,
    win_loss_percentage DECIMAL(5, 2)
);

-- Populate Players Table
DECLARE @i INT = 1;
WHILE @i <= 10
BEGIN
    DECLARE @playerId UNIQUEIDENTIFIER = NEWID();
    DECLARE @totalMatches INT = FLOOR(RAND() * (100 - 20 + 1)) + 20;
    DECLARE @won INT = FLOOR(RAND() * (50 - 10 + 1)) + 10;
    DECLARE @lost INT = @totalMatches - @won;
    DECLARE @winLossPercentage DECIMAL(5, 2) = (CAST(@won AS DECIMAL(5,2)) / @totalMatches) * 100;

    INSERT INTO Players (player_id, name, address, nationality, gender, total_matches_played, won, lost, win_loss_percentage)
    VALUES (
        @playerId,
        'Player ' + CAST(@i AS NVARCHAR),
        'Address ' + CAST(@i AS NVARCHAR),
        (CASE WHEN @i % 2 = 0 THEN 'USA' ELSE 'India' END),
        (CASE WHEN @i % 2 = 0 THEN 'Male' ELSE 'Female' END),
        @totalMatches,
        @won,
        @lost,
        @winLossPercentage
    );

    SET @i = @i + 1;
END;

-- Create Matches Table
CREATE TABLE Matches (
    match_id UNIQUEIDENTIFIER PRIMARY KEY,
    player1_id UNIQUEIDENTIFIER,
    player1_name NVARCHAR(50),
    player1_nationality NVARCHAR(50),
    player2_id UNIQUEIDENTIFIER,
    player2_name NVARCHAR(50),
    player2_nationality NVARCHAR(50),
    match_won_by UNIQUEIDENTIFIER
);

-- Populate Matches Table
DECLARE @j INT = 1;
WHILE @j <= 100
BEGIN
    DECLARE @matchId UNIQUEIDENTIFIER = NEWID();
    DECLARE @player1 UNIQUEIDENTIFIER, @player2 UNIQUEIDENTIFIER;
    DECLARE @player1_name NVARCHAR(50), @player2_name NVARCHAR(50);
    DECLARE @player1_nationality NVARCHAR(50), @player2_nationality NVARCHAR(50);

    SELECT TOP 1 @player1 = player_id, @player1_name = name, @player1_nationality = nationality FROM Players ORDER BY NEWID();
    SELECT TOP 1 @player2 = player_id, @player2_name = name, @player2_nationality = nationality FROM Players WHERE player_id != @player1 ORDER BY NEWID();

    INSERT INTO Matches (match_id, player1_id, player1_name, player1_nationality, player2_id, player2_name, player2_nationality, match_won_by)
    VALUES (
        @matchId,
        @player1,
        @player1_name,
        @player1_nationality,
        @player2,
        @player2_name,
        @player2_nationality,
        (CASE WHEN RAND() > 0.5 THEN @player1 ELSE @player2 END)
    );

    SET @j = @j + 1;
END;

PRINT 'SQL Server setup complete.'
