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