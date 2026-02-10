using System.Diagnostics;
using System.Net;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Infrastructure.Contracts;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace aggregate_api.Infrastructure;

public class AwsQueueService : IAwsQueueService
{
    private readonly ILoggingService _loggingService;
    private readonly IAmazonSQS _sqsClient;
    
    private readonly Stopwatch _stopwatch;
    
    public AwsQueueService(ILoggingService loggingService,
                           IEnvironmentService environmentService,
                           Stopwatch stopwatch)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        var _environmentService = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        _stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        
        _sqsClient = new AmazonSQSClient(RegionEndpoint.GetBySystemName(_environmentService.SqsRegion));
    }
    
    public async Task<ResponseModel<TRequest>> SendMessageAsync<TRequest>(string queueUrl, TRequest request, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AwsQueueService", "SendMessageAsync"));
        
        _stopwatch.Reset();
        _stopwatch.Start();
        
        var r = new ResponseModel<TRequest>(request);
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonConvert.SerializeObject(request)
        };
        
        var sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest, token);
            
        if (sendMessageResponse.HttpStatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(sendMessageResponse.MessageId))
        {
            _loggingService.LogCritical($"Failed to send message to queue. Queue URL: {queueUrl}", r);
        }
        
        _stopwatch.Stop();
        
        _loggingService.LogDebug(LoggingMessages.Stopwatch("AwsQueueService", "SendMessageAsync", _stopwatch.Elapsed));
        
        return r;
    }
    public async Task<ResponseModel<List<Message>>> ReceiveMessageAsync(string queueUrl, int maxNumberOfMessages, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AwsQueueService", "ReceiveMessageAsync"));

        _stopwatch.Reset();
        _stopwatch.Start();
        
        var r = new ResponseModel<List<Message>>(new List<Message>());
        
        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = maxNumberOfMessages
        };
        
        var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, token);

        if (receiveMessageResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _loggingService.LogCritical($"Failed to receive messages from queue. Queue URL: {queueUrl}", r);
        }
        
        if (receiveMessageResponse.Messages != null && receiveMessageResponse.Messages.Count > 0)
        {
            r.Data.AddRange(receiveMessageResponse.Messages);
        }
        
        _stopwatch.Stop();
        
        _loggingService.LogDebug(LoggingMessages.Stopwatch("AwsQueueService", "ReceiveMessageAsync", _stopwatch.Elapsed));

        return r;
    }
    public async Task<ResponseModel> DeleteMessageAsync(string queueUrl, string receiptHandle, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AwsQueueService", "DeleteMessageAsync"));
        
        _stopwatch.Reset();
        _stopwatch.Start();
        
        var r = new ResponseModel();
        
        var deleteMessageBatchResponse = await _sqsClient.DeleteMessageAsync(queueUrl, receiptHandle, token);
        
        if (deleteMessageBatchResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _loggingService.LogCritical($"Failed to delete messages from queue. Queue URL: {queueUrl}", r);
        }
        
        _stopwatch.Stop();
        
        _loggingService.LogDebug(LoggingMessages.Stopwatch("AwsQueueService", "DeleteMessageAsync", _stopwatch.Elapsed));
        
        return r;
    }
}