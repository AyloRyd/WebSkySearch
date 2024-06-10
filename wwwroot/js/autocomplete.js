document.addEventListener("DOMContentLoaded", function () {
    function setupAutocomplete(inputId) {
        const input = document.getElementById(inputId);
        const container = document.createElement("div");
        container.setAttribute("id", inputId + "-autocomplete-list");
        container.setAttribute("class", "autocomplete-items");
        input.parentNode.appendChild(container);

        input.addEventListener("input", function () {
            const term = this.value;
            if (!term) {
                container.innerHTML = "";
                return;
            }

            fetch(`/Search/GetCities?term=${term}`)
                .then(response => response.json())
                .then(data => {
                    container.innerHTML = "";
                    data.forEach(city => {
                        const item = document.createElement("div");
                        item.classList.add("autocomplete-item");
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
    }

    setupAutocomplete("originCity");
    setupAutocomplete("destinationCity");
});
