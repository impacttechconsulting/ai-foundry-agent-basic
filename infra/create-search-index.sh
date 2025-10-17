#!/bin/bash
# Script to create the Azure AI Search index after Terraform deployment

# This script assumes:
# 1. Azure CLI is installed and authenticated
# 2. Terraform has been applied successfully
# 3. The search service "ai-foundry-agent-search" exists

echo "Creating Azure AI Search index: ai-foundry-agent-index"

# Get the resource group name (you may need to adjust this based on your actual resource group)
RESOURCE_GROUP="ai-foundry-agent-rg"
SEARCH_SERVICE="ai-foundry-agent-search"

# Get the admin key for the search service
ADMIN_KEY=$(az search admin-key show --service-name $SEARCH_SERVICE --resource-group $RESOURCE_GROUP --query primaryKey -o tsv)

# Define the index schema
INDEX_SCHEMA=$(cat <<EOF
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
EOF
)

# Create the search index
az search index create \
  --resource-group $RESOURCE_GROUP \
  --service-name $SEARCH_SERVICE \
  --name "ai-foundry-agent-index" \
  --indexes "$INDEX_SCHEMA"

echo "Search index 'ai-foundry-agent-index' created successfully!"
echo "The application is now ready to use Azure AI Search."