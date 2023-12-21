using Microsoft.AspNetCore.Http.Features;
using System.Security.Cryptography.X509Certificates;

namespace OpcUaApi.Models
{
    public class OpcNodeList
    {
        public List<NodeItem> Nodes { get; }

        public OpcNodeList()
        {
            Nodes = new List<NodeItem>();
        }

        public void LoadList(string path, char separator = ',')
        {
            
            Nodes.Clear();                
            try
            {
                string[] list = File.ReadAllLines(path);
                foreach(string item in list)
                {
                    if (item.First() == '#') continue;
                    string[] sepItem = item.Split(separator);
                    if (sepItem.Length == 3)
                    {
                        Nodes.Add(new NodeItem()
                        {
                            Address = sepItem[0].Trim(),
                            TagName = sepItem[1].Trim(),
                            EnableWrite = bool.Parse(sepItem[2])

                        });
                    }
                
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading file: {ex.Message}");
            }
        }

        public void SaveList(string path, char separator = ',')
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("# Address,TagName,EnableWrite");
                    foreach (NodeItem item in Nodes)
                    {
                        sw.WriteLine($"{item.Address}{separator}{item.TagName}{separator}{item.EnableWrite}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving file: {ex.Message}");
            }
        }

        public string? GetAddressByTagName(string TagName)
        {
            NodeItem? result = Nodes.Find(x => x.TagName == TagName);
            return result?.Address;
        }
    }

    public class NodeItem
    {
        public string Address { get; set; }
        public string TagName { get; set; }
        public bool EnableWrite { get; set; }

        public NodeItem()
        {
            Address = "";
            TagName = "";
            EnableWrite = false;
        }
    }

    
}
