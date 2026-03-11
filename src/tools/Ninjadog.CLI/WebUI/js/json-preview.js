/**
 * JSON Preview panel — live syntax-highlighted JSON output.
 */
const JsonPreview = (() => {
    function update(state) {
        const preview = document.getElementById('json-preview');
        const raw = document.getElementById('json-raw');
        const json = buildJson(state);

        // Update highlighted view
        preview.innerHTML = highlight(JSON.stringify(json, null, 2));

        // Update raw textarea if not focused
        if (document.activeElement !== raw) {
            raw.value = JSON.stringify(json, null, 2);
        }
    }

    function buildJson(state) {
        const result = {};
        if (state.config && Object.keys(state.config).length > 0) {
            result.config = state.config;
        }
        if (state.entities && Object.keys(state.entities).length > 0) {
            result.entities = cleanEntities(state.entities);
        }
        if (state.enums && Object.keys(state.enums).length > 0) {
            result.enums = state.enums;
        }
        return result;
    }

    function cleanEntities(entities) {
        const cleaned = {};
        for (const [name, entity] of Object.entries(entities)) {
            const e = {};
            if (entity.properties && Object.keys(entity.properties).length > 0) {
                e.properties = entity.properties;
            }
            if (entity.relationships && Object.keys(entity.relationships).length > 0) {
                e.relationships = entity.relationships;
            }
            if (entity.seedData && entity.seedData.length > 0) {
                e.seedData = entity.seedData;
            }
            cleaned[name] = e;
        }
        return cleaned;
    }

    function highlight(json) {
        return json
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"([^"]+)":/g, '<span class="json-key">"$1"</span>:')
            .replace(/: "([^"]*)"/g, ': <span class="json-string">"$1"</span>')
            .replace(/: (\d+)/g, ': <span class="json-number">$1</span>')
            .replace(/: (true|false)/g, ': <span class="json-bool">$1</span>')
            .replace(/: (null)/g, ': <span class="json-null">$1</span>');
    }

    function getJson(state) {
        return buildJson(state);
    }

    return { update, getJson };
})();
