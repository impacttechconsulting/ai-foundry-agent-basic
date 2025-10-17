# Optional Resources module

resource "azurerm_key_vault" "main" {
  count               = var.key_vault_name != "" ? 1 : 0
  name                = var.key_vault_name
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_name            = "standard"
  tenant_id           = var.tenant_id

  enabled_for_deployment          = true
  enabled_for_disk_encryption     = true
  enabled_for_template_deployment = true
}

resource "azurerm_application_insights" "main" {
  count               = var.application_insights_name != "" ? 1 : 0
  name                = var.application_insights_name
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"
}

# Outputs
output "key_vault_id" {
  value = var.key_vault_name != "" ? azurerm_key_vault.main[0].id : ""
}

output "key_vault_name" {
  value = var.key_vault_name != "" ? azurerm_key_vault.main[0].name : ""
}

output "key_vault_url" {
  value = var.key_vault_name != "" ? azurerm_key_vault.main[0].vault_uri : ""
}

output "app_insights_instrumentation_key" {
  value     = var.application_insights_name != "" ? azurerm_application_insights.main[0].instrumentation_key : ""
  description = "Application Insights instrumentation key (empty if not created)"
  sensitive = true
}

output "app_insights_id" {
  value = var.application_insights_name != "" ? azurerm_application_insights.main[0].id : ""
  description = "Application Insights resource ID (empty if not created)"
}