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

# Output the search index name
output "search_index_name" {
  value       = var.search_index_name
  description = "The name of the search index"
}