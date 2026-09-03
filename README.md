# 🏃 RaceDay System

## 📌 Project Overview

**RaceDay** is a race and event management system designed to help **Organisers** and **Participants** manage running events efficiently.

The system allows:

* 🏃 **Organisers** to create and manage running events.
* 📋 Organisers to create and manage event categories.
* 👥 **Participants** to browse available events.
* 📝 Participants to enrol in race categories.
* 🏆 Participants and Organisers to view race results.

---

## 🚀 Project Progress

| Part          | Description                                                                 | Status      |
| ------------- | --------------------------------------------------------------------------- | ----------- |
| 🟢 **Part 1** | System Planning – ERD, API Endpoint Plan, and SQL Database Script           | ✅ Complete  |
| 🔵 **Part 2** | RESTful API built in C#, Database Integration, Unit Tests, and GitHub CI/CD | 🔜 Upcoming |
| 🟣 **Part 3** | MVC Web Application, API Integration, Azure Blob Storage, and Docker        | 🔜 Upcoming |

---

# 👥 System Roles

The RaceDay System supports different user roles.

| 👤 Role            | 📝 Description                                                                          |
| ------------------ | --------------------------------------------------------------------------------------- |
| 🏃 **Organiser**   | Creates and manages events and categories, views enrolments, and captures race results. |
| 👥 **Participant** | Browses events, enrols in categories, and views their enrolments and results.           |
| 🛡️ **Admin**      | Reserved for future system administration.                                              |

---

# 🗂️ Data Model

The system database is built around **six main entities**.

| Entity                | Description                                                                     |
| --------------------- | ------------------------------------------------------------------------------- |
| 👤 **Role**           | Lookup table containing system roles such as Organiser, Participant, and Admin. |
| 👥 **User**           | Stores all registered users and links each user to a Role.                      |
| 🏁 **Event**          | Represents a running event created by an Organiser.                             |
| 🏃 **Category**       | Represents an event category, such as 5km, 10km, or 21km.                       |
| 📝 **EventEnrolment** | Links a Participant to an Event Category.                                       |
| 🏆 **Result**         | Stores the result for a participant's event enrolment.                          |

📌 The complete relationships and cardinalities are documented in the **Entity Relationship Diagram (ERD)**.

---

# 🔗 Entity Relationships

The system follows the relationships below:

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

| Relationship              | Cardinality |
| ------------------------- | ----------- |
| Role → User               | 1 : Many    |
| User → Event              | 1 : Many    |
| Event → Category          | 1 : Many    |
| User → EventEnrolment     | 1 : Many    |
| Category → EventEnrolment | 1 : Many    |
| EventEnrolment → Result   | 1 : 1       |

---

# 📁 Project Structure

```text
RaceDay-System/
│
├── 📁 docs/
│   ├── 📄 ERD.pdf
│   ├── 📄 RaceDay_API_Endpoint_Plan.pdf
│   └── 🗄️ RaceDay_Database.sql
│
└── 📄 README.md
```

---

# 📚 Documentation

The `docs` folder contains all documentation and database files for **Part 1** of the Portfolio of Evidence.

| 📄 File                              | 📝 Description                                                                                                       |
| ------------------------------------ | -------------------------------------------------------------------------------------------------------------------- |
| `docs/ERD.pdf`                       | Entity Relationship Diagram showing the complete database structure and relationships.                               |
| `docs/RaceDay_API_Endpoint_Plan.pdf` | Planned REST API endpoints covering Authentication, User Profile, Events, Categories, Event Enrolments, and Results. |
| `docs/RaceDay_Database.sql`          | SQL Server script used to create and seed the RaceDay database.                                                      |

---

# 🗄️ ERD and SQL Alignment

The SQL database script is designed to match the **Entity Relationship Diagram (ERD)**.

✅ All six entities are implemented.

✅ Primary keys are included.

✅ Foreign key relationships are implemented.

✅ Entity cardinalities follow the ERD design.

✅ The **1:1 relationship** between `EventEnrolment` and `Result` is implemented.

> 📌 **Note:** There are no deliberate deviations between the ERD and the SQL database design.

---

# ⚙️ Database Setup

Follow these steps to set up the RaceDay database.

### 1️⃣ Open SQL Server Management Studio

Open **SQL Server Management Studio (SSMS)**.

### 2️⃣ Connect to SQL Server

Connect to your SQL Server instance.

### 3️⃣ Open the SQL Script

Open the following file:

```text
docs/RaceDay_Database.sql
```

### 4️⃣ Set Database Context

Ensure that the query window is using the `master` database before running the script.

### 5️⃣ Execute the Script

Run the SQL script.

The script will:

* 🗑️ Drop the existing `RaceDayDB` database if it exists.
* 🆕 Create a new `RaceDayDB` database.
* 🗂️ Create all required tables.
* 🔗 Create primary and foreign key relationships.
* 🌱 Insert sample data.

---

# 🛠️ Technologies

| Technology                | Purpose                                    |
| ------------------------- | ------------------------------------------ |
| 🗄️ **SQL Server**        | Database Management System                 |
| 💻 **SQL**                | Database creation and data manipulation    |
| 📊 **ERD**                | Database design and relationship modelling |
| 🔧 **GitHub**             | Version control and project documentation  |
| 💻 **C#**                 | RESTful API development *(Part 2)*         |
| 🌐 **ASP.NET MVC**        | Web application development *(Part 3)*     |
| ☁️ **Azure Blob Storage** | Cloud file storage *(Part 3)*              |
| 🐳 **Docker**             | Application containerisation *(Part 3)*    |

---

# 🗺️ Development Roadmap

### 🟢 Part 1 — System Planning

* [x] Create Entity Relationship Diagram (ERD)
* [x] Define database entities
* [x] Define relationships and cardinalities
* [x] Create API Endpoint Plan
* [x] Create SQL Database Script
* [x] Seed sample database data
* [x] Upload documentation to GitHub

### 🔵 Part 2 — RESTful API

* [ ] Develop RESTful API using C#
* [ ] Connect API to SQL Server
* [ ] Implement authentication
* [ ] Implement User Profile endpoints
* [ ] Implement Event endpoints
* [ ] Implement Category endpoints
* [ ] Implement Event Enrolment endpoints
* [ ] Implement Result endpoints
* [ ] Create unit tests
* [ ] Configure GitHub CI/CD

### 🟣 Part 3 — Web Application

* [ ] Develop MVC Web Application
* [ ] Connect the MVC application to the API
* [ ] Implement user interfaces
* [ ] Implement Azure Blob Storage
* [ ] Containerise the application using Docker

---

# 🎯 Project Purpose

The purpose of the **RaceDay System** is to provide a structured platform for managing running events.

The system will support the complete event management process, including:

🏁 Event creation
🏃 Race category management
👥 Participant registration
📝 Event enrolment
🏆 Result management

---

# 📌 Current Status

> 🟢 **Part 1: Complete**

The current repository contains the planning and database design documentation required for **Part 1 of the Portfolio of Evidence (POE)**.

🚀 Future development will include the RESTful API and MVC Web Application.

---

<p align="center">
  <b>🏁 RaceDay System</b>
  <br>
  <i>Managing races. Connecting participants. Tracking results.</i>
</p>
