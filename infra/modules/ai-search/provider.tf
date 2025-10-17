terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.49.0"
    }
    azapi = {
      source  = "azure/azapi"
      version = ">= 1.5"
    }
  }
}