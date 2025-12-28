using Microsoft.Extensions.Hosting;

namespace Library.Infrastructure.RabbitMQ
{
    public class LogConsumerHostedService : BackgroundService
    {
        private readonly LogConsumer _consumer;

        public LogConsumerHostedService(LogConsumer consumer)
        {
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start listening to all queues
            await _consumer.StartAllAsync();

            // Keep running until the app shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}