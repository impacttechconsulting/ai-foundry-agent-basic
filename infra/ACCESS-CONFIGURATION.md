# Managed Identity Access Configuration

## Overview
This document explains when different managed identity access methods are enabled in the infrastructure.

## Key Vault Access

### Access Policies (Traditional Method)
- **Resource**: `azurerm_key_vault_access_policy` in `access-policies.tf`
- **When Created**: When `var.key_vault_name` is not empty
- **Permissions**: Keys (Get, List), Secrets (Get, List, Set, Delete, Purge, Recover), Certificates (Get, List, Create, Delete)

### RBAC Role Assignments (Modern Method)  
- **Resources**: 
  - `azurerm_role_assignment` with "Key Vault Secrets User" role
  - `azurerm_role_assignment` with "Key Vault Reader" role
- **When Created**: When `var.key_vault_name` is not empty
- **Permissions**: More granular RBAC permissions for secrets and read access

## Application Insights Access
- **Resources**:
  - `azurerm_role_assignment` with "Application Insights Component Contributor" role
  - `azurerm_role_assignment` with "Application Insights Reader" role
  - `azurerm_role_assignment` with "Application Insights Workbooks Contributor" role
- **When Created**: When `var.application_insights_name` is not empty
- **Permissions**: Component management, data reading, and workbook management

## Azure AI Search Access
- **Resource**: `azurerm_role_assignment` with "Search Service Contributor" role in `role-assignments.tf`
- **When Created**: When `var.search_service_name` is not empty
- **Permissions**: Full management of search service resources

## Default Behavior
By default, only optional resources with non-empty names will have their corresponding role assignments created. This prevents unnecessary resources from being created and associated costs from being incurred.