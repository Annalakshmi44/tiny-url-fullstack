terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

variable "project_name" {
  default = "tinyurl"
}

variable "location" {
  default = "East US"
}

variable "sql_admin_password" {
  sensitive = true
}

variable "secret_token" {
  sensitive = true
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${var.project_name}"
  location = var.location
}

# Azure SQL Server
resource "azurerm_mssql_server" "main" {
  name                         = "sql-${var.project_name}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = var.sql_admin_password
}

resource "azurerm_mssql_database" "main" {
  name      = "db-${var.project_name}"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "Basic"
}

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Storage Account
resource "azurerm_storage_account" "main" {
  name                     = "st${var.project_name}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "logs" {
  name                  = "logs"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "plan-${var.project_name}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "B1"
}

# API Web App
resource "azurerm_linux_web_app" "api" {
  name                = "app-${var.project_name}-api"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    "UseAzureSql"                       = "true"
    "ConnectionStrings__DefaultConnection" = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Database=${azurerm_mssql_database.main.name};User ID=sqladmin;Password=${var.sql_admin_password};Encrypt=True;"
    "SecretToken"                       = var.secret_token
    "BaseUrl"                           = "[app-${var.project_name}-api.azurewebsites.net](https://app-${var.project_name}-api.azurewebsites.net)"
  }
}

# Frontend Web App
resource "azurerm_linux_web_app" "web" {
  name                = "app-${var.project_name}-web"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      node_version = "20-lts"
    }
  }

  app_settings = {
    "API_URL" = "[${azurerm_linux_web_app.api.default_hostname}](https://${azurerm_linux_web_app.api.default_hostname})"
  }
}

# Function App
resource "azurerm_linux_function_app" "cleanup" {
  name                       = "func-${var.project_name}-cleanup"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  service_plan_id            = azurerm_service_plan.main.id
  storage_account_name       = azurerm_storage_account.main.name
  storage_account_access_key = azurerm_storage_account.main.primary_access_key

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  app_settings = {
    "SqlConnectionString" = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Database=${azurerm_mssql_database.main.name};User ID=sqladmin;Password=${var.sql_admin_password};Encrypt=True;"
  }
}

output "api_url" {
  value = "[${azurerm_linux_web_app.api.default_hostname}](https://${azurerm_linux_web_app.api.default_hostname})"
}

output "web_url" {
  value = "[${azurerm_linux_web_app.web.default_hostname}](https://${azurerm_linux_web_app.web.default_hostname})"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.main.fully_qualified_domain_name
}
