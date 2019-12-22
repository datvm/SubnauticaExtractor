using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnautica.ExtractionScript.Models
{

    public static class Utils
    {

        public static readonly HashSet<string> UsingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "TechType", "TechCategory", "HarvestType", "EquipmentType", "QuickSlotType", "BackgroundTypes", "TechGroup", "CraftData_BackgroundType" };

        public static readonly HashSet<string> UsingProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "craftingTimes", "maxCharges", "energyCost", "harvestTypeList", "harvestOutputList", "harvestFinalCutBonusList", "cookedCreatureList", "equipmentTypes", "slotTypes", "backgroundTypes", "pickupSoundList", "dropSoundList", "useEatSound", "poweredPrefab", "buildables", "blacklist", "groups", "itemSizes", "seedSize", "plantSize", "techData", };

    }

}
