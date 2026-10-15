# 🏃 RaceDay System

## 📌 Project Overview

RaceDay is a race and event management system designed to help Organisers and Participants manage running events efficiently.

The system allows:

- 🏃 Organisers to create and manage running events.
- 📋 Organisers to create and manage event categories.
- 👥 Participants to browse available events.
- 📝 Participants to enrol in race categories.
- 🏆 Participants and Organisers to view race results.

---

## 🚀 Project Progress

| Part | Description | Status |
|------|-------------|--------|
| 🟢 **Part 1** | System Planning – ERD, API Endpoint Plan, and SQL Database Script | ✅ Complete |
| 🔵 **Part 2** | RESTful API built in C#, Database Integration, Unit Tests, and GitHub CI/CD | ✅ Complete |
| 🟣 **Part 3** | MVC Web Application, API Integration, Azure Blob Storage, and Docker | 🔜 Upcoming |

---

## 🎥 Video Demonstration

🔗 **Part 2 video link:** PASTE YOUR VIDEO LINK HERE

---

## 👥 System Roles

The RaceDay System supports different user roles.

| 👤 Role | 📝 Description |
|--------|----------------|
| 🏃 **Organiser** | Creates and manages events and categories, views enrolments, and captures race results. |
| 👥 **Participant** | Browses events, enrols in categories, and views their enrolments and results. |
| 🛡️ **Admin** | Reserved for future system administration. Cannot be registered through the API. |

---

## 🗂️ Data Model

The system database is built around six main entities.

| Entity | Description |
|--------|-------------|
| 👤 **Role** | Lookup table containing system roles such as Organiser, Participant, and Admin. |
| 👥 **User** | Stores all registered users and links each user to a Role. |
| 🏁 **Event** | Represents a running event created by an Organiser. |
| 🏃 **Category** | Represents an event category, such as 5km, 10km, or 21km. |
| 📝 **EventEnrolment** | Links a Participant to an Event Category. |
| 🏆 **Result** | Stores the result for a participant's event enrolment. |

📌 The complete relationships and cardinalities are documented in the Entity Relationship Diagram (ERD).

### 🔗 Entity Relationships

```text
Role
 │
 └───< User
         │
         ├───< Event
         │       │
         │       └───< Category
         │               │
         │               └───< EventEnrolment
         │                       │
         │                       └──── Result
         │
         └───< EventEnrolment
```

### 📊 Key Relationships

| Relationship | Cardinality |
|-------------|-------------|
| Role → User | 1 : Many |
| User → Event | 1 : Many |
| Event → Category | 1 : Many |
| User → EventEnrolment | 1 : Many |
| Category → EventEnrolment | 1 : Many |
| EventEnrolment → Result | 1 : 1 |

---

## 📁 Project Structure

```text
Raceday-System/
│
├── 📁 .github/
│   └── 📁 workflows/
│       └── 📄 ci.yml                      GitHub Actions build and test pipeline
│
├── 📁 docs/
│   ├── 📄 ERD.pdf
│   ├── 📄 RaceDay_API_Endpoint_Plan.pdf
│   ├── 🗄️ RaceDay_Database.sql
│   └── 🖼️ ci-green-build.png             Screenshot of the passing CI build
│
├── 📁 RaceDay.Api/                        Part 2 – RESTful API
│   ├── 📁 Controllers/                    Auth, Users, Events, Categories, Enrolments
│   ├── 📁 Models/                         Entity classes matching the database tables
│   ├── 📁 DTOs/                           Request and response classes
│   ├── 📁 Data/                           RaceDayDbContext
│   ├── 📁 Filters/                        RequireRole authorization filter
│   ├── 📄 appsettings.json                Connection string
│   └── 📄 Program.cs                      Sessions, Swagger and database setup
│
├── 📁 RaceDay.Tests/                      Part 2 – Unit and integration tests
├── 📄 RaceDay.Api.slnx                    Visual Studio solution
└── 📄 README.md
```

---

## 📚 Documentation

The docs folder contains the documentation and database files for the Portfolio of Evidence.

| 📄 File | 📝 Description |
|--------|----------------|
| `docs/ERD.pdf` | Entity Relationship Diagram showing the complete database structure and relationships. |
| `docs/RaceDay_API_Endpoint_Plan.pdf` | Planned REST API endpoints covering Authentication, User Profile, Events, Categories, Event Enrolments, and Results. |
| `docs/RaceDay_Database.sql` | SQL Server script used to create and seed the RaceDay database. |
| `docs/ci-green-build.png` | Screenshot of a successful GitHub Actions run. |

---

## 🗄️ ERD and SQL Alignment

The SQL database script is designed to match the Entity Relationship Diagram (ERD), and the Part 2 API models match the SQL script.

- ✅ All six entities are implemented.
- ✅ Primary keys are included.
- ✅ Foreign key relationships are implemented.
- ✅ Entity cardinalities follow the ERD design.
- ✅ The 1:1 relationship between EventEnrolment and Result is implemented.

📌 **Note:** There are no deliberate deviations between the ERD and the SQL database design.

---

## ⚙️ Setup Instructions

Follow these steps to run the Part 2 API on your own computer.

### 🧰 Prerequisites

| Requirement | Purpose |
|-------------|---------|
| 💻 Visual Studio 2022 or later | With the **ASP.NET and web development** workload |
| 🟣 .NET 10 SDK | Builds and runs the API |
| 🗄️ SQL Server Express | Hosts the RaceDay database |
| 🛠️ SQL Server Management Studio (SSMS) | Runs the database script |

### 1️⃣ Create the database

1. Open SQL Server Management Studio (SSMS) and connect to your SQL Server instance.
2. Open `docs/RaceDay_Database.sql`.
3. Make sure the query window is using the `master` database.
4. Click **Execute**.

The script will:

- 🗑️ Drop the existing RaceDayDB database if it exists.
- 🆕 Create a new RaceDayDB database.
- 🗂️ Create all required tables.
- 🔗 Create primary and foreign key relationships.
- 🌱 Insert sample data.

### 2️⃣ Set the connection string

Open `RaceDay.Api/appsettings.json` and change the server name to match your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

📌 In JSON, a backslash must be doubled, for example `Sbonga\\SQLEXPRESS`.

### 3️⃣ Run the API

1. Open `RaceDay.Api.slnx` in Visual Studio.
2. Set `RaceDay.Api` as the startup project and select the **https** profile.
3. Press **Start**. The console shows the address the API listens on, for example `https://localhost:7091`.
4. Open `https://localhost:7091/swagger` in your browser.

⚠️ The seeded users in the SQL script have placeholder passwords and cannot log in. Create your own users with `POST /api/auth/register` (roleId **1** = Organiser, **2** = Participant).

### 4️⃣ Run the tests

In Visual Studio, open **Test**, then **Test Explorer**, then **Run All Tests**. Or from a terminal:

```bash
dotnet test
```

The tests use an in-memory database, so SQL Server is not needed to run them.

---

## 🔐 Authentication and Roles

| 🔑 Feature | 📝 How it works |
|-----------|-----------------|
| 📝 **Register** | `POST /api/auth/register` creates an Organiser or Participant. The Admin role is rejected. |
| 🔒 **Password security** | Passwords are hashed with BCrypt. Plain text passwords are never stored. |
| 🚪 **Login** | `POST /api/auth/login` checks the password and stores the user ID and role in a server-side session. The browser keeps a session cookie. |
| 🚶 **Logout** | `POST /api/auth/logout` clears the session. |
| 🛡️ **Role protection** | A `RequireRole` filter returns **401 Unauthorized** when not logged in and **403 Forbidden** when the role is wrong. |
| 🏁 **Ownership** | Organisers can only change events, categories and results for events they own. Participants can only see and cancel their own enrolments. |

📌 Sessions are held in server memory, so restarting the API logs everyone out.

---

## 🌐 API Endpoints

| Method | Route | 👤 Role | Description |
|--------|-------|--------|-------------|
| POST | `/api/auth/register` | Public | Register an Organiser or Participant |
| POST | `/api/auth/login` | Public | Log in and create a session |
| POST | `/api/auth/logout` | Any | Clear the session |
| GET | `/api/users/me` | Logged in | View own profile |
| PUT | `/api/users/me` | Logged in | Update own profile |
| GET | `/api/events` | Public | List events (optional date and location filters) |
| GET | `/api/events/{id}` | Public | Event details with categories |
| POST | `/api/events` | Organiser | Create an event |
| PUT | `/api/events/{id}` | Organiser (owner) | Update an event |
| DELETE | `/api/events/{id}` | Organiser (owner) | Delete an event |
| GET | `/api/events/{id}/categories` | Public | List an event's categories |
| POST | `/api/events/{id}/categories` | Organiser (owner) | Add a category |
| PUT | `/api/categories/{id}` | Organiser (owner) | Update a category |
| DELETE | `/api/categories/{id}` | Organiser (owner) | Delete a category |
| POST | `/api/categories/{id}/enrol` | Participant | Enrol in a category |
| GET | `/api/users/me/enrolments` | Participant | List own enrolments |
| DELETE | `/api/enrolments/{id}` | Participant (owner) | Cancel own enrolment |
| GET | `/api/events/{id}/enrolments` | Organiser (owner) | Event roster |
| POST | `/api/enrolments/{id}/result` | Organiser (owner) | Capture a result |
| GET | `/api/events/{id}/results` | Public | Event leaderboard |

📖 Every endpoint is documented in Swagger with its purpose, input and possible responses.

---

## 🔀 Differences from the Part 1 Plan

| 📌 Change | 📝 Reason |
|----------|-----------|
| 🔑 **Session authentication instead of JWT** | The Part 1 plan said login returns a JWT. The Part 2 brief requires server-side sessions, so login stores the user ID and role in the session. |
| ⚠️ **Extra 409 Conflict responses** | Deleting an event that still has categories, deleting a category that has enrolments, cancelling an enrolment that has a result, and changing a profile email to one already in use all return 409. Without these checks the database would reject the request with a 500 error. |
| 🛡️ **Ownership checks return 403** | Capturing a result, viewing a roster and similar actions return 403 if the Organiser does not own the event. |
| 📏 **Distance stored on Category** | This follows the Part 1 database design. |

---

## 🧪 Unit Tests

The `RaceDay.Tests` project contains 23 tests that run against an in-memory database.

| 🧩 Area | ✅ Scenarios covered |
|--------|---------------------|
| 🔐 **Authentication** | Register (success, duplicate email, invalid email, blocked Admin role), login (success, wrong password), profile access with and without a login. |
| 🏁 **Events and categories** | Create as Organiser, rejected for Participant (403) and anonymous users (401), update by a non-owner (403), not found (404), add a category, delete rules (409 and 204). |
| 📝 **Enrolments and results** | Enrol (201), duplicate enrolment (409), Organiser blocked from enrolling (403), unknown category (404), capture a result (201), duplicate result (409), leaderboard. |

---

## 🔄 Continuous Integration

A GitHub Actions workflow (`.github/workflows/ci.yml`) restores, builds and runs all tests on every push.

![Green CI build](docs/ci-green-build.png)

---

## 🛠️ Technologies

| Technology | Purpose |
|-----------|---------|
| 🗄️ SQL Server | Database Management System |
| 💻 SQL | Database creation and data manipulation |
| 📊 ERD | Database design and relationship modelling |
| 💻 C# and ASP.NET Core | RESTful API development (Part 2) |
| 🧱 Entity Framework Core | Data access |
| 🔒 BCrypt | Password hashing |
| 📖 Swagger | API documentation and testing |
| 🧪 xUnit | Unit and integration testing |
| 🔧 GitHub and GitHub Actions | Version control and CI/CD |
| 🌐 ASP.NET MVC | Web application development (Part 3) |
| ☁️ Azure Blob Storage | Cloud file storage (Part 3) |
| 🐳 Docker | Application containerisation (Part 3) |

---

## 🗺️ Development Roadmap

### 🟢 Part 1 — System Planning

- [x] Create Entity Relationship Diagram (ERD)
- [x] Define database entities
- [x] Define relationships and cardinalities
- [x] Create API Endpoint Plan
- [x] Create SQL Database Script
- [x] Seed sample database data
- [x] Upload documentation to GitHub

### 🔵 Part 2 — RESTful API

- [x] Develop RESTful API using C#
- [x] Connect API to SQL Server
- [x] Implement authentication
- [x] Implement User Profile endpoints
- [x] Implement Event endpoints
- [x] Implement Category endpoints
- [x] Implement Event Enrolment endpoints
- [x] Implement Result endpoints
- [x] Document endpoints in Swagger
- [x] Create unit tests
- [x] Configure GitHub CI/CD

### 🟣 Part 3 — Web Application

- [ ] Develop MVC Web Application
- [ ] Connect the MVC application to the API
- [ ] Implement user interfaces
- [ ] Implement Azure Blob Storage
- [ ] Containerise the application using Docker

---

## 🎯 Project Purpose

The purpose of the RaceDay System is to provide a structured platform for managing running events.

The system will support the complete event management process, including:

🏁 Event creation • 🏃 Race category management • 👥 Participant registration • 📝 Event enrolment • 🏆 Result management

---

## 📌 Current Status

🟢 **Part 1:** Complete

🔵 **Part 2:** Complete

The repository now contains the planning documentation from Part 1 and the working RESTful API, tests and CI pipeline from Part 2.

🚀 Future development will include the MVC Web Application.

---

# 🏁 RaceDay System

*Managing races. Connecting participants. Tracking results.*
