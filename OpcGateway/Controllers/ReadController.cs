using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Opc.Ua;
using OpcUaApi.Drivers;
using OpcUaApi.Models;
using OpcUaApi.Services;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpcUaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadController : ControllerBase
    {
        private readonly OpcNodeList nodeList;
        private readonly IOpcUaClient opcUaClientService;
        private readonly ILogger<ReadController> logger;
        private readonly IConfiguration config;
        private readonly string? tagFilePath;

        public ReadController(IOpcUaClient OpcUaClient, ILogger<ReadController> Logger, IConfiguration config)
        {
            opcUaClientService = OpcUaClient;
            this.config = config;
            logger = Logger;
            logger.LogDebug("Starting Read Controller");

            tagFilePath = config.GetSection("ClientConfig").GetValue<string>("TagFilePath");

            if (tagFilePath == null)
            {
                logger.LogCritical("TagFilePath not found in appsettings.json");
                throw new NullReferenceException();
            }
            
            if (!System.IO.File.Exists(tagFilePath))
            {
                logger.LogCritical("Tag File not found.");
                throw new FileNotFoundException();
            }

            nodeList = new OpcNodeList();
            nodeList.LoadList(tagFilePath);
        }
        
        // GET: api/<ReadController>
        [HttpGet]
        public ReadResults Get()
        {
            List<string> addresses = new List<string>();
            nodeList.Nodes.ForEach(x => addresses.Add(x.Address));
            return opcUaClientService.GetNodesValues(addresses.ToArray()) ?? ReadResults.CreateError("The response was null");
            
        }

        // GET api/<ReadController>/id
        [HttpGet("{id}")]
        public ReadResults Get(string id)
        {
            string? address = nodeList.Nodes.Find(x => x.TagName == id.Trim())?.Address;
            if (address == null) return ReadResults.CreateError("Tag not found");
            return opcUaClientService.GetNodeValue(address) ?? new ReadResults();
        }

        // POST api/<ReadController>
        [HttpPost]
        public ReadResults Post([FromBody] string body)
        {
            List<string> idList  = JsonSerializer.Deserialize<List<string>>(body)!;
            List<string> addresses = new List<string>();
            foreach (string s in idList)
            {
                addresses.Add(nodeList.Nodes.Find(x => x.TagName == s.Trim())?.Address ?? "");
            }
            
            
            return opcUaClientService.GetNodesValues(addresses.ToArray()) ?? ReadResults.CreateError("The response was null");           
        }
    }
}
