# Azure AI Search module

resource "azurerm_search_service" "ai_search" {
  name                = var.search_service_name
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = var.search_service_sku

  replica_count               = var.search_service_replica_count
  partition_count             = var.search_service_partition_count
  hosting_mode                = var.hosting_mode
  semantic_search_sku         = var.semantic_search_sku
  public_network_access_enabled = var.public_network_access_enabled
}

# Output the search service endpoint
output "search_service_endpoint" {
  value       = "https://${azurerm_search_service.ai_search.name}.search.windows.net"
  description = "The endpoint URL for the Azure AI Search service"
}

# Output the search service admin key
output "search_service_admin_key" {
  value       = azurerm_search_service.ai_search.primary_key
  description = "The primary admin key for the Azure AI Search service"
  sensitive   = true
}

# Output the search service name
output "search_service_name" {
  value       = azurerm_search_service.ai_search.name
  description = "The name of the Azure AI Search service"
}

# Output the search service ID
output "search_service_id" {
  value       = azurerm_search_service.ai_search.id
  description = "The resource ID of the Azure AI Search service"
}