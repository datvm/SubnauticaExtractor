using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models.Exporters
{

    public class FileExporter
    {

        public string OutputFolder { get; private set; }

        ScriptOptions options = ScriptOptions.Instance;

        public FileExporter(string outputFolder)
        {
            this.OutputFolder = outputFolder;
            Directory.CreateDirectory(outputFolder);
        }

        public void Export(DataPack dataPack)
        {
            Console.WriteLine("Exporting to JSON Files...");

            foreach (var item in dataPack.RawItems)
            {
                this.SaveItem(item.Value, item.Name, false);
            }

            foreach (var item in dataPack.References)
            {
                this.SaveItem(item.Values, item.Name, false);
            }
        }

        void SaveItem(object item, string name, bool forceExport)
        {
            if (!forceExport && !Utils.UsingProperties.Contains(name) && !Utils.UsingTables.Contains(name))
            {
                if (!this.options.ExportUnusedData)
                {
                    return;
                }

                if (this.options.AddTildeForUnusedNames)
                {
                    name = "~" + name;
                }
            }

            Console.WriteLine($"\t{name}");

            var content = JsonConvert.SerializeObject(item);
            var filePath = Path.Combine(this.OutputFolder, $"{name}.json");
            File.WriteAllText(filePath, content);
        }

    }

}
