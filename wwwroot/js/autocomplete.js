document.addEventListener("DOMContentLoaded", function () {
    autocomplete("originCity");
    autocomplete("destinationCity");
});

function autocomplete(inputId) {
    const input = document.getElementById(inputId);
    const container = document.createElement("div");
    container.id = `${inputId}-autocomplete-list`;
    container.className = "autocomplete-items";
    input.parentNode.appendChild(container);

    input.addEventListener("input", function () {
        const term = this.value.trim();
        if (!term) {
            container.innerHTML = "";
            return;
        }

        fetch(`/Search/GetCities?term=${encodeURIComponent(term)}`)
            .then(response => response.json())
            .then(data => {
                container.innerHTML = "";
                if (data.length === 1) {
                    input.dataset.singleOption = data[0];
                } else {
                    delete input.dataset.singleOption;
                }
                data.forEach(city => {
                    const item = document.createElement("div");
                    item.className = "autocomplete-item";
                    item.textContent = city;
                    item.addEventListener("click", function () {
                        input.value = city;
                        container.innerHTML = "";
                    });
                    container.appendChild(item);
                });
            });
    });

    document.addEventListener("click", function (e) {
        if (e.target !== input) {
            container.innerHTML = "";
        }
    });

    input.addEventListener("keydown", function (e) {
        if (e.key === "Tab" && input.dataset.singleOption) {
            input.value = input.dataset.singleOption;
            container.innerHTML = "";
        }
    });
}