export class GameData {

    async loadAsync() {
        let tasks = [];

        for (let file of GameData.Files) {
            tasks.push(this._loadFile(file));
        }

        await Promise.all(tasks);

        this._process();
    }

    async _loadFile(file) {
        let url = `/game/${file}.json`;

        let data = await fetch(url)
            .then(r => r.json());
        this[file] = data;
    }

    _process() {
        this._processKeyValuePairs(this.techType, "items", "itemKeys");
        this._processKeyValuePairs(this.techGroup, "techGroups", "techGroupKeys");
        this._processKeyValuePairs(this.techCategory, "techCategories", "techCategoryKeys");
        this._processKeyValuePairs(this.quickSlotType, "quickSlotTypes", "quickSlotTypeKeys");
        this._processKeyValuePairs(this.equipmentType, "equipmentDataTypes", "equipmentTypeKeys");
        this._processKeyValuePairs(this.harvestType, "harvestTypes", "harvestTypeKeys");
        this._processKeyValuePairs(this.craftData_BackgroundType, "backgroundTypeData", "backgroundTypeKeys");

        this._processIdList(this.buildables, this.items, "isBuildable");
        this._processIdList(this.blacklist, this.items, "isBlacklisted");

        for (let groupName in this.groups) {
            let groupInfo = this.groups[groupName];

            let group = this.techGroups[groupName];
            let categories = group.categories = [];

            for (let categoryName in groupInfo) {
                let categoryInfo = groupInfo[categoryName];
                let category = this.techCategories[categoryName];

                category.parent = group;
                categories.push(category);

                let categoryItems = category.items = [];

                for (let itemId of categoryInfo) {
                    let item = this.items[itemId];

                    categoryItems.push(item);
                    item.parent = category;
                }
            }
        }
    }

    _processIdList(idList, targets, keyName) {
        for (let id of idList) {
            targets[id][keyName] = true;
        }
    }

    _processKeyValuePairs(list, dictName, keyName) {
        let dict = this[dictName] = {};
        this[keyName] = Object.keys(list);

        for (let id in list) {
            let name = list[id];
            dict[id] = dict[name] = {
                id: id,
                name: name,
            };
        }
    }

    getItem(idOrName) {
        return this.items[idOrName];
    }

    getItemDetails(idOrName) {
        var item = this.getItem(idOrName);

        if (item.ready) {
            return item;
        }

        item.useEatSound = this.useEatSound[item.name];
        item.equipmentType = this.equipmentDataTypes[this.equipmentTypes[item.name]];
        item.energyCost = this.energyCost[item.name];
        item.dropSound = this.dropSoundList[item.name];
        item.pickupSound = this.pickupSoundList[item.name];
        item.maxCharges = this.maxCharges[item.name];
        item.sizes = this.itemSizes[item.name];
        item.harvestType = this.harvestTypes[this.harvestTypeList[item.name]];
        item.harvestOutput = this.items[this.harvestOutputList[item.name]];
        item.harvestFinalCutBonus = this.harvestFinalCutBonusList[item.name];
        item.slotType = this.quickSlotTypes[this.slotTypes[item.name]];
        item.poweredPrefab = this.poweredPrefab[item.name];
        item.backgroundType = this.backgroundTypeData[this.backgroundTypes[item.name]];

        let craftingInfo = this.techData[item.name];
        if (craftingInfo) {
            let ingredients = [];
            let crafting = item.crafting = {
                amount: craftingInfo._craftAmount,
                ingredients: ingredients,
            };

            for (let ingredient of craftingInfo._ingredients) {
                ingredients.push({
                    item: this.items[ingredient.techType],
                    amount: ingredient.amount,
                });
            }
        }

        item.ready = true;
        return item;
    }

    getCategories(parentId) {
        if (parentId) {
            return this.techGroups[parentId].categories
                || [];
        } else {
            return this.techCategoryKeys.map(q => this.techCategories[q])
                || [];
        }
    }

    getItems(groupId, categoryId) {
        if (categoryId) {
            let category = this.techCategories[categoryId];
            return category.items || [];
        } else if (groupId) {
            let group = this.techGroups[groupId];
            let result = [];

            group.categories = group.categories || [];

            for (let category of group.categories) {
                result = result.concat(category.items);
            }

            return result;
        } else {
            return this.itemKeys
                .map(q => this.items[q]);
        }
    }

}

// Don't use class static variable, does not work in Firefox
GameData.Files = ["pickupSoundList", "maxCharges", "itemSizes", "harvestTypeList", "harvestType", "harvestOutputList", "harvestFinalCutBonusList", "groups", "equipmentTypes", "equipmentType", "energyCost", "dropSoundList", "craftingTimes", "craftData_BackgroundType", "cookedCreatureList", "buildables", "blacklist", "backgroundTypes", "useEatSound", "techType", "techGroup", "techData", "techCategory", "slotTypes", "seedSize", "quickSlotType", "poweredPrefab", "plantSize",];