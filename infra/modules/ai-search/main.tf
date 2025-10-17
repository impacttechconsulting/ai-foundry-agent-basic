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
resource "azapi_resource" "search_index" {
  type      = "Microsoft.Search/searchServices/indexes@2023-11-01"
  name      = var.search_index_name
  parent_id = azurerm_search_service.ai_search.id

  body = {
    fields = [
      {
        name = "id"
        type = "Edm.String"
        key = true
        searchable = false
        filterable = false
        sortable = false
        facetable = false
        retrievable = true
      },
      {
        name = "content"
        type = "Edm.String"
        key = false
        searchable = true
        filterable = false
        sortable = false
        facetable = false
        retrievable = true
      },
      {
        name = "title"
        type = "Edm.String"
        key = false
        searchable = true
        filterable = false
        sortable = true
        facetable = false
        retrievable = true
      },
      {
        name = "filepath"
        type = "Edm.String"
        key = false
        searchable = true
        filterable = false
        sortable = false
        facetable = false
        retrievable = true
      },
      {
        name = "url"
        type = "Edm.String"
        key = false
        searchable = true
        filterable = false
        sortable = false
        facetable = false
        retrievable = true
      },
      {
        name = "metadata_storage_path"
        type = "Edm.String"
        key = false
        searchable = true
        filterable = false
        sortable = false
        facetable = false
        retrievable = true
      }
    ]
  }

  provider = azapi
  schema_validation_enabled = false

  # Make sure the search service is fully provisioned before creating the index
  depends_on = [
    azurerm_search_service.ai_search
  ]
}