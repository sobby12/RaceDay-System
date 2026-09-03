/*
    RaceDay System - Part 1 - Section C
    Final SQL Server Database Script

    Entities:
    Roles, Users, Events, Categories, EventEnrollments, Results

    This script:
    1. Creates RaceDayDB if it does not already exist.
    2. Creates all six tables with primary keys, foreign keys and constraints.
    3. Inserts sample roles, users, events, categories, enrolments and results.
    4. Includes verification queries for SSMS screenshots.

    NOTE:
    PasswordHash values are sample placeholders for Part 1 database testing.
    The Part 2 application must use properly generated password hashes.
*/

IF DB_ID(N'RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/* Remove existing tables in dependency order so the script can be re-run during development. */
IF OBJECT_ID(N'dbo.Results', N'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID(N'dbo.EventEnrollments', N'U') IS NOT NULL DROP TABLE dbo.EventEnrollments;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID(N'dbo.Events', N'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

/* 1. ROLES */
CREATE TABLE dbo.Roles
(
    RoleId INT IDENTITY(1,1) NOT NULL,
    RoleName VARCHAR(30) NOT NULL,

    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
GO

/* 2. USERS */
CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    RoleId INT NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(120) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Phone VARCHAR(20) NULL,

    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),

    CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles(RoleId)
);
GO

/* 3. EVENTS */
CREATE TABLE dbo.Events
(
    EventId INT IDENTITY(1,1) NOT NULL,
    OrganizerId INT NOT NULL,
    EventName VARCHAR(120) NOT NULL,
    Description VARCHAR(500) NOT NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EventType VARCHAR(10) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL
        CONSTRAINT DF_Events_EntryFee DEFAULT (0.00),
    MaxParticipants INT NOT NULL,

    CONSTRAINT PK_Events PRIMARY KEY (EventId),

    CONSTRAINT FK_Events_Organizers
        FOREIGN KEY (OrganizerId)
        REFERENCES dbo.Users(UserId),

    CONSTRAINT CK_Events_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Events_EventType
        CHECK (EventType IN ('Run', 'Walk', 'Cycle')),

    CONSTRAINT CK_Events_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT CK_Events_MaxParticipants
        CHECK (MaxParticipants > 0)
);
GO

/* 4. CATEGORIES */
CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    CategoryName VARCHAR(80) NOT NULL,
    CategoryType VARCHAR(10) NOT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    CategoryDistanceKm DECIMAL(6,2) NULL,

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT UQ_Categories_Event_CategoryName
        UNIQUE (EventId, CategoryName),

    /* Supports the composite foreign key from EventEnrollments. */
    CONSTRAINT UQ_Categories_Event_CategoryId
        UNIQUE (EventId, CategoryId),

    CONSTRAINT CK_Categories_Type
        CHECK (CategoryType IN ('Age', 'Distance')),

    CONSTRAINT CK_Categories_AgeOrDistance
        CHECK
        (
            (CategoryType = 'Age'
             AND MinAge IS NOT NULL
             AND MaxAge IS NOT NULL
             AND MinAge >= 0
             AND MaxAge >= MinAge
             AND CategoryDistanceKm IS NULL)
            OR
            (CategoryType = 'Distance'
             AND CategoryDistanceKm IS NOT NULL
             AND CategoryDistanceKm > 0
             AND MinAge IS NULL
             AND MaxAge IS NULL)
        )
);
GO

/* 5. EVENT ENROLMENTS */
CREATE TABLE dbo.EventEnrollments
(
    EnrollmentId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    UserId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrollmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_EventEnrollments_EnrollmentDate DEFAULT (SYSDATETIME()),
    Status VARCHAR(20) NOT NULL
        CONSTRAINT DF_EventEnrollments_Status DEFAULT ('Registered'),

    CONSTRAINT PK_EventEnrollments PRIMARY KEY (EnrollmentId),

    CONSTRAINT FK_EventEnrollments_Events
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT FK_EventEnrollments_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(UserId),

    /* Ensures that the selected category belongs to the selected event. */
    CONSTRAINT FK_EventEnrollments_EventCategory
        FOREIGN KEY (EventId, CategoryId)
        REFERENCES dbo.Categories(EventId, CategoryId),

    /* A participant cannot register for the same event more than once. */
    CONSTRAINT UQ_EventEnrollments_Event_User
        UNIQUE (EventId, UserId),

    CONSTRAINT CK_EventEnrollments_Status
        CHECK (Status IN ('Registered', 'Cancelled', 'Completed'))
);
GO

/* 6. RESULTS */
CREATE TABLE dbo.Results
(
    ResultId INT IDENTITY(1,1) NOT NULL,
    EnrollmentId INT NOT NULL,
    FinishPosition INT NULL,
    FinishTimeSeconds INT NULL,
    ResultStatus VARCHAR(20) NOT NULL
        CONSTRAINT DF_Results_ResultStatus DEFAULT ('Official'),

    CONSTRAINT PK_Results PRIMARY KEY (ResultId),

    /* One result per enrolment. */
    CONSTRAINT UQ_Results_Enrollment UNIQUE (EnrollmentId),

    CONSTRAINT FK_Results_Enrollments
        FOREIGN KEY (EnrollmentId)
        REFERENCES dbo.EventEnrollments(EnrollmentId),

    CONSTRAINT CK_Results_FinishPosition
        CHECK (FinishPosition IS NULL OR FinishPosition > 0),

    CONSTRAINT CK_Results_FinishTime
        CHECK (FinishTimeSeconds IS NULL OR FinishTimeSeconds >= 0),

    CONSTRAINT CK_Results_Status
        CHECK (ResultStatus IN ('Official', 'Pending', 'Disqualified'))
);
GO

/* =========================================================
   SAMPLE DATA
   ========================================================= */

/* Roles */
INSERT INTO dbo.Roles (RoleName)
VALUES
    ('Organizer'),
    ('Participant');
GO

/* Users: 2 Organizers and 3 Participants */
INSERT INTO dbo.Users
    (RoleId, FirstName, LastName, Email, PasswordHash, Phone)
VALUES
    (1, 'Thabo', 'Mokoena', 'thabo@raceday.co.za', 'HASH_SAMPLE_THABO', '0825551001'),
    (1, 'Naledi', 'Dlamini', 'naledi@raceday.co.za', 'HASH_SAMPLE_NALEDI', '0835551002'),
    (2, 'Sibusiso', 'Nkosi', 'sibusiso@example.co.za', 'HASH_SAMPLE_SIBUSISO', '0845551003'),
    (2, 'Lerato', 'Molefe', 'lerato@example.co.za', 'HASH_SAMPLE_LERATO', '0725551004'),
    (2, 'Kabelo', 'Maseko', 'kabelo@example.co.za', 'HASH_SAMPLE_KABELO', '0715551005');
GO

/* Events: Run, Walk and Cycle are all represented. */
INSERT INTO dbo.Events
    (OrganizerId, EventName, Description, EventDate, Location,
     DistanceKm, EventType, EntryFee, MaxParticipants)
VALUES
    (1, 'Cape Town 10K Road Race',
     'A 10 kilometre road running event.',
     '2026-10-18', 'Cape Town, Western Cape',
     10.00, 'Run', 150.00, 5000),

    (2, 'Rustenburg Community Walk',
     'A community walking event promoting health and participation.',
     '2026-10-25', 'Rustenburg, North West',
     5.00, 'Walk', 80.00, 1500),

    (1, 'Pretoria Cycle Classic',
     'A road cycling event for recreational and competitive cyclists.',
     '2026-11-22', 'Pretoria, Gauteng',
     50.00, 'Cycle', 300.00, 2000);
GO

/* Event-specific age and distance categories */
INSERT INTO dbo.Categories
    (EventId, CategoryName, CategoryType, MinAge, MaxAge, CategoryDistanceKm)
VALUES
    (1, 'Under 20', 'Age', 0, 19, NULL),
    (1, 'Senior',    'Age', 20, 39, NULL),
    (1, '10km',      'Distance', NULL, NULL, 10.00),

    (2, 'Under 20',  'Age', 0, 19, NULL),
    (2, 'Adult',     'Age', 20, 59, NULL),
    (2, '5km',       'Distance', NULL, NULL, 5.00),

    (3, 'Open',      'Age', 18, 59, NULL),
    (3, '50km',      'Distance', NULL, NULL, 50.00);
GO

/* Participant enrolments.
   Each row records the participant, event and selected category. */
INSERT INTO dbo.EventEnrollments
    (EventId, UserId, CategoryId, EnrollmentDate, Status)
VALUES
    (1, 3, 2, '2026-09-01T09:00:00', 'Registered'),
    (1, 4, 3, '2026-09-01T09:15:00', 'Registered'),
    (2, 5, 5, '2026-09-01T10:00:00', 'Registered'),
    (2, 3, 6, '2026-09-01T10:20:00', 'Registered'),
    (3, 4, 8, '2026-09-01T11:00:00', 'Registered');
GO

/* Sample results linked to enrolments 1, 2 and 3. */
INSERT INTO dbo.Results
    (EnrollmentId, FinishPosition, FinishTimeSeconds, ResultStatus)
VALUES
    (1, 1, 2520, 'Official'),
    (2, 2, 2615, 'Official'),
    (3, 1, 2100, 'Official');
GO

/* =========================================================
   VERIFICATION QUERIES
   ========================================================= */

/* Table list */
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

/* Display all records */
SELECT * FROM dbo.Roles;
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Events;
SELECT * FROM dbo.Categories;
SELECT * FROM dbo.EventEnrollments;
SELECT * FROM dbo.Results;
GO

/* Record counts */
SELECT 'Roles' AS TableName, COUNT(*) AS RecordCount FROM dbo.Roles
UNION ALL
SELECT 'Users', COUNT(*) FROM dbo.Users
UNION ALL
SELECT 'Events', COUNT(*) FROM dbo.Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM dbo.Categories
UNION ALL
SELECT 'EventEnrollments', COUNT(*) FROM dbo.EventEnrollments
UNION ALL
SELECT 'Results', COUNT(*) FROM dbo.Results;
GO

/* Join query showing participant, event, selected category and result */
SELECT
    ee.EnrollmentId,
    u.FirstName + ' ' + u.LastName AS Participant,
    e.EventName,
    c.CategoryName,
    ee.Status AS EnrollmentStatus,
    r.FinishPosition,
    r.FinishTimeSeconds,
    r.ResultStatus
FROM dbo.EventEnrollments ee
INNER JOIN dbo.Users u
    ON ee.UserId = u.UserId
INNER JOIN dbo.Events e
    ON ee.EventId = e.EventId
INNER JOIN dbo.Categories c
    ON ee.EventId = c.EventId
   AND ee.CategoryId = c.CategoryId
LEFT JOIN dbo.Results r
    ON ee.EnrollmentId = r.EnrollmentId
ORDER BY ee.EnrollmentId;
GO
