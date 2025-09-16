# AiChatApi

A .NET 9 Web API for AI-powered chat functionality using Ollama and PostgreSQL.

## Architecture

- **AiChatApi.WebApi**: Main API project
- **AiChatApi.Domain**: Business logic and entities
- **AiChatApi.Infrastructure**: Data access and external services

## Prerequisites

- Docker and Docker Compose
- Ollama (for AI functionality)
- .NET 9 SDK (for local development)

## Quick Start

### 1. Start PostgreSQL and API with Docker Compose

```bash
# Production setup
docker-compose up -d

# Development setup with hot reload
docker-compose -f docker-compose.dev.yml up -d
```

### 2. Install and Run Ollama

```bash
# Install Ollama (if not already installed)
# Visit: https://ollama.ai/download

# Start Ollama server
ollama serve

# Pull a model (in another terminal)
ollama pull llama3.2
```

### 3. Run Database Migrations

```bash
# Navigate to WebApi project
cd AiChatApi.WebApi

# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 4. Access the API

- **API**: http://localhost:8080 (production) or http://localhost:5053 (development)
- **PostgreSQL**: localhost:5432
- **Ollama**: http://localhost:11434

## Configuration

### Environment Variables

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Ollama__BaseUrl`: Ollama server URL (default: http://localhost:11434)
- `Ollama__DefaultModel`: Default model to use (default: llama3.2)

### Docker Compose Services

- **postgres**: PostgreSQL 15 database
- **api**: ASP.NET Core Web API

## Development

### Local Development

```bash
# Start only PostgreSQL
docker-compose up postgres -d

# Run API locally
cd AiChatApi.WebApi
dotnet run
```

## API Endpoints

*Endpoints will be available after implementing controllers*

## Database Schema

- **ChatSessions**: Chat conversation sessions
- **ChatMessages**: Individual messages in conversations

## Technologies

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- OllamaSharp
- Docker & Docker Compose
