using Microsoft.EntityFrameworkCore;
using Subnautica.WikiDbExtractor.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.WikiDbExtractor.Models.Exporters
{
    public class EntryExporter
    {

        ScriptOptions options = ScriptOptions.Instance;

        public Assembly GameAssembly { get; private set; }

        string outputFile;
        DbContextOptions<SubnauticaEntities> dbOptions;
        public async Task ExportAsync()
        {
            await this.InitializeAsync();

            var exporterTypes = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(q => !q.IsAbstract && q.IsClass && q.GetInterfaces().Any(p => p == typeof(IExporter)))
                .ToList();

            var expectedConstructor = new Type[] { typeof(EntryExporter), };
            var constructorParams = new object[] { this, };

            foreach (var exporterType in exporterTypes)
            {
                var exporterConstructor = exporterType.GetConstructor(expectedConstructor);
                if (exporterConstructor == null)
                {
                    continue;
                }

                var exporter = exporterConstructor.Invoke(constructorParams) as IExporter;

                if (exporter.ShouldExport)
                {
                    Console.WriteLine(exporterType.Name);

                    await exporter.ExportAsync();
                }
            }
        }

        async Task InitializeAsync()
        {
            // Output File
            Directory.CreateDirectory(this.options.OutputFolder);
            this.outputFile = Path.Combine(this.options.OutputFolder, "data.db");
            if (File.Exists(this.outputFile))
            {
                File.Delete(this.outputFile);
                await Task.Delay(100);
            }

            // Data Context
            var connectionString = $"Data Source={Path.Combine(this.outputFile)}";
            var builder = new DbContextOptionsBuilder<SubnauticaEntities>();
            builder.UseSqlite(connectionString);
            this.dbOptions = builder.Options;

            // Initialize the database
            using (var dc = this.CreateDc())
            {
                await dc.Database.MigrateAsync();
            }

            // Game Assembly
            this.GameAssembly = Assembly.LoadFrom(Path.Combine(this.options.GameFolder, @"Subnautica_Data\Managed\Assembly-CSharp.dll"));
        }

        public SubnauticaEntities CreateDc()
        {
            return new SubnauticaEntities(this.dbOptions);
        }

    }
}
