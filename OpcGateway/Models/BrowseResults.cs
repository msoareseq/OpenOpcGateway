namespace OpcUaApi.Models
{
    public class BrowseResult
    {
        public string? id { get; set; }        
    }
    public class BrowseResults
    {
        public List<BrowseResult> browseResults { get; set; }
        public bool succeeded { get; set; }
        public string? reason { get; set; }

        public BrowseResults()
        {
            browseResults = new List<BrowseResult>();
            reason = string.Empty;            
        }
        
    }

}

