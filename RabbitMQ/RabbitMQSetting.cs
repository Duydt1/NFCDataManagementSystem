namespace NFC.RabbitMQ
{
    public class RabbitMQSetting
    {
        public string HostName { get; set; }
        public string ConnectionString { get; set; }
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password  { get; set; }
    }
}
