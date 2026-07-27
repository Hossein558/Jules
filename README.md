# Jules Panel

A modern, responsive, and beautifully designed web panel for interacting with the **Jules API** (Google's agentic coding platform). Built with **.NET 8 Blazor Server** and a fully customized CSS UI (Glassmorphism & Dark Premium Theme).

## 🌟 Features

- **Session Management:**
  - View all Jules sessions grouped by state (`In Progress`, `Completed`, `Archived`, `Other`).
  - Create new sessions with a starting prompt.
  - Automatically load branches for private/public GitHub repositories linked to Jules.
  - Require plan approval before agent execution.

- **Activity Tracking:**
  - Real-time display of Jules's activities.
  - Clearly separated messages between the `User` and the `Agent`.
  - Reference indicators when a new Plan is proposed by the agent.

- **Interactive Workflow:**
  - Integrated command prompt to send subsequent instructions to the agent.
  - `Ctrl + Enter` shortcut for quick messaging.
  - One-click plan approval when the agent needs authorization to proceed.

- **Premium UI/UX:**
  - A highly responsive, full-width application layout.
  - Pure CSS tabs and customized layout (No heavy JS interop reliance for structural elements).
  - RTL (Right-to-Left) native support out of the box.

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A valid **Jules API Key**

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Hossein558/Jules.git
   cd Jules
   ```

2. **Configure your API Key:**
   Open `appsettings.json` (or set it in your secrets/environment variables) and insert your key:
   ```json
   {
     "Jules": {
       "ApiKey": "YOUR_JULES_API_KEY",
       "BaseUrl": "https://jules.googleapis.com/v1alpha/"
     }
   }
   ```
   *(Note: Never commit your actual API key to version control!)*

3. **Run the application:**
   ```bash
   dotnet run
   ```
   The application will start, usually accessible at `http://localhost:5000` or `https://localhost:5001`.

## 📁 Project Structure

- `Components/Pages/Home.razor`: The core UI, layout structure, and state management.
- `wwwroot/app.css`: The customized CSS stylesheet implementing the dark premium theme.
- `Models/JulesModels.cs`: C# Data Transfer Objects (DTOs) mapping to the Jules API responses.
- `Services/JulesApiService.cs`: The HTTP client wrapper handling authentication, serialization, and all endpoints.

## 🛠 Tech Stack

- **Framework:** .NET 8 Blazor (Interactive Server Render Mode)
- **Styling:** Custom CSS (Flexbox, Variables, CSS Animations)
- **Networking:** `HttpClient` with standard `System.Text.Json` serialization

## 🤝 Contribution

Feel free to open issues or submit pull requests for any enhancements or bug fixes.
