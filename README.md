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

# RaceDay

RaceDay is a platform for running and cycling events. Organisers create events and record results. Participants find events, enrol and see their finish times.

| Part | What it is | Folder |
|---|---|---|
| 1 | Database design and API plan | `docs/` or your Part 1 folder |
| 2 | ASP.NET Core REST API with JWT auth, EF Core and tests | `RaceDay.API/`, `RaceDay.API.Tests/` |
| 3 | ASP.NET Core MVC web app, Azure Blob Storage, Docker | `RaceDay3.MVC/` |

**Demo video (unlisted YouTube):** <paste your link here>

---

# Part 3: MVC Web Application

The MVC app is the website front end. It gets all its data from the Part 2 API. It has no database access and no duplicated business logic. Images go from the MVC app to the API, and the API stores them in Azure Blob Storage. The MVC app never talks to Azure directly.

## Features

**Authentication and navigation**
- Register and log in through the API.
- The session stores the user's token, ID, role and name.
- Organisers and Participants each see a different menu.
- Protected pages redirect logged-out users to Login.
- Logging out clears the session.

**Organiser**
- Dashboard with total events, total enrolments, enrolments per event and upcoming dates.
- Create, edit and delete events (name, description, date, location, distance, type).
- Upload an event banner image.
- Add and remove categories for each event.
- View enrolments with their status, and confirm them.
- Enter and publish finish times and positions.

**Participant**
- Browse upcoming events and filter by name, type and date.
- View event details and available categories.
- Enrol in an event by choosing a category.
- My Events page showing enrolment status (green for Confirmed, amber for Pending).
- My Results page showing event, date, category, finish time and position (for example, 47th of 312).
- Profile page with a profile picture upload.

## Project structure

```
RaceDay3.MVC/
├── Controllers/    Account, Home, Events, Organiser, Participant, Profile
├── Services/       ApiClient (the only class that calls the API)
├── Filters/        SessionAuthorize (session and role guard)
├── Models/         ApiModels (API JSON), ViewModels (forms and pages)
├── Helpers/        Session and formatting helpers
├── Views/          Razor views for each role
├── wwwroot/css/    RaceDay theme
├── Dockerfile
└── appsettings.json
```

## Configuration

In `RaceDay3.MVC/appsettings.json`, set the address of your running API:

```json
"ApiSettings": { "BaseUrl": "http://localhost:<API-PORT>/" }
```

## Run locally

1. Set up the database and run the API (see Part 2).
2. Run the MVC project:

```bash
cd RaceDay3.MVC
dotnet run
```

## Run with Docker

```bash
docker build -t raceday-mvc ./RaceDay3.MVC

docker run --rm -p 8080:8080 \
  --add-host=host.docker.internal:host-gateway \
  -e ApiSettings__BaseUrl=http://host.docker.internal:<API-PORT>/ \
  raceday-mvc
```

Then open http://localhost:8080. The API must be running first. If your API is hosted online, use its URL instead of `host.docker.internal`.

On Windows PowerShell, write the `docker run` command on one line without the `\` characters.

## Azure Blob Storage

1. An organiser or participant picks an image in the MVC app.
2. MVC sends the file to the API (`POST api/events/{id}/banner` or `POST api/users/profile/picture`).
3. The API validates the image (JPG, PNG or WebP, up to 5 MB) and uploads it to Azure Blob Storage.
4. The API saves the image URL in SQL Server and returns it.
5. MVC displays the image from that URL.

## CI/CD

GitHub Actions (`.github/workflows/dotnet.yml`) runs on every push. It restores and builds the API, tests and MVC project, runs the unit tests, then builds the MVC Docker image.

**Latest run:**


## Demo video

<paste your link here>

The video covers the full running app for both roles, the MVC structure, Azure Blob Storage, and the Docker setup.

Repository: [RaceDay on GitHub](https://github.com/brianjunior12/RaceDay)

## 11. Video Walkthrough

[View the RaceDay video walkthrough](https://youtu.be/rncqGdzAWPI)
