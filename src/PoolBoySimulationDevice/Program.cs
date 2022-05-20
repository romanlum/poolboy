// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

Console.WriteLine("Hello, World!");
DeviceClient client = DeviceClient.CreateFromConnectionString("xxx");
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

        reportedProperties["lastPatchId"] = patchID;
        try
        {
            await client.UpdateReportedPropertiesAsync(reportedProperties);
        }catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}