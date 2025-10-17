# Azure AI Search Index Setup

## Overview
This Terraform configuration creates the necessary Azure infrastructure for the AI Foundry Agent application, including:
- Azure AI Search service named `ai-foundry-agent-search`
- App Service with environment variables configured to connect to the search service
- Role assignments for managed identity authentication

## Search Index Creation
The search index named `ai-foundry-agent-index` needs to be created after the infrastructure is deployed. The application expects this index to exist with the following schema:

### Required Index Schema
```
{
  "name": "ai-foundry-agent-index",
  "fields": [
    {
      "name": "id",
      "type": "Edm.String",
      "key": true,
      "searchable": false,
      "filterable": true,
      "retrievable": true,
      "sortable": true,
      "facetable": false
    },
    {
      "name": "content",
      "type": "Edm.String",
      "searchable": true,
      "filterable": false,
      "retrievable": true,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "title",
      "type": "Edm.String",
      "searchable": true,
      "filterable": false,
      "retrievable": true,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "filepath",
      "type": "Edm.String",
      "searchable": true,
      "filterable": true,
      "retrievable": true,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "url",
      "type": "Edm.String",
      "searchable": true,
      "filterable": false,
      "retrievable": true,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "metadata_storage_path",
      "type": "Edm.String",
      "searchable": false,
      "filterable": true,
      "retrievable": true,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "metadata_storage_name",
      "type": "Edm.String",
      "searchable": true,
      "filterable": false,
      "retrievable": true,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "contentVector",
      "type": "Collection(Edm.Single)",
      "searchable": true,
      "filterable": false,
      "retrievable": true,
      "sortable": false,
      "facetable": false
    }
  ],
  "suggesters": [
    {
      "name": "sg",
      "searchMode": "analyzingInfixMatching",
      "sourceFields": ["title", "content"]
    }
  ],
  "scoringProfiles": [
    {
      "name": "documentScoring",
      "textWeights": {
        "weights": [
          {"name": "title", "weight": 2},
          {"name": "content", "weight": 1},
          {"name": "filepath", "weight": 0.5}
        ]
      }
    }
  ]
}
```

## Creating the Search Index

After running `terraform apply`, you can create the index using one of these methods:

### Method 1: Azure CLI
```bash
# Get the search service admin key from Terraform output
SEARCH_KEY=$(az search admin-key show --service-name ai-foundry-agent-search --resource-group ai-foundry-agent-rg --query primaryKey -o tsv)

# Create the index
az search index create \
  --resource-group ai-foundry-agent-rg \
  --service-name ai-foundry-agent-search \
  --name ai-foundry-agent-index \
  --indexes 'PASTE_THE_SCHEMA_JSON_ABOVE' \
  --query-key $SEARCH_KEY
```

### Method 2: Using Application Code
The application is configured to use managed identity authentication to access Azure AI Search:

- Endpoint: https://ai-foundry-agent-search.search.windows.net
- Index name: ai-foundry-agent-index
- Authentication: Managed Identity (enabled via `AzureAISearch__UseManagedIdentity=true`)

## Application Configuration
The App Service is configured with these environment variables:
- `AzureAISearch__Endpoint`: The search service endpoint
- `AzureAISearch__IndexName`: `ai-foundry-agent-index`
- `AzureAISearch__UseManagedIdentity`: `true` (for managed identity authentication)

With this setup, the application will be able to connect to the Azure AI Search service once the index is created.