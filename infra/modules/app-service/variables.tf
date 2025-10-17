variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for the resources"
  type        = string
}

variable "app_service_plan_name" {
  description = "Name of the App Service Plan"
  type        = string
}

variable "app_service_name" {
  description = "Name of the App Service"
  type        = string
}

variable "app_service_sku" {
  description = "SKU for the App Service Plan"
  type        = string
}

variable "ai_project_endpoint" {
  description = "Azure AI Project endpoint"
  type        = string
}

variable "ai_agent_id" {
  description = "AI Agent ID"
  type        = string
}

variable "app_insights_instrumentation_key" {
  description = "Application Insights instrumentation key (optional)"
  type        = string
  default     = ""
}

variable "key_vault_url" {
  description = "Key Vault URL (optional)"
  type        = string
  default     = ""
}

variable "search_service_endpoint" {
  description = "Azure AI Search service endpoint (optional)"
  type        = string
  default     = ""
}

variable "search_service_admin_key" {
  description = "Azure AI Search service admin key (optional)"
  type        = string
  default     = ""
}

variable "search_index_name" {
  description = "Name of the Azure AI Search index to use (optional)"
  type        = string
  default     = ""
}

variable "storage_account_name" {
  description = "Storage account name for RAG documents (optional)"
  type        = string
  default     = ""
}