# App Service module

resource "azurerm_service_plan" "main" {
  name                = var.app_service_plan_name
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku
}

resource "azurerm_linux_web_app" "main" {
  name                = var.app_service_name
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }
    ftps_state      = "Disabled"
    http2_enabled   = true
    minimum_tls_version = "1.2"
  }

  app_settings = {
    "AIProjectEndpoint" = var.ai_project_endpoint
    "AIAgentId"         = var.ai_agent_id
    "ASPNETCORE_ENVIRONMENT" = "Production"
  }
}

# Outputs
output "app_service_url" {
  value = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "app_service_id" {
  value = azurerm_linux_web_app.main.id
}