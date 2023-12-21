using System.Text.Json.Serialization;

namespace OpcUaApi.Models
{
    public class WriteItem
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("v")]
        public object? Value { get; set; }
    }      
}
