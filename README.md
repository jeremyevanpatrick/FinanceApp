# Budget Planner Application

A full-stack **Budget Planner Application** built with **Blazor WebAssembly** and **ASP.NET Core Web API**, designed to help users manage finances, track expenses, and maintain financial awareness.

---

## 🚀 Features

* 💰 **Budget Management**

  * Track income and expenses
  * Organize financial data efficiently
  * Monitor spending habits over time

* 🔐 **Authentication & Account Management**

  * Built with ASP.NET Core Identity
  * JWT-based authentication
  * Secure user registration and login flows

* 🌐 **RESTful API**

  * 18 endpoints supporting core application functionality
  * Clean separation between frontend and backend

* ⚙️ **Background Services**

  * Logging and diagnostics
  * Email processing (notifications, alerts)
  * Scheduled maintenance tasks

---

## 🛠️ Tech Stack

* **Frontend:** Blazor WebAssembly
* **Backend:** ASP.NET Core Web API
* **Authentication:** ASP.NET Core Identity + JWT
* **Database:** SQL Server
* **ORM:** Entity Framework Core
* **Testing:** xUnit
* **Background Jobs:** Hosted Services

---

## 🔐 Authentication Flow

1. User logs in with email and password via the Sessions endpoint
2. Server returns:

   * **JWT access token** (in response body)
   * **Refresh token** stored as a secure, HTTP-only cookie
3. Client uses the access token for authenticated API requests
4. When the access token expires:

   * Client calls the `/sessions/refresh` endpoint
   * Server validates and **rotates the refresh token**
   * A new access token and refresh token cookie are issued
5. On logout:

   * Refresh token is revoked server-side
   * Refresh token cookie is deleted

---

## ⚙️ Background Services

Background workers handle:

* 📄 Application logging and monitoring
* 📧 Email notifications and processing
* 🧹 Scheduled cleanup and maintenance tasks

---

## 📄 License

This project is licensed under the MIT License.
