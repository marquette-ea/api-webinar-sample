CREATE DATABASE YourGasUtility
GO

USE YourGasUtility
GO

CREATE TABLE [OpArea] (
  [ID] SMALLINT IDENTITY(1,1) PRIMARY KEY, 
  [Name] NVARCHAR(50) NOT NULL
);


CREATE TABLE [LoadObservation] (
  [Date] DATE NOT NULL,
  [OpArea] SMALLINT NOT NULL,
  [Value] FLOAT NOT NULL,

  CONSTRAINT [PK_LoadObservation] PRIMARY KEY ([Date], [OpArea]),
  CONSTRAINT [FK_LoadObservation_OpArea] FOREIGN KEY ([OpArea]) REFERENCES [OpArea]([ID])
);

CREATE TABLE [LoadForecast] (
  [ID] INT IDENTITY(1,1) PRIMARY KEY,
  [Date] DATE NOT NULL,
  [OpArea] SMALLINT NOT NULL,

  CONSTRAINT [AK_LoadForecast] UNIQUE ([Date], [OpArea]),
  CONSTRAINT [FK_LoadForecast_OpArea] FOREIGN KEY ([OpArea]) REFERENCES [OpArea]([ID])
);

CREATE TABLE [LoadForecastValue] (
  [Forecast] INT NOT NULL,
  [Horizon] TINYINT NOT NULL,
  [Value] FLOAT NOT NULL,

  CONSTRAINT [PK_LoadForecastValue] PRIMARY KEY ([Forecast], [Horizon]),
  CONSTRAINT [FK_LoadForecastValue_LoadForecast] FOREIGN KEY ([Forecast]) REFERENCES [LoadForecast]([ID])
);
GO

INSERT INTO [OpArea] ([Name]) VALUES ('Metropolis');
INSERT INTO [OpArea] ([Name]) VALUES ('Smallville');
GO
