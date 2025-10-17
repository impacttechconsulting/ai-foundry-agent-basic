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