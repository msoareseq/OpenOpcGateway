using System.Text.Json.Serialization;

namespace OpcUaApi.Models
{
    public class ReadResult
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("s")]
        public bool Succeed { get; set; }
        
        [JsonPropertyName("r")]
        public string? Reason { get; set; }
        
        [JsonPropertyName("v")]
        public object? Value { get; set; }
        
        [JsonPropertyName("t")]
        public long Timestamp { get; set; }
    }

    public class ReadResults
    {
        public List<ReadResult> readResults {get ;set;}

        public ReadResults()
        {
            readResults = new List<ReadResult>();
        }
        
        public static ReadResults CreateError(string error)
        {
            ReadResults errorResults = new ReadResults();
            errorResults.readResults.Add(new ReadResult { Succeed = false, Reason = error });
            return errorResults;
        }
    }
}
