# Terraform Infrastructure

This directory contains Terraform scripts to provision the Azure resources needed for the AI Foundry Agent application.

## Resources Provisioned

- Azure Resource Group (modules/resource-group/)
- App Service Plan and App Service with .NET 8 support (modules/app-service/)
- Optional Key Vault (for storing secrets) (modules/optional-resources/)
- Optional Application Insights (for monitoring) (modules/optional-resources/)

## Prerequisites

1. Install Terraform
2. Install Azure CLI and login: `az login`
3. You must have appropriate permissions in your Azure subscription

## Setup Instructions

1. **Initialize Terraform**:
   ```bash
   cd infra
   terraform init
   ```

2. **Create a terraform.tfvars file** (copy from example):
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```
   Edit the terraform.tfvars file with your specific values.

3. **Review the execution plan**:
   ```bash
   terraform plan
   ```

4. **Apply the configuration**:
   ```bash
   terraform apply
   ```

5. **When done, optionally destroy resources**:
   ```bash
   terraform destroy
   ```

## Module Structure

This Terraform configuration uses modules to organize resources:

- `modules/resource-group/` - Creates the Azure Resource Group
- `modules/app-service/` - Creates App Service Plan and App Service with .NET 8 support
- `modules/optional-resources/` - Creates optional resources (Key Vault, Application Insights)
- Root directory - Orchestrates the modules and defines outputs

## Important Notes

- The terraform.tfstate file contains the state of your infrastructure. Keep it secure.
- Do not commit terraform.tfstate or terraform.tfvars to git (they are in .gitignore).
- The AI Project and Agent resources need to be created separately in Azure AI Foundry (Terraform does not create these resources).
- The `ai_project_endpoint` and `ai_agent_id` variables must be provided after creating the AI resources in Azure AI Foundry.
- After deployment, configure your app's AI settings using the outputs from Terraform.
- By default, the configuration uses Azure's lowest cost tier for Linux App Service (B1 - Basic) to minimize costs.
- Optional resources like Key Vault and Application Insights are not created unless explicitly configured (to avoid costs).

## Variables

| Variable | Description | Default |
|----------|-------------|---------|
| resource_group_name | Name of the resource group | ai-foundry-agent-rg |
| location | Azure region | East US |
| app_service_plan_name | Name of the App Service Plan | ai-foundry-agent-plan |
| app_service_name | Name of the App Service | ai-foundry-agent-app |
| app_service_sku | SKU for the App Service Plan (B1=B1 Basic - lowest cost for Linux, S1=Standard, P1=Premium) | B1 |
| ai_project_endpoint | Azure AI Project endpoint | "" |
| ai_agent_id | AI Agent ID | "" |
| key_vault_name | Name of the Key Vault (optional) | "" |
| application_insights_name | Name of Application Insights (optional) | "" |