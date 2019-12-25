using LitJson;
using Microsoft.EntityFrameworkCore;
using Subnautica.WikiDbExtractor.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.WikiDbExtractor.Models.Exporters
{

    public class LanguageExporter : BaseExporter
    {

        public string LanguageFolder => Path.Combine(this.UnmanagedFolder, "LanguageFiles");

        public LanguageExporter(EntryExporter entryExporter) : base(entryExporter)
        {
        }

        public override bool ShouldExport => this.options.Exports.Languages;

        public override async Task ExportAsync()
        {
            var languageLimits = new HashSet<string>(this.options.Languages.LimitToLanguages ?? new string[0], StringComparer.OrdinalIgnoreCase);
            var languageFiles = Directory.EnumerateFiles(this.LanguageFolder, "*.json");

            foreach (var languageFile in languageFiles)
            {
                var languageName = Path.GetFileNameWithoutExtension(languageFile);

                if (languageLimits.Count > 0 && !languageLimits.Contains(languageName))
                {
                    continue;
                }

                Console.WriteLine($"\t{languageName}");

                // Purposely create a DC for each language
                // so  we do not have to clear trackings for those insertions
                using (var dc = this.CreateDc())
                {
                    var keys = (await dc.LanguageKeys
                        .ToListAsync())
                        .ToDictionary(q => q.Key);

                    var languageEntity = new Language()
                    {
                        Name = languageName,
                    };
                    dc.Languages.Add(languageEntity);

                    JsonData data;
                    using (var reader = new StreamReader(languageFile))
                    {
                        data = JsonMapper.ToObject(reader);
                    }

                    foreach (var key in data.Keys)
                    {
                        if (!keys.TryGetValue(key, out var languageKey))
                        {
                            keys[key] = languageKey = new LanguageKey()
                            {
                                Key = key,
                            };

                            dc.LanguageKeys.Add(languageKey);
                        }

                        dc.LanguageTexts.Add(new LanguageText()
                        {
                            Language = languageEntity,
                            LanguageKey = languageKey,
                            Text = (string)data[key],
                        });
                    }

                    await dc.SaveChangesAsync();
                }
            }
        }
    }

}
