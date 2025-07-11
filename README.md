# 😄 Anekdotify - Joke Sharing Platform

#### A comprehensive joke sharing and rating platform built with .NET 8, Blazor Server, and modern web technologies.  
#### This project represents a significant step in my development journey — building a full-stack web application with clean architecture, real-time interactions, user authentication, and containerized deployment.

---

## 🎯 Project Overview

Anekdotify is a social platform where users can share, discover, and rate jokes. The application features a complete user management system, joke approval workflow, interactive comments, and a sophisticated rating system. Built with modern .NET technologies and containerized for easy deployment.

---

## ✨ Key Features

+ 🔐 **User Authentication & Authorization** - Complete user registration, login, and profile management
+ 📝 **Joke Management** - Submit, approve, and categorize jokes with administrative oversight
+ ⭐ **Rating System** - Like and rate jokes with comprehensive feedback mechanisms
+ 💬 **Interactive Comments** - Real-time commenting system with nested replies and ratings
+ 📚 **Personal Collections** - Save favorite jokes and track viewing history
+ 🏷️ **Classification System** - Organize jokes by categories and sources
+ 📊 **Analytics** - Track user engagement and joke performance
+ 🚀 **Real-time Updates** - Blazor Server enables live UI updates without page refreshes
+ 🐳 **Containerized Deployment** - Docker-based setup with PostgreSQL, Redis, and pgAdmin

---

## 🛠️ Technology Stack & Architecture

### Backend Technologies
Technology | Purpose
---|---
**.NET 8.0** | Main application framework
**ASP.NET Core Web API** | RESTful API endpoints
**Entity Framework Core** | ORM for database operations
**PostgreSQL** | Primary database
**Redis** | Caching and session management
**ASP.NET Core Identity** | Authentication and authorization
**JWT Tokens** | Secure API authentication
**Serilog** | Structured logging

### Frontend Technologies
Technology | Purpose
---|---
**Blazor Server** | Interactive web UI with server-side rendering
**Blazor Bootstrap** | UI component library
**SignalR** | Real-time communication (implicit with Blazor Server)
**Blazored.LocalStorage** | Client-side data persistence
**Blazored.Toast** | User notifications

### DevOps & Infrastructure
Technology | Purpose
---|---
**Docker & Docker Compose** | Containerization and orchestration
**pgAdmin** | Database administration tool
**Health Checks** | Service monitoring and reliability
**Multi-stage Dockerfiles** | Optimized container builds
**.NET Aspire** | Application orchestration and service discovery

---

## 📦 Project Architecture

```plaintext
/Anekdotify.Api
    ├── Controllers/          // API endpoints (JokeController, UserController, etc.)
    ├── Authentication/       // JWT and identity configuration
    └── Program.cs           // API startup and configuration

/Anekdotify.Frontend
    ├── Components/          // Blazor components
    │   ├── Pages/          // Main application pages
    │   ├── Layout/         // UI layout components
    │   └── Shared/         // Reusable UI components
    ├── Services/           // Frontend service layer
    ├── Authentication/     // Client-side auth handling
    └── Program.cs         // Frontend startup configuration

/Anekdotify.Models
    ├── Entities/          // Domain entities (User, Joke, Comment, etc.)
    ├── DTOs/             // Data transfer objects
    └── Models/           // Application models

/Anekdotify.BL
    ├── Services/         // Business logic services
    ├── Interfaces/       // Service contracts
    ├── Mappers/         // Entity-DTO mapping
    └── Helpers/         // Utility classes

/Anekdotify.Database
    ├── Context/         // EF Core database context
    ├── Configurations/ // Entity configurations
    └── Migrations/     // Database migrations

/Anekdotify.Common
    └── Shared/         // Common utilities and extensions

/Anekdotify.ServiceDefaults
    └── Extensions/     // Service configuration extensions

/Anekdotify.AppHost
    └── Program.cs     // Application orchestration (Aspire)

/Anekdotify.Api.Tests
    └── Controllers/   // Unit tests for API controllers
```

---

## 🗃️ Database Schema

### Core Entities
- **Users** - User profiles with ASP.NET Identity integration
- **Jokes** - Main content with approval workflow and metadata
- **Comments** - Hierarchical commenting system
- **Classifications** - Joke categories and tags
- **Sources** - Origin tracking for jokes
- **Ratings & Likes** - User engagement metrics
- **UserSavedJokes** - Personal collections
- **UserViewedJokes** - Engagement analytics

### Key Relationships
- Users can submit, rate, and save jokes
- Jokes have classifications and sources
- Comments support nested replies with ratings
- Comprehensive audit trail for all user actions

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/RustamKholov/Anekdotify.git
cd Anekdotify
```

2. **Restore dependencies**
```bash
dotnet restore
```

3. **Set up environment variables**
```bash
# Create .env file for Docker Compose
touch .env
```

Add the following environment variables to your `.env` file:
```env
# Database Configuration
POSTGRES_PASSWORD=your_strong_password_here

# Redis Configuration
REDIS_PASSWORD=your_redis_password_here

# JWT Configuration
JWT_ISSUER=https://anekdotify.com
JWT_AUDIENCE=https://anekdotify.com
JWT_SIGNING_KEY=your_jwt_signing_key_here_minimum_32_characters
JWT_REFRESH_SIGNING_KEY=your_jwt_refresh_signing_key_here_minimum_32_characters

# pgAdmin Configuration
PGADMIN_PASSWORD=your_pgadmin_password_here
```

4. **Run with Docker Compose**
```bash
docker-compose -f docker-compose.prod.yml up -d
```

5. **Access the application**
- **Frontend**: http://localhost:5001
- **API**: http://localhost:5000
- **pgAdmin**: http://localhost:5050

### Development Environment

For local development without Docker:

1. **Prerequisites**
   - PostgreSQL server running on localhost:5432
   - Redis server running on localhost:6379

2. **Database Setup**
```bash
# Create database
createdb anekdotify

# Run migrations
dotnet ef database update --project Anekdotify.Database --startup-project Anekdotify.Api
```

> **Note**: The database will be automatically seeded with initial data including sample jokes and classifications.

3. **Run the applications**
```bash
# Start the API (terminal 1)
dotnet run --project Anekdotify.Api

# Start the Frontend (terminal 2)
dotnet run --project Anekdotify.Frontend

# Or use .NET Aspire for orchestration
dotnet run --project Anekdotify.AppHost
```

### Using .NET Aspire

The project includes .NET Aspire for local development orchestration:

```bash
# Install .NET Aspire workload
dotnet workload install aspire

# Run with Aspire (recommended for development)
dotnet run --project Anekdotify.AppHost
```

This will automatically start all services and provide a dashboard for monitoring.

---

## 🔧 Configuration

### Database Connection
Configure PostgreSQL connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=anekdotify;Username=your_user;Password=your_password"
  }
}
```

### JWT Configuration
Set up JWT authentication:
```json
{
  "JWT": {
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "SigningKey": "your-signing-key"
  }
}
```

---

## 🏗️ Development Journey

This project represents a significant milestone in my development journey, building upon the foundation laid by my previous [Minesweeper](https://github.com/RustamKholov/Minesweeper) project. Key learning achievements include:

### Architecture & Design Patterns
- **Clean Architecture** - Separated concerns across multiple projects
- **Dependency Injection** - Comprehensive DI container configuration
- **Repository Pattern** - Abstracted data access with Entity Framework
- **Service Layer Pattern** - Encapsulated business logic
- **DTO Pattern** - Clean data transfer between layers

### Advanced .NET Concepts
- **Blazor Server** - Interactive web UI with server-side rendering
- **Entity Framework Core** - Advanced ORM usage with migrations
- **ASP.NET Core Identity** - Full authentication and authorization
- **JWT Authentication** - Secure API access
- **Health Checks** - Application monitoring and reliability

### Database Design
- **Relational Modeling** - Complex entity relationships
- **Database Migrations** - Schema versioning and updates
- **Indexing Strategy** - Performance optimization
- **Data Seeding** - Initial data population

### DevOps & Deployment
- **Docker Containerization** - Multi-service container orchestration
- **Docker Compose** - Service orchestration and networking
- **Health Monitoring** - Service availability checks
- **Environment Configuration** - Flexible deployment settings

---

## 🎨 Application Features

### User Authentication
- Registration and login with email verification
- JWT-based API authentication
- Role-based authorization
- Password reset functionality

### Joke Management
- Submit new jokes for approval
- Administrative approval workflow
- Joke classification and tagging
- Source attribution

### Social Features
- Like and rate jokes
- Comment on jokes with nested replies
- Save jokes to personal collections
- View joke history and analytics

### Administrative Tools
- User management
- Joke moderation
- Content classification
- Usage analytics

---

## 📈 Performance & Scalability

### Caching Strategy
- Redis for session storage
- Application-level caching for frequently accessed data
- Database query optimization

### Database Optimization
- Proper indexing on frequently queried columns
- Efficient pagination for large datasets
- Optimized Entity Framework queries

### Container Optimization
- Multi-stage Docker builds
- Minimal base images
- Health checks for reliability

---

## 🔮 Future Enhancements

- [ ] **Mobile App** - React Native or MAUI companion app
- [ ] **Advanced Analytics** - Comprehensive usage dashboards
- [ ] **Social Features** - User following and personalized feeds
- [ ] **Content Moderation** - AI-powered content filtering
- [ ] **API Documentation** - Swagger/OpenAPI integration
- [ ] **Internationalization** - Multi-language support
- [ ] **Search & Discovery** - Enhanced joke discovery features
- [ ] **Microservices** - Service decomposition for scalability

---

## 🧪 Testing

The project includes comprehensive testing with unit tests and integration tests:

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Projects
```bash
# Run only API tests
dotnet test Anekdotify.Api.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
```plaintext
/Anekdotify.Api.Tests
├── Controllers/         // Unit tests for API controllers
│   ├── JokeControllerTests.cs
│   └── AccountControllerTests.cs
├── Integration/         // Integration tests
│   ├── JokeIntegrationTests.cs
│   └── AuthIntegrationTests.cs
├── ControllerTestBase.cs    // Base class for controller tests
└── IntegrationTestBase.cs   // Base class for integration tests
```

### Test Coverage
- **Controller Tests** - API endpoint testing with mocked dependencies
- **Integration Tests** - End-to-end testing with test database
- **Authentication Tests** - JWT token validation and user authentication
- **Business Logic Tests** - Service layer validation
- **Database Tests** - Entity Framework operations and migrations

---

## 🔧 Troubleshooting

### Common Issues

**Docker containers not starting**
```bash
# Check logs
docker-compose -f docker-compose.prod.yml logs

# Restart services
docker-compose -f docker-compose.prod.yml restart
```

**Database connection issues**
```bash
# Check PostgreSQL service
docker-compose -f docker-compose.prod.yml exec postgres pg_isready -U anekdotify_user

# Reset database
docker-compose -f docker-compose.prod.yml down -v
docker-compose -f docker-compose.prod.yml up -d
```

**Build errors**
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Performance Tips

- Use Redis for caching in production
- Enable response compression
- Configure connection pooling
- Monitor with Application Insights (optional)

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙌 Acknowledgments

This project builds upon the architectural foundations and learning experiences from my previous projects, particularly [Minesweeper](https://github.com/RustamKholov/Minesweeper). It represents continued growth in full-stack development, clean architecture, and modern web technologies.

---

## 📞 Contact

**Rustam Kholov**  
- GitHub: [@RustamKholov](https://github.com/RustamKholov)  
- LinkedIn: [Rustam Kholov](https://www.linkedin.com/in/rustam-kholov/)

---

*Built with ❤️ using .NET 8, Blazor Server, and modern web technologies*