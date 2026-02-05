
# Epoka Electronics

**Epoka Electronics**

 Epoka Electronins an e-commerce web application. It is an online electronics store where users can browse products, add them to a cart, place orders, and where the admin can manage the catalog and orders through the admin dashboard.

## This project was built using:

- Frontend: HTML, CSS, JavaScript
- Backend: C# with ASP.NET Core Web API
- Database: SQLite with Entity Framework Core
- Authentication: ASP.NET Identity + JWT



##  Project Authors

This project was developed by:

- Xhevdet Cekaj
- Eduard Jukaj
- Glen Shraka
- Aldis Saliasi



## Purpose of the Project

The purpose of Epoka Electronics is:

- Full-stack web development
- Database design and management with Entity Framework Core
- Authentication and authorization with JWT
- Real e-commerce logic such as cart management, checkout, and order processing
- Admin management of products, categories, and orders



## System Architecture

Browser (HTML/CSS/JS) → ASP.NET Core Web API (C#) → SQLite Database (EF Core)



## Project Structure

EpokaElectronics
 ├── EpokaElectronics.sln
 └── EpokaElectronics.Api
      ├── Controllers
      ├── Data
      ├── Dtos
      ├── Entities
      ├── Models
      ├── Seed
      ├── Services
      ├── wwwroot (Where the front end is)
      ├── Program.cs
      └── appsettings.json



## Features

### Customer Features
- Browse products by category
- Search and filter products
- View product details
- Add/remove items from cart
- Checkout and create orders
- View order history
- Register and login with JWT authentication

### Admin Features
- Add/Delete categories
- Create/Update/Delete products
- View all orders
- Change order status



## Authentication

- Users register and login using ASP.NET Identity
- JWT tokens are used for secure API access
- Admin role is seeded automatically



## Database

- SQLite database file: epoka_electronics.db
- Automatically created when the app runs
- Seeded with demo categories and products


## How to Run the Project

1. Open terminal in EpokaElectronics.Api
2. Go into: EpokaElectronics/EpokaElectronics.Api
3. Run:
   dotnet restore
   dotnet run
4. Open in browser:
   https://localhost:7182

  

## Demo Admin Account

- Email: admin@epoka-electronics.com
- Password: Admin123!


