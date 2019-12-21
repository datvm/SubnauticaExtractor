using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models
{

    public static class Utils
    {

        public static readonly HashSet<string> UsingProperties = new HashSet<string>()
            { "TechType", "TechCategory", "HarvestType", "EquipmentType", "QuickSlotType", "BackgroundType", "TechGroup" };

    }

}
