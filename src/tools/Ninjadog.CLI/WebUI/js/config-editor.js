/**
 * Config tab — form fields for the top-level "config" section.
 */
const ConfigEditor = (() => {
    let dirBrowserOpen = false;
    let dirBrowserPath = '.';

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
                            <label class="field-label">Root Namespace</label>
                            <input class="field-input" data-field="rootNamespace" value="${esc(c.rootNamespace || '')}" />
                        </div>
                        <div>
                            <label class="field-label">Output Path</label>
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
                            <label class="field-label">Provider</label>
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
                        <label class="field-label">Origins (comma-separated)</label>
                        <input class="field-input" data-field="corsOrigins" value="${esc((cors.origins || []).join(', '))}" />
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
                            Soft Delete
                        </label>
                        <label class="flex items-center gap-2 text-sm">
                            <input type="checkbox" class="field-checkbox" data-field="auditing" ${features.auditing ? 'checked' : ''} />
                            Auditing
                        </label>
                    </div>
                </div>
            </div>
        `;

        // Bind change events
        container.querySelectorAll('[data-field]').forEach(el => {
            const event = el.type === 'checkbox' ? 'change' : 'input';
            el.addEventListener(event, () => collectConfig(container, state));
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
