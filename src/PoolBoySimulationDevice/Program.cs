// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

Console.WriteLine("Hello, World!");
DeviceClient client = DeviceClient.CreateFromConnectionString("HostName=poolboyhub.azure-devices.net;DeviceId=TestDevice;SharedAccessKey=xxx");
await client.SetDesiredPropertyUpdateCallbackAsync(callback,client);
Console.ReadKey();

async Task callback(TwinCollection desiredProperties, object userContext)
{
    if(desiredProperties != null)
    {
        int patchID = desiredProperties["patchId"];
        Console.WriteLine($"Retrieved Config with PatchId {patchID}");
        var twin = await client.GetTwinAsync();
        TwinCollection reportedProperties = new TwinCollection();
        try
        {
            reportedProperties["lastPatchId"] = patchID;

            var startTime = twin.Properties.Desired["poolPumpConfig"]["startTime"].ToString();
            var stopTime = twin.Properties.Desired["poolPumpConfig"]["stopTime"].ToString();

        if (TimeOnly.Parse(startTime) >= TimeOnly.FromDateTime(DateTime.Now) &&
           TimeOnly.Parse(stopTime) <= TimeOnly.FromDateTime(DateTime.Now))
        {
            reportedProperties["poolPumpStatus"] = new  TwinCollection();
            reportedProperties["poolPumpStatus"]["active"] = true;

        }
        else
        {

            reportedProperties["poolPumpStatus"] = new TwinCollection();
            reportedProperties["poolPumpStatus"]["active"] = false;
        }
            reportedProperties["chlorinePumpStatus"] = new TwinCollection();
            reportedProperties["chlorinePumpStatus"]["active"] = true;
            reportedProperties["chlorinePumpStatus"]["startedAt"] = DateTimeOffset.Now.ToUnixTimeSeconds();
            reportedProperties["chlorinePumpStatus"]["runId"] = new Random().Next();



            await client.UpdateReportedPropertiesAsync(reportedProperties);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}