# Dynamics 365 with Azure Hybrid Relays
This repository is to support [this blog post](https://blackwaltz.co.uk/blog/hybrid-relay-listeners-with-dynamics-365).

## How to get started

This repository contains two main projects, an ASP.NET Core WebApi Project and a Microsoft Dynamics 365 Plugin Project. Both will need to be configured and hosted in order for this example to work.

### Pre-requisites

- NET Core v2.1+
- Visual Studio 2017
- Dynamics 365 Plugin Registration Tool
- An Azure Subscription with:
  - A Hybrid Namespace
  - A Hybrid Connection
  - A Shared Access Policy

### ASP.NET Core WebApi

Clone the repository, and navigate to the `WebApi` directory.

You will need to specify an environment variable with the key `RelayConnectionString`. This can be done by editing `launch.json` in the .vscode folder if you are using Visual Studio Code. The value of this variable needs to the SAS Connection string from the Azure Portal.

Once this has been done run the following commands:

```
dotnet restore
dotnet run
```

This will intialise the project, and start the listener. It should report that it is listening on the correct address within the console.

### Dynamics 365 Plugin

Clone the repository, and open the `IBM.HybridRelayPlugin.sln` file in Visual Studio from the `Plugin` directory.

Change the `Execute()` method to achieve the business logic you desire. This example simply updates an account record.

Select the release configuration you desire, and build the dll.

Using the plugin registration tool, upload the assembly to the Dynamics 365 instance of your choice.

Create a new step agains the assembly for the entity/action of your choice, and enter the relay configuration into the Secure Config textbox. This is a json string in the following format:

```json
{
    "Url": "https://<namespace>.servicebus.windows.net/<hybridconnectionname>",
    "SasKeyName" : "<saskeyname>",
    "SasKey" : "<saskey>"
}
```

Save the plugin step.

To trigger the process, perform the business step required in Dynamics 365 and (if you're debugging) you should see break-points hit within your ASP.NET WebApi listener