# Main Terraform configuration to call modules

# Data sources
data "azurerm_client_config" "current" {}

# Call resource group module
module "resource_group" {
  source              = "./modules/resource-group"
  resource_group_name = var.resource_group_name
  location            = var.location
}

# Call storage account module for RAG documents
module "storage_account" {
  source               = "./modules/storage-account"
  resource_group_name  = module.resource_group.resource_group_name
  location             = var.location
  storage_account_name = var.storage_account_name
}

# Call AI Search module
module "ai_search" {
  source              = "./modules/ai-search"
  resource_group_name = module.resource_group.resource_group_name
  location            = var.location
  search_service_name = var.search_service_name
  search_service_sku  = var.search_service_sku
  search_index_name   = var.search_index_name
}

# Call optional resources module
module "optional_resources" {
  source                    = "./modules/optional-resources"
  resource_group_name       = module.resource_group.resource_group_name
  location                  = var.location
  key_vault_name            = var.key_vault_name
  application_insights_name = var.application_insights_name
  tenant_id                 = data.azurerm_client_config.current.tenant_id
}

# Call app service module
module "app_service" {
  source                           = "./modules/app-service"
  resource_group_name              = module.resource_group.resource_group_name
  location                         = var.location
  app_service_plan_name            = var.app_service_plan_name
  app_service_name                 = var.app_service_name
  app_service_sku                  = var.app_service_sku
  ai_project_endpoint              = var.ai_project_endpoint
  ai_agent_id                      = var.ai_agent_id
  key_vault_url                    = module.optional_resources.key_vault_url
  app_insights_instrumentation_key = module.optional_resources.app_insights_instrumentation_key
  search_service_endpoint          = module.ai_search.search_service_endpoint
  search_service_admin_key         = module.ai_search.search_service_admin_key
  search_index_name                = var.search_index_name
  storage_account_name             = module.storage_account.storage_account_name
}