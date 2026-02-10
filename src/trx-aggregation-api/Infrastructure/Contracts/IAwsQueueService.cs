using aggregate_api.Application.Domain.Models;
using Amazon.SQS.Model;

namespace aggregate_api.Infrastructure.Contracts;

public interface IAwsQueueService
{
    Task<ResponseModel<TRequest>> SendMessageAsync<TRequest>(string queueUrl, TRequest request, CancellationToken token);
    Task<ResponseModel<List<Message>>> ReceiveMessageAsync(string queueUrl, int maxNumberOfMessages, CancellationToken token);
    Task<ResponseModel> DeleteMessageAsync(string queueUrl, string receiptHandle, CancellationToken token);
}