variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for the resources"
  type        = string
}

variable "key_vault_name" {
  description = "Name of the Key Vault (optional, will not be created if left empty) - NOTE: Cost associated"
  type        = string
  default     = ""
}

variable "application_insights_name" {
  description = "Name of Application Insights (optional, will not be created if left empty) - NOTE: Cost associated"
  type        = string
  default     = ""
}

variable "tenant_id" {
  description = "Azure tenant ID"
  type        = string
}