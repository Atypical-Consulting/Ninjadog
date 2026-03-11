/**
 * Seed Data tab — per-entity seed data table editor.
 */
const SeedEditor = (() => {
    function render(container, state) {
        state.entities = state.entities || {};
        const entityNames = Object.keys(state.entities);

        if (entityNames.length === 0) {
            container.innerHTML = '<p class="text-sm text-gray-500">No entities defined. Add entities first.</p>';
            return;
        }

        container.innerHTML = `
            <div class="section-title">Seed Data</div>
            ${entityNames.map(name => seedEntityCard(name, state.entities[name])).join('')}
        `;

        bindEvents(container, state);
    }

    function seedEntityCard(name, entity) {
        const props = Object.keys(entity.properties || {});
        const seedData = entity.seedData || [];

        return `
        <div class="entity-card" data-seed-entity="${esc(name)}">
            <div class="entity-header">
                <span class="font-medium text-sm">${esc(name)}</span>
                <div class="flex items-center gap-2">
                    <span class="text-xs text-gray-500">${seedData.length} rows</span>
                    <button class="btn-sm btn-ghost seed-add-row" data-entity="${esc(name)}">+ Row</button>
                </div>
            </div>
            ${seedData.length > 0 || props.length > 0 ? `
            <div class="entity-body overflow-x-auto">
                <table class="data-table">
                    <thead><tr>
                        ${props.map(p => `<th>${esc(p)}</th>`).join('')}
                        <th></th>
                    </tr></thead>
                    <tbody>
                        ${seedData.map((row, i) => `<tr data-entity="${esc(name)}" data-row="${i}">
                            ${props.map(p => `<td><input class="field-input py-1 text-xs seed-field" data-prop="${esc(p)}" value="${esc(row[p] ?? '')}" /></td>`).join('')}
                            <td><button class="btn-sm btn-danger seed-remove-row" data-entity="${esc(name)}" data-row="${i}">X</button></td>
                        </tr>`).join('')}
                    </tbody>
                </table>
            </div>` : '<div class="entity-body"><p class="text-xs text-gray-500">No properties defined for this entity.</p></div>'}
        </div>`;
    }

    function bindEvents(container, state) {
        // Add row
        container.querySelectorAll('.seed-add-row').forEach(btn => {
            btn.addEventListener('click', () => {
                const entity = btn.dataset.entity;
                state.entities[entity].seedData = state.entities[entity].seedData || [];
                const row = {};
                Object.keys(state.entities[entity].properties || {}).forEach(p => { row[p] = ''; });
                state.entities[entity].seedData.push(row);
                render(container, state);
                App.onStateChanged();
            });
        });

        // Remove row
        container.querySelectorAll('.seed-remove-row').forEach(btn => {
            btn.addEventListener('click', () => {
                const entity = btn.dataset.entity;
                const idx = parseInt(btn.dataset.row, 10);
                state.entities[entity].seedData.splice(idx, 1);
                if (state.entities[entity].seedData.length === 0) {
                    delete state.entities[entity].seedData;
                }
                render(container, state);
                App.onStateChanged();
            });
        });

        // Field changes
        container.querySelectorAll('.seed-field').forEach(el => {
            el.addEventListener('input', () => collectSeedData(container, state));
        });
    }

    function collectSeedData(container, state) {
        container.querySelectorAll('tr[data-entity][data-row]').forEach(tr => {
            const entity = tr.dataset.entity;
            const idx = parseInt(tr.dataset.row, 10);
            const row = {};
            tr.querySelectorAll('.seed-field').forEach(f => {
                const v = f.value;
                const prop = f.dataset.prop;
                // Try to parse as number or boolean
                if (v === 'true') row[prop] = true;
                else if (v === 'false') row[prop] = false;
                else if (v !== '' && !isNaN(Number(v))) row[prop] = Number(v);
                else row[prop] = v;
            });
            state.entities[entity].seedData = state.entities[entity].seedData || [];
            state.entities[entity].seedData[idx] = row;
        });
        App.onStateChanged();
    }

    function esc(s) {
        return String(s).replace(/&/g,'&amp;').replace(/"/g,'&quot;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    return { render };
})();
