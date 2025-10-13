# Resource Group module

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# Output resource group id
output "resource_group_id" {
  value = azurerm_resource_group.main.id
}

output "resource_group_name" {
  value = azurerm_resource_group.main.name
}