# BookingPlatform - Travel and Accommodation Booking Backend

A cleanly architected backend solution for a modern **Travel and Accommodation Booking Platform**, supporting hotel search, booking, payments, user authentication, and admin management. This backend is designed using **Clean Architecture principles** to ensure modularity, scalability, and maintainability.

## Project Architecture

The solution follows Clean Architecture with the following layers:

BookingPlatform.API --> API Layer (Controllers, Middleware)

BookingPlatform.Application --> Business Logic (DTOs, Interfaces, Services)

BookingPlatform.Domain --> Core Entities, Enums, Interfaces, Exceptions

BookingPlatform.Infrastructure --> Implementation (EF Core, JWT, Services)


---

## Tech Stack

- **ASP.NET Core Web API**
- **Entity Framework Core**
- **Clean Architecture**
- **JWT Authentication**
- **Stripe API (for Payments)**
- **Swagger/OpenAPI**
- **SQL Server / SQLite (for local testing)**

---

## Features

### Authentication
- JWT-based secure login and registration
- Role-based access control (User/Admin)

### Hotel Discovery
- Robust hotel search with filters (price, star rating, room type)
- Trending destinations
- Featured deals
- Recently visited hotels

### Hotel Details
- Full hotel info with images and ratings
- Room types and availability
- Add to cart functionality

### Booking & Checkout
- Cart system to manage multiple bookings
- Checkout with user info and payment
- Stripe payment integration
- Confirmation page with booking summary
- Email confirmation with invoice

### Admin Management
- Admin dashboards for managing:
  - Cities
  - Hotels
  - Rooms
- Create, update, and delete entities
- Grids with filters, pagination, and update forms

---

## Testing
- Unit tests are written for application services.
- Integration testing is planned for key API endpoints.
- Uses in-memory database for testing scenarios.

## API Documentation
All endpoints are documented using Swagger, available at /swagger once the app is running.

## Project Management
- Tasks managed using Jira with clear user stories and sprint tracking.
- Follows Agile best practices and includes versioning and CI/CD guidelines.

## Deployment (Planned)
- Docker support for local and cloud deployment.
- CI/CD pipeline setup using GitHub Actions.
- Targeting deployment on Azure.

---

