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

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
    ftps_state          = "Disabled"
    http2_enabled       = true
    minimum_tls_version = "1.2"
    always_on           = false
  }

  app_settings = merge(
    {
      "AIProjectEndpoint"        = var.ai_project_endpoint
      "AIAgentId"                = var.ai_agent_id
      "ASPNETCORE_ENVIRONMENT"   = "Production"
      "ASPNETCORE_HTTP_PORTS"    = "8080"
    },
    {
      "AzureOpenAIEmbeddings__Endpoint" = var.azure_openai_embeddings_endpoint
      "AzureOpenAIEmbeddings__ApiKey"   = var.azure_openai_embeddings_api_key
      "AzureOpenAIEmbeddings__DeploymentName"   = var.azure_openai_embeddings_deployment_name
    },
    var.key_vault_url != "" ? {
      "AZURE_KEY_VAULT_URL" = var.key_vault_url
    } : {},
    var.app_insights_instrumentation_key != "" ? {
      "APPLICATIONINSIGHTS_CONNECTION_STRING" = "InstrumentationKey=${var.app_insights_instrumentation_key}"
    } : {},
    var.search_service_endpoint != "" ? {
      "AzureAISearch__Endpoint"  = var.search_service_endpoint
      "AzureAISearch__IndexName" = var.search_index_name
      # Using managed identity for authentication instead of API key
      "AzureAISearch__UseManagedIdentity" = "true"
    } : {},
    var.storage_account_name != "" ? {
      "AzureStorage__AccountName"        = var.storage_account_name
      "AzureStorage__UseManagedIdentity" = "true"
    } : {}
  )
}