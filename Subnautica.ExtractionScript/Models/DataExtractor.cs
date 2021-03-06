﻿using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models
{

    public class DataExtractor
    {

        public string GameFolder { get; private set; }
        public string UnmanagedFolder
        {
            get
            {
                return Path.Combine(this.GameFolder, "SNUnmanagedData");
            }
        }

        public string LanguageFolder
        {
            get
            {
                return Path.Combine(this.UnmanagedFolder, "LanguageFiles");
            }
        }

        public string EntryFile
        {
            get
            {
                return Path.Combine(this.GameFolder, @"Subnautica_Data\Managed\Assembly-CSharp.dll");
            }
        }

        public DataExtractor(string gameFolder)
        {
            this.GameFolder = gameFolder;
        }

        public List<string> SupportedLanguages()
        {
            return Directory.EnumerateFiles(this.LanguageFolder, "*.json")
                .Select(q => Path.GetFileNameWithoutExtension(q))
                .ToList();
        }

        public Dictionary<string, string> ExtractLanguage(string name)
        {
            var filePath = Path.Combine(this.LanguageFolder, $"{name}.json");

            JsonData jsonData;
            using (var reader = new StreamReader(filePath))
            {
                jsonData = JsonMapper.ToObject(reader);
            }

            var result = new Dictionary<string, string>();
            foreach (var key in jsonData.Keys)
            {
                result[key] = (string) jsonData[key];
            }

            return result;
        }

        public DataPack Extract()
        {
            var assembly = Assembly.LoadFrom(this.EntryFile);

            var craftData = assembly.GetType("CraftData");
            var fields = craftData
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Concat(craftData.GetFields(BindingFlags.Static | BindingFlags.NonPublic));

            var rawItems = fields
                .Select(q => new DataItem()
                {
                    Name = q.Name,
                    Value = q.GetValue(null),
                })
                .Where(q => q.Value != null)
                .ToList();

            var enumTypes = assembly.GetTypes()
                .Where(q => q.IsEnum)
                .ToList();

            var references = enumTypes
                .Select(q =>
                {
                    var enumFields = q.GetFields(BindingFlags.Static | BindingFlags.Public);
                    var values = new Dictionary<int, string>();

                    foreach (var field in enumFields)
                    {
                        var value = field.GetValue(null);
                        values[Convert.ToInt32(value)] = field.Name;
                    }

                    return new DataEnum()
                    {
                        Name = q.FullName
                            .Replace('+', '_')
                            .Replace('.', '_'),
                        Values = values,
                    };
                }).ToList();

            return new DataPack()
            {
                RawItems = rawItems,
                References = references,
            };
        }

    }

    public class DataPack
    {

        public List<DataEnum> References { get; set; }
        public List<DataItem> RawItems { get; set; }

    }

    public class DataEnum
    {

        public string Name { get; set; }
        public Dictionary<int, string> Values { get; set; }

    }

    public class DataItem
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

}
