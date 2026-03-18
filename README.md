# Pinterest Clone

A Pinterest-inspired web application built with **ASP.NET Core MVC**, **C#**, **Entity Framework Core**, **SQL Server**, **HTML**, **CSS**, **JavaScript**, and **Sass**. The project allows users to register, confirm email addresses, create and manage pins, organize content into boards, interact with other users, and explore shared visual content. :contentReference[oaicite:0]{index=0}

## Overview

This project recreates core Pinterest-style functionality in an MVC architecture. It includes authentication-related flows, pin sharing, board management, profile features, likes, comments, follow-based content discovery, and email-based actions such as account confirmation and password reset. :contentReference[oaicite:1]{index=1}

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
- Explore page with popular, new, and recommended pins :contentReference[oaicite:2]{index=2}

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
- SMTP email service for confirmation and password reset :contentReference[oaicite:3]{index=3}

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

## How to Run the Project
1. Clone the repository
   git clone https://github.com/ulviasadov/Pinterest_Clone.git
   cd Pinterest_Clone
