using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.WikiDbExtractor.Models
{

    public class ScriptOptions
    {

        public static readonly ScriptOptions Instance = new ScriptOptions();

        public string GameFolder { get; set; }
        public string OutputFolder { get; set; }
        public ExportOptions Exports { get; set; }
        public LanguagesOptions Languages { get; set; }

        public class ExportOptions
        {

            public bool Languages { get; set; }

        }

        public class LanguagesOptions
        {
            public string[] LimitToLanguages { get; set; }
        }

        private ScriptOptions()
        {
            var content = File.ReadAllText("options.json");
            JsonConvert.PopulateObject(content, this);
        }

    }

}
