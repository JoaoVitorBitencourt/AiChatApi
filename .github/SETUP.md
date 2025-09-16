# GitHub Actions Setup Guide

This repository includes two GitHub Actions workflows for building and deploying the AI Chat API.

## Workflows

### 1. Build Workflow (`build.yml`)
- **Triggers**: Push and PR to main/master/develop branches
- **Purpose**: Builds the .NET application, runs tests, and pushes Docker image to Docker Hub
- **Output**: Docker image tagged with branch name, commit SHA, and latest

### 2. Deploy Workflow (`deploy.yml`)
- **Triggers**: Runs after successful completion of build workflow on main/master branches
- **Purpose**: Deploys the Docker image to your production server
- **Process**: Pulls latest image, stops old container, starts new container

## Required Secrets

Configure these secrets in your GitHub repository settings (Settings → Secrets and variables → Actions):

### Docker Hub Secrets
- `DOCKER_USERNAME`: Your Docker Hub username
- `DOCKER_PASSWORD`: Your Docker Hub password or access token

### Server Deployment Secrets
- `SERVER_HOST`: Your server's IP address or hostname
- `SERVER_USERNAME`: SSH username for your server
- `SERVER_SSH_KEY`: Private SSH key for server access
- `SERVER_PORT`: SSH port (optional, defaults to 22)
- `CONNECTION_STRING`: Database connection string for production
- `OLLAMA_BASE_URL`: Ollama service URL for production

## Server Requirements

Your deployment server should have:
- Docker installed and running
- SSH access configured
- The user should be in the `docker` group or have sudo access for Docker commands

## Environment Variables

The deployment workflow sets these environment variables for the container:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection`: From secrets
- `Ollama__BaseUrl`: From secrets

## Health Check

The application includes a health check endpoint at `/health` that returns:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Manual Deployment

You can also manually deploy by running these commands on your server:

```bash
# Login to Docker Hub
docker login

# Pull the latest image
docker pull your-username/aichatapi:latest

# Stop and remove existing container
docker stop aichatapi-webapi
docker rm aichatapi-webapi

# Run new container
docker run -d \
  --name aichatapi-webapi \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  -e Ollama__BaseUrl="your-ollama-url" \
  your-username/aichatapi:latest
```

## Troubleshooting

### Build Issues
- Check that all required secrets are configured
- Verify Docker Hub credentials are correct
- Ensure the Dockerfile builds successfully locally

### Deployment Issues
- Verify SSH key has access to the server
- Check that Docker is running on the server
- Ensure the server user has Docker permissions
- Check server logs: `docker logs aichatapi-webapi`

### Health Check Failures
- Verify the application starts correctly
- Check environment variables are set properly
- Ensure the port 8080 is accessible
