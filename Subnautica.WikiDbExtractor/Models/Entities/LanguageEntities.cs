using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.WikiDbExtractor.Models.Entities
{

    public class Language
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

    }

    public class LanguageKey
    {
        public int Id { get; set; }

        [Required]
        public string Key { get; set; }
    }

    public class LanguageText
    {
        public int Id { get; set; }

        public int LanguageKeyId { get; set; }
        public LanguageKey LanguageKey { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public string Text { get; set; }

    }

}
