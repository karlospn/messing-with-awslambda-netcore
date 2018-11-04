using System.IO;
using Common.Utils;
using Newtonsoft.Json;

namespace Common.Dtos
{
    public class Message
    {
        public Post Post { get; set; }

        [JsonConverter(typeof(MemoryStreamJsonConverter))]
        public Stream Audio { get; set; }

        public string AudioType { get; set; }
       
    }


}
