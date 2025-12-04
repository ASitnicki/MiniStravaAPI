-- ============================
-- Tabela: Users
-- Przechowuje dane użytkowników aplikacji
-- ============================
CREATE TABLE [Users] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(), 
        -- Klucz główny typu GUID

    [Email] NVARCHAR(256) NOT NULL UNIQUE, 
        -- E-mail użytkownika, unikalny
    [PasswordHash] VARBINARY(MAX) NOT NULL,
        -- Zahaszowane hasło (binarne dane)

    [PasswordSalt] VARBINARY(MAX) NOT NULL,
        -- Salt użyty do haszowania hasła – binarne dane

    [FirstName] NVARCHAR(100),
        -- Imię użytkownika

    [LastName] NVARCHAR(100),
        -- Nazwisko użytkownika

    [BirthDate] DATE,
        -- Data urodzenia bez czasu

    [Gender] NVARCHAR(20),
        -- Płeć – tekst, np. "male", "female", "other"

    [HeightCm] INT,
        -- Wzrost użytkownika w centymetrach

    [WeightKg] DECIMAL(5,2),
        -- Waga z dokładnością do dwóch miejsc po przecinku

    [AvatarUrl] NVARCHAR(1024),
        -- URL do awatara użytkownika

    [IsAdmin] BIT NOT NULL DEFAULT 0,
        -- Czy użytkownik jest administratorem? 

    [PreferredLanguage] NVARCHAR(10) NOT NULL DEFAULT 'pl',
        -- Preferowany język interfejsu, np. 'pl', 'en'

    [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
        -- Moment utworzenia rekordu z informacją o strefie czasowej (UTC)

    [LastLoginAt] DATETIMEOFFSET NULL
        -- Ostatnie logowanie użytkownika
);

-- ============================
-- Tabela: Activities
-- Zawiera informacje o aktywnościach sportowych użytkowników
-- ============================
CREATE TABLE [Activities] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(), 
        -- Identyfikator aktywności

    [UserId] UNIQUEIDENTIFIER NOT NULL,
        -- Id użytkownika wykonującego aktywność (FK)

    [Name] NVARCHAR(200),
        -- Nazwa aktywności, np. "Morning Run"

    [ActivityType] NVARCHAR(50) NOT NULL,
        -- Typ aktywności, np. 'running', 'cycling'

    [StartTime] DATETIMEOFFSET NOT NULL,
        -- Dokładny czas rozpoczęcia

    [EndTime] DATETIMEOFFSET,
        -- Czas zakończenia (opcjonalnie jeśli aktywność trwa)

    [DurationSeconds] INT,
        -- Całkowity czas trwania w sekundach

    [DistanceMeters] DECIMAL(10,2) DEFAULT 0,
        -- Dystans w metrach z dokładnością do 2 miejsc

    [AveragePaceSecPerKm] INT NULL,
        -- Średnie tempo w sekundach na km

    [AverageSpeedMps] DECIMAL(6,3) NULL,
        -- Średnia prędkość w m/s

    [Notes] NVARCHAR(MAX),
        -- Dodatkowe notatki użytkownika

    [PhotoUrl] NVARCHAR(1024),
        -- Zdjęcie powiązane z aktywnością (opcjonalne)

    [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
        -- Moment zapisania aktywności

    CONSTRAINT FK_Activities_Users FOREIGN KEY (UserId) 
        REFERENCES [Users](Id) ON DELETE CASCADE
        -- Jeśli użytkownik zostanie usunięty → usuń jego aktywności
);

-- Indeks dla szybkiego pobierania aktywności użytkownika po czasie
CREATE INDEX IX_Activities_UserId_StartTime 
ON Activities(UserId, StartTime);

-- Indeks dla filtrowania po typie aktywności
CREATE INDEX IX_Activities_ActivityType 
ON Activities(ActivityType);

-- ============================
-- Tabela: TrackPoints
-- Zawiera szczegółowe punkty GPS dla każdej aktywności
-- ============================
CREATE TABLE [TrackPoints] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        -- Autonumerowany klucz główny

    [ActivityId] UNIQUEIDENTIFIER NOT NULL,
        -- Id aktywności (FK)

    [Sequence] INT NOT NULL,
        -- Numer kolejny punktu (od 1 do N)

    [Timestamp] DATETIMEOFFSET NOT NULL,
        -- Czas pomiaru punktu GPS

    [Latitude] DECIMAL(9,6) NOT NULL,
        -- Szerokość geograficzna z dokładnością do 6 miejsc

    [Longitude] DECIMAL(9,6) NOT NULL,
        -- Długość geograficzna z dokładnością do 6 miejsc

    [ElevationMeters] DECIMAL(7,2) NULL,
        -- Wysokość n.p.m.

    [SpeedMps] DECIMAL(6,3) NULL,
        -- Prędkość chwilowa

    CONSTRAINT FK_TrackPoints_Activities FOREIGN KEY (ActivityId) 
        REFERENCES Activities(Id) ON DELETE CASCADE
);

-- Indeks umożliwia szybkie pobranie punktów w kolejności
CREATE INDEX IX_TrackPoints_ActivityId_Sequence 
ON TrackPoints(ActivityId, Sequence);

-- ============================
-- Tabela: ActivityPhotos
-- Osobne zdjęcia przypisane do aktywności
-- ============================
CREATE TABLE [ActivityPhotos] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        -- Id zdjęcia

    [ActivityId] UNIQUEIDENTIFIER NOT NULL,
        -- Id aktywności

    [Url] NVARCHAR(1024) NOT NULL,
        -- Adres URL zdjęcia

    [UploadedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
        -- Czas dodania zdjęcia

    CONSTRAINT FK_ActivityPhotos_Activities FOREIGN KEY (ActivityId) 
        REFERENCES Activities(Id) ON DELETE CASCADE
);

-- ============================
-- Tabela: ApiClients
-- Akces do API
-- ============================
CREATE TABLE [ApiClients] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        -- Id klienta API

    [UserId] UNIQUEIDENTIFIER NULL,
        -- Opcjonalnie: który użytkownik korzysta z API

    [ClientName] NVARCHAR(200),
        -- Nazwa aplikacji lub integracji

    [ApiKeyHash] VARBINARY(MAX),
        -- Hash klucza API dla bezpieczeństwa

    [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME()
        -- Moment utworzenia wpisu klienta
);

-- ============================
-- Tabela: SyncSessions
-- Logi synchronizacji / eksportów
-- ============================
CREATE TABLE [SyncSessions] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        -- Id sesji synchronizacji

    [UserId] UNIQUEIDENTIFIER NOT NULL,
        -- Użytkownik, którego dotyczy

    [StartedAt] DATETIMEOFFSET NOT NULL,
        -- Początek synchronizacji

    [CompletedAt] DATETIMEOFFSET NULL,
        -- Koniec synchronizacji

    [Status] NVARCHAR(50) NOT NULL,
        -- Status procesu, np. 'running', 'completed', 'failed'

    CONSTRAINT FK_SyncSessions_Users FOREIGN KEY (UserId) 
        REFERENCES [Users](Id) ON DELETE CASCADE
);
