variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "ai-foundry-agent-rg"
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "East US"
}

variable "app_service_plan_name" {
  description = "Name of the App Service Plan"
  type        = string
  default     = "ai-foundry-agent-app-service-plan"
}

variable "app_service_name" {
  description = "Name of the App Service"
  type        = string
  default     = "ai-foundry-agent-app-service"
}

variable "app_service_sku" {
  description = "SKU for the App Service Plan (B1=B1 Basic - lowest cost for Linux, S1=Standard, P1=Premium)"
  type        = string
  default     = "F1" # Free tier
}

variable "ai_project_endpoint" {
  description = "Azure AI Project endpoint"
  type        = string
  default     = "https://impac-mgnvazjq-eastus2.services.ai.azure.com/api/projects/impact-ai-foundry-project"
}

variable "ai_agent_id" {
  description = "AI Agent ID"
  type        = string
  default     = "asst_OHueTDrq9jpp37QlgtXHPzwk"
}

variable "ai_project_name" {
  description = "Name of the Azure AI Project"
  type        = string
  default     = "impact-ai-foundry-project"
}

variable "key_vault_name" {
  description = "Name of the Key Vault (optional)"
  type        = string
  default     = ""
}

variable "application_insights_name" {
  description = "Name of Application Insights (optional)"
  type        = string
  default     = ""
}

variable "search_service_name" {
  description = "Name of the Azure AI Search service"
  type        = string
  default     = "ai-foundry-agent-search"
}

variable "search_service_sku" {
  description = "SKU for the Azure AI Search service (free, basic, standard, etc.)"
  type        = string
  default     = "basic"
}

variable "search_index_name" {
  description = "Name of the Azure AI Search index to use"
  type        = string
  default     = "ai-foundry-agent-index"
}