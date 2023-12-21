using OpcUaApi.Drivers;
using OpcUaApi.Models;

namespace OpcUaApi.Services
{
    public class OpcUaClientService : IOpcUaClient
    {
        private readonly IConfiguration config;
        private OpcUaClient opcUaClient;
        
        public OpcUaClientService(IConfiguration Config)
        {
            config = Config;
            string url = config.GetSection("OpcUaServer").GetSection("Url").Value!;
            opcUaClient = new OpcUaClient(url, true, 0);
            opcUaClient.StartClient();
        }

        public BrowseResults? Browse()
        {
            return opcUaClient.Browse();
        }

        public ReadResults? GetNodesValues(string[] nodesIds)
        {
            return opcUaClient.GetNodesValues(nodesIds);
        }

        public ReadResults? GetNodeValue(string nodeId)
        {
            return opcUaClient.GetNodeValue(nodeId);
        }

        public WriteResults? SetNodesValues(string[] nodesIds, object[] values)
        {
            return opcUaClient.SetNodesValues(nodesIds, values);
        }
    }
}
