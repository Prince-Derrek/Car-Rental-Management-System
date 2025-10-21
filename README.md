# 🚗 Car Rental Management System (CRMS): Real-Time GPS Fleet Tracking

This repository contains the complete source code for a modern, secure, full-stack **Car Rental Management System (CRMS)** designed for peer-to-peer (P2P) vehicle sharing.

The primary feature is the **Real-Time GPS Tracking** capability, which utilizes a simulated background service and SignalR to provide instant vehicle location updates on a Leaflet map.

---

## ✨ Key Features

This application is built on a clean, layered architecture, ensuring scalability and maintainability.

| Feature Area             | Description                                                                                                                                                             | Primary Technology           |
| :----------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------- |
| **Real-Time Tracking** | Live visualization of vehicle movement (simulated Nairobi $\leftrightarrow$ Thika route) using SignalR broadcasting enriched telemetry data to a Leaflet.js map frontend. | SignalR, Leaflet.js          |
| **Secure Authentication** | Role-based access control (Owner/Renter) secured by **JWT (JSON Web Tokens)**. Includes **email confirmation** workflow (logged to console in development).            | ASP.NET Identity Hashing, JWT |
| **Vehicle Management** | Owners can securely add, update (excluding plate), and manage their fleet inventory via API calls, handling potential conflicts like duplicate plates.                   | ASP.NET Core Web API, EF Core |
| **Booking & Workflow** | Renters request bookings. Owners approve/reject. **Price calculated server-side**. Includes **client-side validation** and handles booking conflicts.                     | Clean Service Layer, EF Core |
| **Data Persistence** | Optimized SQL Server schema with relationships enforced via EF Core Fluent API. Includes columns for vehicle pricing and booking totals.                                | EF Core, SQL Server          |

---

## 🏗️ Architecture Overview

The system is structured into a backend API and an MVC frontend, strictly separated for maintainability.

### 1. Backend: CRMS_API (ASP.NET Core Web API)

The API is the central data provider, built using API Controllers and a service layer pattern. It handles business logic, data persistence, and real-time communication.

| Component             | Responsibility                                                                                                                           | Key Dependencies                   |
| :-------------------- | :--------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------- |
| **AuthService** | User registration (with email confirmation token generation), login (checks confirmation status), password hashing, JWT generation.        | `IPasswordHasher<User>`, `IJwtGenerator`, `IEmailService` |
| **BookingService** | Handling rental requests, server-side price calculation, availability checks (preventing overlaps), and status updates by owners.        | EF Core, Business Validation       |
| **VehicleService** | CRUD operations for vehicles, ensuring plate uniqueness, owner authorization checks, and preventing deletion of booked vehicles.         | EF Core, Custom Exceptions         |
| **TelemetryService** | Persists GPS points, **enriches** telemetry data with vehicle details (Plate, Make/Model), and broadcasts updates via the SignalR Hub. | `IHubContext<TelemetryHub>`, EF Core |
| **GpsSimulatorService** | Background service (`IHostedService`) generating timed telemetry data for a specific route and pushing it to `TelemetryService`.        | `BackgroundService`, `IServiceScopeFactory` |
| **ConsoleEmailService** | Development-only implementation of `IEmailService` that logs confirmation links to the backend console.                               | `ILogger`                          |
| **Controllers (API)** | Handle incoming HTTP requests (expecting JSON via `[FromBody]` or form data via `[FromForm]`), validate DTOs, call services, return responses. | `[ApiController]`, `[Authorize]`   |

### 2. Frontend: CRMS_UI (ASP.NET Core MVC)

The UI layer communicates exclusively with the backend API via the centralized `ApiService`. It handles user interaction, presentation, and session management.

| Component             | Responsibility                                                                                                                                  | Key Dependencies                             |
| :-------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------- |
| **ApiService** | Centralized `HttpClient` handler. Sends requests (JSON via `PostAsync`, form data via `PostFormAsync`), adds JWT Bearer token, handles API responses/errors. | `HttpClient`, Session, `IConfiguration`      |
| **Controllers (MVC)** | Handle routing, view model binding (including manual binding workarounds if needed), session management, role-based redirects, call `ApiService`. | Session, `IApiService`, Model Binding        |
| **Views (.cshtml)** | Present the UI using **Tailwind CSS**. Implement **client-side validation**. Include **Leaflet.js** and **SignalR** client scripts for the map.       | Tailwind CSS, Leaflet.js, SignalR, jQuery    |
| **ViewModels** | Define data structures for views (`CarCreateUpdateViewModel`, etc.) and mirror API DTOs (`CreateVehicleDto`, etc.) for API calls.                   | DataAnnotations                              |

---

## 🛠️ Technical Stack

| Category            | Technology                        | Version | Notes                                                              |
| :------------------ | :-------------------------------- | :------ | :----------------------------------------------------------------- |
| **Backend Framework** | ASP.NET Core Web API              | .NET 8  | Secure, cross-platform RESTful API.                                |
| **Database** | SQL Server Developer Ed.          | N/A     | Persistent, indexed data storage.                                  |
| **ORM** | Entity Framework Core             | .NET 8  | Fluent API for schema configuration, Migrations.                   |
| **Real-Time** | ASP.NET Core SignalR              | .NET 8  | WebSocket-based data transmission.                                 |
| **Frontend Framework**| ASP.NET Core MVC                  | .NET 8  | Server-side rendering for the web application.                     |
| **Styling** | Tailwind CSS                      | Latest  | Utility-first, responsive design (requires Node/npm for build).    |
| **Mapping** | Leaflet.js                        | CDN     | Lightweight, open-source mapping library.                          |
| **Client Libs** | jQuery, jQuery Validate Unobtrusive | Latest  | Added via LibMan for client-side form validation.                  |
| **Auth** | JWT Bearer Authentication         | N/A     | Stateless, secure API authentication with email confirmation flow. |

---

## 🚀 Getting Started

### Prerequisites

You must have the following installed locally:
1.  **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
2.  **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** (Developer Edition preferred)
3.  **[Node.js and npm](https://nodejs.org/en/download)** (Required if using the Tailwind CSS CLI for CSS building)

### 1. Backend Setup

This configures the database, security, and real-time simulator services.

1.  **Configure Connection String & Base URL:** Update `CRMS_API/appsettings.Development.json` with your SQL Server details and the correct base URL (including port) for the API itself.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=CarRentalDB;User Id=YourUser;Password=YourPassword;TrustServerCertificate=True;"
    },
    "ApiSettings": {
      "BaseUrl": "https://localhost:7124/api" // Your API's URL
    ```
2.  **Apply Migrations:** Navigate to the `CRMS_API` project directory in your terminal and run:
    ```bash
    dotnet ef database update
    ```

### 2. Frontend Setup

This configures the connection to the backend and client-side libraries.

1.  **Configure API URL:** Update `CRMS_UI/appsettings.Development.json` to point to your running backend API.
    ```json
    "ApiSettings": {
      "BaseUrl": "https://localhost:7124/api", // Your API's URL
      "SignalRHub:" "https://localhost:7124/api/your_telemetry_Hub_endpoint_"
    }
    ```
2.  **(If using Tailwind CLI):** Install dependencies: Navigate to the `CRMS_UI` project directory:
    ```bash
    npm install
    ```
3.  **Install Client Libraries (if missing):** Ensure jQuery and validation scripts are present in `wwwroot/lib` (use LibMan via VS: Right-click project > Add > Client-Side Library...).

### 3. Run the Application

You will need **two running instances** (or terminals): one for the API (backend) and one for the frontend.

1.  **Run Backend API:** Start the `CRMS_API` project (e.g., via `dotnet run` or Visual Studio).
2.  **Run Frontend MVC:** Start the `CRMS_UI` project (e.g., via `dotnet run` or Visual Studio).
3.  **(If using Tailwind CLI):** Ensure the Tailwind watch script is running in a separate terminal:
    ```bash
    npm run watch
    ```

The application should now be accessible at its configured HTTPS URL (e.g., `https://localhost:7269`).

---

## 💡 Usage & Demonstration

### Test Accounts

Register new users via the UI. Use the **browser console output** to find the email confirmation link during development.

| Role      | Email Example       | Password   | Access Level                                               |
| :-------- | :------------------ | :--------- | :--------------------------------------------------------- |
| **Owner** | tester@owner.com    | owner@123  | Full access to Car CRUD, Rental Approval, Tracking.        |
| **Renter**| renter@tester.com | renter@123 | Can view available cars, create bookings, view history. |

### Real-Time GPS Tracking Demo

1.  Log in as the **Owner**.
2.  Navigate to the **Tracking** page.
3.  The Leaflet map will initialize, centered near Nairobi.
4.  Wait for the `GpsSimulatorService` (running in the backend) to start sending data.
5.  Vehicle markers (for vehicles configured in the simulator, e.g., ID 1) will appear and move along the simulated route: **Nairobi CBD $\leftrightarrow$ Thika $\leftrightarrow$ Nairobi CBD**.
6.  The **Live Telemetry Console** below the map shows enriched data arriving via SignalR.

---

## 🐳 Deployment (Containerization Goal)

The final step for production readiness is containerization. The architecture is designed for ease of deployment, utilizing environment variables for connection strings.

Future steps will include providing a `Dockerfile` and a `docker-compose.yml` file to containerize the API and Frontend services, enabling scalable, cloud-native deployment.

---