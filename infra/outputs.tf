# Output values

output "app_service_url" {
  value       = module.app_service.app_service_url
  description = "URL of the deployed application"
}

output "resource_group_name" {
  value       = module.resource_group.resource_group_name
  description = "Name of the resource group"
}

output "app_insights_instrumentation_key" {
  value       = module.optional_resources.app_insights_instrumentation_key
  description = "Application Insights instrumentation key (if created)"
  sensitive   = true
}

output "search_service_endpoint" {
  value       = module.ai_search.search_service_endpoint
  description = "The endpoint URL for the Azure AI Search service"
}

output "search_service_admin_key" {
  value       = module.ai_search.search_service_admin_key
  description = "The primary admin key for the Azure AI Search service"
  sensitive   = true
}

output "search_service_name" {
  value       = module.ai_search.search_service_name
  description = "The name of the Azure AI Search service"
}

output "search_service_id" {
  value       = module.ai_search.search_service_id
  description = "The resource ID of the Azure AI Search service"
}

output "app_service_principal_id" {
  value       = module.app_service.app_service_principal_id
  description = "The principal ID of the App Service managed identity"
}

output "app_insights_id" {
  value       = module.optional_resources.app_insights_id
  description = "The resource ID of the Application Insights (if created)"
}

output "search_index_name" {
  value       = module.ai_search.search_index_name
  description = "The name of the search index created in the Azure AI Search service"
}