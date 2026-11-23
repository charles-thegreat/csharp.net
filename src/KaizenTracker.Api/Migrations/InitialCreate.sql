IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123222738_InitialCreate'
)
BEGIN
    CREATE TABLE [Proposals] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Area] nvarchar(100) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [SubmittedAt] datetimeoffset NOT NULL,
        [CompletedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Proposals] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123222738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Proposals_Area] ON [Proposals] ([Area]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123222738_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Proposals_Status] ON [Proposals] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123222738_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123222738_InitialCreate', N'10.0.15');
END;

COMMIT;
GO

