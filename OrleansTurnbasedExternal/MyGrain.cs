using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrleansTurnbasedExternal
{
    public class MyGrain : Grain, IMyGrain
    {
        private readonly IGrainFactory grainFactory;
        private readonly ILogger<MyGrain> logger;
        private readonly Random random;
        private Guid activationId;
        private Dictionary<int, MyWorker> workers;
        private int activeTurns;

        public MyGrain(IGrainFactory grainFactory, ILogger<MyGrain> logger)
        {
            this.grainFactory = grainFactory;
            this.logger = logger;
            random = new Random();
        }

        public override Task OnActivateAsync()
        {
            activationId = Guid.NewGuid();
            return Task.CompletedTask;
        }

        public Task StartWorkers(int count)
        {
            if (workers is null)
            {
                workers = new Dictionary<int, MyWorker>();
                for (int i = 0; i < count; i++)
                {
                    ActivateWorker(i, 0);
                }
            }

            return Task.CompletedTask;
        }

        private void ActivateWorker(int streamId, int fileData)
        {
            var self = grainFactory.GetGrain<IMyGrain>(this.GetPrimaryKey()).AsReference<IMyGrain>();

            var worker = new MyWorker(streamId);

            workers.Add(streamId, worker);

            worker.Start(
                self,
                activationId,
                fileData
            );
        }

        public async Task<bool> UpdateMetricsDataAsync(int streamId, int payload, Guid activationId)
        {
            activeTurns++;

            await Task.Delay(random.Next(0, 1000));

            var message = $"Active turns: {activeTurns}";

            if (activeTurns > 1)
            {
                logger.LogWarning(message);
            }
            else
            {
                logger.LogInformation(message);
            }

            activeTurns--;

            return false;
        }
    }

    public interface IMyGrain : IGrainWithGuidKey
    {
        Task StartWorkers(int count);
        Task<bool> UpdateMetricsDataAsync(int streamId, int payload, Guid activationId);
    }
}
