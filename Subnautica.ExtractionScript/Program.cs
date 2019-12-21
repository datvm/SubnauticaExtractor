using Subnautica.ExtractionScript.Models;
using Subnautica.ExtractionScript.Models.Exporters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript
{
    class Program
    {

        public static async Task Main(string[] args)
        {
            var options = ScriptOptions.Instance;

            var extractor = new DataExtractor(options.GameFolder);
            var dataPack = extractor.Extract();

            if (options.ExportToFiles)
            {
                new FileExporter(options.OutputFolder).Export(dataPack);
            }

            if (options.ExportToDatabase)
            {
                var outputFile = Path.Combine(options.OutputFolder, "data.db");
                await new SQLiteExporter(outputFile).ExportAsync(dataPack);
            }
        }

    }
}
