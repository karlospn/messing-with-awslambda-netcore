using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Common.Dtos;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DynamoLambda
{
    public class DynamoLambda
    {
        private readonly AmazonSQSClient _sqsClient;
        private readonly Table _table;

        public DynamoLambda()
        {
            _sqsClient = new AmazonSQSClient();
            var client = new AmazonDynamoDBClient();
            _table = Table.LoadTable(client, "Post");
        }
        
        public async Task Handler(Post input, ILambdaContext context)
        {
            var id = Guid.NewGuid().ToString();

            await AddRecordToDynamoDb(input, id);
            await SendToSqsAsync(input, id);
        }

        private async Task AddRecordToDynamoDb(Post input, string id)
        {

            var post = new Document
            {
                ["Id"] = id,
                ["Text"] = input.Text,
                ["Uri"] = input.Uri,
                ["Status"] = input.Status,
                ["VoiceId"] = input.VoiceId,
            };

            await _table.PutItemAsync(post);
        }


        public async Task SendToSqsAsync(Post post, string id)
        {
            post.Id = id;
            await _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = "YOUR_SQS_URI/posts-queue",
                MessageBody = JsonConvert.SerializeObject(post)
            });

        }

      
    }
}
