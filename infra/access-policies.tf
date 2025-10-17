# Access policies for managed identities - separate to avoid circular dependencies

# Key Vault access policy for App Service managed identity (traditional method)
resource "azurerm_key_vault_access_policy" "app_service_key_vault_access" {
  count        = var.key_vault_name != "" ? 1 : 0
  key_vault_id = module.optional_resources.key_vault_id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = module.app_service.app_service_principal_id

  key_permissions = [
    "Get", "List"
  ]

  secret_permissions = [
    "Get", "List", "Set", "Delete", "Purge", "Recover"
  ]

  certificate_permissions = [
    "Get", "List", "Create", "Delete"
  ]

  depends_on = [
    module.app_service
  ]
}

# Key Vault RBAC role assignment for App Service managed identity (modern method)
resource "azurerm_role_assignment" "app_service_key_vault_secrets_user" {
  count                = var.key_vault_name != "" ? 1 : 0
  scope                = module.optional_resources.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = module.app_service.app_service_principal_id

  depends_on = [
    module.app_service
  ]
}

resource "azurerm_role_assignment" "app_service_key_vault_reader" {
  count                = var.key_vault_name != "" ? 1 : 0
  scope                = module.optional_resources.key_vault_id
  role_definition_name = "Key Vault Reader"
  principal_id         = module.app_service.app_service_principal_id

  depends_on = [
    module.app_service
  ]
}