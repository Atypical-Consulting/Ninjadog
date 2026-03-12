/**
 * Enums tab — list of enum definitions with value management.
 * No modals: uses inline forms for add/remove.
 * Supports undo and entity color coding.
 */
const EnumEditor = (() => {
    function render(container, state) {
        state.enums = state.enums || {};
        const names = Object.keys(state.enums);

        container.innerHTML = `
            <div class="flex items-center justify-between mb-4">
                <div class="section-title mb-0">Enums (${names.length})</div>
                <div id="enum-add-area">
                    <button id="btn-add-enum" class="btn-sm btn-primary">+ Add Enum</button>
                    <div id="enum-add-form" class="inline-add-form hidden">
                        <input id="enum-add-input" class="field-input text-sm py-1" style="width:200px" placeholder="Enum name (PascalCase)" />
                        <button id="enum-add-confirm" class="btn-sm btn-primary">Create</button>
                        <button id="enum-add-cancel" class="btn-sm btn-ghost">Cancel</button>
                    </div>
                </div>
            </div>
            <div id="enum-list">${names.map(n => enumCard(n, state.enums[n])).join('')}</div>
        `;

        // Inline add enum
        const addBtn = container.querySelector('#btn-add-enum');
        const addForm = container.querySelector('#enum-add-form');
        const addInput = container.querySelector('#enum-add-input');
        const addConfirm = container.querySelector('#enum-add-confirm');
        const addCancel = container.querySelector('#enum-add-cancel');

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
            state.enums[name] = [];
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

        bindEvents(container, state);
    }

    function enumCard(name, values) {
        const colorDot = `<span class="entity-color-dot" style="background: ${App.getEntityColor(name)}"></span>`;

        return `
        <div class="entity-card" data-enum="${esc(name)}">
            <div class="entity-header">
                <span class="font-medium text-sm">${colorDot}${esc(name)}</span>
                <div class="flex items-center gap-2">
                    <span class="text-xs text-gray-500">${values.length} values</span>
                    <button class="btn-sm btn-danger enum-remove" data-enum="${esc(name)}" data-confirmed="false">Remove</button>
                </div>
            </div>
            <div class="entity-body">
                <div class="flex flex-wrap gap-2 mb-2">
                    ${values.map((v, i) => `
                        <span class="enum-value-tag">
                            ${esc(v)}
                            <button class="enum-val-remove" data-enum="${esc(name)}" data-index="${i}">&times;</button>
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
        // Remove enum — two-click confirmation (no modal)
        container.querySelectorAll('.enum-remove').forEach(btn => {
            let confirmTimer = null;
            btn.addEventListener('click', e => {
                e.stopPropagation();
                const name = btn.dataset.enum;
                if (btn.dataset.confirmed === 'true') {
                    clearTimeout(confirmTimer);
                    App.pushUndo();
                    delete state.enums[name];
                    if (Object.keys(state.enums).length === 0) delete state.enums;
                    render(container, state);
                    App.onStateChanged();
                } else {
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

        // Remove value
        container.querySelectorAll('.enum-val-remove').forEach(btn => {
            btn.addEventListener('click', () => {
                const name = btn.dataset.enum;
                const idx = parseInt(btn.dataset.index, 10);
                App.pushUndo();
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
        App.pushUndo();
        state.enums[enumName].push(val);
        render(container, state);
        App.onStateChanged();
    }

    function esc(s) {
        return String(s).replace(/&/g,'&amp;').replace(/"/g,'&quot;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    return { render };
})();
