using Microsoft.Azure.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoolBoy.AzChlorineSchedulerFunc.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostsettings, s) =>
    {
        var configuration = hostsettings.Configuration;
        s.AddSingleton(RegistryManager.CreateFromConnectionString(configuration["azureiothubconnectionstring"]));
        s.AddScoped<IoTHubService>();
        s.AddScoped<ChlorineDosierService>();
        s.AddScoped<OndiloService>();
    })
    .Build();

host.Run();