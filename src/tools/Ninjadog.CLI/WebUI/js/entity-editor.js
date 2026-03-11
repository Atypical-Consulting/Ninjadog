/**
 * Entities tab — list of entities with expandable property/relationship editors.
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
                <button id="btn-add-entity" class="btn-sm btn-primary">+ Add Entity</button>
            </div>
            <div id="entity-list">${names.map(n => entityCard(n, state.entities[n], state)).join('')}</div>
        `;

        container.querySelector('#btn-add-entity').addEventListener('click', () => {
            const name = prompt('Entity name (PascalCase):');
            if (!name) return;
            state.entities[name] = { properties: {} };
            render(container, state);
            App.onStateChanged();
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
                    <button class="btn-sm btn-danger entity-remove" data-entity="${esc(name)}">Remove</button>
                </div>
            </div>
            <div class="entity-body">
                <div class="mb-3">
                    <div class="flex items-center justify-between mb-2">
                        <span class="text-xs font-medium text-gray-400">Properties</span>
                        <button class="btn-sm btn-ghost prop-add" data-entity="${esc(name)}">+ Property</button>
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
                        <button class="btn-sm btn-ghost rel-add" data-entity="${esc(name)}">+ Relationship</button>
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
        // Remove entity
        container.querySelectorAll('.entity-remove').forEach(btn => {
            btn.addEventListener('click', e => {
                e.stopPropagation();
                const name = btn.dataset.entity;
                if (confirm(`Remove entity "${name}"?`)) {
                    delete state.entities[name];
                    render(container, state);
                    App.onStateChanged();
                }
            });
        });

        // Add property
        container.querySelectorAll('.prop-add').forEach(btn => {
            btn.addEventListener('click', () => {
                const entity = btn.dataset.entity;
                const pname = prompt('Property name (camelCase):');
                if (!pname) return;
                state.entities[entity].properties = state.entities[entity].properties || {};
                state.entities[entity].properties[pname] = { type: 'string' };
                render(container, state);
                App.onStateChanged();
            });
        });

        // Remove property
        container.querySelectorAll('.prop-remove').forEach(btn => {
            btn.addEventListener('click', () => {
                const tr = btn.closest('tr');
                const entity = tr.dataset.entity;
                const prop = tr.dataset.prop;
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

        // Add relationship
        container.querySelectorAll('.rel-add').forEach(btn => {
            btn.addEventListener('click', () => {
                const entity = btn.dataset.entity;
                const rname = prompt('Relationship name:');
                if (!rname) return;
                state.entities[entity].relationships = state.entities[entity].relationships || {};
                state.entities[entity].relationships[rname] = { type: 'hasMany', targetEntity: '' };
                render(container, state);
                App.onStateChanged();
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

            // Handle rename
            if (newName !== origName) {
                delete state.entities[entity].properties[origName];
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
