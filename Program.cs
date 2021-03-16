using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OrleansTurnbasedExternal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(opts =>
                        {
                            opts.ClusterId = nameof(OrleansTurnbasedExternal);
                            opts.ServiceId = nameof(OrleansTurnbasedExternal);
                        })
                        .Configure<EndpointOptions>(opts =>
                        {
                            opts.AdvertisedIPAddress = IPAddress.Loopback;
                        })
                        .AddMemoryGrainStorageAsDefault();
                })
                .Build();

            await host.StartAsync();

            var client = (IClusterClient)host.Services.GetService(typeof(IClusterClient));

            await client.GetGrain<IMyGrain>(Guid.Empty).StartWorkers(10);

            await Task.Delay(int.MaxValue);
        }
    }
}
