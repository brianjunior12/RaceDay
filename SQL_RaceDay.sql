CREATE DATABASE RaceDay;

USE RaceDay

CREATE TABLE [Role] (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    FOREIGN KEY (RoleID) REFERENCES [Role](RoleID)
);

CREATE TABLE [Event] (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName VARCHAR(100) NOT NULL,
    Event_Discrip VARCHAR(250) NOT NULL,
    EventDate DATETIME NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventType VARCHAR(80) NOT NULL,
    Location VARCHAR(250) NOT NULL, 
    organiserID INT NOT NULL,
    FOREIGN KEY (organiserID) REFERENCES [User](UserId)
);

CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL,
    CategoryType VARCHAR(250) NOT NULL,
    EventID INT NOT NULL,
    FOREIGN KEY (EventID) REFERENCES [Event](EventID)
);

CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    Enrolment_status VARCHAR(100) NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (ParticipantID) REFERENCES [User](UserId),
    FOREIGN KEY (EventID) REFERENCES [Event](EventID),
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

CREATE TABLE Results (
    ResultsID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME NOT NULL,
    FinishPosition INT NOT NULL,
    FOREIGN KEY (EnrolmentID) REFERENCES Enrolment(EnrolmentID)
);

-- 1. Roles (RoleID 1 = Organiser, RoleID 2 = Participant)
INSERT INTO [Role] (RoleName) VALUES 
('Organiser'), 
('Participant');

-- 2. Users (UserIDs 1 & 2 = Organisers; UserIDs 3 to 7 = Participants)
INSERT INTO [User] (FirstName, LastName, Email, password_hash, RoleID) VALUES 
('Sibusiso', 'Dlamini', 'sibusiso.dlamini@raceday.co.za', '$2a$11$0KjV/lX.K90C2W7wT8oD3e3O9mK2E4.k7vT1W3y4Z5A6B7C8D9E0F', 1),
('Klara', 'van der Merwe', 'klara.vdm@raceday.co.za', '$2y$11$pggcd7vyfB3WyKQSmmKAG.Z510f2Ew59icYZpQUpd1pgczmgyQ6k.', 1),
('Thabo', 'Mokoena', 'thabo.mokoena@gmail.com', '$2y$11$DfWf81maA0sIaLvh0prayuwYq/fu66g565e8L8mVKgTLJ4veJHGMa', 2),
('Jessica', 'Smith', 'jessica.smith@yahoo.com', '$2y$11$8/mIvualoegFRShQjtWrbOBvSv1AqNIjbKK1tDjChFa9M4/v8Lika', 2),
('Liam', 'Patel', 'liam.patel@gmail.com', '$2y$10$MyYsPcCB903.tP9rJ2Lv..I6AYq02lZotjR6KmJb6fJFOIDVcpoFK', 2),
('Nomvula', 'Khumalo', 'nomvula.k@gmail.com', '$2y$11$ZdXS5g4Z5novdZQdOdoauOSUw9DK8XWfZI4bMNcy.FPIaQk3PHAa2', 2),
('Ethan', 'Naidoo', 'ethan.naidoo@yahoo.com', '$2y$11$po.ohMF6GMl19EjuLe2po.ukvfEfU0d7VD6740GAex7wn/SuuXhne', 2);

-- 3. Events (Linked to Organisers 1 and 2)
INSERT INTO [Event] (EventName, Event_Discrip, EventDate, Distance, EventType, Location, organiserID) VALUES 
('Soweto Marathon', 'Annual road running event through the historic streetscapes of Soweto.', '2027-11-07 05:30:00', 42.20, 'Run', 'Soweto, Gauteng', 1),
('Durban Promenade Run', 'Scenic coastal road race along the Indian Ocean beachfront.', '2027-08-15 06:00:00', 21.10, 'Run', 'Durban, KwaZulu-Natal', 1),
('Karisma MTB Challenge', 'Premier off-road mountain biking challenge through forestry trails.', '2027-05-22 07:00:00', 65.00, 'Cycle', 'Stellenbosch, Western Cape', 2),
('Tshwane Classic', 'Major municipal cycling race hosted across city highways.', '2027-10-10 06:15:00', 98.00, 'Cycle', 'Pretoria, Gauteng', 2);

-- 4. Categories (Linked to Events 1 through 4)
INSERT INTO Category (CategoryName, CategoryType, EventID) VALUES
('Full Marathon 42km', 'Distance', 1),
('Half Marathon 21km', 'Distance', 2),
('MTB Challenge 65km', 'Distance', 3),
('City Cycle 98km', 'Distance', 4);

-- 5. Enrolments (Participants 3 to 7)
INSERT INTO Enrolment (ParticipantID, EnrolmentDate, EventID, CategoryID, Enrolment_status) VALUES
(3, '2026-09-01 08:30:00', 1, 1, 'Confirmed'),
(4, '2026-09-02 09:15:00', 1, 1, 'Confirmed'),
(5, '2026-09-03 10:00:00', 2, 2, 'Pending'),  
(6, '2026-09-04 11:45:00', 3, 3, 'Confirmed'),
(7, '2026-09-05 14:20:00', 4, 4, 'Pending');  

-- 6. Results (Only for Confirmed Enrolments 1, 2, and 4)
INSERT INTO Results (EnrolmentID, FinishTime, FinishPosition) VALUES
(1, '03:15:42', 45),
(2, '03:42:10', 112),
(4, '02:50:05', 18);

SELECT * FROM [Role];
SELECT * FROM [User];
SELECT * FROM [Event];
SELECT * FROM Category;
SELECT * FROM Enrolment;
SELECT * FROM Results;