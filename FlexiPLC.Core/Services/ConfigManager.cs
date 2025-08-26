using FlexiPLC.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace FlexiPLC.Core.Services
{
    public class ConfigManager
    {
        public static PlcConfig LoadConfig(string filePath)
        {   //string to PlcConfig
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<PlcConfig>(json);
        }
    }

    public class PlcConfig
    {
        public string PlcServiceTypeName { get; set; }
        public string ConnectionAddress { get; set; }
        public int ConnectionPort { get; set; }
        public List<PlcItem> Items { get; set; }
    }
}