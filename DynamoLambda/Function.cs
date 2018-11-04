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
            _table = Table.LoadTable(client, Environment.GetEnvironmentVariable("DynamoDb_Table"));
        }
        
        public async Task Handler(Post input, ILambdaContext context)
        {
            try
            {
                var id = Guid.NewGuid().ToString();

                await AddRecordToDynamoDb(input, id);
                await SendToSqsAsync(input, id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }        
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
                QueueUrl = Environment.GetEnvironmentVariable("SQS_Queue"),
                MessageBody = JsonConvert.SerializeObject(post)
            });

        }

      
    }
}
