CREATE DATABASE QiymetIdareetmeDB;
GO

USE QiymetIdareetmeDB;
GO

CREATE TABLE Students (
    StudentID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL
);

CREATE TABLE Courses (
    CourseID INT PRIMARY KEY IDENTITY(1,1),
    CourseName NVARCHAR(100) NOT NULL
);

CREATE TABLE Grades (
    GradeID INT PRIMARY KEY IDENTITY(1,1),
    StudentID INT FOREIGN KEY REFERENCES Students(StudentID),
    CourseID INT FOREIGN KEY REFERENCES Courses(CourseID),
    Score INT CHECK (Score >= 0 AND Score <= 100)
);

INSERT INTO Students (FullName) VALUES (N'Aqsin Elekberov'), (N'Abbas Ebusetderov'), (N'Rəşad Quliyev');
INSERT INTO Courses (CourseName) VALUES (N'Proqramlaşdırma'), (N'Verilənlər Bazası'), (N'Riyaziyyat');