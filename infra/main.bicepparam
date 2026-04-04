using './main.bicep'

param location = 'brazilsouth'
param projectName = 'github-jonathanperis'
param containerImage = 'ghcr.io/jonathanperis/blazor-mudblazor-starter:latest'
param appServicePlanName = 'github-jonathanperis'
param webAppName = 'blazor-mudblazor-starter'
param appInsightsName = 'blazor-mudblazor-starter'
param logAnalyticsWorkspaceName = 'blazor-mudblazor-workspace'
