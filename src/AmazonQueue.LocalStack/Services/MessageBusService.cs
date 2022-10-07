using Amazon.SQS;
using Amazon.SQS.Model;

namespace AmazonQueue.LocalStack.Services
{
    public class MessageBusService
    {
        private readonly IAmazonSQS _busQueue;

        public MessageBusService()
        {
            _busQueue = CreateBusQueue();
        }

        public async Task Polling(string queueUrl)
        {
            while (true)
            {
                Console.Write("Digite a mensagem: ");
                string message = Console.ReadLine();
                var messageSend = $"{message} - Send at {DateTime.Now.ToLongTimeString()}";

                await SendAsync(queueUrl, messageSend);

                var messages = await SubscribeAsync(queueUrl);

                if (messages.Any())
                {
                    Console.WriteLine($"{messages.Count} mensagens recebidas");

                    foreach (var msg in messages)
                    {
                        Console.WriteLine(msg.Body);
                        Console.WriteLine($"{msg.MessageId} processada com sucesso");
                        await DeleteAsync(queueUrl, msg.ReceiptHandle);
                    }
                }
            }
        }

        public async Task<string> CreateQueue(string queueName)
        {
            try
            {
                var queues = await _busQueue.ListQueuesAsync("");

                var matche = queues.QueueUrls.Where(x => x.EndsWith(queueName)).FirstOrDefault();

                if (matche is not null) return matche;

                var attributes = new Dictionary<string, string>();
                attributes.Add(QueueAttributeName.VisibilityTimeout, "120");

                var queue = new CreateQueueRequest
                {
                    QueueName = queueName,
                    Attributes = attributes
                };

                var response = await _busQueue.CreateQueueAsync(queue);

                return response.QueueUrl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SendAsync(string queueUrl, string message)
        {
            try
            {
                var sendRequest = new SendMessageRequest(queueUrl, message);

                var response = await _busQueue.SendMessageAsync(sendRequest);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<List<Message>> SubscribeAsync(string queueUrl, int maxNumberOfMessages = 10, int waitTimeSeconds = 5)
        {
            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = maxNumberOfMessages,
                    WaitTimeSeconds = waitTimeSeconds
                };

                var response = await _busQueue.ReceiveMessageAsync(request);

                return response.Messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task DeleteAsync(string queueUrl, string receiptHandle)
        {
            try
            {
                var request = new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = receiptHandle
                };

                await _busQueue.DeleteMessageAsync(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IAmazonSQS CreateBusQueue()
        {
            var config = new AmazonSQSConfig { ServiceURL = "http://localhost:4566", AuthenticationRegion = "us-east-1", UseHttp = true };

            return new AmazonSQSClient(config);
        }
    }
}
