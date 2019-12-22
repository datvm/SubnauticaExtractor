export class Common {

    static toggleLoading(show) {
        document.querySelector("#loader").classList.toggle("d-none", !show);

        document.querySelectorAll("[data-show-when-loaded]")
            .forEach(el => el.classList.toggle("d-none", show));
    }

}

Element.prototype.addEventDelegate = function (eventName, cssMatch, callback) {
    this.addEventListener(eventName, function(e) {
        for (let target = e.target; target && target != this; target = target.parentNode) {
            if (target.matches(cssMatch)) {
                callback(e, target);

                break;
            }
        }
    });
}