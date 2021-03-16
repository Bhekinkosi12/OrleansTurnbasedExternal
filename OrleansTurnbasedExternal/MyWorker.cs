using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansTurnbasedExternal
{
    public class MyWorker
    {
        private readonly int streamId;
        private IMyGrain grain;
        private Guid activationId;
        private TaskScheduler orleansTaskScheduler;
        private bool isStarted;
        private bool isDisposing;
        
        public MyWorker(int streamId)
        {
            this.streamId = streamId;
        }

        public void Start(
            IMyGrain grain,
            Guid activationId,
            TaskScheduler orleansTaskScheduler,
            int fileData)
        {
            if (isDisposing || isStarted)
            {
                return;
            }

            isStarted = true;

            this.grain = grain;
            this.activationId = activationId;
            this.orleansTaskScheduler = orleansTaskScheduler;

            Task.Run(() => PullLoopAsync(fileData));
        }

        private async Task PullLoopAsync(int fileData)
        {
            while (!isDisposing)
            {
                await PullAsync(fileData);
                await Task.Delay(10);
            }
        }

        private async Task PullAsync(int fileData)
        {
            await UpdateStreamDataOnOrleansSchedulerAsync(fileData);
        }

        private async Task UpdateStreamDataOnOrleansSchedulerAsync(int payload)
        {
            var stayActive = await Task.Factory.StartNew(() =>
            {
                return grain.UpdateMetricsDataAsync(streamId, payload, activationId);
            }, CancellationToken.None, TaskCreationOptions.None, scheduler: orleansTaskScheduler).Unwrap();

            if (!stayActive && !isDisposing)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            isDisposing = true;
        }
    }
}