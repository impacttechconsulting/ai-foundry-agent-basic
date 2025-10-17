# Role assignments for Application Insights managed identity access

# Allow App Service to read and write Application Insights data
resource "azurerm_role_assignment" "app_service_application_insights_contributor" {
  count                = var.application_insights_name != "" ? 1 : 0
  scope                = module.optional_resources.app_insights_id # This references the module output in main.tf
  role_definition_name = "Application Insights Component Contributor"
  principal_id         = module.app_service.app_service_principal_id

  depends_on = [
    module.app_service
  ]
}

# Allow App Service to read Application Insights data
resource "azurerm_role_assignment" "app_service_application_insights_reader" {
  count                = var.application_insights_name != "" ? 1 : 0
  scope                = module.optional_resources.app_insights_id
  role_definition_name = "Application Insights Reader"
  principal_id         = module.app_service.app_service_principal_id

  depends_on = [
    module.app_service
  ]
}

# Allow App Service to manage Application Insights workbooks
resource "azurerm_role_assignment" "app_service_application_insights_workbook_contributor" {
  count                = var.application_insights_name != "" ? 1 : 0
  scope                = module.optional_resources.app_insights_id
  role_definition_name = "Application Insights Workbooks Contributor"
  principal_id         = module.app_service.app_service_principal_id

  depends_on = [
    module.app_service
  ]
}