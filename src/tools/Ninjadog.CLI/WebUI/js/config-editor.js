/**
 * Config tab — form fields for the top-level "config" section.
 */
const ConfigEditor = (() => {
    let dirBrowserOpen = false;
    let dirBrowserPath = '.';
    let interactedFields = new Set();

    function tooltip(text) {
        return `<span class="tooltip-wrapper">
    <span class="tooltip-icon">?</span>
    <span class="tooltip-content">${esc(text)}</span>
</span>`;
    }

    function render(container, state) {
        const c = state.config || {};
        const cors = c.cors || {};
        const features = c.features || {};
        const database = c.database || {};

        container.innerHTML = `
            <div class="space-y-4">
                <div class="section-card">
                    <div class="section-title">General</div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="field-label">Name</label>
                            <input class="field-input" data-field="name" value="${esc(c.name || '')}" />
                            <div class="field-error-msg" data-error-for="name"></div>
                        </div>
                        <div>
                            <label class="field-label">Version</label>
                            <input class="field-input" data-field="version" value="${esc(c.version || '')}" />
                        </div>
                        <div class="col-span-2">
                            <label class="field-label">Description</label>
                            <input class="field-input" data-field="description" value="${esc(c.description || '')}" />
                        </div>
                        <div>
                            <label class="field-label">Root Namespace ${tooltip('The root C# namespace for generated code. Example: MyCompany.Api')}</label>
                            <input class="field-input" data-field="rootNamespace" value="${esc(c.rootNamespace || '')}" />
                            <div class="field-error-msg" data-error-for="rootNamespace"></div>
                        </div>
                        <div>
                            <label class="field-label">Output Path ${tooltip('Directory where generated files will be written, relative to ninjadog.json')}</label>
                            <div class="flex gap-2">
                                <input class="field-input flex-1" data-field="outputPath" value="${esc(c.outputPath || '.')}" />
                                <button id="btn-browse-output" class="btn-sm btn-ghost flex items-center gap-1 whitespace-nowrap" title="Browse directories">
                                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>
                                    Browse
                                </button>
                            </div>
                            <div id="dir-browser" class="mt-2 hidden"></div>
                        </div>
                    </div>
                </div>

                <div class="section-card">
                    <div class="section-title">Database</div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="field-label">Provider ${tooltip('The database engine for generated repository code')}</label>
                            <select class="field-select" data-field="databaseProvider">
                                <option value="sqlite" ${database.provider === 'sqlite' || !database.provider ? 'selected' : ''}>SQLite</option>
                                <option value="postgres" ${database.provider === 'postgres' ? 'selected' : ''}>PostgreSQL</option>
                                <option value="sqlserver" ${database.provider === 'sqlserver' ? 'selected' : ''}>SQL Server</option>
                            </select>
                        </div>
                    </div>
                </div>

                <div class="section-card">
                    <div class="section-title">CORS</div>
                    <div>
                        <label class="field-label">Origins (comma-separated) ${tooltip('Allowed origins for cross-origin requests. Use * for all origins (not recommended for production)')}</label>
                        <input class="field-input" data-field="corsOrigins" value="${esc((cors.origins || []).join(', '))}" />
                        <div class="field-error-msg" data-error-for="corsOrigins"></div>
                    </div>
                    <div class="grid grid-cols-2 gap-4 mt-3">
                        <div>
                            <label class="field-label">Methods (comma-separated)</label>
                            <input class="field-input" data-field="corsMethods" value="${esc((cors.methods || []).join(', '))}" />
                        </div>
                        <div>
                            <label class="field-label">Headers (comma-separated)</label>
                            <input class="field-input" data-field="corsHeaders" value="${esc((cors.headers || []).join(', '))}" />
                        </div>
                    </div>
                </div>

                <div class="section-card">
                    <div class="section-title">Features</div>
                    <div class="flex items-center gap-6">
                        <label class="flex items-center gap-2 text-sm">
                            <input type="checkbox" class="field-checkbox" data-field="softDelete" ${features.softDelete ? 'checked' : ''} />
                            Soft Delete ${tooltip('Adds IsDeleted and DeletedAt columns instead of permanently deleting records')}
                        </label>
                        <label class="flex items-center gap-2 text-sm">
                            <input type="checkbox" class="field-checkbox" data-field="auditing" ${features.auditing ? 'checked' : ''} />
                            Auditing ${tooltip('Adds CreatedAt and UpdatedAt timestamp columns to all entities')}
                        </label>
                    </div>
                </div>

                ${renderEmptyState(state)}
            </div>
        `;

        // Bind change events with focus-based undo (push once on focus, not every keystroke)
        container.querySelectorAll('[data-field]').forEach(el => {
            const event = el.type === 'checkbox' ? 'change' : 'input';
            let undoPushed = false;
            el.addEventListener('focus', () => { undoPushed = false; });
            el.addEventListener(event, () => {
                interactedFields.add(el.dataset.field);
                if (!undoPushed) {
                    App.pushUndo();
                    undoPushed = true;
                }
                collectConfig(container, state);
            });
        });

        // Browse button
        container.querySelector('#btn-browse-output').addEventListener('click', () => {
            dirBrowserOpen = !dirBrowserOpen;
            if (dirBrowserOpen) {
                dirBrowserPath = container.querySelector('[data-field="outputPath"]').value || '.';
                loadDirectories(container, state);
            } else {
                container.querySelector('#dir-browser').classList.add('hidden');
            }
        });

        // Empty state buttons
        const btnTemplate = container.querySelector('#btn-start-template');
        if (btnTemplate) {
            btnTemplate.addEventListener('click', () => App.showTemplatePicker());
        }
        const btnScratch = container.querySelector('#btn-start-scratch');
        if (btnScratch) {
            btnScratch.addEventListener('click', () => {
                const entitiesTab = document.querySelector('[data-tab="entities"]');
                if (entitiesTab) entitiesTab.click();
            });
        }
    }

    function renderEmptyState(state) {
        const entities = state.entities || {};
        if (Object.keys(entities).length > 0) return '';

        return `
            <div class="empty-state">
                <div class="empty-state-icon">
                    <svg viewBox="0 0 24 24" width="64" height="64" fill="none" stroke="currentColor" stroke-width="1.5">
                        <path d="M12 2L9 9L2 12L9 15L12 22L15 15L22 12L15 9L12 2Z"/>
                    </svg>
                </div>
                <div class="empty-state-title">Ready to build your API</div>
                <div class="empty-state-text">Configure your project settings above, then add entities to generate a complete CRUD Web API.</div>
                <div class="empty-state-actions">
                    <button class="btn-sm btn-primary" id="btn-start-template">Start from Template</button>
                    <button class="btn-sm btn-ghost" id="btn-start-scratch">Add First Entity</button>
                </div>
            </div>
        `;
    }

    function validateFields(container) {
        let valid = true;

        // Clear all previous errors
        container.querySelectorAll('.field-error').forEach(el => el.classList.remove('field-error'));
        container.querySelectorAll('.field-error-msg').forEach(el => { el.textContent = ''; el.style.display = 'none'; });

        // Name validation
        if (interactedFields.has('name')) {
            const nameInput = container.querySelector('[data-field="name"]');
            const nameVal = (nameInput?.value || '').trim();
            const nameError = container.querySelector('[data-error-for="name"]');
            if (!nameVal) {
                setFieldError(nameInput, nameError, 'Project name is required');
                valid = false;
            } else if (!/^[a-zA-Z0-9.\-]+$/.test(nameVal)) {
                setFieldError(nameInput, nameError, 'Use only letters, numbers, dots, and hyphens');
                valid = false;
            }
        }

        // Root Namespace validation
        if (interactedFields.has('rootNamespace')) {
            const nsInput = container.querySelector('[data-field="rootNamespace"]');
            const nsVal = (nsInput?.value || '').trim();
            const nsError = container.querySelector('[data-error-for="rootNamespace"]');
            if (nsVal && !/^[A-Z][a-zA-Z0-9]*(\.[A-Z][a-zA-Z0-9]*)*$/.test(nsVal)) {
                setFieldError(nsInput, nsError, 'Invalid namespace format. Use PascalCase with dots');
                valid = false;
            }
        }

        // CORS Origins validation
        if (interactedFields.has('corsOrigins')) {
            const originsInput = container.querySelector('[data-field="corsOrigins"]');
            const originsVal = (originsInput?.value || '').trim();
            const originsError = container.querySelector('[data-error-for="corsOrigins"]');
            if (originsVal) {
                const origins = originsVal.split(',').map(s => s.trim()).filter(Boolean);
                const allValid = origins.every(o => o === '*' || /^https?:\/\//.test(o));
                if (!allValid) {
                    setFieldError(originsInput, originsError, 'Origins must be URLs (http/https) or *');
                    valid = false;
                }
            }
        }

        return valid;
    }

    function setFieldError(input, errorDiv, message) {
        if (input) input.classList.add('field-error');
        if (errorDiv) {
            errorDiv.textContent = message;
            errorDiv.style.display = 'block';
        }
    }

    async function loadDirectories(container, state) {
        const browser = container.querySelector('#dir-browser');
        browser.classList.remove('hidden');
        browser.innerHTML = '<div class="dir-browser"><div class="dir-current">Loading...</div></div>';

        try {
            const data = await NinjadogApi.getDirectories(dirBrowserPath);
            renderDirBrowser(container, state, data);
        } catch {
            browser.innerHTML = '<div class="dir-browser"><div class="dir-current">Failed to load directories</div></div>';
        }
    }

    function renderDirBrowser(container, state, data) {
        const browser = container.querySelector('#dir-browser');
        const dirs = data.directories || [];

        let html = '<div class="dir-browser">';
        html += `<div class="dir-current">
            <span>${esc(data.absolute || data.current)}</span>
            <button class="dir-close-btn btn-sm btn-ghost" style="padding:2px 8px;font-size:10px;">Close</button>
        </div>`;

        // Parent directory
        if (data.parent) {
            html += `<button class="dir-item dir-item-parent" data-path="${esc(data.parent)}">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
                ..
            </button>`;
        }

        // Subdirectories
        dirs.forEach(d => {
            const subPath = data.current === '.' ? d : `${data.current}/${d}`;
            html += `<button class="dir-item" data-path="${esc(subPath)}">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>
                ${esc(d)}
            </button>`;
        });

        // Select current directory
        html += `<button class="dir-item dir-item-select" data-select="${esc(data.current)}">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"/></svg>
            Select this folder
        </button>`;

        html += '</div>';
        browser.innerHTML = html;

        // Bind navigation
        browser.querySelectorAll('.dir-item:not(.dir-item-select)').forEach(btn => {
            btn.addEventListener('click', () => {
                dirBrowserPath = btn.dataset.path;
                loadDirectories(container, state);
            });
        });

        // Bind select
        browser.querySelector('.dir-item-select').addEventListener('click', () => {
            const input = container.querySelector('[data-field="outputPath"]');
            input.value = browser.querySelector('.dir-item-select').dataset.select;
            dirBrowserOpen = false;
            browser.classList.add('hidden');
            collectConfig(container, state);
        });

        // Bind close
        browser.querySelector('.dir-close-btn').addEventListener('click', () => {
            dirBrowserOpen = false;
            browser.classList.add('hidden');
        });
    }

    function collectConfig(container, state) {
        const val = f => (container.querySelector(`[data-field="${f}"]`)?.value || '').trim();
        const checked = f => container.querySelector(`[data-field="${f}"]`)?.checked || false;
        const csvArr = f => val(f).split(',').map(s => s.trim()).filter(Boolean);

        // Run validation (visual feedback only, does not block state updates)
        validateFields(container);

        state.config = state.config || {};
        state.config.name = val('name');
        state.config.version = val('version');
        state.config.description = val('description');
        state.config.rootNamespace = val('rootNamespace');
        state.config.outputPath = val('outputPath') || '.';

        // Database
        const provider = val('databaseProvider');
        if (provider && provider !== 'sqlite') {
            state.config.database = { provider };
        } else {
            delete state.config.database;
        }

        // CORS
        const origins = csvArr('corsOrigins');
        if (origins.length > 0) {
            state.config.cors = { origins };
            const methods = csvArr('corsMethods');
            const headers = csvArr('corsHeaders');
            if (methods.length) state.config.cors.methods = methods;
            if (headers.length) state.config.cors.headers = headers;
        } else {
            delete state.config.cors;
        }

        // Features
        const sd = checked('softDelete');
        const au = checked('auditing');
        if (sd || au) {
            state.config.features = {};
            if (sd) state.config.features.softDelete = true;
            if (au) state.config.features.auditing = true;
        } else {
            delete state.config.features;
        }

        App.onStateChanged();
    }

    function esc(s) {
        return s.replace(/&/g,'&amp;').replace(/"/g,'&quot;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    return { render };
})();
