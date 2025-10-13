# Makefile for AI Foundry Agent - Build and Deploy Orchestration

# Variables
PROJECT_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
CLIENT_APP_DIR := $(PROJECT_DIR)src/client-app
SRC_DIR := $(PROJECT_DIR)src
INFRA_DIR := $(PROJECT_DIR)infra

.PHONY: help build-client build-server build deploy deploy-infra deploy-app clean test run-dev run-prod terraform-init terraform-plan terraform-apply terraform-destroy terraform-validate

# Default target
help:
	@echo "AI Foundry Agent - Makefile"
	@echo "============================"
	@echo "Usage:"
	@echo "  make build                 # Build both client and server"
	@echo "  make build-client          # Build React client application"
	@echo "  make build-server          # Build .NET server application"
	@echo "  make deploy                # Deploy the complete solution"
	@echo "  make deploy-infra          # Deploy infrastructure with Terraform"
	@echo "  make deploy-app            # Deploy application to Azure App Service"
	@echo "  make run-dev               # Run development setup"
	@echo "  make run-prod              # Run production setup"
	@echo "  make test                  # Run tests"
	@echo "  make clean                 # Clean build artifacts"
	@echo "  make terraform-init        # Initialize Terraform"
	@echo "  make terraform-plan        # Plan Terraform changes"
	@echo "  make terraform-apply       # Apply Terraform changes"
	@echo "  make terraform-destroy     # Destroy Terraform resources"
	@echo "  make terraform-validate    # Validate Terraform configuration"

# Build client-side React application
build-client:
	@echo "Building React client application..."
	@cd $(CLIENT_APP_DIR) && npm install
	@cd $(CLIENT_APP_DIR) && npm run build
	@echo "React client application built successfully."

# Build server-side .NET application
build-server:
	@echo "Building .NET server application..."
	@cd $(SRC_DIR) && dotnet build
	@echo ".NET server application built successfully."

# Build both client and server
build: build-client build-server
	@echo "Both client and server applications built successfully."

# Initialize Terraform
terraform-init:
	@echo "Initializing Terraform..."
	@cd $(INFRA_DIR) && terraform init
	@echo "Terraform initialized successfully."

# Validate Terraform configuration
terraform-validate:
	@echo "Validating Terraform configuration..."
	@cd $(INFRA_DIR) && terraform validate
	@echo "Terraform configuration is valid."

# Plan Terraform changes
terraform-plan: terraform-init
	@echo "Planning Terraform changes..."
	@cd $(INFRA_DIR) && terraform plan
	@echo "Terraform plan completed."

# Apply Terraform changes
terraform-apply: terraform-init
	@echo "Applying Terraform changes. You will be prompted to confirm..."
	@cd $(INFRA_DIR) && terraform apply
	@echo "Terraform changes applied."

# Destroy Terraform resources (be careful!)
terraform-destroy:
	@echo "Destroying Terraform resources. You will be prompted to confirm..."
	@cd $(INFRA_DIR) && terraform destroy
	@echo "Terraform resources destroyed."

# Deploy infrastructure (Terraform)
deploy-infra: terraform-apply
	@echo "Infrastructure deployed successfully."

# Deploy application to Azure App Service
deploy-app: build
	@echo "Deploying application to Azure App Service..."
	@cd $(SRC_DIR) && dotnet publish -c Release
	@echo "Application published. Deploy to Azure App Service with:"
	@echo "  az webapp deployment source config-zip --resource-group <resource-group-name> --name <app-service-name> --src $(SRC_DIR)/bin/Release/net8.0/publish/ai-foundry-agent.zip"
	@echo "Or use your preferred deployment method."

# Deploy the complete solution (infrastructure + application)
deploy: deploy-infra deploy-app
	@echo "Complete solution deployed successfully."

# Run development setup
run-dev: build-client
	@echo "Starting development servers..."
	@echo "Open two terminals to run both client and server development servers."
	@echo "Terminal 1: cd $(CLIENT_APP_DIR) && npm start"
	@echo "Terminal 2: cd $(SRC_DIR) && dotnet run"
	@echo "Note: The React dev server will be available at http://localhost:3000 and the API at http://localhost:5000."

# Run production build locally
run-prod: build
	@echo "Starting production server..."
	@cd $(SRC_DIR) && dotnet run
	@echo "Production server started."

# Run tests
test: build
	@echo "Running tests..."
	@cd $(SRC_DIR) && dotnet test
	@cd $(CLIENT_APP_DIR) && npm test -- --watchAll=false
	@echo "Tests completed."

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	@cd $(CLIENT_APP_DIR) && rm -rf node_modules
	@cd $(CLIENT_APP_DIR) && rm -rf build
	@cd $(SRC_DIR) && dotnet clean
	@cd $(SRC_DIR) && rm -rf bin obj
	@rm -rf $(SRC_DIR)/wwwroot
	@echo "Build artifacts cleaned."