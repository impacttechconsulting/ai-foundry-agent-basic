# Output values

output "app_service_url" {
  value = module.app_service.app_service_url
  description = "URL of the deployed application"
}

output "resource_group_name" {
  value = module.resource_group.resource_group_name
  description = "Name of the resource group"
}

output "app_insights_instrumentation_key" {
  value     = module.optional_resources.app_insights_instrumentation_key
  description = "Application Insights instrumentation key (if created)"
  sensitive = true
}