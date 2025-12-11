# Hospital Management System (HMS) Frontend

A modern, responsive web application for managing hospital operations, built with **ASP.NET Core Razor Pages**. This frontend interacts with a backend REST API to handle authentication, patient management, appointment scheduling, and staff administration.

## Features

*   **Role-Based Access Control (RBAC):**
    *   **Admin:** Manage user accounts (create, update, deactivate).
    *   **Receptionist:** Register patients, schedule appointments, view daily dashboard.
    *   **Doctor / Nurse:** View personal schedules, patient details, and manage appointments.
*   **Patient Management:** Search, view, and update patient records.
*   **Appointment Scheduling:** Interactive scheduling with conflict detection (handled by backend).
*   **Modern UI:** Responsive design using CSS variables, Flexbox, and Grid for a clean, professional look.

## Technologies Used

*   **Framework:** ASP.NET Core 8.0 (Razor Pages)
*   **Language:** C#
*   **Styling:** Custom CSS (Variables, Flexbox, Grid), Remix Icons
*   **HTTP Client:** Typed `HttpClient` with `IHttpClientFactory`
*   **Authentication:** Cookie-based auth with JWT token management

## Getting Started

### Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   Visual Studio 2022 or VS Code

### Installation

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd HMS-Frontend
    ```

2.  **Configure the Backend URL:**
    Open `appsettings.json` and ensure the `Backend:BaseUrl` points to your running API instance.
    ```json
    "Backend": {
      "BaseUrl": "https://hospital-management-backend-lrvz.onrender.com/"
    }
    ```

3.  **Run the Application:**
    ```bash
    dotnet run
    ```
    The application will start at `https://localhost:7194` (or similar).

### Login & Roles

The application supports the following roles. Use valid credentials from your backend database.

*   **Admin:** Access to `/Admin` dashboard.
*   **Receptionist:** Access to `/Dashboard`.
*   **Doctor:** Access to `/Dashboard`.
*   **Nurse:** Access to `/Dashboard`.

> **Tip for Testing:** You can enable a "Fake API" mode for testing UI flows without a backend.
> 1. Open `appsettings.json`.
> 2. Set `"Frontend": { "UseFakeApi": true }`.
> 3. Login with any username and password `123456`.

## Project Structure

*   **Pages/**: Razor Pages (UI).
    *   `Account/`: Login logic.
    *   `Admin/`: User management.
    *   `Appointments/`: Scheduling.
    *   `Patients/`: Patient records.
    *   `Dashboard`: Main landing page for staff.
*   **Services/**: Business logic and API integration.
    *   `ApiService.cs`: Facade for API operations.
    *   `UsersApiClient.cs`: User management API client.
*   **Models/**: Data Transfer Objects (DTOs).

## License

[MIT](LICENSE)
