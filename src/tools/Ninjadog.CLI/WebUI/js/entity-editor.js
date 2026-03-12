/**
 * Entities tab — list of entities with expandable property/relationship editors.
 * No modals: uses inline forms for add/remove. Property renames propagate to seed data.
 */
const EntityEditor = (() => {
    const propertyTypes = [
        'string', 'int', 'long', 'decimal', 'double', 'float',
        'bool', 'DateTime', 'DateOnly', 'TimeOnly', 'Guid'
    ];
    const relationshipTypes = ['hasMany', 'hasOne', 'belongsTo'];

    function render(container, state) {
        state.entities = state.entities || {};
        const names = Object.keys(state.entities);

        container.innerHTML = `
            <div class="flex items-center justify-between mb-4">
                <div class="section-title mb-0">Entities (${names.length})</div>
                <div id="entity-add-area">
                    <button id="btn-add-entity" class="btn-sm btn-primary">+ Add Entity</button>
                    <div id="entity-add-form" class="inline-add-form hidden">
                        <input id="entity-add-input" class="field-input text-sm py-1" style="width:200px" placeholder="Entity name (PascalCase)" />
                        <button id="entity-add-confirm" class="btn-sm btn-primary">Create</button>
                        <button id="entity-add-cancel" class="btn-sm btn-ghost">Cancel</button>
                    </div>
                </div>
            </div>
            <div id="entity-list">${names.map(n => entityCard(n, state.entities[n], state)).join('')}</div>
        `;

        // Inline add entity
        const addBtn = container.querySelector('#btn-add-entity');
        const addForm = container.querySelector('#entity-add-form');
        const addInput = container.querySelector('#entity-add-input');
        const addConfirm = container.querySelector('#entity-add-confirm');
        const addCancel = container.querySelector('#entity-add-cancel');

        addBtn.addEventListener('click', () => {
            addBtn.classList.add('hidden');
            addForm.classList.remove('hidden');
            addInput.value = '';
            addInput.focus();
        });

        addConfirm.addEventListener('click', () => {
            const name = addInput.value.trim();
            if (!name) return;
            state.entities[name] = { properties: {} };
            render(container, state);
            App.onStateChanged();
        });

        addCancel.addEventListener('click', () => {
            addForm.classList.add('hidden');
            addBtn.classList.remove('hidden');
        });

        addInput.addEventListener('keydown', e => {
            if (e.key === 'Enter') addConfirm.click();
            if (e.key === 'Escape') addCancel.click();
        });

        // Bind all entity interactions
        bindEntityEvents(container, state);
    }

    function entityCard(name, entity, state) {
        const props = entity.properties || {};
        const rels = entity.relationships || {};
        const propNames = Object.keys(props);
        const relNames = Object.keys(rels);

        return `
        <div class="entity-card" data-entity="${esc(name)}">
            <div class="entity-header">
                <span class="font-medium text-sm">${esc(name)}</span>
                <div class="flex items-center gap-2">
                    <span class="text-xs text-gray-500">${propNames.length} props</span>
                    <button class="btn-sm btn-danger entity-remove" data-entity="${esc(name)}" data-confirmed="false">Remove</button>
                </div>
            </div>
            <div class="entity-body">
                <div class="mb-3">
                    <div class="flex items-center justify-between mb-2">
                        <span class="text-xs font-medium text-gray-400">Properties</span>
                        <div class="prop-add-area" data-entity="${esc(name)}">
                            <button class="btn-sm btn-ghost prop-add-btn" data-entity="${esc(name)}">+ Property</button>
                            <div class="inline-add-form hidden prop-add-form">
                                <input class="field-input text-xs py-1 prop-add-input" style="width:150px" placeholder="Property name (camelCase)" data-entity="${esc(name)}" />
                                <button class="btn-sm btn-primary prop-add-confirm" data-entity="${esc(name)}">Add</button>
                                <button class="btn-sm btn-ghost prop-add-cancel" data-entity="${esc(name)}">Cancel</button>
                            </div>
                        </div>
                    </div>
                    ${propNames.length > 0 ? `
                    <table class="data-table">
                        <thead><tr>
                            <th>Name</th><th>Type</th><th>Key</th><th>Required</th>
                            <th>MaxLen</th><th>MinLen</th><th>Min</th><th>Max</th><th>Pattern</th><th></th>
                        </tr></thead>
                        <tbody>
                            ${propNames.map(p => propRow(name, p, props[p])).join('')}
                        </tbody>
                    </table>` : '<p class="text-xs text-gray-500">No properties defined.</p>'}
                </div>

                <div>
                    <div class="flex items-center justify-between mb-2">
                        <span class="text-xs font-medium text-gray-400">Relationships</span>
                        <div class="rel-add-area" data-entity="${esc(name)}">
                            <button class="btn-sm btn-ghost rel-add-btn" data-entity="${esc(name)}">+ Relationship</button>
                            <div class="inline-add-form hidden rel-add-form">
                                <input class="field-input text-xs py-1 rel-add-input" style="width:150px" placeholder="Relationship name" data-entity="${esc(name)}" />
                                <button class="btn-sm btn-primary rel-add-confirm" data-entity="${esc(name)}">Add</button>
                                <button class="btn-sm btn-ghost rel-add-cancel" data-entity="${esc(name)}">Cancel</button>
                            </div>
                        </div>
                    </div>
                    ${relNames.length > 0 ? `
                    <table class="data-table">
                        <thead><tr><th>Name</th><th>Type</th><th>Target Entity</th><th>Foreign Key</th><th></th></tr></thead>
                        <tbody>
                            ${relNames.map(r => relRow(name, r, rels[r])).join('')}
                        </tbody>
                    </table>` : '<p class="text-xs text-gray-500">No relationships defined.</p>'}
                </div>
            </div>
        </div>`;
    }

    function propRow(entity, propName, prop) {
        return `<tr data-entity="${esc(entity)}" data-prop="${esc(propName)}">
            <td><input class="field-input py-1 text-xs prop-field" data-key="name" value="${esc(propName)}" /></td>
            <td><select class="field-select py-1 text-xs prop-field" data-key="type">
                ${propertyTypes.map(t => `<option value="${t}" ${prop.type === t ? 'selected' : ''}>${t}</option>`).join('')}
            </select></td>
            <td class="text-center"><input type="checkbox" class="field-checkbox prop-field" data-key="isKey" ${prop.isKey ? 'checked' : ''} /></td>
            <td class="text-center"><input type="checkbox" class="field-checkbox prop-field" data-key="required" ${prop.required ? 'checked' : ''} /></td>
            <td><input class="field-input py-1 text-xs w-16 prop-field" data-key="maxLength" type="number" value="${prop.maxLength ?? ''}" /></td>
            <td><input class="field-input py-1 text-xs w-16 prop-field" data-key="minLength" type="number" value="${prop.minLength ?? ''}" /></td>
            <td><input class="field-input py-1 text-xs w-16 prop-field" data-key="min" type="number" value="${prop.min ?? ''}" /></td>
            <td><input class="field-input py-1 text-xs w-16 prop-field" data-key="max" type="number" value="${prop.max ?? ''}" /></td>
            <td><input class="field-input py-1 text-xs w-20 prop-field" data-key="pattern" value="${esc(prop.pattern || '')}" /></td>
            <td><button class="btn-sm btn-danger prop-remove">X</button></td>
        </tr>`;
    }

    function relRow(entity, relName, rel) {
        return `<tr data-entity="${esc(entity)}" data-rel="${esc(relName)}">
            <td><input class="field-input py-1 text-xs rel-field" data-key="name" value="${esc(relName)}" /></td>
            <td><select class="field-select py-1 text-xs rel-field" data-key="type">
                ${relationshipTypes.map(t => `<option value="${t}" ${rel.type === t ? 'selected' : ''}>${t}</option>`).join('')}
            </select></td>
            <td><input class="field-input py-1 text-xs rel-field" data-key="targetEntity" value="${esc(rel.targetEntity || '')}" /></td>
            <td><input class="field-input py-1 text-xs rel-field" data-key="foreignKey" value="${esc(rel.foreignKey || '')}" /></td>
            <td><button class="btn-sm btn-danger rel-remove">X</button></td>
        </tr>`;
    }

    function bindEntityEvents(container, state) {
        // Remove entity — two-click confirmation (no modal)
        container.querySelectorAll('.entity-remove').forEach(btn => {
            let confirmTimer = null;
            btn.addEventListener('click', e => {
                e.stopPropagation();
                const name = btn.dataset.entity;
                if (btn.dataset.confirmed === 'true') {
                    // Second click — actually remove
                    clearTimeout(confirmTimer);
                    delete state.entities[name];
                    render(container, state);
                    App.onStateChanged();
                } else {
                    // First click — ask confirmation
                    btn.dataset.confirmed = 'true';
                    btn.textContent = 'Sure?';
                    btn.classList.remove('btn-danger');
                    btn.classList.add('btn-confirm-danger');
                    confirmTimer = setTimeout(() => {
                        btn.dataset.confirmed = 'false';
                        btn.textContent = 'Remove';
                        btn.classList.add('btn-danger');
                        btn.classList.remove('btn-confirm-danger');
                    }, 3000);
                }
            });
        });

        // Inline add property
        container.querySelectorAll('.prop-add-btn').forEach(btn => {
            const entity = btn.dataset.entity;
            const area = btn.closest('.prop-add-area');
            const form = area.querySelector('.prop-add-form');
            const input = area.querySelector('.prop-add-input');
            const confirm = area.querySelector('.prop-add-confirm');
            const cancel = area.querySelector('.prop-add-cancel');

            btn.addEventListener('click', () => {
                btn.classList.add('hidden');
                form.classList.remove('hidden');
                input.value = '';
                input.focus();
            });

            confirm.addEventListener('click', () => {
                const pname = input.value.trim();
                if (!pname) return;
                state.entities[entity].properties = state.entities[entity].properties || {};
                state.entities[entity].properties[pname] = { type: 'string' };
                render(container, state);
                App.onStateChanged();
            });

            cancel.addEventListener('click', () => {
                form.classList.add('hidden');
                btn.classList.remove('hidden');
            });

            input.addEventListener('keydown', e => {
                if (e.key === 'Enter') confirm.click();
                if (e.key === 'Escape') cancel.click();
            });
        });

        // Remove property
        container.querySelectorAll('.prop-remove').forEach(btn => {
            btn.addEventListener('click', () => {
                const tr = btn.closest('tr');
                const entity = tr.dataset.entity;
                const prop = tr.dataset.prop;

                // Also remove from seed data
                if (state.entities[entity].seedData) {
                    state.entities[entity].seedData.forEach(row => {
                        delete row[prop];
                    });
                }

                delete state.entities[entity].properties[prop];
                render(container, state);
                App.onStateChanged();
            });
        });

        // Property field changes
        container.querySelectorAll('.prop-field').forEach(el => {
            const event = el.type === 'checkbox' ? 'change' : 'input';
            el.addEventListener(event, () => collectProperties(container, state));
        });

        // Inline add relationship
        container.querySelectorAll('.rel-add-btn').forEach(btn => {
            const entity = btn.dataset.entity;
            const area = btn.closest('.rel-add-area');
            const form = area.querySelector('.rel-add-form');
            const input = area.querySelector('.rel-add-input');
            const confirm = area.querySelector('.rel-add-confirm');
            const cancel = area.querySelector('.rel-add-cancel');

            btn.addEventListener('click', () => {
                btn.classList.add('hidden');
                form.classList.remove('hidden');
                input.value = '';
                input.focus();
            });

            confirm.addEventListener('click', () => {
                const rname = input.value.trim();
                if (!rname) return;
                state.entities[entity].relationships = state.entities[entity].relationships || {};
                state.entities[entity].relationships[rname] = { type: 'hasMany', targetEntity: '' };
                render(container, state);
                App.onStateChanged();
            });

            cancel.addEventListener('click', () => {
                form.classList.add('hidden');
                btn.classList.remove('hidden');
            });

            input.addEventListener('keydown', e => {
                if (e.key === 'Enter') confirm.click();
                if (e.key === 'Escape') cancel.click();
            });
        });

        // Remove relationship
        container.querySelectorAll('.rel-remove').forEach(btn => {
            btn.addEventListener('click', () => {
                const tr = btn.closest('tr');
                const entity = tr.dataset.entity;
                const rel = tr.dataset.rel;
                delete state.entities[entity].relationships[rel];
                if (Object.keys(state.entities[entity].relationships).length === 0) {
                    delete state.entities[entity].relationships;
                }
                render(container, state);
                App.onStateChanged();
            });
        });

        // Relationship field changes
        container.querySelectorAll('.rel-field').forEach(el => {
            el.addEventListener('input', () => collectRelationships(container, state));
        });
    }

    function collectProperties(container, state) {
        container.querySelectorAll('tr[data-entity][data-prop]').forEach(tr => {
            const entity = tr.dataset.entity;
            const origName = tr.dataset.prop;
            const fields = tr.querySelectorAll('.prop-field');
            let newName = origName;
            const prop = {};

            fields.forEach(f => {
                const key = f.dataset.key;
                if (key === 'name') {
                    newName = f.value.trim() || origName;
                } else if (f.type === 'checkbox') {
                    if (f.checked) prop[key] = true;
                } else if (f.type === 'number') {
                    const v = f.value.trim();
                    if (v !== '') prop[key] = parseInt(v, 10);
                } else {
                    const v = f.value.trim();
                    if (v) prop[key] = v;
                }
            });

            if (!prop.type) prop.type = 'string';

            // Handle rename — also propagate to seed data
            if (newName !== origName) {
                delete state.entities[entity].properties[origName];

                // Update seed data keys
                if (state.entities[entity].seedData) {
                    state.entities[entity].seedData.forEach(row => {
                        if (origName in row) {
                            row[newName] = row[origName];
                            delete row[origName];
                        }
                    });
                }
            }
            state.entities[entity].properties[newName] = prop;
        });

        App.onStateChanged();
    }

    function collectRelationships(container, state) {
        container.querySelectorAll('tr[data-entity][data-rel]').forEach(tr => {
            const entity = tr.dataset.entity;
            const origName = tr.dataset.rel;
            const fields = tr.querySelectorAll('.rel-field');
            let newName = origName;
            const rel = {};

            fields.forEach(f => {
                const key = f.dataset.key;
                if (key === 'name') {
                    newName = f.value.trim() || origName;
                } else {
                    const v = f.value.trim();
                    if (v) rel[key] = v;
                }
            });

            if (!rel.type) rel.type = 'hasMany';

            if (newName !== origName) {
                delete state.entities[entity].relationships[origName];
            }
            state.entities[entity].relationships = state.entities[entity].relationships || {};
            state.entities[entity].relationships[newName] = rel;
        });

        App.onStateChanged();
    }

    function esc(s) {
        return String(s).replace(/&/g,'&amp;').replace(/"/g,'&quot;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    return { render };
})();
