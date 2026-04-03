@description('Azure region for all resources.')
param location string = 'brazilsouth'

@description('Name of the resource group (used for tagging).')
param projectName string = 'github-jonathanperis'

@description('App Service Plan SKU.')
@allowed(['F1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v3', 'P2v3', 'P3v3'])
param appServicePlanSku string = 'F1'

@description('Container image to deploy (e.g. ghcr.io/org/repo:tag).')
param containerImage string = 'ghcr.io/jonathanperis/blazor-mudblazor-starter:latest'

@description('Name of the App Service Plan.')
param appServicePlanName string = 'github-jonathanperis'

@description('Name of the Web App.')
param webAppName string = 'blazor-mudblazor-starter'

@description('Name of the Application Insights instance.')
param appInsightsName string = 'blazor-mudblazor-starter'

@description('Name of the Log Analytics Workspace.')
param logAnalyticsWorkspaceName string = 'blazor-mudblazor-workspace'

// ── Log Analytics Workspace ─────────────────────────────────────────────────
module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'deploy-log-analytics'
  params: {
    location: location
    workspaceName: logAnalyticsWorkspaceName
    projectName: projectName
  }
}

// ── Application Insights ────────────────────────────────────────────────────
module appInsights 'modules/appInsights.bicep' = {
  name: 'deploy-app-insights'
  params: {
    location: location
    appInsightsName: appInsightsName
    logAnalyticsWorkspaceId: logAnalytics.outputs.workspaceId
    projectName: projectName
  }
}

// ── App Service Plan ─────────────────────────────────────────────────────────
module appServicePlan 'modules/appServicePlan.bicep' = {
  name: 'deploy-app-service-plan'
  params: {
    location: location
    appServicePlanName: appServicePlanName
    sku: appServicePlanSku
    projectName: projectName
  }
}

// ── Web App ──────────────────────────────────────────────────────────────────
module webApp 'modules/webApp.bicep' = {
  name: 'deploy-web-app'
  params: {
    location: location
    webAppName: webAppName
    appServicePlanId: appServicePlan.outputs.planId
    containerImage: containerImage
    appInsightsConnectionString: appInsights.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    projectName: projectName
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────
output webAppUrl string = webApp.outputs.defaultHostname
output appInsightsName string = appInsights.outputs.name
