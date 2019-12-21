using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models
{

    public class ScriptOptions
    {

        public static readonly ScriptOptions Instance = new ScriptOptions();

        public string GameFolder { get; set; }
        public string OutputFolder { get; set; }
        public bool ExportToDatabase { get; set; }
        public bool ExportToFiles { get; set; }
        public bool ExportUnusedData { get; set; }
        public bool AddTildeForUnusedNames { get; set; }
        

        private ScriptOptions()
        {
            var content = File.ReadAllText("options.json");
            JsonConvert.PopulateObject(content, this);
        }

    }

}
