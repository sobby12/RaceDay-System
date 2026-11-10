# RaceDay System

## 1. Project Overview

RaceDay is a web-based event management system developed to support the organisation and management of running, walking and cycling events.

The system provides two main user roles: **Participants** and **Organisers**. Participants can register and log in, browse available events, enrol in events, manage their enrolments and view their results. Organisers can manage events and categories, view participant enrolments and record race results.

The system was developed using ASP.NET Core Web API and ASP.NET Core MVC, with Microsoft SQL Server used for data storage. The API handles the application's business logic and database operations, while the MVC application provides the user interface.

The main aim of RaceDay is to provide a centralised platform that improves event administration, participant management and the recording of race results.

## 2. Project Objectives

The main objectives of the RaceDay system are to:

- Provide a platform where users can register and log in securely.
- Allow participants to browse available sporting events.
- Allow participants to enrol in events and manage their enrolments.
- Allow participants to view their own race results.
- Allow organisers to create, update and delete events.
- Allow organisers to create and manage event categories.
- Allow organisers to view participant enrolments.
- Allow organisers to record and manage race results.
- Store application information in a structured SQL Server database.
- Apply authentication, authorisation and data validation to protect the system.

## 3. System Features

### 3.1 User Registration and Login

The system provides user registration and login functionality.

Users can create accounts by providing the required personal information. The system stores user details in the database and uses password hashing to protect stored passwords.

After successful login, the API generates a JSON Web Token (JWT). The token is used to authenticate requests to protected API endpoints.

### 3.2 Participant Features

Participants can:

- Log in to their accounts.
- Browse available RaceDay events.
- View event information.
- Enrol in available events.
- View their own enrolments.
- View their race results.
- Access participant dashboard navigation.

### 3.3 Organiser Features

Organisers can:

- Access the organiser dashboard.
- Create, view, update and delete events.
- Manage event categories.
- View participant enrolments.
- Record race results.
- View and manage recorded results.

### 3.4 Results Management

The system allows organisers to record race results for registered participants.

Result information includes:

- Enrolment ID.
- Finishing position.
- Finishing time in seconds.
- Result status.

The result status can be recorded as Official, Pending or Disqualified, depending on the result information being entered.

Participants can access their own results through the participant functionality.

## 4. Technologies Used

| Technology / Tool | Purpose |
|---|---|
| C# | Main programming language |
| .NET 10 | Application development framework |
| ASP.NET Core Web API | Backend services and API endpoints |
| ASP.NET Core MVC | Web interface and user interaction |
| Entity Framework Core | Database access and object-relational mapping |
| Microsoft SQL Server | Database management |
| SQL Server Management Studio (SSMS) | Database creation and management |
| JSON Web Tokens (JWT) | API authentication |
| Swagger / OpenAPI | API documentation and endpoint testing |
| Visual Studio | Application development |
| Git and GitHub | Version control and source code hosting |
| GitHub Actions | Repository validation workflow |
| draw.io | Entity Relationship Diagram design |
| xUnit | Automated unit testing |

## 5. System Architecture

RaceDay follows a layered application structure.

### 5.1 Presentation Layer

The ASP.NET Core MVC application provides the user interface. It allows participants and organisers to interact with the system through dashboards, forms, tables and navigation links.

### 5.2 Application and API Layer

The ASP.NET Core Web API processes requests from the MVC application. It handles business logic, validation, authentication, authorisation and communication with the database.

### 5.3 Database Layer

Microsoft SQL Server stores the system's information, including users, roles, events, categories, enrolments and results.

### 5.4 General Request Flow

1. A user interacts with the RaceDay MVC application.
2. The MVC controller sends a request to the Web API.
3. The API validates the request and checks authentication where required.
4. The API communicates with the SQL Server database.
5. The API returns a response to the MVC application.
6. The MVC application displays the relevant information to the user.

## 6. Database Design

The RaceDay system uses a relational database named **RaceDayDB**.

The database contains six main entities:

| Table | Description |
|---|---|
| Roles | Stores the different user roles, including Organiser and Participant. |
| Users | Stores user account and personal information. |
| Events | Stores information about sporting events. |
| Categories | Stores event categories, including distance and age categories. |
| EventEnrollments | Stores participant enrolments in events. |
| Results | Stores the recorded results for participant enrolments. |

### 6.1 Database Relationships

The main database relationships include:

- A role can be assigned to multiple users.
- A user is assigned to a role.
- An event can contain multiple categories.
- A user can have multiple event enrolments.
- An event can have multiple participant enrolments.
- An enrolment can have a recorded result.

### 6.2 Data Integrity

Database constraints are used to maintain data accuracy and consistency.

These include:

- Primary keys to uniquely identify records.
- Foreign keys to maintain relationships between tables.
- Unique constraints to prevent duplicate email addresses.
- Check constraints to restrict certain values, such as valid category types.
- Required fields to prevent important information from being left empty.

The database also applies validation rules to help prevent invalid or duplicate records.

## 7. Authentication and Security

Security is an important part of the RaceDay system.

### 7.1 Password Hashing

Passwords are hashed before being stored in the database. This helps protect users' passwords if database information is exposed.

### 7.2 JSON Web Token Authentication

The API uses JWT authentication. After a successful login, a token is generated and must be included in requests to protected endpoints.

### 7.3 Role-Based Authorisation

The system separates permissions between Participants and Organisers.

**Participants** are allowed to access participant functionality, such as browsing events, managing their enrolments and viewing their own results.

**Organisers** are allowed to access management functionality, such as managing events and categories, viewing enrolments and recording results.

Access to organiser-only functionality is restricted for users who do not have the required permissions.

### 7.4 Input Validation

The application validates submitted information to help prevent incomplete or invalid data from being stored.

### 7.5 Data Protection

Database relationships and constraints help maintain data integrity. Authentication and authorisation help prevent unauthorised access to protected resources.

## 8. API Development

The RaceDay Web API provides endpoints for the main system functions.

| API Area | Functionality |
|---|---|
| Authentication | User registration and login |
| Events | Retrieve and manage sporting events |
| Categories | Create, retrieve and manage event categories |
| Enrolments | Register participants for events and manage enrolments |
| Results | Retrieve and manage participant results |
| Participant Results | Retrieve results belonging to the authenticated participant |

### 8.1 API Documentation and Testing

Swagger / OpenAPI is used to document and test the API endpoints.

Swagger allows developers to:

- View available API endpoints.
- Submit GET, POST, PUT and DELETE requests.
- View response status codes.
- Inspect returned JSON data.
- Test protected endpoints using JWT bearer authentication.

The API uses appropriate HTTP response codes, including:

- `200 OK` – Request completed successfully.
- `201 Created` – A new resource was created.
- `204 No Content` – Request completed successfully without returning content.
- `400 Bad Request` – Submitted information is invalid.
- `401 Unauthorized` – Authentication is required or invalid.
- `403 Forbidden` – The user does not have permission.
- `404 Not Found` – The requested resource could not be found.
- `409 Conflict` – The request conflicts with existing data.

## 9. Testing

Testing was performed to check that the system's main functions operate as expected.

### 9.1 Automated Unit Testing

The RaceDay.API.Tests project uses xUnit to perform automated tests on the Category model.

The following five tests were implemented:

| Test | Expected Result |
|---|---|
| Category_ShouldBeValid_WhenDetailsAreCorrect | Valid category information passes validation. |
| CategoryName_ShouldBeInvalid_WhenEmpty | An empty category name fails validation. |
| CategoryName_ShouldBeInvalid_WhenExceeds80Characters | A category name longer than 80 characters fails validation. |
| CategoryType_ShouldBeInvalid_WhenEmpty | An empty category type fails validation. |
| CategoryType_ShouldBeInvalid_WhenExceeds10Characters | A category type longer than 10 characters fails validation. |

**Automated test outcome:** All five tests passed successfully in Visual Studio Test Explorer.

### 9.2 Manual System Testing

Manual testing was also performed to verify the main application workflows.

The following functionality was tested:

- User login and access to the application.
- Participant dashboard navigation.
- Organiser dashboard navigation.
- Browsing available events.
- Viewing participant enrolments.
- Viewing participant results.
- Recording a new result as an organiser.
- Displaying saved results.
- Preventing participants from accessing organiser-only pages.
- API authentication using JWT bearer tokens.
- API responses for successful and unsuccessful requests.

### 9.3 Test Results

The five automated unit tests passed successfully.

Manual testing also confirmed that the main participant and organiser navigation functions worked and that organiser-only pages were blocked when accessed by a participant account.

These tests helped identify and correct issues during development.

## 10. GitHub and Version Control

Git and GitHub were used to manage the source code and track development changes.

The project repository is available at:

**GitHub Repository:**  
https://github.com/sobby12/RaceDay-System

Version control was used to:

- Track changes made during development.
- Commit completed features.
- Maintain the project source code.
- Push changes to the remote repository.
- Keep project documentation and development files together.

### 10.1 GitHub Actions

A GitHub Actions workflow is included in the repository under:

`.github/workflows/validate.yml`

The workflow performs repository validation checks.

It helps verify that the required project documentation and repository structure are present.

## 11. Project Structure

The solution is organised into separate projects to improve maintainability and make the different application responsibilities easier to manage.

The main project components are:

```text
RaceDay/
│
├── RaceDay.API/
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Services/
│   ├── appsettings.json
│   └── Program.cs
│
├── RaceDay.MVC/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── wwwroot/
│   └── Program.cs
│
├── RaceDay.API.Tests/
│   └── UnitTest1.cs
│
├── docs/
│   ├── RaceDay_ERD.drawio.png
│   ├── RaceDay.docx
│   ├── RaceDay_Database_Final.sql
│   └── github-actions-success.png
│
├── .github/
│   └── workflows/
│       └── validate.yml
│
└── README.md
```

*Note: The structure above shows the main project components. Individual files and folders may vary as development continues.*

## 12. Running the Project

### Prerequisites

Before running the project, ensure that the following are installed:

- Visual Studio with the required .NET development workload.
- .NET 10 SDK.
- Microsoft SQL Server.
- SQL Server Management Studio.
- Git.

### Setup Instructions

1. Clone the GitHub repository:

   `git clone https://github.com/sobby12/RaceDay-System.git`

2. Open the RaceDay solution in Visual Studio.

3. Create or restore the RaceDayDB database using the supplied SQL database script.

4. Update the database connection string in the API configuration to match the local SQL Server instance.

5. Configure the JWT issuer, audience and signing key using suitable local development settings. Do not commit private signing keys or passwords to GitHub.

6. Restore the required NuGet packages.

7. Build the solution.

8. Run the RaceDay.API project.

9. Run the RaceDay.MVC project.

10. Open the MVC application in a browser and log in using an available account.

### API Documentation

When the API is running, its Swagger documentation can be accessed through the Swagger URL configured by the application.

Use the Swagger Authorize feature to test endpoints that require a JWT bearer token.

## 13. Project Documentation

The following supporting documents and files are included in the GitHub repository:

- **Entity Relationship Diagram (ERD):**  
  https://github.com/sobby12/RaceDay-System/blob/master/docs/RaceDay_ERD.drawio.png

- **Project Documentation:**  
  https://github.com/sobby12/RaceDay-System/blob/master/docs/RaceDay.docx

- **Database SQL Script:**  
  https://github.com/sobby12/RaceDay-System/blob/master/docs/RaceDay_Database_Final.sql

- **GitHub Actions Validation Screenshot:**  
  https://github.com/sobby12/RaceDay-System/blob/master/docs/github-actions-success.png

## 14. Future Improvements

Although the main RaceDay functionality has been implemented, the system can be improved further in future versions.

Possible improvements include:

- Adding automated integration tests for API endpoints.
- Increasing automated test coverage for events, enrolments and results.
- Improving password recovery functionality.
- Adding email notifications for event enrolments and updates.
- Improving the user profile management experience.
- Adding more detailed reports for organisers.
- Improving the application's responsiveness on mobile devices.
- Expanding automated build and test validation through GitHub Actions.

These improvements would help make the system more reliable, user-friendly and suitable for a larger number of events and participants.

## 15. Project Demonstration

A demonstration video of the RaceDay project is available at:

Watch the RaceDay System project demonstration here:

[Watch RaceDay System Presentation](https://youtu.be/fH0eoyTcUs8)

## 16. Conclusion

The RaceDay system demonstrates the development of a web-based event management application using ASP.NET Core Web API, ASP.NET Core MVC and Microsoft SQL Server.

The system provides separate functionality for Participants and Organisers, allowing users to browse events, manage enrolments, view results and manage event information.

The project also demonstrates the use of relational database design, API development, JWT authentication, role-based authorisation, data validation, automated unit testing and version control.

The implementation and testing of these features provide a foundation for a functional and maintainable event management system.
