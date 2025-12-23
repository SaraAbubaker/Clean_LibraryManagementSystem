using RabbitMQ.Client;

namespace Library.Infrastructure.RabbitMQ
{
    public class RabbitMqConnection
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqConnection(string hostName = "localhost", string userName = "guest", string password = "guest")
        {
            _factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            return await _factory.CreateConnectionAsync();
        }
    }
}
