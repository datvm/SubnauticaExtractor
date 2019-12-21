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

        public FileExporter(string outputFolder)
        {
            this.OutputFolder = outputFolder;
            Directory.CreateDirectory(outputFolder);
        }

        public void Export(DataPack dataPack)
        {
            foreach (var item in dataPack.RawItems)
            {
                this.SaveItem(item.Value, item.Name);
            }

            foreach (var item in dataPack.References)
            {
                this.SaveItem(item.Values, "_" + item.Name);
            }
        }

        void SaveItem(object item, string name)
        {
            var content = JsonConvert.SerializeObject(item);
            var filePath = Path.Combine(this.OutputFolder, $"{name}.json");
            File.WriteAllText(filePath, content);
        }

    }

}
