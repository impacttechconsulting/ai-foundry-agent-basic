# AI Foundry Agent Sample Application

This repository contains a sample .NET 8 application demonstrating how to build an AI agent using Azure AI Foundry capabilities. The sample showcases best practices for developing intelligent agents that can interact with users and perform various tasks.

## Overview

The AI Foundry Agent sample application demonstrates:

- Integration with Azure OpenAI Service
- Intelligent agent architecture patterns
- Natural language processing capabilities
- Azure deployment strategies

## Project Structure

```
ai-foundry-agent-basic/
├── src/                     # Main application source code
│   ├── AiFoundryAgent.csproj
│   ├── Program.cs
│   ├── GlobalUsings.cs
│   ├── Controllers/
│   │   └── AgentController.cs
│   └── appsettings.json
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

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- Azure subscription with access to Azure AI Foundry services

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
AZURE_OPENAI_ENDPOINT=https://YOUR_RESOURCE_NAME.openai.azure.com/
AZURE_OPENAI_API_KEY=YOUR_API_KEY
AZURE_OPENAI_DEPLOYMENT_NAME=YOUR_DEPLOYMENT_NAME
```

Alternatively, you can configure these in `appsettings.json` or via Azure Key Vault.

### 4. Run the application

```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

### 5. Using the API

Once running, you can test the AI agent endpoint:

```
POST /api/agent/process
Content-Type: application/json

{
  "input": "Hello, how are you?"
}
```

## Building and Testing

### Build the application:

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

This project includes a GitHub Actions workflow to deploy the application to Azure App Service. To use this workflow:

1. Create an Azure Resource Group and App Service
2. Set up GitHub Secrets with your Azure credentials:
   - `AZURE_WEBAPP_PUBLISH_PROFILE`: Your Azure App Service publish profile
3. Push your code to the `main` branch to trigger the deployment

The workflow is configured in `.github/workflows/deploy-azure.yml`.

## Azure OpenAI Setup

To use the AI capabilities, you'll need:

1. Create an Azure OpenAI resource in the Azure portal
2. Deploy a model (e.g., gpt-35-turbo or gpt-4)
3. Configure the endpoint, API key, and deployment name in your settings

The application currently uses a mock implementation that returns simulated responses. To enable real AI capabilities:

1. Update the OpenAIService in Program.cs with the correct Azure SDK implementation
2. Ensure your Azure credentials are properly configured
3. The service will automatically connect to Azure when credentials are present

## Architecture

The application follows a modular architecture with:

- **Agent Services**: Core logic for AI agent functionality in `OpenAIService`
- **Controllers**: API endpoints in `AgentController.cs`
- **Configuration**: App settings and dependency injection
- **Global Usings**: Centralized using statements in `GlobalUsings.cs`

## Contributing

We welcome contributions! Please see the [contributing guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.