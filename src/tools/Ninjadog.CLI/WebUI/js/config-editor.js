/**
 * Config tab — form fields for the top-level "config" section.
 */
const ConfigEditor = (() => {
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
                            <input class="field-input" data-field="outputPath" value="${esc(c.outputPath || '.')}" />
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
