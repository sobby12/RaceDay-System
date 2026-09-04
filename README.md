# RaceDay-System

## System Description

RaceDay System is a race and event management system designed to support the management of running, walking and cycling events.

The system allows users to register and log in, manage their profiles, view available events and view event details. Participants can enrol in events by selecting a category, manage their enrolments and view their own race results. Organizers can create, update and delete events and categories, view event enrolments, and record and update participant results.

The database is implemented using Microsoft SQL Server and is called `RaceDayDB`. The database contains six main entities:

- Roles
- Users
- Events
- Categories
- EventEnrollments
- Results

The SQL database includes primary keys, foreign keys, unique constraints and check constraints to maintain data integrity. It also contains sample data and verification queries for testing the database in SQL Server Management Studio (SSMS).

## User Roles

### Organizer

The Organizer is responsible for managing race events and their categories. An Organizer can:

- Create new events
- Update existing events
- Delete events
- Create event categories
- Update event categories
- Delete event categories
- View participants enrolled in an event
- Record participant results
- Update existing results

The Organizer is also able to register, log in and manage their own user profile.

### Participant

The Participant is a user who takes part in race events. A Participant can:

- Register and log in
- View and update their own profile
- View available events
- View event details
- View event categories
- Enrol in an event by selecting a category
- View their own event enrolments
- Cancel an enrolment
- View their own race results

## API

The API endpoint plan defines endpoints for authentication, user profiles, events, categories, enrolments and results.

Examples include:

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Log in a registered user
- `GET /api/events` - View available events
- `POST /api/events` - Create an event
- `GET /api/events/{id}` - View a specific event
- `POST /api/events/{eventId}/enrolments` - Enrol in an event
- `GET /api/results/me` - View the logged-in participant's results

The complete endpoint plan is available in the `/docs` folder.

## Database

The RaceDay database is implemented using Microsoft SQL Server.

The SQL script:

1. Creates the `RaceDayDB` database.
2. Creates the six database tables.
3. Defines primary keys, foreign keys and constraints.
4. Inserts sample roles, users, events, categories, enrolments and results.
5. Provides verification queries for testing the database in SSMS.

## Project Documentation

The planning documents are available in the `/docs` folder:

- [ERD](docs/RaceDay_ERD.drawio.png)
- [Endpoint Plan](docs/RaceDay.docx)
- [SQL Database Script](docs/RaceDay_Database_Final.sql)

## CI/CD

GitHub Actions is used to validate the required repository structure and documentation.

A workflow will check that the required `/docs` folder and project files are present.

### Successful Build



![Successful GitHub Actions Build](docs/github-actions-success.png)

## Project Video

An unlisted YouTube video will be provided showing the planning documents, ERD decisions, endpoint plan and SQL script being executed in SQL Server Management Studio.

[Watch the RaceDay System project walkthrough on YouTube](https://youtu.be/IO0UysTTx9Q)
