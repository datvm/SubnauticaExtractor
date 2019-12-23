import { Common } from "./common.js";
import { GameData } from "./game-data.js";

class CalculatorPage {

    ready = false;
    selectedItems = [];

    lstItems = document.querySelector("#lst-items");
    lstSelectedItems = document.querySelector("#lst-selected-items");
    txtSearch = document.querySelector("#txt-search");
    lstMaterials = document.querySelector("#lst-materials");

    templateSelectItem = document.querySelector("#template-select-item").innerHTML;
    templateMaterialItem = document.querySelector("#template-material").innerHTML;

    async initializeAsync() {
        this.gameData = new GameData();
        await this.gameData.loadAsync();

        this._showItems();

        this.ready = true;
        Common.toggleLoading(false);
        this._addEventListeners();
    }

    _addEventListeners() {
        this.txtSearch.addEventListener("input",
            () => this._showItems());

        this.lstItems.addEventDelegate("click", "[data-id]",
            (e, target) => this._onItemSelected(e, target));

        this.lstSelectedItems.addEventDelegate("click", ".btn-delete",
            (e, target) => this._onDeleteButtonClicked(e, target));
    }

    _onItemSelected(e, target) {
        let id = target.getAttribute("data-id");
        let item = this.gameData.getItemDetails(id);

        let el = Common.fromTemplate(this.templateSelectItem);

        var amount = 1;

        if (item.crafting) {
            amount = item.crafting.amount;
        }
        el.querySelector("[data-name]").innerHTML = `${item.name} x${amount}`;

        el.setAttribute("data-id", id);
        el.setAttribute("data-index", this.selectedItems.length);
        this.lstSelectedItems.append(el);

        this.selectedItems.push(item);

        this._calculateMaterials();
    }

    _onDeleteButtonClicked(e, target) {
        let el = target.closest("[data-id]");
        let index = el.getAttribute("data-index");

        this.selectedItems.splice(Number(index), 1);
        el.remove();

        let counter = 0;
        for (let child of this.lstSelectedItems.children) {
            child.setAttribute("data-index", counter++);
        }

        this._calculateMaterials();
    }

    _showItems() {
        let filter = this.txtSearch.value.toLowerCase();

        this.lstItems.innerHTML = "";

        let items = this.gameData.getItems();
        for (let item of items) {
            let name = item.name;
            if (filter && !name.toLowerCase().includes(filter)) {
                continue;
            }

            let el = document.createElement("div");
            el.className = "item";
            el.innerHTML = item.name;
            el.setAttribute("data-id", item.id);

            this.lstItems.append(el);
        }
    }

    _calculateMaterials() {
        var materials = {};

        for (let item of this.selectedItems) {
            this._calculateMaterialsForItem(item, 1, materials);
        }

        this.lstMaterials.innerHTML = "";
        for (let material in materials) {
            let count = materials[material];

            let el = Common.fromTemplate(this.templateMaterialItem);
            el.querySelector("[data-name]").innerHTML = material;
            el.querySelector("[data-quantity]").innerHTML = count;

            this.lstMaterials.append(el);
        }
    }

    _calculateMaterialsForItem(item, count, materials) {
        if (item.crafting) {
            for (let ingredient of item.crafting.ingredients) {
                this._calculateMaterialsForItem(ingredient.item, ingredient.amount * count, materials);
            }
        } else {
            materials[item.name] = (materials[item.name] || 0) + count;
        }
    }

}

(async function () {
    let page = window.page = new CalculatorPage();
    await page.initializeAsync();
})();