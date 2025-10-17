# Managed Identity Configuration Guide

This document describes the managed identity setup for the AI Foundry Agent infrastructure.

## Services with Managed Identity

### App Service
- **Type**: System-assigned managed identity
- **Purpose**: Allows the App Service to authenticate to other Azure services without storing credentials
- **Configuration**: Enabled in `modules/app-service/main.tf`

### Azure AI Search Integration
- **Role Assignment**: Search Service Contributor (or more specific roles as needed)
- **File**: `role-assignments.tf`
- **Purpose**: Allows the App Service to manage and query the Azure AI Search service

### Key Vault Integration
- **Access Policy**: Configured with appropriate permissions for keys, secrets, and certificates
- **File**: `access-policies.tf`
- **Purpose**: Allows the App Service to access secrets stored in Key Vault

## Security Benefits

1. **No Credential Storage**: Applications can authenticate to Azure services without storing secrets
2. **Automated Credential Management**: Azure handles credential rotation automatically
3. **Principle of Least Privilege**: Each service is granted only the permissions it requires
4. **Auditing**: All managed identity authentication can be audited in Azure AD logs

## Configuration Details

The application code (`src/Program.cs`) uses `DefaultAzureCredential()` which in order tries:
1. Environment variables
2. Managed identity (when running in Azure)
3. Other authentication methods

This allows the same code to work in both development and production environments.