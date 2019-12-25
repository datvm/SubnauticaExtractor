using Subnautica.WikiDbExtractor.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.WikiDbExtractor.Models.Exporters
{

    public interface IExporter
    {

        bool ShouldExport { get; }

        Task ExportAsync();

    }

    public abstract class BaseExporter : IExporter
    {

        protected ScriptOptions options = ScriptOptions.Instance;

        public string GameFolder
        {
            get
            {
                return this.options.GameFolder;
            }
        }
        public string UnmanagedFolder
        {
            get
            {
                return Path.Combine(this.GameFolder, "SNUnmanagedData");
            }
        }
        public EntryExporter EntryExporter { get; private set; }

        public BaseExporter(EntryExporter entryExporter)
        {
            this.EntryExporter = entryExporter;
        }

        public SubnauticaEntities CreateDc()
        {
            return this.EntryExporter.CreateDc();
        }

        public abstract bool ShouldExport { get; }
        public abstract Task ExportAsync();
    }

}
