param location string
param webAppName string
param appServicePlanId string
param containerImage string
param appInsightsConnectionString string
param projectName string
param alwaysOn bool

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app,linux,container'
  tags: {
    project: projectName
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerImage}'
      alwaysOn: alwaysOn
      webSocketsEnabled: true
      healthCheckPath: '/healthz/ready'
      http20Enabled: true
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'WEBSITES_PORT'
          value: '5000'
        }
        {
          name: 'Learning__EnableDemoAuth'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://ghcr.io'
        }
        {
          // App Service storage at /home survives restarts, so SQLite and data-protection keys stay together.
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'true'
        }
        {
          name: 'Learning__DataDirectory'
          value: '/home/data'
        }
        {
          // App Service terminates TLS; trust its forwarded headers so cookies are marked Secure.
          name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
          value: 'true'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
      ]
    }
  }
}

// Deployments pull a container image; disable basic-auth publishing credentials for FTP and SCM.
resource ftpCredentials 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  parent: webApp
  name: 'ftp'
  properties: {
    allow: false
  }
}

resource scmCredentials 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  parent: webApp
  name: 'scm'
  properties: {
    allow: false
  }
}

output defaultHostname string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
