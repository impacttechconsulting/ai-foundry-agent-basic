# AI Foundry Agent Sample Application

This repository contains a sample .NET 8 application demonstrating how to build an AI agent using Azure AI Foundry capabilities. The sample showcases best practices for developing intelligent agents that can interact with users and perform various tasks.

## Overview

The AI Foundry Agent sample application demonstrates:

- Integration with Azure OpenAI Service
- Intelligent agent architecture patterns
- Natural language processing capabilities
- RAG (Retrieval-Augmented Generation) capabilities with Azure AI Search
- Modern React front-end with TypeScript
- Azure deployment strategies
- Infrastructure as code with Terraform
- Build orchestration with Makefile

## Project Structure

```
ai-foundry-agent-basic/
├── Makefile                 # Makefile for building, testing, and deploying
├── src/                     # Main application source code
│   ├── AiFoundryAgent.csproj
│   ├── Program.cs
│   ├── GlobalUsings.cs
│   ├── Controllers/
│   │   ├── ChatController.cs
│   │   ├── HomeController.cs
│   │   └── AgentsController.cs
│   ├── Models/
│   │   └── AgentModels.cs
│   ├── Configuration/
│   │   └── ChatApiOptions.cs
│   ├── Views/
│   │   └── Home/
│   │       └── (React app now serves the UI instead of Index.cshtml)
│   ├── client-app/          # React client application
│   │   ├── package.json
│   │   ├── src/
│   │   │   └── App.tsx      # Main React component
│   │   └── public/
│   └── appsettings.json
├── infra/                   # Terraform infrastructure as code
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   ├── provider.tf
│   └── modules/
├── test/                    # Unit and integration tests
│   ├── AiFoundryAgent.Tests.csproj
│   └── AgentServiceTests.cs
├── .github/
│   └── workflows/
│       └── deploy-azure.yml  # GitHub Actions workflow for Azure deployment
├── .gitignore
├── Dockerfile
├── README.md
├── AiFoundryAgent.sln
└── appsettings.json
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- Azure subscription with access to Azure AI Foundry services

## Azure Resources Required

To deploy this application to Azure, you will need to create the following resources:

1. **Azure App Service** (or App Service Plan)
   - To host your .NET application
   - Provides web hosting with auto-scaling capabilities

2. **Azure AI Foundry Resources**
   - **AI Project**: Contains and manages your AI agents
   - **AI Agent(s)**: The actual intelligent agents your application will interact with

3. **Azure AI Search** (for RAG implementation)
   - Provides indexing and search capabilities for your own data
   - Enables Retrieval-Augmented Generation (RAG) scenarios
   - Free tier supports up to 50 MB of data and 5,000 documents

4. **[Optional] Azure Key Vault**
   - To securely store your AI service keys and configuration
   - Recommended for production deployments

5. **[Optional] Azure Application Insights**
   - For monitoring your application performance and usage
   - Provides logs, metrics, and diagnostic capabilities

### Creating Required Azure Resources

#### Using Azure CLI:

```bash
# Variables
RESOURCE_GROUP="your-resource-group-name"
LOCATION="East US"
APP_SERVICE_PLAN="your-app-service-plan"
WEB_APP_NAME="your-web-app-name"
AI_PROJECT_NAME="your-ai-project-name"

# Create Resource Group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service Plan
az appservice plan create --name $APP_SERVICE_PLAN --resource-group $RESOURCE_GROUP --sku B1 --is-linux

# Create Web App
az webapp create --resource-group $RESOURCE_GROUP --plan $APP_SERVICE_PLAN --name $WEB_APP_NAME --runtime "DOTNETCORE|8.0"

# Configure your AI Foundry resources via Azure portal or Azure CLI
# You'll need to set up an AI Project and Agent in Azure AI Foundry
```

#### Required Configuration Values:

After creating the Azure resources, you'll need these values for your application:

- `AIProjectEndpoint`: Your Azure AI Project endpoint URL
- `AIAgentId`: The ID of your Azure AI Agent

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/impacttechconsulting/ai-foundry-agent-basic.git
cd ai-foundry-agent-basic
```

### 2. Navigate to the src directory

```bash
cd src
```

### 3. Configure environment variables

Create a `.env` file in the `src` directory with the following settings:

```env
AIProjectEndpoint=https://your-ai-project-resource.azure.com/api/projects/your-ai-project
AIAgentId=your-agent-id
```

Alternatively, you can configure these in `appsettings.json` or via Azure Key Vault.

### 4. Run the application

#### Production Mode
```bash
dotnet run
```

The application will be available at `http://localhost:5000`.

#### Development Mode
For development with hot reloading, run both the React dev server and the .NET backend:

Terminal 1 (React client):
```bash
cd src/client-app
npm start
```

Terminal 2 (ASP.NET Core API):
```bash
cd src
dotnet run
```

In development mode, the React app will be available at `http://localhost:3000` and will communicate with the API running at `http://localhost:5000`.

### 5. Using the Application

The application provides a web-based chat interface for interacting with your AI agent:

#### Production Mode
- Navigate to `http://localhost:5000` to access the chat interface

#### Development Mode
- Navigate to `http://localhost:3000` to access the React development server with hot reloading
- The React app communicates with the API running at `http://localhost:5000`

#### API Endpoints (available in both modes)
- Or use the API endpoints directly:
  - `POST /chat/threads` - Create a new chat thread
  - `POST /chat/completions/{threadId}` - Send a message to a thread

### 6. API Endpoints

The application provides several API endpoints:

- **GET /api/agents** - Get a list of all available agents
- **GET /api/agents/{id}** - Get details for a specific agent
- **POST /chat/threads** - Create a new conversation thread
- **POST /chat/completions/{threadId}** - Send a message to a thread

## Building and Testing

### Using the Makefile (Recommended)

This project includes a Makefile for easy orchestration of build, test, and deployment operations:

```bash
# View all available commands
make help

# Build both client and server applications
make build

# Build only the React client application
make build-client

# Build only the .NET server application
make build-server

# Run tests for both applications
make test

# Clean build artifacts
make clean
```

### Direct .NET commands:

```bash
# Build the main project
dotnet build src/AiFoundryAgent.csproj

# Build and run tests
dotnet test test/AiFoundryAgent.Tests.csproj
```

### Run tests:

```bash
dotnet test
```

## Containerization

The project includes a Dockerfile for containerized deployment:

```bash
# Build the Docker image
docker build -t ai-foundry-agent .

# Run the container
docker run -p 8080:80 ai-foundry-agent
```

## Azure Deployment

This project includes infrastructure as code (Terraform) and a GitHub Actions workflow to deploy the application to Azure App Service.

### Using the Makefile for Infrastructure and Deployment (Recommended)

The project includes a Makefile that orchestrates both infrastructure setup and application deployment:

```bash
# Plan and deploy infrastructure using Terraform
make deploy-infra

# Deploy the application to Azure App Service
make deploy-app

# Deploy the complete solution (infrastructure + application)
make deploy

# Terraform operations
make terraform-init        # Initialize Terraform
make terraform-plan        # Plan infrastructure changes
make terraform-apply       # Apply infrastructure changes
make terraform-destroy     # Destroy infrastructure resources
make terraform-validate    # Validate Terraform configuration
```

### Using GitHub Actions

The project includes a GitHub Actions workflow to deploy the application to Azure App Service. To use this workflow:

1. Set up your Azure resources as described above (or use the Makefile to deploy infrastructure)
2. Configure GitHub Secrets with your Azure credentials:
   - `AZURE_WEBAPP_PUBLISH_PROFILE`: Your Azure App Service publish profile (can be obtained from the Azure portal)
3. Push your code to the `main` branch to trigger the deployment

The workflow is configured in `.github/workflows/deploy-azure.yml`.

### Configuring GitHub Secrets

1. Go to your GitHub repository
2. Navigate to Settings > Secrets and variables > Actions
3. Add a new secret:
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Copy the publish profile from your Azure App Service (in Azure portal, go to your App Service > Overview > Get publish profile)

## Architecture

The application follows a modern architecture with:

- **AI Integration**: Uses Azure AI Agents Persistent SDK for agent communication
- **Web Interface**: Modern chat UI using Bootstrap-like styling
- **API Endpoints**: RESTful APIs for agent management
- **Configuration**: Centralized configuration with validation
- **Global Usings**: Centralized using statements in `GlobalUsings.cs`

## Contributing

We welcome contributions! Please see the [contributing guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.