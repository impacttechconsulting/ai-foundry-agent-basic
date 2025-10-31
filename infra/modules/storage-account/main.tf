# Storage Account module for RAG solution

resource "azurerm_storage_account" "rag_storage" {
  name                       = var.storage_account_name
  resource_group_name        = var.resource_group_name
  location                   = var.location
  account_tier               = "Standard"
  account_replication_type   = "LRS"
  account_kind               = "StorageV2"
  https_traffic_only_enabled = true
  min_tls_version            = "TLS1_2"

  # Enable managed identity for the storage account
  identity {
    type = "SystemAssigned"
  }
}

# Create a container for documents
resource "azurerm_storage_container" "documents" {
  name                  = "documents"
  storage_account_id    = azurerm_storage_account.rag_storage.id
  container_access_type = "private"
}
