using Microsoft.AspNetCore.Mvc;
using OpcUaApi.Drivers;
using OpcUaApi.Models;
using OpcUaApi.Services;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpcUaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WriteController : ControllerBase
    {
        private readonly OpcNodeList nodeList;
        private readonly IOpcUaClient opcUaClientService;
        private readonly ILogger<WriteController> logger;

        private readonly string? tagFilePath;


        public WriteController(IOpcUaClient OpcUaClient, ILogger<WriteController> Logger, IConfiguration config)
        {
            opcUaClientService = OpcUaClient;
            logger = Logger;
            logger.LogDebug("Starting Write Controller");

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
        
        // POST api/<WriteController>
        [HttpPost]
        public WriteResults Post([FromBody] object body)
        {
            if (body == null)
            {
                logger.LogError("WriteController: Post: body is null");
                return WriteResults.CreateError("The response was null");
            }

            List<WriteItem> writeItemList = JsonSerializer.Deserialize<List<WriteItem>>(body.ToString()!)!;
            
            List<string> ids = new List<string>();
            List<object> values = new List<object>();

            foreach (WriteItem item in writeItemList)
            {
                if (item.Id == null || item.Value == null) continue;

                ids.Add(nodeList.Nodes.Find(x => x.TagName == item.Id.Trim())?.Address ?? "");
                values.Add(item.Value);
            }

            return opcUaClientService.SetNodesValues(ids.ToArray(), values.ToArray()) ?? WriteResults.CreateError("The response was null");
        }
    }
}
