[![Build, Test & Deploy All User Management Projects on Azure](https://github.com/Bilal44/inflo-user-management/actions/workflows/inflo-azure-ci.yml/badge.svg)](https://github.com/Bilal44/inflo-user-management/actions/workflows/inflo-azure-ci.yml)

# Inflo User Management System
The Inflo User Management system consists of 3 different major components:
- .NET 9 ASP.NET Core full stack web application
- .NET 9 RESTful Web API with OpenAPI documentation
- .NET 9 Blazor Web application that consumes the aforementioned REST API

This README serves as a detailed guide on how to use the applications, libraries used, dependencies, testing, Dockerisation and architecture.

## Table of Contents
- [Task Checklist](#task-checklist)
- [Design Decisions](#design-decisions)
- [Architecture](#architecture)
- [ASP.NET Website](#aspnet-website)
- [Libraries and Dependencies](#libraries-and-dependencies)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Authentication](#authentication)
- [Rate Limiting](#rate-limiting)
- [Hangfire Dashboard](#hangfire-dashboard)
- [Dockerisation](#dockerisation)
- [Deployment](#deployment)
  - [Docker Images](#docker-images)
  - [Containers](#containers)

## Task Checklist
### Standard
- [x] Filtering users by their active status

### Standard
- [x] Adding `DateOfBirth` to the model and relevant views
- [x] Add, View, Edit, Delete actions and views with validation and error/success notifications

### Advanced
- [x] View screens for all and single log entries
- [x] Successful actions on any user are now being logged asynchronously using Hangfire
- [x] User and log entries are linked through foreign keys and are easily navigable on the website without loading excess data (no implicit joins)
- [x] Logs can filtered by date range/user id/partially matching words in log itself. The results are automatically paginated and are sortable.

### Expert
- [x] Designed a secure RESTful API with API key authentication and rate limiting
- [x] Added Blazor client to interact with the API
- [x] Added OpenAPI documentation
- [x] The ASP.NET website and API now use SQL server database
- [x] The solution is configurable for better maintenance and sensitive data is securely stored as env variables

### Platform
- [x] Version controlled using Git/GitHub
- [x] CI pipeline with automated test suites
- [x] CD pipeline to deploy it to Azure
- [x] Hangfire to process tasks asynchronously

### Future Ideas
- Review the tests, add tests against the "real" database or at least TestContainers
- Azure Service Bus integration over AMQP and Terraform IaC support for vendor-agnostic deployments
- Cursor pagination
- JWT and claims-based authorization
- Retries and switch-breaker for improved resilience

## Design Decisions
- Although I was initially planning on it, I did not implement a user login system. If interested, the code and deployed version of a similar system is [available here](https://github.com/Bilal44/WeeBitsHR). Although the architecture is quite simple, it uses the same ASP.NET Core Identity and generates a random password for a new user who can now log into the system as an employee.
- I didn't touch the design of ASP.NET website much because instructions were very explicit. I took some liberty with Blazor design though. I am not a huge fan of redundant delete-confirm pages. ;)
- Only successful view, create, edit and delete are being logged. Any unsuccessful attempts are ignored.
- The user validation is intentionally kept quite lax to not negatively affect user experience.
- I have kept Serilog logging to a minimum because there is a manual audit trail feature as part of the exercise anyway.
- I intentionally allowed warnings in the API project. Otherwise, the linter wanted me to add XML comments to every public class, even `Program.cs` and wouldn't build without it.
- I initially wanted to try out Microsoft's very own OpenAPI library and then used `Sacalar` on top of it. Unfortunately, it looks like XML support won't be coming before .NET 10. So, I installed Swashbuckle and used SwaggerUI. Both can be accessed at http://inflo-api.azurewebsites.com, just need to add `/scalar` or `/swagger` in front of it.

## Architecture
The project follows the existing MVC layered architecture, extending naturally from the existing codebase with sufficient separation concerns and modular components. Here is an overview of the project structure:

```
.NET 9 Solution
├── UserManagement.Web
│   ├── Controllers
│   │   ├── HomeController.cs
│   │   ├── LogController.cs
│   │   └── UserController.cs
│   ├── Views
│   │   ├── Home
│   │   │   └── Index.cshtml
│   │   ├── Log
│   │   │   ├── List.cshtml
│   │   │   └── View.cshtml
│   │   ├── User
│   │   │   ├── Add.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   ├── Delete.cshtml
│   │   │   ├── View.cshtml
│   │   │   └── List.cshtml
│   │   └── Shared
│   └── wwwroot
│       └── assets, css, js, etc.
│
├── UserManagement.Data
│   ├── Entities
│   │   ├── User.cs
│   │   └── Log.cs
│   ├── Repositories
│   │   └── Repository.cs
│   └── Context
│
├── UserManagement.Services
│   ├── Domain
│   ├── UserService.cs
│   └── LogService.cs
│
├── UserManagement.Api
│   ├── Controllers
│   │   ├── LogController.cs
│   │   └── UserController.cs
│   ├── Authentication
│   │   ├── AuthConstants.cs
│   │   └── AuthMiddleware.cs
│   └── Program.cs
│
├── UserManagement.Client
│   ├── Components
│   │   ├── Layout
│   │   │   └── MainLayout.razor
│   │   └── Pages
│   │       ├── Home.razor
│   │       ├── Error.razor
│   │       └── Users
│   │           ├── List.razor
│   │           └── UserModal.razor
│   └── wwwroot
│       └── assets, css, js, etc.
│
├── Tests
│   ├── UserManagement.Api.Tests
│   ├── UserManagement.Web.Tests
│   ├── UserManagement.Services.Tests
│   └── UserManagement.Data.Tests
│
└──...
```

## ASP.NET Website
The ASP.NET solution is built using .NET 9 and provides all specified features for user management as well as paginated search and filtering of user audit trails (logs). It allows users to be added, retrieved, updated and deleted from the associated Microsoft SQL Server database. It also records all major interactions (view, add, update and delete) with individual user accounts and saves the audit trail in a separate logs section, which can be viewed and filtered by data range log message. The log management system is designed with scalability and improved user experience in mind and supports pagination and asynchronous data recording for a smoother experience over a much resilient system.

**[Live Azure Demo with Azure SQL Database](https://inflo-web.azurewebsites.net)**

## Libraries and Dependencies
The following libraries and frameworks are required to run these projects:

- Visual Studio 2022/VS Code/Rider etc.
- .NET SDK 9.0 or later
- A local SQL server instance e.g. SQL Express, IDE, containerised etc.
- Entity Framework
- ASP.NET MVC
- Serilog
- Newtonsoft Json v13+ due to found vulnerability
- Hangfire and Hangfire Dashboard Basic Authorization
- Blazored Toast
- Microsoft OpenApi
- Scalar
- Swashbuckle SwaggerGen and SwaggerUI
- xUnit, FakeIEasy and FluentAssertions (for testing only)
- Visual Studio Tools for Containers 1.14.0 (For running containerised service via Visual Studio 2022)
- Docker and Docker Compose (optional)

Ensure that you have the required SDK, libraries and dependencies installed before proceeding with the installation.

## Getting Started
To run the Inflo projects, follow these steps:

1. Clone the repository:

   ```bash
   git clone https://github.com/Bilal44/inflo-user-management.git

2. Navigate to a project directory:

   ```bash
   cd inflo-user-management/{project}
   ```

3. Build the project:

   ```bash
   dotnet build
   ```

4. Run the project:

   ```bash
   dotnet run
   ```

Pleas note that the API project should be running for the Blazor front end application to work. The projects will start after executing `dotnet run` and should be accessible at:

| Project | Local IP Address | Azure Deployment URL |
|  --------  |  -------  |  -------  |
|ASP.NET website|https://localhost:7084|https://inflo-web.azurewebsites.net|
|REST API|https://localhost:7000|https://inflo-api.azurewebsites.net/swagger|
|Blazor Client|https://localhost:7182|https://inflo-blazor.azurewebsites.net|

## API Endpoints
Once the Inflo User Management API is running, you can interact with it using an API client such as Postman or cURL. The API provides the following endpoints:

**[Blazor Client Deployed on Azure](https://inflo-blazor.azurewebsites.net)**

The RESTful API exposes the following endpoints:

- `GET /users`: Retrieves all users, can be optionally filtered by users' `active` status.
- `GET /users/{id}`: Retrieves a user by its id.
- `POST /users`: Creates a new user after validation.
- `PUT /users/{id}`: Updates an existing user after validation.

![Image](https://github.com/user-attachments/assets/7f9824a5-7242-4308-942e-463e373363d4)

For detailed information on each endpoint, including request and response formats, refer to the [Scalar](https://inflo-api.azurewebsites.net/scalar) or [Swagger](https://inflo-api.azurewebsites.net/swagger) OpenAPI documentations.

## Testing
The projects support unit and integration tests with xUnit, FakeItEasy, FluentAssertions, InMemory Database and WebApplicationFactory libraries. The tests are located in their respective .Test project under the `Tests` directory.

![image](https://github.com/user-attachments/assets/5d15c6c3-0348-4563-82d8-5a30a68eef57)

**Note:** Some tests were commented out due to conflicts with current Azure deployments.

## Authentication
The API includes a custom authentication layer to ensure secure access to the API resources. The correct API key must be supplied in headers with 'x-api-key' tag for the request to be successfully processed by the server. The Blazor client is already set up with authentication and requires the following environment variables to be supplied in the `appsettings.json` file:

   ```
    {
    "Api": {
        "BaseUrl": "",
        "ApiKey": ""
        }
    }
   ```

![image](https://github.com/user-attachments/assets/590f453a-696b-448c-a259-334765b33022)

## Rate Limiting
The API uses [sliding window](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0#slide) rate limiter to restrict 10 request per 10 seconds. Any request exceeding that limit are processed as soon as limit is lifted.

![Image](rate-limiting-demo.gif)

## Hangfire Dashboard
The logs are processed through Hangfire's asynchronous database task queuing system. A real-time view of that can be observed through Hangfire dashboard. It is secured behind a username and password. I have made it configurable, so the credentials can always be updated without touching the code.

![image](https://github.com/user-attachments/assets/5d34e597-d5e9-458b-9448-f4a3374ac10c)

![image](https://github.com/user-attachments/assets/757b68cd-4d41-4577-ad96-53a11d1f8abe)

**Note:** SSL is marked optional for easier Docker deployments.

## Dockerisation
All projects support containerised deployments through Docker and Docker Compose. There are no prerequisites, libraries or dependencies required in order to run the containerised client and server apart from having Docker and Docker Compose installed on your system (and Git Bash if using Windows).

## Deployment
1. Clone the repository:

   ```bash
   git clone https://github.com/Bilal44/inflo-user-management.git
   ```

2. Either double-click the `deploy-services.sh` file or use the following commands on Linux from the root solution directory:

   ```bash
   chmod +x deploy-services.sh
   ./deploy-services.sh
   ```

| Project | Local IP Address over Docker |
|  --------  |  -------  |
|MS SQL Server|http://localhost:1433|
|ASP.NET website|http://localhost:5050|
|REST API|http://localhost:5000|
|Blazor Client|http://localhost:5100|

If a project happens to abruptly stop while the others are running, executing `./deploy-services.sh` will redeploy it or you can manually restart the container.

**Note:** The installation process will also install (and remove if not used) a containerised instance of SQL Server 2019, which may take several minutes.

### Docker Images
![Docker Images](https://github.com/user-attachments/assets/c5684bca-2b57-4af5-b5bc-08b867ea0561)

### Containers
![image](https://github.com/user-attachments/assets/d55ef91c-2bd7-4675-812d-9059fc441964)


### Clean Up
1. Either double-click the `remove-services.sh` file or use the following commands on Linux from the root solution directory:

   ```bash
   chmod +x remove-services.sh
   ./remove-services.sh
   ```

---

# User Management Technical Exercise

The exercise is an ASP.NET Core web application backed by Entity Framework Core, which faciliates management of some fictional users.
We recommend that you use [Visual Studio (Community Edition)](https://visualstudio.microsoft.com/downloads) or [Visual Studio Code](https://code.visualstudio.com/Download) to run and modify the application.

**The application uses an in-memory database, so changes will not be persisted between executions.**

## The Exercise
Complete as many of the tasks below as you feel comfortable with. These are split into 4 levels of difficulty
* **Standard** - Functionality that is common when working as a web developer
* **Advanced** - Slightly more technical tasks and problem solving
* **Expert** - Tasks with a higher level of problem solving and architecture needed
* **Platform** - Tasks with a focus on infrastructure and scaleability, rather than application development.

### 1. Filters Section (Standard)

The users page contains 3 buttons below the user listing - **Show All**, **Active Only** and **Non Active**. Show All has already been implemented. Implement the remaining buttons using the following logic:
* Active Only – This should show only users where their `IsActive` property is set to `true`
* Non Active – This should show only users where their `IsActive` property is set to `false`

### 2. User Model Properties (Standard)

Add a new property to the `User` class in the system called `DateOfBirth` which is to be used and displayed in relevant sections of the app.

### 3. Actions Section (Standard)

Create the code and UI flows for the following actions
* **Add** – A screen that allows you to create a new user and return to the list
* **View** - A screen that displays the information about a user
* **Edit** – A screen that allows you to edit a selected user from the list
* **Delete** – A screen that allows you to delete a selected user from the list

Each of these screens should contain appropriate data validation, which is communicated to the end user.

### 4. Data Logging (Advanced)

Extend the system to capture log information regarding primary actions performed on each user in the app.
* In the **View** screen there should be a list of all actions that have been performed against that user.
* There should be a new **Logs** page, containing a list of log entries across the application.
* In the Logs page, the user should be able to click into each entry to see more detail about it.
* In the Logs page, think about how you can provide a good user experience - even when there are many log entries.

### 5. Extend the Application (Expert)

Make a significant architectural change that improves the application.
Structurally, the user management application is very simple, and there are many ways it can be made more maintainable, scalable or testable.
Some ideas are:
* Re-implement the UI using a client side framework connecting to an API. Use of Blazor is preferred, but if you are more familiar with other frameworks, feel free to use them.
* Update the data access layer to support asynchronous operations.
* Implement authentication and login based on the users being stored.
* Implement bundling of static assets.
* Update the data access layer to use a real database, and implement database schema migrations.

### 6. Future-Proof the Application (Platform)

Add additional layers to the application that will ensure that it is scaleable with many users or developers. For example:
* Add CI pipelines to run tests and build the application.
* Add CD pipelines to deploy the application to cloud infrastructure.
* Add IaC to support easy deployment to new environments.
* Introduce a message bus and/or worker to handle long-running operations.

## Additional Notes

* Please feel free to change or refactor any code that has been supplied within the solution and think about clean maintainable code and architecture when extending the project.
* If any additional packages, tools or setup are required to run your completed version, please document these thoroughly.
