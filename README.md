# HealMe

HealMe is a comprehensive telemedicine platform designed to connect patients with doctors seamlessly. It facilitates appointment booking, real-time communication, and AI-assisted health queries.

## 👥 Team

*   **Back-end Dev**: Buziak Artem
*   **Front-end**: Bestanchuk Bohdan
*   **Mobile**: Tomashevskiy Pavlo
*   **AI Service**: Stratiy Sviatoslav

## 🚀 Project Overview & Flow

The application is built using a **Modular Monolith** architecture with **.NET 8**.

1.  **Identity & Profiles**: Users register as either a **Patient** or a **Doctor**.
    *   Doctors manage their professional profiles (Specialization, Bio, Fees).
    *   Patients manage their personal health profiles.
2.  **Availability & Booking**:
    *   Doctors set their availability slots.
    *   Patients browse doctors and book appointments.
3.  **Appointments**:
    *   Full lifecycle management: Booking -> Confirmation -> Completion/Cancellation.
    *   Includes a 4-hour cancellation policy.
4.  **Real-Time Chat**:
    *   Integrated **SignalR** chat for each appointment.
    *   Secure, private communication between Patient and Doctor.
5.  **AI Assistant**:
    *   AI-powered chat interface for general health questions and assistance.

## 🛠 Tech Stack

*   **Core**: .NET 8 API
*   **Database**: PostgreSQL (Entity Framework Core)
*   **Real-time**: SignalR
*   **Architecture**: Modular Monolith (Identity, Doctors, Patients, Appointments, Chat, AI)
*   **Cloud**: Azure App Service & Azure Database for PostgreSQL

---

## 💻 Local Deployment

### Prerequisites
*   .NET 8 SDK
*   PostgreSQL (running locally or via Docker)
*   IDE (Rider, Visual Studio, or VS Code)

### Steps
1.  **Clone the repository**.
2.  **Configure Database**:
    *   Update `appsettings.json` with your local PostgreSQL connection string:
        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Host=localhost;Port=5432;Database=HealMe;Username=postgres;Password=your_password"
        }
        ```
3.  **Apply Migrations**:
    *   The application is configured to apply migrations automatically on startup.
    *   Alternatively, run: `dotnet ef database update` for each module context.
4.  **Run the Application**:
    ```bash
    dotnet run --project HealMe.API/HealMe.API.csproj
    ```
5.  **Access API**:
    *   Swagger UI: `http://localhost:5283/swagger` (or configured port)

---

## ☁️ Cloud Deployment (Azure)

### Prerequisites
*   Azure Account (Free Tier works)
*   Azure CLI (optional)

### 1. Database Setup (Azure Database for PostgreSQL)
1.  Create a **Flexible Server** in Azure.
2.  Select **Burstable B1ms** tier (Free tier eligible).
3.  Allow public access from Azure services in Networking settings.
4.  Copy your connection string.

### 2. App Service Setup
1.  Create an **Azure App Service** (Linux, .NET 8).
2.  Select **Free F1** plan.

### 3. Configuration
In your App Service **Settings -> Environment variables**, add:

| Name | Value |
| :--- | :--- |
| `ConnectionStrings__DefaultConnection` | `Host=<host>;Port=5432;Database=HealMe;Username=<user>;Password=<pass>;` |
| `JwtConfig__Secret` | Your secure random secret key |
| `AiService` | URL of the external AI Service |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### 4. Deploy Code
*   **Option A: GitHub Actions (Recommended)**
    *   Go to **Deployment Center** in App Service.
    *   Connect your GitHub repo. Azure will set up the CI/CD pipeline automatically.
*   **Option B: Manual Publish (Rider/VS)**
    *   Right-click Project -> **Publish** -> **Azure**.
    *   Select your App Service and deploy.

### 5. Verify
*   Visit `https://<your-app-name>.azurewebsites.net/swagger` to verify the API is running.
