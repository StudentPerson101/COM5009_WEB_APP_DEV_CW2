(() => {
    const root = document.documentElement;
    root.dataset.app = "movie-watchlist-tracker";

    document.addEventListener("click", (event) => {
        const trigger = event.target.closest("[data-confirm]");

        if (!(trigger instanceof HTMLElement)) {
            return;
        }

        const message = trigger.dataset.confirm;
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });

    document.querySelectorAll("[data-character-count-for]").forEach((counter) => {
        const targetId = counter.getAttribute("data-character-count-for");
        const target = targetId ? document.getElementById(targetId) : null;

        if (!(target instanceof HTMLTextAreaElement)) {
            return;
        }

        const maxLength = target.maxLength > 0 ? target.maxLength : 0;
        const updateCounter = () => {
            counter.textContent = maxLength
                ? `${target.value.length} / ${maxLength}`
                : `${target.value.length}`;
        };

        target.addEventListener("input", updateCounter);
        updateCounter();
    });

    document.querySelectorAll("[data-movie-autocomplete]").forEach((container) => {
        const input = container.querySelector("[data-movie-autocomplete-input]");
        const menu = container.querySelector("[data-movie-autocomplete-menu]");

        if (!(input instanceof HTMLInputElement) || !(menu instanceof HTMLElement)) {
            return;
        }

        const suggestionsUrl = input.dataset.suggestionsUrl;
        if (!suggestionsUrl) {
            return;
        }

        let debounceId = 0;
        let activeIndex = -1;
        let activeController = null;

        const getOptions = () => Array.from(menu.querySelectorAll("[data-movie-suggestion]"));

        const setExpanded = (isExpanded) => {
            input.setAttribute("aria-expanded", isExpanded ? "true" : "false");
            menu.hidden = !isExpanded;
        };

        const closeMenu = () => {
            activeIndex = -1;
            input.removeAttribute("aria-activedescendant");
            setExpanded(false);
        };

        const setActiveOption = (index) => {
            const options = getOptions();
            activeIndex = index;

            options.forEach((option, optionIndex) => {
                const isActive = optionIndex === activeIndex;
                option.classList.toggle("is-active", isActive);
                option.setAttribute("aria-selected", isActive ? "true" : "false");
            });

            const activeOption = options[activeIndex];
            if (activeOption instanceof HTMLElement) {
                input.setAttribute("aria-activedescendant", activeOption.id);
                activeOption.scrollIntoView({ block: "nearest" });
            }
            else {
                input.removeAttribute("aria-activedescendant");
            }
        };

        const selectSuggestion = (button) => {
            const title = button.dataset.movieTitle;
            if (!title) {
                return;
            }

            input.value = title;
            closeMenu();
            input.form?.requestSubmit();
        };

        const renderSuggestions = (suggestions) => {
            menu.replaceChildren();

            if (!Array.isArray(suggestions) || suggestions.length === 0) {
                closeMenu();
                return;
            }

            suggestions.forEach((suggestion, index) => {
                const button = document.createElement("button");
                const title = typeof suggestion.title === "string" ? suggestion.title : "";
                const releaseYear = Number.isInteger(suggestion.releaseYear)
                    ? ` ${suggestion.releaseYear}`
                    : "";

                button.id = `movie-title-suggestion-${index}`;
                button.className = "autocomplete-option";
                button.type = "button";
                button.role = "option";
                button.dataset.movieSuggestion = "true";
                button.dataset.movieTitle = title;
                button.setAttribute("aria-selected", "false");

                const titleText = document.createElement("span");
                titleText.className = "autocomplete-option-title";
                titleText.textContent = title;

                const metaText = document.createElement("span");
                metaText.className = "autocomplete-option-meta";
                metaText.textContent = releaseYear.trim();

                button.append(titleText, metaText);
                menu.append(button);
            });

            setExpanded(true);
            setActiveOption(-1);
        };

        const fetchSuggestions = async () => {
            const query = input.value.trim();

            if (query.length < 2) {
                closeMenu();
                return;
            }

            if (activeController) {
                activeController.abort();
            }

            activeController = new AbortController();

            try {
                const url = new URL(suggestionsUrl, window.location.origin);
                url.searchParams.set("query", query);

                const response = await fetch(url, {
                    headers: { Accept: "application/json" },
                    signal: activeController.signal
                });

                if (!response.ok) {
                    closeMenu();
                    return;
                }

                renderSuggestions(await response.json());
            }
            catch (error) {
                if (error.name !== "AbortError") {
                    closeMenu();
                }
            }
        };

        input.addEventListener("input", () => {
            window.clearTimeout(debounceId);
            debounceId = window.setTimeout(fetchSuggestions, 180);
        });

        input.addEventListener("keydown", (event) => {
            const options = getOptions();

            if (event.key === "Escape") {
                closeMenu();
                return;
            }

            if (menu.hidden || options.length === 0) {
                return;
            }

            if (event.key === "ArrowDown") {
                event.preventDefault();
                setActiveOption(activeIndex >= options.length - 1 ? 0 : activeIndex + 1);
            }

            if (event.key === "ArrowUp") {
                event.preventDefault();
                setActiveOption(activeIndex <= 0 ? options.length - 1 : activeIndex - 1);
            }

            if (event.key === "Enter" && activeIndex >= 0) {
                event.preventDefault();
                const selectedOption = options[activeIndex];
                if (selectedOption instanceof HTMLButtonElement) {
                    selectSuggestion(selectedOption);
                }
            }
        });

        menu.addEventListener("pointerdown", (event) => {
            const suggestion = event.target.closest("[data-movie-suggestion]");
            if (suggestion instanceof HTMLButtonElement) {
                event.preventDefault();
                selectSuggestion(suggestion);
            }
        });

        document.addEventListener("click", (event) => {
            if (!container.contains(event.target)) {
                closeMenu();
            }
        });
    });
})();
