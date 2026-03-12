/**
 * Entities tab — list of entities with expandable property/relationship editors.
 * No modals: uses inline forms for add/remove. Property renames propagate to seed data.
 * Features: color coding, duplication, drag-to-reorder, presets, bulk actions, sidebar nav.
 */
const EntityEditor = (() => {
    const propertyTypes = [
        'string', 'int', 'long', 'decimal', 'double', 'float',
        'bool', 'DateTime', 'DateOnly', 'TimeOnly', 'Guid'
    ];
    const relationshipTypes = ['hasMany', 'hasOne', 'belongsTo'];

    const presets = {
        id:          [{ name: 'id', type: 'Guid', isKey: true }],
        name:        [{ name: 'name', type: 'string', required: true, maxLength: 200 }],
        timestamps:  [
            { name: 'createdAt', type: 'DateTime' },
            { name: 'updatedAt', type: 'DateTime' }
        ],
        email:       [{ name: 'email', type: 'string', required: true, maxLength: 255, pattern: '^[^@]+@[^@]+$' }],
        description: [{ name: 'description', type: 'string', maxLength: 2000 }]
    };

    // Module state for sidebar navigation
    let focusedEntity = null;
    // Track which properties are checked per entity for bulk actions
    let checkedProps = {};

    function render(container, state) {
        state.entities = state.entities || {};
        const names = Object.keys(state.entities);
        const useSidebar = names.length >= 5;

        // Ensure focusedEntity is valid
        if (useSidebar) {
            if (!focusedEntity || !state.entities[focusedEntity]) {
                focusedEntity = names[0] || null;
            }
        } else {
            focusedEntity = null;
        }

        // Reset checked props for entities that no longer exist
        Object.keys(checkedProps).forEach(e => {
            if (!state.entities[e]) delete checkedProps[e];
        });

        const entityListHtml = useSidebar
            ? (focusedEntity ? entityCard(focusedEntity, state.entities[focusedEntity], state) : '')
            : names.map(n => entityCard(n, state.entities[n], state)).join('');

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
            ${useSidebar ? `
            <div class="flex gap-4">
                <div class="entity-sidebar">
                    <div class="entity-sidebar-header">Entities</div>
                    ${names.map(n => `
                        <button class="entity-sidebar-item ${n === focusedEntity ? 'active' : ''}" data-entity="${esc(n)}">
                            <span class="entity-color-dot" style="background: ${App.getEntityColor(n)}"></span>
                            ${esc(n)}
                        </button>
                    `).join('')}
                </div>
                <div class="flex-1 min-w-0">
                    <div id="entity-list">${entityListHtml}</div>
                </div>
            </div>` : `
            <div id="entity-list">${entityListHtml}</div>`}
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
            App.pushUndo();
            state.entities[name] = { properties: {} };
            focusedEntity = name;
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

        // Sidebar navigation
        if (useSidebar) {
            container.querySelectorAll('.entity-sidebar-item').forEach(btn => {
                btn.addEventListener('click', () => {
                    focusedEntity = btn.dataset.entity;
                    render(container, state);
                });
            });
        }

        // Bind all entity interactions
        bindEntityEvents(container, state);
    }

    function entityCard(name, entity, state) {
        const props = entity.properties || {};
        const rels = entity.relationships || {};
        const propNames = Object.keys(props);
        const relNames = Object.keys(rels);
        const checked = checkedProps[name] || {};
        const checkedCount = propNames.filter(p => checked[p]).length;

        return `
        <div class="entity-card" data-entity="${esc(name)}">
            <div class="entity-header">
                <div class="flex items-center gap-2">
                    <span class="entity-color-dot" style="background: ${App.getEntityColor(name)}"></span>
                    <span class="font-medium text-sm">${esc(name)}</span>
                    <span class="badge badge-secondary">${propNames.length} prop${propNames.length !== 1 ? 's' : ''}</span>
                    ${relNames.length > 0 ? `<span class="badge badge-secondary">${relNames.length} rel${relNames.length !== 1 ? 's' : ''}</span>` : ''}
                </div>
                <div class="flex items-center gap-2">
                    <button class="btn-sm btn-ghost entity-clone" data-entity="${esc(name)}">Clone</button>
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
                    <div class="preset-bar" data-entity="${esc(name)}">
                        <span class="text-xs text-gray-500 mr-1">Quick add:</span>
                        <button class="preset-btn" data-preset="id" data-entity="${esc(name)}">ID Field</button>
                        <button class="preset-btn" data-preset="name" data-entity="${esc(name)}">Name</button>
                        <button class="preset-btn" data-preset="timestamps" data-entity="${esc(name)}">Timestamps</button>
                        <button class="preset-btn" data-preset="email" data-entity="${esc(name)}">Email</button>
                        <button class="preset-btn" data-preset="description" data-entity="${esc(name)}">Description</button>
                    </div>
                    ${checkedCount > 0 ? `
                    <div class="bulk-toolbar" data-entity="${esc(name)}">
                        <span class="text-xs">${checkedCount} selected</span>
                        <button class="btn-sm btn-danger bulk-delete" data-entity="${esc(name)}">Delete Selected</button>
                        <button class="btn-sm btn-ghost bulk-toggle-required" data-entity="${esc(name)}">Toggle Required</button>
                    </div>` : ''}
                    ${propNames.length > 0 ? `
                    <table class="data-table">
                        <thead><tr>
                            <th></th><th></th><th>Name</th><th>Type</th><th>Key</th><th>Required</th>
                            <th>MaxLen</th><th>MinLen</th><th>Min</th><th>Max</th><th>Pattern</th><th></th>
                        </tr></thead>
                        <tbody>
                            ${propNames.map((p, i) => propRow(name, p, props[p], i, checked[p])).join('')}
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

    function propRow(entity, propName, prop, index, isChecked) {
        return `<tr data-entity="${esc(entity)}" data-prop="${esc(propName)}" data-index="${index}" draggable="true">
            <td class="drag-handle" title="Drag to reorder">&#10303;</td>
            <td class="text-center"><input type="checkbox" class="field-checkbox prop-bulk-check" data-entity="${esc(entity)}" data-prop="${esc(propName)}" ${isChecked ? 'checked' : ''} /></td>
            <td><input class="field-input py-1 text-xs prop-field" style="min-width:120px" data-key="name" value="${esc(propName)}" /></td>
            <td><select class="field-select py-1 text-xs prop-field" style="min-width:110px" data-key="type">
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
        // Clone entity
        container.querySelectorAll('.entity-clone').forEach(btn => {
            btn.addEventListener('click', e => {
                e.stopPropagation();
                const name = btn.dataset.entity;
                const cloneName = name + 'Copy';
                App.pushUndo();
                state.entities[cloneName] = JSON.parse(JSON.stringify(state.entities[name]));
                focusedEntity = cloneName;
                render(container, state);
                App.onStateChanged();
                App.showToast(`Cloned "${name}" as "${cloneName}"`, 'success');
            });
        });

        // Remove entity — two-click confirmation (no modal)
        container.querySelectorAll('.entity-remove').forEach(btn => {
            let confirmTimer = null;
            btn.addEventListener('click', e => {
                e.stopPropagation();
                const name = btn.dataset.entity;
                if (btn.dataset.confirmed === 'true') {
                    // Second click — actually remove
                    clearTimeout(confirmTimer);
                    App.pushUndo();
                    delete state.entities[name];
                    delete checkedProps[name];
                    if (focusedEntity === name) {
                        const remaining = Object.keys(state.entities);
                        focusedEntity = remaining[0] || null;
                    }
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

        // Preset buttons
        container.querySelectorAll('.preset-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const entityName = btn.dataset.entity;
                const presetKey = btn.dataset.preset;
                const presetItems = presets[presetKey];
                if (!presetItems) return;

                App.pushUndo();
                state.entities[entityName].properties = state.entities[entityName].properties || {};

                presetItems.forEach(item => {
                    const propDef = Object.assign({}, item);
                    const pName = propDef.name;
                    delete propDef.name;
                    if (!propDef.type) propDef.type = 'string';
                    state.entities[entityName].properties[pName] = propDef;
                });

                render(container, state);
                App.onStateChanged();
                App.showToast(`Added "${presetKey}" preset to ${entityName}`, 'success');
            });
        });

        // Bulk delete
        container.querySelectorAll('.bulk-delete').forEach(btn => {
            btn.addEventListener('click', () => {
                const entityName = btn.dataset.entity;
                const checked = checkedProps[entityName] || {};
                const toDelete = Object.keys(checked).filter(p => checked[p]);
                if (toDelete.length === 0) return;

                App.pushUndo();
                toDelete.forEach(p => {
                    // Also remove from seed data
                    if (state.entities[entityName].seedData) {
                        state.entities[entityName].seedData.forEach(row => {
                            delete row[p];
                        });
                    }
                    delete state.entities[entityName].properties[p];
                });
                delete checkedProps[entityName];
                render(container, state);
                App.onStateChanged();
                App.showToast(`Deleted ${toDelete.length} properties`, 'success');
            });
        });

        // Bulk toggle required
        container.querySelectorAll('.bulk-toggle-required').forEach(btn => {
            btn.addEventListener('click', () => {
                const entityName = btn.dataset.entity;
                const checked = checkedProps[entityName] || {};
                const toToggle = Object.keys(checked).filter(p => checked[p]);
                if (toToggle.length === 0) return;

                App.pushUndo();
                toToggle.forEach(p => {
                    const prop = state.entities[entityName].properties[p];
                    if (prop) {
                        if (prop.required) {
                            delete prop.required;
                        } else {
                            prop.required = true;
                        }
                    }
                });
                render(container, state);
                App.onStateChanged();
            });
        });

        // Bulk checkboxes
        container.querySelectorAll('.prop-bulk-check').forEach(cb => {
            cb.addEventListener('change', () => {
                const entityName = cb.dataset.entity;
                const propName = cb.dataset.prop;
                if (!checkedProps[entityName]) checkedProps[entityName] = {};
                if (cb.checked) {
                    checkedProps[entityName][propName] = true;
                } else {
                    delete checkedProps[entityName][propName];
                }
                render(container, state);
            });
        });

        // Drag-to-reorder properties
        bindDragReorder(container, state);

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
                App.pushUndo();
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

                App.pushUndo();

                // Also remove from seed data
                if (state.entities[entity].seedData) {
                    state.entities[entity].seedData.forEach(row => {
                        delete row[prop];
                    });
                }

                delete state.entities[entity].properties[prop];
                if (checkedProps[entity]) delete checkedProps[entity][prop];
                render(container, state);
                App.onStateChanged();
            });
        });

        // Property field changes (focus-based undo — push once per focus, not every keystroke)
        container.querySelectorAll('.prop-field').forEach(el => {
            const event = el.type === 'checkbox' ? 'change' : 'input';
            let undoPushed = false;
            el.addEventListener('focus', () => { undoPushed = false; });
            el.addEventListener(event, () => {
                if (!undoPushed) {
                    App.pushUndo();
                    undoPushed = true;
                }
                collectProperties(container, state);
            });
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
                App.pushUndo();
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
                App.pushUndo();
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

    function bindDragReorder(container, state) {
        let dragSrcIndex = null;
        let dragEntity = null;

        container.querySelectorAll('tr[data-entity][data-prop]').forEach(tr => {
            tr.addEventListener('dragstart', e => {
                dragSrcIndex = parseInt(tr.dataset.index, 10);
                dragEntity = tr.dataset.entity;
                tr.classList.add('dragging');
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/plain', '');
            });

            tr.addEventListener('dragover', e => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                // Only highlight if same entity
                if (tr.dataset.entity === dragEntity) {
                    tr.classList.add('drag-over');
                }
            });

            tr.addEventListener('dragleave', () => {
                tr.classList.remove('drag-over');
            });

            tr.addEventListener('drop', e => {
                e.preventDefault();
                tr.classList.remove('drag-over');
                const targetIndex = parseInt(tr.dataset.index, 10);
                const entityName = tr.dataset.entity;

                if (entityName !== dragEntity || dragSrcIndex === targetIndex) return;

                App.pushUndo();

                // Reorder properties using ordered keys
                const props = state.entities[entityName].properties || {};
                const keys = Object.keys(props);
                const movedKey = keys[dragSrcIndex];

                // Remove from old position
                keys.splice(dragSrcIndex, 1);
                // Insert at new position
                keys.splice(targetIndex, 0, movedKey);

                // Rebuild properties object in new order
                const reordered = {};
                keys.forEach(k => { reordered[k] = props[k]; });
                state.entities[entityName].properties = reordered;

                render(container, state);
                App.onStateChanged();
            });

            tr.addEventListener('dragend', () => {
                tr.classList.remove('dragging');
                container.querySelectorAll('.drag-over').forEach(el => el.classList.remove('drag-over'));
                dragSrcIndex = null;
                dragEntity = null;
            });
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

                // Update checked props tracking
                if (checkedProps[entity] && checkedProps[entity][origName]) {
                    checkedProps[entity][newName] = true;
                    delete checkedProps[entity][origName];
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
