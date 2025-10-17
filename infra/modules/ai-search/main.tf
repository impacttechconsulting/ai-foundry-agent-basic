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

# Create the search index within the search service
resource "azurerm_search_index" "search_index" {
  name                = var.search_index_name
  search_service_name = azurerm_search_service.ai_search.name
  resource_group_name = var.resource_group_name

  # Define the fields for the search index
  field {
    name = "id"
    type = "Edm.String"
    key  = true
  }

  field {
    name     = "content"
    type     = "Edm.String"
    searchable = true
  }

  field {
    name     = "title"
    type     = "Edm.String"
    searchable = true
    sortable = true
  }

  field {
    name     = "filepath"
    type     = "Edm.String"
    searchable = true
  }

  field {
    name     = "url"
    type     = "Edm.String"
    searchable = true
  }

  field {
    name     = "metadata_storage_path"
    type     = "Edm.String"
    searchable = true
  }

  depends_on = [azurerm_search_service.ai_search]
}