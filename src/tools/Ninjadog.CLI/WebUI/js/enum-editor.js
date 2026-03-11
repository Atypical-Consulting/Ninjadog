/**
 * Enums tab — list of enum definitions with value management.
 */
const EnumEditor = (() => {
    function render(container, state) {
        state.enums = state.enums || {};
        const names = Object.keys(state.enums);

        container.innerHTML = `
            <div class="flex items-center justify-between mb-4">
                <div class="section-title mb-0">Enums (${names.length})</div>
                <button id="btn-add-enum" class="btn-sm btn-primary">+ Add Enum</button>
            </div>
            <div id="enum-list">${names.map(n => enumCard(n, state.enums[n])).join('')}</div>
        `;

        container.querySelector('#btn-add-enum').addEventListener('click', () => {
            const name = prompt('Enum name (PascalCase):');
            if (!name) return;
            state.enums[name] = [];
            render(container, state);
            App.onStateChanged();
        });

        bindEvents(container, state);
    }

    function enumCard(name, values) {
        return `
        <div class="entity-card" data-enum="${esc(name)}">
            <div class="entity-header">
                <span class="font-medium text-sm">${esc(name)}</span>
                <div class="flex items-center gap-2">
                    <span class="text-xs text-gray-500">${values.length} values</span>
                    <button class="btn-sm btn-danger enum-remove" data-enum="${esc(name)}">Remove</button>
                </div>
            </div>
            <div class="entity-body">
                <div class="flex flex-wrap gap-2 mb-2">
                    ${values.map((v, i) => `
                        <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-gray-700 rounded text-xs">
                            ${esc(v)}
                            <button class="text-gray-400 hover:text-red-400 enum-val-remove" data-enum="${esc(name)}" data-index="${i}">&times;</button>
                        </span>
                    `).join('')}
                </div>
                <div class="flex items-center gap-2">
                    <input class="field-input text-xs py-1 flex-1 enum-val-input" data-enum="${esc(name)}" placeholder="New value..." />
                    <button class="btn-sm btn-ghost enum-val-add" data-enum="${esc(name)}">Add</button>
                </div>
            </div>
        </div>`;
    }

    function bindEvents(container, state) {
        // Remove enum
        container.querySelectorAll('.enum-remove').forEach(btn => {
            btn.addEventListener('click', e => {
                e.stopPropagation();
                const name = btn.dataset.enum;
                if (confirm(`Remove enum "${name}"?`)) {
                    delete state.enums[name];
                    if (Object.keys(state.enums).length === 0) delete state.enums;
                    render(container, state);
                    App.onStateChanged();
                }
            });
        });

        // Remove value
        container.querySelectorAll('.enum-val-remove').forEach(btn => {
            btn.addEventListener('click', () => {
                const name = btn.dataset.enum;
                const idx = parseInt(btn.dataset.index, 10);
                state.enums[name].splice(idx, 1);
                render(container, state);
                App.onStateChanged();
            });
        });

        // Add value
        container.querySelectorAll('.enum-val-add').forEach(btn => {
            btn.addEventListener('click', () => addValue(btn.dataset.enum, container, state));
        });

        container.querySelectorAll('.enum-val-input').forEach(input => {
            input.addEventListener('keydown', e => {
                if (e.key === 'Enter') addValue(input.dataset.enum, container, state);
            });
        });
    }

    function addValue(enumName, container, state) {
        const input = container.querySelector(`.enum-val-input[data-enum="${enumName}"]`);
        const val = input.value.trim();
        if (!val) return;
        state.enums[enumName].push(val);
        render(container, state);
        App.onStateChanged();
    }

    function esc(s) {
        return String(s).replace(/&/g,'&amp;').replace(/"/g,'&quot;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    return { render };
})();
