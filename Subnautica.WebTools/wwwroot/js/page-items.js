import { Common } from "./common.js";
import { GameData } from "./game-data.js";

class ItemPage {

    ready = false;
    selectingItem = null;

    txtSearch = document.querySelector("#txt-search");
    lstItems = document.querySelector("#lst-items");
    cboGroups = document.querySelector("#cbo-groups");
    cboCategories = document.querySelector("#cbo-categories");

    lblName = document.querySelector("#lbl-name");
    lblGroup = document.querySelector("#lbl-group");
    lblCategory = document.querySelector("#lbl-category");
    pnlCategory = document.querySelector("#pnl-category");
    lstTags = document.querySelector("#lst-tags");
    lstInfo = document.querySelector("#lst-info");
    lstIngredients = document.querySelector("#lst-ingredients");
    lblCraftAmount = document.querySelector("#lbl-amount");

    async initializeAsync() {
        this.gameData = new GameData();
        await this.gameData.loadAsync();

        this._showGroups();

        this.ready = true;
        Common.toggleLoading(false);
        this._addEventListeners();
    }

    _addEventListeners() {
        this.txtSearch.addEventListener("input",
            () => this._showItems());
        this.cboGroups.addEventListener("change",
            () => this._showCategories());
        this.cboCategories.addEventListener("change",
            () => this._showItems());

        this.lstItems.addEventDelegate("click", ".item",
            (e, target) => this._onItemClick(e, target));

        this.lblGroup.addEventListener("click",
            (e) => this._onCategoryLinkClick(e));
        this.lblCategory.addEventListener("click",
            (e) => this._onCategoryLinkClick(e));

        document.body.addEventDelegate("click", "[data-item-id]",
            (e, target) => this._onItemLinkClick(e, target));
    }

    _onItemLinkClick(e, target) {
        event.preventDefault();

        let itemId = target.getAttribute("data-item-id");
        let item = this.selectingItem = this.gameData.getItemDetails(itemId);

        let category = item.parent;
        if (category) {
            let group = category.parent;

            this._selectGroupAndCategory(group.id, category.id);
        }
        this._showItemDetails();
    }

    _onItemClick(e, target) {
        let itemId = target.getAttribute("data-id");

        let item = this.selectingItem = this.gameData.getItemDetails(itemId);
        this._showItemDetails();
    }

    _showItems() {
        let filter = this.txtSearch.value.toLowerCase();
        this.lstItems.innerHTML = "";

        let groupId = this.cboGroups.value;
        let categoryId = this.cboCategories.value;

        let items = this.gameData.getItems(groupId, categoryId);
        for (let item of items) {
            let id = item.id;
            let name = item.name;

            if (id == 0 || (filter && !name.toLowerCase().includes(filter))) {
                continue;
            }

            let div = document.createElement("div");
            div.classList.add("item");
            div.innerHTML = name;
            div.setAttribute("data-id", id);

            this.lstItems.append(div);
        }


    }

    _showGroups() {
        let cbo = this.cboGroups;
        this._initWithAllSelectOptions(cbo);

        for (let groupId of this.gameData.techGroupKeys) {
            let group = this.gameData.techGroups[groupId];
            this._addSelectOption(cbo, groupId, group.name);
        }

        this._showCategories();
    }

    _showCategories() {
        let cbo = this.cboCategories;
        this._initWithAllSelectOptions(cbo);

        let selectedGroupId = this.cboGroups.value;
        let categories = this.gameData.getCategories(selectedGroupId);

        for (let category of categories) {
            this._addSelectOption(cbo, category.id, category.name);
        }

        this._showItems();
    }

    _showItemDetails() {
        let item = this.selectingItem;

        this.lblName.innerHTML = item.name;

        let category = item.parent;

        if (category) {
            this.lblCategory.innerHTML = category.name;
            this.lblCategory.setAttribute("data-category-id", category.id);

            let group = category.parent;
            this.lblGroup.innerHTML = group.name;
            this.lblGroup.setAttribute("data-group-id", group.id);
        }
        this.pnlCategory.classList.toggle("invisible", !category);
        
        var tags = [];

        if (item.sizes) {
            tags.push(`${item.sizes.x} x ${item.sizes.y}`);
        }

        if (item.equipmentType) {
            tags.push("Equipment: " + item.equipmentType.name);
        }

        if (item.isBlacklisted) {
            tags.push("Blacklisted");
        }

        if (item.isBuildable) {
            tags.push("Buildable");
        }

        if (item.slotType) {
            tags.push(item.slotType.name);
        }

        if (item.backgroundType) {
            tags.push(item.backgroundType.name);
        }

        this.lstTags.innerHTML = "";
        for (let tag of tags) {
            let span = document.createElement("span");
            span.innerHTML = tag;
            this.lstTags.append(span);
        }

        var info = [];

        if (item.energyCost) {
            info.push("Energy Cost: " + item.energyCost);
        }

        if (item.maxCharges) {
            info.push("Max Charges: " + item.maxCharges);
        }

        if (item.harvestType) {
            let temp = "Harvest by " + item.harvestType.name;

            if (item.harvestOutput) {
                temp += `, result in <a href='#' data-item-id='${item.harvestOutput.id}'>${item.harvestOutput.name}</a>.`;

                if (item.harvestFinalCutBonus) {
                    temp += ` Final cut bonus: ${item.harvestFinalCutBonus}.`;
                }
            } else {
                temp += ".";
            }

            info.push(temp);
        }

        this.lstInfo.innerHTML = "";
        for (let infoItem of info) {
            let li = document.createElement("li");
            li.innerHTML = infoItem;
            this.lstInfo.append(li);
        }

        if (!info.length) {
            this.lstInfo.innerHTML = "None."
        }

        this.lstIngredients.innerHTML = "";
        let crafting = item.crafting;
        if (crafting) {
            this.lblCraftAmount.innerHTML = crafting.amount;

            for (let ingredient of crafting.ingredients) {
                let p = document.createElement("p");

                let item = ingredient.item;
                p.innerHTML = `${ingredient.amount} x <a href='#' data-item-id='${item.id}'>${item.name}</a>`;

                this.lstIngredients.append(p);
            }
        } else {
            this.lblCraftAmount.innerHTML = "0";
        }
        
    }

    _onCategoryLinkClick(e) {
        event.preventDefault();

        let target = event.currentTarget;
        let groupId = target.getAttribute("data-group-id");
        let categoryId = target.getAttribute("data-category-id");

        this._selectGroupAndCategory(groupId, categoryId);
    }

    _initWithAllSelectOptions(cbo) {
        cbo.innerHTML = "";

        this._addSelectOption(cbo, "", "All");
        this._addSelectOption(cbo, "", "───────────", true);
    }

    _addSelectOption(cbo, value, text, disabled) {
        let opt = document.createElement("option");
        opt.value = value;
        opt.innerHTML = text;
        opt.disabled = disabled;

        cbo.append(opt);
    }

    _selectGroupAndCategory(groupId, categoryId) {
        if (groupId === null) {
            let category = this.gameData.techCategories[categoryId];
            let group = category.parent;

            this.cboGroups.querySelector(`[value='${group.id}']`)
                .selected = true;
            this._showCategories();

            this.cboCategories.querySelector(`[value='${categoryId}']`)
                .selected = true;
            this._showItems();
        } else {
            this.cboGroups.querySelector(`[value='${groupId}']`)
                .selected = true;
            this.cboCategories.querySelector(`[value='']`)
                .selected = true;

            this._showCategories();
        }
    }

}

(async function () {
    let page = window.page = new ItemPage();
    await page.initializeAsync();
})();