using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OpcUaApi.Models;

namespace OpcUaApi.Drivers
{
    public interface IOpcUaClient
    {
        public BrowseResults? Browse();
        public ReadResults? GetNodeValue(string nodeId);
        public ReadResults? GetNodesValues(string[] nodesIds);
        public WriteResults? SetNodesValues(string[] nodesIds, object[] values);
    }
}
