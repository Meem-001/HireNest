# HireNest

HireNest is a full-stack web application that connects students, companies, and administrators on a single platform. It manages the complete internship lifecycle: posting opportunities, skill-based matching, applications, CV management, freemium payments, and automated email communication, with an AI-powered assistant built on the Google Gemini API.

## ? Features

- **Role-based authentication** for Students, Companies, and Admins
- **Internship management**: Posting, browsing, and detailed listings
- **Skill-based matching**: Percentage match calculation between student skills and internship requirements
- **CV Management**: Built-in CV Builder and file upload support
- **Application workflow**: Streamlined apply, accept, and reject decisions
- **Automated email notifications**: Instant SMTP emails when applications are accepted or rejected
- **Freemium business model**: Companies get 5 free internship posts, with premium upgrades via a simulated payment gateway controlled by the Admin
- **AI Chatbot**: "HireNest AI Assistant" integrated with the Google Gemini API for real-time user support
- **RESTful API Endpoints**: Dedicated API controllers for external data consumption, fully documented with Swagger UI

## ??? Technologies Used

- **Backend**: ASP.NET Core 8 MVC & Web API
- **Database**: MySQL (Entity Framework Core 8 with `Pomelo.EntityFrameworkCore.MySql`)
- **Email**: Gmail SMTP with App Password
- **AI**: Google Gemini API (REST calls via `HttpClient`)
- **Frontend**: Bootstrap 5, JavaScript, jQuery
- **Security**: BCrypt password hashing, cookie-based authentication, EF Core parameterized queries (SQL injection protection)
- **API Documentation**: Swagger UI (Swashbuckle)

## ?? Project Structure

### Controllers (MVC - Web UI)
- `AccountController.cs` — Registration, login, and role-based authentication
- `AdminController.cs` — Approvals, premium control, and platform oversight
- `ApplicationController.cs` — Student applications and status updates
- `ChatController.cs` — Endpoint for the AI chatbot
- `CVController.cs` — CV upload and CV Builder
- `HomeController.cs` — Landing page and internship browsing
- `InternshipController.cs` — Internship posting and details
- `PaymentController.cs` — Premium purchases (simulated payment)
- `StudentController.cs` — Student skills and match percentage

### API Controllers (RESTful Endpoints)
- `Api/InternshipsApiController.cs` — REST endpoints for fetching internship data
- `Api/ApplicationsApiController.cs` — REST endpoints for fetching application data

### Models
- `User.cs` — Users with roles (Student / Company / Admin)
- `Internship.cs` — Internship listings with required skills
- `Application.cs` — Links students to internships with status
- `CV.cs` — Student resume data
- `CompanyProfile.cs` — Company details and post limits
- `PaymentTransaction.cs` — Premium payment records

### Data (Entity Framework Core)
- `AppDbContext.cs` — Central EF Core DbContext managing all database sets and relationships
- `UserRepository.cs`, `InternshipRepository.cs`, `ApplicationRepository.cs`, `CVRepository.cs`, `CompanyProfileRepository.cs`, `TransactionRepository.cs` — Repository Pattern implementation using modern EF Core LINQ queries for clean, maintainable data access

### Services
- `GeminiService.cs` — Google Gemini API integration for the chatbot
- `EmailService.cs` — SMTP email handling for accept/reject notifications

### Views
- `Account/` — Login and registration forms
- `Admin/` — Admin dashboard and controls
- `Application/`, `Internship/`, `Student/`, `CV/`, `Payment/` — Module-specific pages
- `Shared/_Layout.cshtml` — Global layout including the AI chat widget

## ?? Security Notes

- `appsettings.json` is strictly `.gitignore`d; all secrets (connection strings, API keys, SMTP passwords) stay on the local machine.
- Passwords are securely hashed with **BCrypt** before database storage.
- The Gemini API key is only used server-side and is never exposed to the browser.
- **SQL Injection Protection**: All database queries are executed via Entity Framework Core's parameterized LINQ queries, completely eliminating the risk of SQL injection attacks.