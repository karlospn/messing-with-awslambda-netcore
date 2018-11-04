using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Common.Dtos;
using Newtonsoft.Json;
using Message = Common.Dtos.Message;
using Common.Utils;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PollyLambda
{
    public class PollyLambda
    {
        private readonly AmazonPollyClient _pollyClient;
        private readonly AmazonSQSClient _sqsClient;

        public PollyLambda()
        {
            _pollyClient = new AmazonPollyClient();
            _sqsClient = new AmazonSQSClient();
        }

        public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
        {

            foreach (var msg in sqsEvent.Records)
            {
                try
                {
                    var post = JsonConvert.DeserializeObject<Post>(msg.Body);

                    var result = await ConvertFromTextUsingPolly(post);
                    var message = new Message
                    {
                        Post = post,
                        Audio = new MemoryStream(result.AudioStream.ToArrayBytes()),
                        AudioType = result.ContentType
                    };

                    await SendToSqsAsync(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }             
            }            
        }

        private async Task<SynthesizeSpeechResponse> ConvertFromTextUsingPolly(Post post)
        {
            var pollyRequest =  new SynthesizeSpeechRequest
            {
                Text = post.Text,
                VoiceId = post.VoiceId,
                OutputFormat = OutputFormat.Mp3,
            };

            var result = await _pollyClient.SynthesizeSpeechAsync(pollyRequest);
          
            return result;
        }

        public async Task SendToSqsAsync(Message msg)
        {
            await _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = "YOUR_SQS_QUEUE/s3-queue",
                MessageBody = JsonConvert.SerializeObject(msg)
            });

        }

      


    }
}
