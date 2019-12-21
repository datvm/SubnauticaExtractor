using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models.Exporters
{

    public class SQLiteExporter
    {

        static readonly HashSet<string> UsingTables = new HashSet<string>()
            { "TechType", "TechCategory", "HarvestType", "EquipmentType", "QuickSlotType", "BackgroundType", };

        public string OutputFile { get; private set; }

        SqliteConnection connection;

        public SQLiteExporter(string outputFile)
        {
            this.OutputFile = outputFile;
        }

        public async Task ExportAsync(DataPack dataPack)
        {
            // Delete if exist
            if (File.Exists(this.OutputFile))
            {
                File.Delete(this.OutputFile);
            }

            // Create Folder
            var folder = Path.GetDirectoryName(this.OutputFile);
            Directory.CreateDirectory(folder);

            var connectionString = this.GetConnectionString();
            using (this.connection = new SqliteConnection(connectionString))
            {
                this.connection.Open();

                foreach (var item in dataPack.References)
                {
                    await this.ExportEnumAsync(item);
                }

                await this.ExportCraftingAsync(dataPack.RawItems);
            }
        }

        string GetConnectionString()
        {
            var builder = new SqliteConnectionStringBuilder($"Data Source = {this.OutputFile}");

            return builder.ConnectionString;
        }

        async Task ExportEnumAsync(DataEnum dataEnum)
        {
            var tableName = dataEnum.Name;
            Console.WriteLine($"Exporting {tableName}");

            if (!UsingTables.Contains(tableName))
            {
                tableName = "Z_" + tableName;
            }

            using (var command = this.connection.CreateCommand())
            {
                command.CommandText = $"CREATE TABLE {tableName}(Id INTEGER PRIMARY KEY, Name TEXT)";

                await command.ExecuteNonQueryAsync();
            }

            var insertCommand = new StringBuilder();
            insertCommand.Append($"INSERT INTO {tableName}(Id, Name) VALUES ");

            foreach (var item in dataEnum.Values)
            {
                insertCommand.Append($"({item.Key}, '{item.Value}'),");
            }

            insertCommand.Remove(insertCommand.Length - 1, 1);

            using (var command = this.connection.CreateCommand())
            {
                command.CommandText = insertCommand.ToString();

                await command.ExecuteNonQueryAsync();
            }
        }

        async Task ExportCraftingAsync(List<DataItem> rawItems)
        {
            Console.WriteLine("Exporting Crafting Data");


        }

    }

}
