/**
 * Seed Data tab — per-entity seed data table editor.
 * Supports auto-generation of key values (Guid or int/long).
 */
const SeedEditor = (() => {
    /** Find the key property name and type for an entity, or null if none. */
    function findKeyProp(entity) {
        const props = entity.properties || {};
        for (const [name, def] of Object.entries(props)) {
            if (def.isKey) return { name, type: def.type };
        }
        return null;
    }

    /** Generate a RFC-4122 v4 GUID. */
    function generateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    }

    /** Next integer key: max existing + 1 (starts at 1). */
    function nextIntKey(entity, keyName) {
        const rows = entity.seedData || [];
        let max = 0;
        rows.forEach(row => {
            const v = Number(row[keyName]);
            if (!isNaN(v) && v > max) max = v;
        });
        return max + 1;
    }

    /** Generate a key value based on property type. */
    function generateKeyValue(entity, keyProp) {
        const t = (keyProp.type || '').toLowerCase();
        if (t === 'guid') return generateGuid();
        if (t === 'int' || t === 'long') return nextIntKey(entity, keyProp.name);
        return '';
    }

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
        const keyProp = findKeyProp(entity);
        const canAutoKey = keyProp && ['guid', 'int', 'long'].includes((keyProp.type || '').toLowerCase());

        return `
        <div class="entity-card" data-seed-entity="${esc(name)}">
            <div class="entity-header">
                <span class="font-medium text-sm">${esc(name)}</span>
                <div class="flex items-center gap-2">
                    <span class="text-xs text-gray-500">${seedData.length} rows</span>
                    ${canAutoKey && seedData.length > 0 ? `<button class="btn-sm btn-ghost seed-fill-keys" data-entity="${esc(name)}" title="Auto-fill empty ${esc(keyProp.name)} values">Fill Keys</button>` : ''}
                    <button class="btn-sm btn-ghost seed-add-row" data-entity="${esc(name)}">+ Row</button>
                </div>
            </div>
            ${seedData.length > 0 || props.length > 0 ? `
            <div class="entity-body overflow-x-auto">
                <table class="data-table">
                    <thead><tr>
                        ${props.map(p => {
                            const isKey = entity.properties[p] && entity.properties[p].isKey;
                            return `<th>${esc(p)}${isKey ? ' <span class="text-yellow-500" title="Key field">&#x1f511;</span>' : ''}</th>`;
                        }).join('')}
                        <th></th>
                    </tr></thead>
                    <tbody>
                        ${seedData.map((row, i) => `<tr data-entity="${esc(name)}" data-row="${i}">
                            ${props.map(p => {
                                const isKey = entity.properties[p] && entity.properties[p].isKey;
                                const canGen = isKey && ['guid', 'int', 'long'].includes((entity.properties[p].type || '').toLowerCase());
                                return `<td class="relative">`
                                    + `<input class="field-input py-1 text-xs seed-field${isKey ? ' font-mono' : ''}" data-prop="${esc(p)}" value="${esc(row[p] ?? '')}" />`
                                    + (canGen ? `<button class="seed-gen-key absolute right-1 top-1/2 -translate-y-1/2 text-xs text-gray-400 hover:text-blue-500 cursor-pointer" data-entity="${esc(name)}" data-row="${i}" title="Generate ${esc(entity.properties[p].type)} key">&#x21bb;</button>` : '')
                                    + `</td>`;
                            }).join('')}
                            <td><button class="btn-sm btn-danger seed-remove-row" data-entity="${esc(name)}" data-row="${i}">X</button></td>
                        </tr>`).join('')}
                    </tbody>
                </table>
            </div>` : '<div class="entity-body"><p class="text-xs text-gray-500">No properties defined for this entity.</p></div>'}
        </div>`;
    }

    function bindEvents(container, state) {
        // Add row — auto-generate key value
        container.querySelectorAll('.seed-add-row').forEach(btn => {
            btn.addEventListener('click', () => {
                const entity = btn.dataset.entity;
                const ent = state.entities[entity];
                ent.seedData = ent.seedData || [];
                const row = {};
                Object.keys(ent.properties || {}).forEach(p => { row[p] = ''; });

                // Auto-fill key
                const keyProp = findKeyProp(ent);
                if (keyProp && ['guid', 'int', 'long'].includes((keyProp.type || '').toLowerCase())) {
                    row[keyProp.name] = generateKeyValue(ent, keyProp);
                }

                ent.seedData.push(row);
                render(container, state);
                App.onStateChanged();
            });
        });

        // Generate single key
        container.querySelectorAll('.seed-gen-key').forEach(btn => {
            btn.addEventListener('click', () => {
                const entity = btn.dataset.entity;
                const idx = parseInt(btn.dataset.row, 10);
                const ent = state.entities[entity];
                const keyProp = findKeyProp(ent);
                if (!keyProp) return;
                ent.seedData[idx][keyProp.name] = generateKeyValue(ent, keyProp);
                render(container, state);
                App.onStateChanged();
            });
        });

        // Fill all empty keys
        container.querySelectorAll('.seed-fill-keys').forEach(btn => {
            btn.addEventListener('click', () => {
                const entity = btn.dataset.entity;
                const ent = state.entities[entity];
                const keyProp = findKeyProp(ent);
                if (!keyProp || !ent.seedData) return;
                ent.seedData.forEach(row => {
                    const val = row[keyProp.name];
                    if (val === '' || val === undefined || val === null) {
                        row[keyProp.name] = generateKeyValue(ent, keyProp);
                    }
                });
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
