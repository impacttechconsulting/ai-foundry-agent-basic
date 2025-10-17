variable "storage_account_name" {
  description = "Name of the storage account"
  type        = string
  default     = "aifoundryragstorage"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for the storage account"
  type        = string
  default     = "East US"
}