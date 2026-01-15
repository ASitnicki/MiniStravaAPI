SET NOCOUNT ON;

-- ============================
-- Tabela: Users
-- ============================
IF OBJECT_ID(N'[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [Users] (
        [Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [PK_Users] PRIMARY KEY
            CONSTRAINT [DF_Users_Id] DEFAULT NEWID(),

        [Email] NVARCHAR(256) NOT NULL,
        [PasswordHash] VARBINARY(MAX) NOT NULL,
        [PasswordSalt] VARBINARY(MAX) NOT NULL,

        [FirstName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NULL,
        [BirthDate] DATE NULL,
        [Gender] NVARCHAR(20) NULL,
        [HeightCm] INT NULL,
        [WeightKg] DECIMAL(5,2) NULL,
        [AvatarUrl] NVARCHAR(1024) NULL,

        [IsAdmin] BIT NOT NULL
            CONSTRAINT [DF_Users_IsAdmin] DEFAULT 0,

        [PreferredLanguage] NVARCHAR(10) NOT NULL
            CONSTRAINT [DF_Users_PreferredLanguage] DEFAULT 'pl',

        [CreatedAt] DATETIMEOFFSET NOT NULL
            CONSTRAINT [DF_Users_CreatedAt] DEFAULT SYSUTCDATETIME(),

        [LastLoginAt] DATETIMEOFFSET NULL
    );
END;

IF COL_LENGTH('Users', 'MustChangePassword') IS NULL
BEGIN
    ALTER TABLE [Users]
    ADD [MustChangePassword] BIT NOT NULL
        CONSTRAINT DF_Users_MustChangePassword DEFAULT 0;
END
IF COL_LENGTH('Users', 'PasswordResetToken') IS NULL
BEGIN
    ALTER TABLE [Users]
    ADD [PasswordResetToken] NVARCHAR(200) NULL;
END

IF COL_LENGTH('Users', 'PasswordResetTokenExpiresAt') IS NULL
BEGIN
    ALTER TABLE [Users]
    ADD [PasswordResetTokenExpiresAt] DATETIMEOFFSET NULL;
END

-- Unikalność Email
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Users_Email'
      AND object_id = OBJECT_ID(N'[Users]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_Users_Email] ON [Users]([Email]);
END;


-- ============================
-- Tabela: Activities
-- ============================
IF OBJECT_ID(N'[Activities]', N'U') IS NULL
BEGIN
    CREATE TABLE [Activities] (
        [Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [PK_Activities] PRIMARY KEY
            CONSTRAINT [DF_Activities_Id] DEFAULT NEWID(),

        [UserId] UNIQUEIDENTIFIER NOT NULL,

        [Name] NVARCHAR(200) NULL,

        [ActivityType] NVARCHAR(50) NOT NULL,

        [StartTime] DATETIMEOFFSET NOT NULL,

        [EndTime] DATETIMEOFFSET NULL,

        [DurationSeconds] INT NULL,

        [DistanceMeters] DECIMAL(10,2) NOT NULL
            CONSTRAINT [DF_Activities_DistanceMeters] DEFAULT 0,

        [AveragePaceSecPerKm] INT NULL,
        [AverageSpeedMps] DECIMAL(6,3) NULL,

        [Notes] NVARCHAR(MAX) NULL,

        [PhotoUrl] NVARCHAR(1024) NULL,

        [CreatedAt] DATETIMEOFFSET NOT NULL
            CONSTRAINT [DF_Activities_CreatedAt] DEFAULT SYSUTCDATETIME()
    );
END;

-- FK Activities -> Users (twórz tylko jeśli brak)
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Activities_Users'
)
BEGIN
    ALTER TABLE [Activities]
    ADD CONSTRAINT [FK_Activities_Users]
        FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
        ON DELETE CASCADE;
END;

-- Indeks: UserId + StartTime
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Activities_UserId_StartTime'
      AND object_id = OBJECT_ID(N'[Activities]')
)
BEGIN
    CREATE INDEX [IX_Activities_UserId_StartTime]
    ON [Activities]([UserId], [StartTime]);
END;

-- Indeks: ActivityType
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Activities_ActivityType'
      AND object_id = OBJECT_ID(N'[Activities]')
)
BEGIN
    CREATE INDEX [IX_Activities_ActivityType]
    ON [Activities]([ActivityType]);
END;


-- ============================
-- Tabela: TrackPoints
-- ============================
IF OBJECT_ID(N'[TrackPoints]', N'U') IS NULL
BEGIN
    CREATE TABLE [TrackPoints] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT [PK_TrackPoints] PRIMARY KEY,

        [ActivityId] UNIQUEIDENTIFIER NOT NULL,
        [Sequence] INT NOT NULL,

        [Timestamp] DATETIMEOFFSET NOT NULL,

        [Latitude] DECIMAL(9,6) NOT NULL,
        [Longitude] DECIMAL(9,6) NOT NULL,

        [ElevationMeters] DECIMAL(7,2) NULL,
        [SpeedMps] DECIMAL(6,3) NULL
    );
END;

-- FK TrackPoints -> Activities
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TrackPoints_Activities'
)
BEGIN
    ALTER TABLE [TrackPoints]
    ADD CONSTRAINT [FK_TrackPoints_Activities]
        FOREIGN KEY ([ActivityId]) REFERENCES [Activities]([Id])
        ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TrackPoints_ActivityId_Sequence'
      AND object_id = OBJECT_ID(N'[TrackPoints]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_TrackPoints_ActivityId_Sequence]
    ON [TrackPoints]([ActivityId], [Sequence]);
END;


-- ============================
-- Tabela: ActivityPhotos
-- ============================
IF OBJECT_ID(N'[ActivityPhotos]', N'U') IS NULL
BEGIN
    CREATE TABLE [ActivityPhotos] (
        [Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [PK_ActivityPhotos] PRIMARY KEY
            CONSTRAINT [DF_ActivityPhotos_Id] DEFAULT NEWID(),

        [ActivityId] UNIQUEIDENTIFIER NOT NULL,
        [Url] NVARCHAR(1024) NOT NULL,

        [UploadedAt] DATETIMEOFFSET NOT NULL
            CONSTRAINT [DF_ActivityPhotos_UploadedAt] DEFAULT SYSUTCDATETIME()
    );
END;

-- FK ActivityPhotos -> Activities
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_ActivityPhotos_Activities'
)
BEGIN
    ALTER TABLE [ActivityPhotos]
    ADD CONSTRAINT [FK_ActivityPhotos_Activities]
        FOREIGN KEY ([ActivityId]) REFERENCES [Activities]([Id])
        ON DELETE CASCADE;
END;

-- Indeks ActivityPhotos(ActivityId)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ActivityPhotos_ActivityId'
      AND object_id = OBJECT_ID(N'[ActivityPhotos]')
)
BEGIN
    CREATE INDEX [IX_ActivityPhotos_ActivityId]
    ON [ActivityPhotos]([ActivityId]);
END;


-- ============================
-- Tabela: ApiClients
-- ============================
IF OBJECT_ID(N'[ApiClients]', N'U') IS NULL
BEGIN
    CREATE TABLE [ApiClients] (
        [Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [PK_ApiClients] PRIMARY KEY
            CONSTRAINT [DF_ApiClients_Id] DEFAULT NEWID(),

        [UserId] UNIQUEIDENTIFIER NULL,
        [ClientName] NVARCHAR(200) NULL,
        [ApiKeyHash] VARBINARY(MAX) NULL,

        [CreatedAt] DATETIMEOFFSET NOT NULL
            CONSTRAINT [DF_ApiClients_CreatedAt] DEFAULT SYSUTCDATETIME()
    );
END;

-- FK ApiClients -> Users (UserId opcjonalny, więc SET NULL)
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_ApiClients_Users'
)
BEGIN
    ALTER TABLE [ApiClients]
    ADD CONSTRAINT [FK_ApiClients_Users]
        FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
        ON DELETE SET NULL;
END;

-- Indeks ApiClients(UserId)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ApiClients_UserId'
      AND object_id = OBJECT_ID(N'[ApiClients]')
)
BEGIN
    CREATE INDEX [IX_ApiClients_UserId]
    ON [ApiClients]([UserId]);
END;


-- ============================
-- Tabela: SyncSessions
-- ============================
IF OBJECT_ID(N'[SyncSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [SyncSessions] (
        [Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [PK_SyncSessions] PRIMARY KEY
            CONSTRAINT [DF_SyncSessions_Id] DEFAULT NEWID(),

        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [StartedAt] DATETIMEOFFSET NOT NULL,
        [CompletedAt] DATETIMEOFFSET NULL,
        [Status] NVARCHAR(50) NOT NULL
    );
END;

-- FK SyncSessions -> Users
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_SyncSessions_Users'
)
BEGIN
    ALTER TABLE [SyncSessions]
    ADD CONSTRAINT [FK_SyncSessions_Users]
        FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
        ON DELETE CASCADE;
END;

-- Indeks SyncSessions(UserId, StartedAt)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_SyncSessions_UserId_StartedAt'
      AND object_id = OBJECT_ID(N'[SyncSessions]')
)
BEGIN
    CREATE INDEX [IX_SyncSessions_UserId_StartedAt]
    ON [SyncSessions]([UserId], [StartedAt]);
END;
