# RaceDay Event Management System

##  Project Overview

RaceDay is a comprehensive event management system designed for race organizers to manage running and cycling events. The system supports role-based access control with two primary roles:

- **Organisers**: Create and manage events, categories, and results
- **Participants**: Browse events, enrol in categories, and view results

## Database Schema

The database consists of 6 core tables:

| Table | Description | Primary Key | Foreign Keys |
|-------|-------------|-------------|--------------|
| Role | User roles (Organiser/Participant) | RoleID | - |
| User | System users | UserId | RoleID → Role |
| Event | Race events | EventID | organiserID → User |
| Category | Event categories | CategoryID | EventID → Event |
| Enrolment | Participant enrolments | EnrolmentID | ParticipantID → User, EventID → Event, CategoryID → Category |
| Results | Race results | ResultsID | EnrolmentID → Enrolment |

### Entity Relationship Diagram

[ERD](docs/ERD.png)

##  API Endpoints

The RESTful API provides full CRUD operations for all entities. See the complete [API Endpoint Plan](docs/API-Endpoint-Plan.md) for details.

Key endpoints include:
- Authentication: `/api/auth/register`, `/api/auth/login`
- Events: CRUD operations at `/api/events`
- Categories: Manage event categories
- Enrolments: Enrol and manage participation
- Results: Record and view race results

##  Setup Instructions

### Prerequisites
- SQL Server (any edition)
- SQL Server Management Studio (SSMS)

### Database Setup
1. Clone this repository
2. Open SSMS and run `docs/RaceDay-Schema.sql`
3. Verify the database and tables are created successfully

### Sample Data
The SQL script includes:
- 2 Organisers
- 5 Participants
- 3 Events
- Event categories
- Sample enrolments
- Sample results

##  CI/CD Pipeline

This repository uses GitHub Actions to validate the project structure automatically.

[CI/CD Build Passing](docs/build-passing.png)

The workflow validates:
-  Required `/docs` folder exists
-  ERD file is present (PNG or PDF)
-  API Endpoint Plan is present (MD or PDF)
-  SQL Script is present with CREATE TABLE and INSERT statements
-  README.md exists

##  Video Walkthrough

(https://youtu.be/rncqGdzAWPI)

The video covers:
- ERD design decisions and relationships
- API endpoint planning rationale
- SQL script walkthrough and live execution

##  Tags

`sql-server`, `database-design`, `rest-api`, `entity-relationship-diagram`, `github-actions`, `ci-cd`

