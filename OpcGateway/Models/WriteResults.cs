using System.Text.Json.Serialization;

namespace OpcUaApi.Models
{
    public class WriteResult
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("s")]
        public bool Succeed { get; set; }

        [JsonPropertyName("r")]
        public string? Reason { get; set; }        
    }

    public class WriteResults
    {
        public List<WriteResult> writeResults {get ;set;}

        public WriteResults()
        {
            writeResults = new List<WriteResult>();            
        }
        public static WriteResults CreateError(string error)
        {
            WriteResults errorResults = new WriteResults();
            errorResults.writeResults.Add(new WriteResult { Succeed = false, Reason = error });
            return errorResults;
        }
    }

}
