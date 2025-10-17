terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.49.0"
    }
    azapi = {
      source  = "azure/azapi"
      version = ">= 1.0"
    }
  }
  backend "local" {
    path = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
  subscription_id = "2bac8264-0bf2-4c6e-bada-7057ee793a23"
}

provider "azapi" {
  subscription_id = "2bac8264-0bf2-4c6e-bada-7057ee793a23"
}
 