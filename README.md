# NoteApplication

Project Overview

This system uses Vue 3 + Tailwind CSS for the frontend and ASP.NET Core Web API for the backend.
It also integrates with SQL Server and Redis.

Key Features
Enforces strong password policy during registration.

Prevents duplicate usernames.

Locks out users for 5 minutes after 5 failed login attempts.

Uses Redis to store user tokens and their IDs.

Implements authentication and authorization for secured endpoints.


Setup Instructions
Step 1
notes-app
Rename .envsample to .env.

Update the VITE_API_BASE_URL in .env if needed.

Install dependencies and start the development server:

bash
Copy
Edit
npm install
npm run dev


Step 2
note-api
Rename appsettingsSample.json to appsettings.json.

Populate appsettings.json with the required configuration values, including:

SQL Server connection string

Redis settings

JWT configuration, specifying the issuer, audience, and a secret key with a minimum length of 31 characters.



Step 3
Instead of using Entity Framework migrations, run the following SQL scripts to create the required tables


-- Create Users table
CREATE TABLE [Users] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(256) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [FailedLoginAttempts] INT NOT NULL DEFAULT 0,
    [LockoutEnd] DATETIME2 NULL
);

-- Create Notes table
CREATE TABLE [Notes] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Content] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [delby] INT NULL,
    [deleted_at] DATETIME2 NULL,
);

use this sql query instead migrate file to get table 
