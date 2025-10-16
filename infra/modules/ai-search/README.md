# Azure AI Search Module

This module creates an Azure AI Search service that can be used for Retrieval-Augmented Generation (RAG) scenarios. The search service allows you to index your own data and perform semantic search operations to enhance your AI applications.

## Features

- Creates an Azure AI Search service with the specified SKU
- Configured with a free tier (50 MB) to support initial RAG implementation
- Provides secure access keys and endpoint URL for applications
- Supports semantic search capabilities for enhanced query results

## Variables

- `search_service_name`: Name of the Azure AI Search service (defaults to "ai-foundry-agent-search")
- `resource_group_name`: Name of the resource group to deploy the search service in
- `location`: Azure region for the search service (defaults to "East US")
- `search_service_sku`: SKU for the Azure AI Search service (defaults to "free")
- `search_service_replica_count`: Number of replicas for the search service (defaults to 1)
- `search_service_partition_count`: Number of partitions for the search service (defaults to 1)
- `hosting_mode`: Hosting mode for the search service (defaults to "default")
- `semantic_search_sku`: Semantic search SKU (defaults to "free")
- `public_network_access_enabled`: Whether public network access is enabled (defaults to true)

## Outputs

- `search_service_endpoint`: The endpoint URL for the Azure AI Search service
- `search_service_admin_key`: The primary admin key for the Azure AI Search service
- `search_service_name`: The name of the Azure AI Search service