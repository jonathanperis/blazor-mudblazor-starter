using './main.bicep'

param location = 'brazilsouth'
param projectName = 'github-jonathanperis'
// Replace with the verified image@sha256 digest from the Main Release run summary before deploying.
param containerImage = 'ghcr.io/jonathanperis/blazor-mudblazor-starter:latest'
param appServicePlanName = 'github-jonathanperis'
param appServicePlanSku = 'B1'
param webAppName = 'blazor-mudblazor-starter'
param appInsightsName = 'blazor-mudblazor-starter'
param logAnalyticsWorkspaceName = 'blazor-mudblazor-workspace'
