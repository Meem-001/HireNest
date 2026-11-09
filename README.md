# HireNest — Internship Management System
HireNest is a full-stack web application that connects students, companies, and administrators on a single platform. It manages the complete internship lifecycle: posting opportunities, skill-based matching, applications, CV management, freemium payments, and automated email communication, with an AI-powered assistant built on the Google Gemini API.

# Features
- Role-based authentication for Students, Companies, and Admins
- Internship posting, browsing, and detailed listings
- Skill-based match percentage between students and internships
- CV upload and built-in CV Builder
- Application workflow with accept / reject decisions
- Automated email notifications when an application is accepted or rejected
- Freemium model for companies: 5 free internship posts, then premium via a simulated payment gateway, controlled by the Admin
- AI chatbot (HireNest AI Assistant) integrated with the Google Gemini API

# Technologies Used
-  Backend: ASP.NET Core 8 MVC
- Database: MySQL (MySql.Data with Dapper)
- Email: Gmail SMTP with App Password
- AI: Google Gemini API (REST calls via HttpClient)
- Frontend: Bootstrap 5, JavaScript, jQuery
- Security: BCrypt password hashing, cookie-based authentication
- API Documentation: Swagger UI (Swashbuckle)


## Project Structure
Controllers:
  - AccountController.cs — registration, login, and role-based authentication
  - AdminController.cs — approvals, premium control, and platform oversight
  - ApplicationController.cs — student applications and status updates
  - ChatController.cs — endpoint for the AI chatbot
  - CVController.cs — CV upload and CV Builder
  - HomeController.cs — landing page and internship browsing
  - InternshipController.cs — internship posting and details
  - PaymentController.cs — premium purchases (simulated payment)
  - StudentController.cs — student skills and match percentage

    Models:
  - User.cs — users with roles (Student / Company / Admin)
  - Internship.cs — internship listings with required skills
  - Application.cs — links students to internships with status
  - CV.cs — student resume data
  - CompanyProfile.cs — company details
  - PaymentTransaction.cs — premium payment records

   Data:
  - DbHelper.cs — MySQL connection helper
  - UserRepository.cs, InternshipRepository.cs, ApplicationRepository.cs, CVRepository.cs, CompanyProfileRepository.cs, TransactionRepository.cs — Repository Pattern for all database access

 Services:
  - GeminiService.cs — Google Gemini API integration for the chatbot
  - EmailService.cs — SMTP emails for accept / reject notifications

- **Views**
  - Account/— login and registration forms
  - Admin/ — admin dashboard and controls
  - Application/, Internship/, Student/, CV/, Payment/ — module pages
  - Shared/_Layout.cshtml — global layout including the AI chat widget


## Security Notes
- appsettings.json is gitignored; all secrets stay on the local machine
- Passwords are hashed with BCrypt before storage
- The Gemini API key is only used server-side and never exposed to the browser

