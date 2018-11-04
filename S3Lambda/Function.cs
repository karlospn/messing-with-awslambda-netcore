using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Common.Dtos;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace S3Lambda
{
    public class S3Lambda
    {
        private readonly AmazonS3Client _s3Client;
        private readonly Table _table;

        public S3Lambda()
        {
            _s3Client = new AmazonS3Client();

            var client = new AmazonDynamoDBClient();
            _table = Table.LoadTable(client, Environment.GetEnvironmentVariable("DynamoDb_Table"));
        }

        public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
        {
            foreach (var msg in sqsEvent.Records)
            {
                try
                {
                    var message = JsonConvert.DeserializeObject<Message>(msg.Body);
                    await AddResultToS3(message);
                    await UpdateDynamoDb(message.Post);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private async Task AddResultToS3(Message msg)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = Environment.GetEnvironmentVariable("S3_bucket_name"),
                Key = $"{msg.Post.Id}.mp3",
                ContentType = msg.AudioType,
                InputStream = msg.Audio
            };
            await _s3Client.PutObjectAsync(putRequest);
        }


        private async Task UpdateDynamoDb(Post post)
        {
            var bucketRegionUri = Environment.GetEnvironmentVariable("S3_bucket_uri");
            var bucketName = Environment.GetEnvironmentVariable("S3_bucket_name");
            var doc = new Document
            {
                ["Uri"] = $"{bucketRegionUri}/{bucketName}/{post.Id}.mp3",
                ["Status"] = "Processed",
            };

            await _table.UpdateItemAsync(doc, post.Id);
        }

    
    }
}
