# Variables for Azure AI Search module

variable "search_service_name" {
  description = "Name of the Azure AI Search service"
  type        = string
  default     = "ai-foundry-agent-search"
}

variable "search_index_name" {
  description = "Name of the search index to create"
  type        = string
  default     = "ai-foundry-agent-index"
}

variable "resource_group_name" {
  description = "Name of the resource group to deploy the search service in"
  type        = string
}

variable "location" {
  description = "Azure region for the search service"
  type        = string
  default     = "East US"
}

variable "search_service_sku" {
  description = "SKU for the Azure AI Search service (free, basic, standard, etc.)"
  type        = string
  default     = "free"  # Free tier allows up to 50 MB of data
}

variable "search_service_replica_count" {
  description = "Number of replicas for the search service"
  type        = number
  default     = 1
}

variable "search_service_partition_count" {
  description = "Number of partitions for the search service"
  type        = number
  default     = 1
}

variable "hosting_mode" {
  description = "Hosting mode for the search service (default, 'highDensity', 'limited')"
  type        = string
  default     = "default"
}

variable "semantic_search_sku" {
  description = "Semantic search SKU ('free' or 'standard')"
  type        = string
  default     = "free"
}

variable "public_network_access_enabled" {
  description = "Whether public network access is enabled for the search service"
  type        = bool
  default     = true
}