# Pinterest Clone

A Pinterest-inspired web application built with **ASP.NET Core MVC**, **C#**, **Entity Framework Core**, **SQL Server**, **HTML**, **CSS**, **JavaScript**, and **Sass**. The project allows users to register, confirm email addresses, create and manage pins, organize content into boards, interact with other users, and explore shared visual content.

## Overview

This project recreates core Pinterest-style functionality in an MVC architecture. It includes authentication-related flows, pin sharing, board management, profile features, likes, comments, follow-based content discovery, and email-based actions such as account confirmation and password reset.

## Main Features

- User registration and login
- Email confirmation after registration
- Forgot password and password reset via email
- User profile page with bio and profile photo upload
- Create, view, and manage pins
- Upload images for pins
- Save pins to boards
- Create, edit, and delete boards
- Private board support
- Like and unlike pins
- Add comments to pins
- Report pins
- Search pins and users
- Personalized feed based on followed users
- Explore page with popular, new, and recommended pins

## Technologies Used

**Backend**
- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server

**Frontend**
- HTML
- CSS
- JavaScript
- Sass

**Other**
- Session-based authentication flow
- SMTP email service for confirmation and password reset

## Project Structure

```bash
Pinterest_Clone/
│
├── Controllers/
├── Data/
├── DTOs/
├── Migrations/
├── Models/
├── Services/
├── ViewComponents/
├── ViewModels/
├── Views/
├── wwwroot/
├── Program.cs
├── PinterestClone.csproj
└── package.json
```

The project uses a structured MVC-based layout with separate folders for controllers, models, data access, services, view models, and frontend assets.

## Database Design

The application uses Entity Framework Core with SQL Server and includes entities such as:

- User
- Pin
- Board
- PinBoard
- PinLike
- PinComment
- Follow
- PinReport

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/ulviasadov/Pinterest_Clone.git
cd Pinterest_Clone
```

### 2. Configure the database connection

Update the DefaultConnection value in appsettings.json to match your SQL Server setup. The project is configured to use SQL Server through Entity Framework Core.

```md
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PinterestCloneDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 3. Configure SMTP settings

Set your SMTP credentials in appsettings.json so email confirmation and password reset features can work.

```md
```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@example.com",
  "Password": "your-app-password",
  "FromEmail": "your-email@example.com"
}
```

### 4. Apply migrations

```bash
dotnet ef database update
```

### 5. Restore dependencies

```bash
dotnet restore
npm install
```

### 6. Compile Sass

```bash
npm run sass
```

### 7. Run the application

```bash
dotnet run
```

## Important Note

Before using this project publicly, sensitive configuration values such as connection strings and SMTP credentials should be removed from appsettings.json and stored securely using environment variables or user secrets. The current repository view shows both a SQL Server connection string and SMTP credentials in appsettings.json, so rotating those secrets would be a good idea.

## Author

Developed by **Ulvi Asadov**
GitHub: [ulviasadov](https://github.com/ulviasadov)
