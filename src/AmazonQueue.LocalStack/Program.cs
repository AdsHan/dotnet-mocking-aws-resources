using AmazonQueue.LocalStack.Services;

namespace AmazonQueue.LocalStack;

class Program
{
    static async Task Main(string[] args)
    {
        var busService = new MessageBusService();

        var queueUrl = await busService.CreateQueue("teste-queue");

        await busService.Polling(queueUrl);
    }
}
