# RaceDay Event Management System

## 1. Project Overview

RaceDay is an ASP.NET Core Web API built with .NET 8 for managing running and cycling events.

The API supports two roles:

* **Organisers:** Manage events, categories, enrolments, and race results.
* **Participants:** Browse events, enrol in categories, and view their enrolments and results.

## 2. Technologies Used

* ASP.NET Core Web API (.NET 8)
* Entity Framework Core
* SQL Server
* JWT Bearer authentication
* BCrypt password hashing
* Swagger / OpenAPI
* xUnit automated testing
* GitHub Actions CI/CD

## 3. Database Schema

The database contains the following core tables:

| Table     | Purpose                       |
| --------- | ----------------------------- |
| Role      | Stores user roles             |
| User      | Stores user accounts          |
| Event     | Stores race events            |
| Category  | Stores event categories       |
| Enrolment | Stores participant enrolments |
| Results   | Stores race results           |

See the [Entity Relationship Diagram](docs/ERD_RaceDay.pdf) and [API Endpoint Plan](docs/endpoint-plan.pdf).

## 4. API Endpoints

The API includes the following endpoint groups:

* **Authentication:** `/api/auth/register`, `/api/auth/login`
* **Events:** `/api/events`
* **Categories:** `/api/categories`
* **Enrolments:** `/api/enrolments`
* **Results:** `/api/results`

The API uses role-based authorization to restrict access to protected operations.

## 5. Prerequisites

Install the following before running the project:

* .NET 8 SDK
* SQL Server or SQL Server LocalDB
* Visual Studio 2022 (recommended)

## 6. Database Setup

1. Open SQL Server Management Studio or another SQL Server client.
2. Open the [`database.sql`](database.sql) script in the repository root.
3. Execute the script to create and populate the database.
4. Check the connection string in `RaceDay.API/appsettings.json` and update it for your SQL Server instance if necessary.

Do not commit passwords, private keys, or other secrets to the repository.

## 7. Running the API

1. Open `RaceDay.API/RaceDay.API.sln` in Visual Studio 2022.
2. Restore the NuGet packages.
3. Confirm the database connection string is correct.
4. Set `RaceDay.API` as the startup project.
5. Run the project.

When Swagger is enabled in the development environment, open the Swagger URL displayed in the application output to explore and test the endpoints.

## 8. Authentication

1. Register an account using `/api/auth/register`.
2. Log in using `/api/auth/login`.
3. Copy the JWT token returned by the login endpoint.
4. In Swagger, select **Authorize** and enter the token using the format expected by the Bearer authentication configuration.
5. Use the token to test protected endpoints according to the account's role.

## 9. Automated Tests

The automated tests are located in `RaceDay.API.Tests`.

Run the tests from the solution directory using:

```bash
dotnet test RaceDay.API.Tests/RaceDay.API.Tests.csproj
```

## 10. CI/CD

GitHub Actions workflows validate the repository and build and test the API. Check the **Actions** tab in GitHub for the latest workflow results.

Repository: [RaceDay on GitHub](https://github.com/brianjunior12/RaceDay)

## 11. Video Walkthrough

[View the RaceDay video walkthrough](https://youtu.be/rncqGdzAWPI)
