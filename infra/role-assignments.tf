# Role assignments for managed identities

# Assign Search Service Contributor role to the App Service managed identity
# This allows full management of search service resources
resource "azurerm_role_assignment" "app_service_search_contributor" {
  count                = var.search_service_name != "" ? 1 : 0
  scope                = module.ai_search.search_service_id
  role_definition_name = "Search Service Contributor"
  principal_id         = module.app_service.app_service_principal_id

  # Ensure the App Service and its identity are created before assigning roles
  depends_on = [
    module.app_service
  ]
}

# Alternative: More specific roles for production scenarios
# For read-only access to search indexes:
# resource "azurerm_role_assignment" "app_service_search_queryer" {
#   count                = var.search_service_name != "" ? 1 : 0
#   scope                = module.ai_search.search_service_id
#   role_definition_name = "Search Index Data Reader"
#   principal_id         = module.app_service.app_service_principal_id
#   depends_on = [
#     module.app_service
#   ]
# }

# For read/write access to search indexes:
# resource "azurerm_role_assignment" "app_service_search_publisher" {
#   count                = var.search_service_name != "" ? 1 : 0
#   scope                = module.ai_search.search_service_id
#   role_definition_name = "Search Index Data Contributor"
#   principal_id         = module.app_service.app_service_principal_id
#   depends_on = [
#     module.app_service
#   ]
# }