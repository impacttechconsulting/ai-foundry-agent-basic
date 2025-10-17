output "storage_account_name" {
  value = azurerm_storage_account.rag_storage.name
}

output "storage_account_id" {
  value = azurerm_storage_account.rag_storage.id
}

output "storage_account_primary_blob_endpoint" {
  value = azurerm_storage_account.rag_storage.primary_blob_endpoint
}

output "documents_container_name" {
  value = azurerm_storage_container.documents.name
}

output "processed_container_name" {
  value = azurerm_storage_container.processed.name
}