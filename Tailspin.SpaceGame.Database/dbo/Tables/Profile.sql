
CREATE TABLE [dbo].[Profiles] (
    [id]                INT           NOT NULL,
    [userName]          NVARCHAR (50) NOT NULL,
    [avatarUrl]         NVARCHAR (50) NULL,
    [birthdate]         DATETIME      NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);