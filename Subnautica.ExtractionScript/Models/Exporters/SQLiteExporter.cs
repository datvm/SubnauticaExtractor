using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models.Exporters
{

    public class SQLiteExporter
    {
        private const string TechTypeTable = "TechType";
        private const string TechGroupTable = "TechCategory";

        private const string Integer = "INTEGER";
        private const string Real = "REAL";
        private const string Text = "TEXT";
        static readonly HashSet<string> UsingTables = Utils.UsingTables;

        public string OutputFile { get; private set; }

        SqliteConnection connection;
        ScriptOptions options = ScriptOptions.Instance;
        public SQLiteExporter(string outputFile)
        {
            this.OutputFile = outputFile;
        }

        public async Task ExportAsync(DataPack dataPack)
        {
            Console.WriteLine("Exporting Database...");

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
            if (!UsingTables.Contains(tableName))
            {
                if (!this.options.ExportUnusedData)
                {
                    return;
                }

                if (this.options.AddTildeForUnusedNames)
                {
                    tableName = "~" + tableName;
                }
            }

            Console.WriteLine($"\tExporting {tableName}");

            using (var command = this.connection.CreateCommand())
            {
                command.CommandText = $"CREATE TABLE '{tableName}'(Id INTEGER PRIMARY KEY, Name TEXT)";

                await command.ExecuteNonQueryAsync();
            }

            var insertCommand = new StringBuilder();
            insertCommand.Append($"INSERT INTO '{tableName}'('Id', 'Name') VALUES ");

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
            Console.WriteLine("\tExporting Crafting Data...");

            var miscValues = new Dictionary<string, string>();

            foreach (var item in rawItems)
            {
                Console.WriteLine($"\t\t{item.Name}");

                switch (item.Name)
                {
                    case "craftingTimes":
                    case "maxCharges":
                    case "energyCost":
                        await this.ExportDictColumn<float>(item.Value as IDictionary, item.Name, Real);
                        break;
                    case "harvestTypeList":
                    case "harvestOutputList":
                    case "harvestFinalCutBonusList":
                    case "cookedCreatureList":
                    case "equipmentTypes":
                    case "slotTypes":
                        await this.ExportDictColumn<int>(item.Value as IDictionary, item.Name, Integer);
                        break;
                    case "backgroundTypes":
                        await this.ExportDictColumn<byte>(item.Value as IDictionary, item.Name, Integer);
                        break;
                    case "pickupSoundList":
                    case "dropSoundList":
                    case "useEatSound":
                    case "poweredPrefab":
                        await this.ExportDictColumn<string>(item.Value as IDictionary, item.Name, Text);
                        break;
                    case "buildables":
                    case "blacklist":
                        await this.ExportBit(item.Value as IEnumerable, item.Name);
                        break;
                    case "groups":
                        await this.ExportGroups(item.Value as IDictionary);
                        break;
                    case "itemSizes":
                        await this.ExportDictColumnVector(item.Value as IDictionary, item.Name);
                        break;
                    case "seedSize":
                    case "plantSize":
                        {
                            dynamic value = item.Value;
                            miscValues[item.Name + "X"] = value.x.ToString();
                            miscValues[item.Name + "Y"] = value.y.ToString();
                        }
                        break;
                    case "techData":
                        await this.ExportTechDataAsync(item.Value as IDictionary);
                        break;
                }
            }

            await this.ExportMiscDataAsync(miscValues);
        }

        async Task ExportTechDataAsync(IDictionary dictionary)
        {
            const string TableName = "TechTypeIngredients";

            const string Column = "CraftAmount";
            await this.AddTechTypeColumn(Column, Integer);

            var createTableStatement = $"CREATE TABLE {TableName}(Id {Integer} PRIMARY KEY, TechTypeId {Integer}, IngredientTechTypeId {Integer}, Quantity {Integer}, FOREIGN KEY(TechTypeId) REFERENCES '{TechTypeTable}'('Id'), FOREIGN KEY(IngredientTechTypeId) REFERENCES '{TechTypeTable}'('Id'));";
            await this.ExecuteDbCommandAsync(createTableStatement);

            var statements = new StringBuilder();
            statements.Append($"INSERT INTO {TableName}(TechTypeId, IngredientTechTypeId, Quantity) VALUES ");

            var craftAmountStatements = new StringBuilder();

            foreach (DictionaryEntry item in dictionary)
            {
                var id = (int)item.Key;
                var value = item.Value;

                var valueType = value.GetType();
                var craftAmountProp = valueType.GetProperty("craftAmount");
                var ingredientsProp = valueType.GetField("_ingredients");

                int craftAmount = (int) craftAmountProp.GetValue(value);
                craftAmountStatements.UpdateTable(TechTypeTable, Column, craftAmount, id);

                var ingredients = (IList)ingredientsProp.GetValue(value);
                var ingredientType = ingredientsProp.FieldType.BaseType.GetGenericArguments()[0];
                var ingredientTechTypeProp = ingredientType.GetProperty("techType");
                var ingredientAmountProp = ingredientType.GetProperty("amount");

                foreach (var ingredient in ingredients)
                {
                    var ingredientId = (int)ingredientTechTypeProp.GetValue(ingredient);
                    var amount = (int)ingredientAmountProp.GetValue(ingredient);

                    statements.Append($"({id}, {ingredientId}, {amount}),");
                }
            }
            statements.Remove(statements.Length - 1, 1);

            await this.ExecuteDbCommandAsync(statements.ToString());
            await this.ExecuteDbCommandAsync(craftAmountStatements.ToString());
        }

        async Task ExportMiscDataAsync(Dictionary<string, string> values)
        {
            const string TableName = "Misc";

            await this.ExecuteDbCommandAsync($"CREATE TABLE {TableName}(Id {Integer} PRIMARY KEY, Name {Text}, Value {Text});");

            var statements = new StringBuilder();
            statements.Append($"INSERT INTO {TableName}(Name, Value) VALUES ");

            foreach (var value in values)
            {
                statements.Append($"('{value.Key}', '{value.Value}'),");
            }
            statements.Remove(statements.Length - 1, 1);

            await this.ExecuteDbCommandAsync(statements.ToString());
        }

        async Task ExportDictColumn<TValue>(IDictionary dict, string column, string type)
        {
            await this.AddTechTypeColumn(column, type);

            var statements = new StringBuilder();

            foreach (DictionaryEntry item in dict)
            {
                statements.UpdateTable(TechTypeTable, column, (TValue)item.Value, (int)item.Key);
            }

            await this.ExecuteDbCommandAsync(statements.ToString());
        }

        async Task ExportDictColumnVector(IDictionary dict, string column)
        {
            var column1 = column + "X";
            var column2 = column + "Y";

            await this.AddTechTypeColumn(column1, Integer);
            await this.AddTechTypeColumn(column2, Integer);

            var statements = new StringBuilder();

            foreach (DictionaryEntry item in dict)
            {
                dynamic value = item.Value;

                statements.Append($"UPDATE {TechTypeTable} SET {column1} = {value.x}, {column2} = {value.y} WHERE Id = {(int) item.Key};");
            }

            await this.ExecuteDbCommandAsync(statements.ToString());
        }

        async Task ExportGroups(IDictionary groups)
        {
            const string ParentColumn = "TechGroupId";
            const string TechTypeColumn = "CategoryId";

            await this.AddColumn(TechGroupTable, ParentColumn, Integer);
            await this.AddTechTypeColumn(TechTypeColumn, Integer);

            var statements = new StringBuilder();

            foreach (DictionaryEntry group1 in groups)
            {
                var cat1 = (int)group1.Key;

                foreach (DictionaryEntry group2 in group1.Value as IDictionary)
                {
                    var cat2 = (int)group2.Key;

                    statements.UpdateTable(TechGroupTable, ParentColumn, cat1, cat2);

                    foreach (int item in group2.Value as IEnumerable)
                    {
                        statements.UpdateTable(TechTypeTable, TechTypeColumn, cat2, item);
                    }
                }
            }

            await this.ExecuteDbCommandAsync(statements.ToString());
        }

        async Task ExportBit(IEnumerable list, string column)
        {
            await this.AddTechTypeColumn(column, Integer);

            var stringBuilder = new StringBuilder();
            stringBuilder.UpdateTable(TechTypeTable, column, 0, null);
            foreach (int item in list)
            {
                stringBuilder.UpdateTable(TechTypeTable, column, 1, item);
            }

            await this.ExecuteDbCommandAsync(stringBuilder.ToString());
        }

        async Task AddTechTypeColumn(string column, string type)
        {
            await this.AddColumn(TechTypeTable, column, type);
        }

        async Task AddColumn(string table, string column, string type)
        {
            await this.ExecuteDbCommandAsync($"ALTER TABLE {table} ADD {column} {type};");
        }

        async Task ExecuteDbCommandAsync(string commandText)
        {
            using (var command = this.connection.CreateCommand())
            {
                command.CommandText = commandText;
                await command.ExecuteNonQueryAsync();
            }
        }

    }

}
