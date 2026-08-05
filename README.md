# Finance App

A full-stack budget planning and personal finance management application built with **ASP.NET Core Web API** and two alternative front-end implementations using **Angular** and **Blazor WebAssembly**. Users can track income and expenses, organize financial data by category, and monitor spending habits over time through an interactive single-page application.

---

## Features

**Budget & Finance Management**
- Track income and expense transactions
- Organize financial data by category
- Monitor spending habits and trends over time
- Maintain a running view of financial health

**Authentication & Account Management**
- User registration and login via ASP.NET Core Identity
- JWT access tokens with HTTP-only refresh token cookie rotation
- Secure logout with server-side token revocation

**RESTful API**
- 18 endpoints covering the full scope of application functionality
- Clean separation between frontend and backend — the API can be consumed independently

**Background Services**
- Application logging and diagnostics
- Email notification processing
- Scheduled cleanup and maintenance tasks

---

## Tech Stack

| Technology | Purpose |
|---|---|
| Angular 22 | Client-side SPA frontend |
| Blazor WebAssembly | Client-side SPA frontend |
| ASP.NET Core Web API | Backend REST API |
| ASP.NET Core Identity | User management and authentication |
| JWT (JSON Web Tokens) | Stateless API authentication |
| Entity Framework Core | ORM and database migrations |
| SQL Server | Primary data store |
| xUnit | Unit testing |
| Azure Pipelines | CI/CD |

---

## Database Contexts

The API uses three separate EF Core DbContexts, each bundled as its own self-contained migration executable in the CI pipeline:

| Context | Purpose |
|---|---|
| `AppDbContext` | Core application data — transactions, budgets, categories |
| `AuthDbContext` | Identity and authentication data — users, refresh tokens |
| `LoggingDbContext` | Persisted application logs |

---

## Authentication Flow

1. User registers or logs in via `POST /sessions`
2. The server issues a short-lived **JWT access token** (returned in the response body) and a long-lived **refresh token** stored as a secure, HTTP-only cookie
3. The Angular/Blazor client attaches the access token to subsequent API requests
4. When the access token expires, the client calls `POST /sessions/refresh`
5. The server validates the refresh token cookie, **rotates** it (invalidating the old one), and issues a new access token and refresh token cookie
6. On logout, the refresh token is revoked server-side and the cookie is deleted

The HTTP-only cookie design prevents refresh token access from JavaScript, protecting against XSS-based token theft.

---

## License

This project is licensed under the MIT License.
