/**
 * Ninjadog Config Builder — main application state and orchestration.
 */
const App = (() => {
    let state = {};
    let activeTab = 'config';
    let rawEditMode = false;
    let validateTimer = null;

    async function init() {
        // Load config from server
        try {
            const config = await NinjadogApi.getConfig();
            state = config || {};
        } catch {
            state = {};
        }

        setupTabs();
        setupHeaderButtons();
        setupRawToggle();
        renderActiveTab();
        JsonPreview.update(state);
        scheduleValidation();
    }

    function setupTabs() {
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                activeTab = btn.dataset.tab;
                document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                document.querySelectorAll('.tab-content').forEach(c => c.classList.add('hidden'));
                document.getElementById(`tab-${activeTab}`).classList.remove('hidden');
                renderActiveTab();
            });
        });
    }

    function setupHeaderButtons() {
        document.getElementById('btn-save').addEventListener('click', async () => {
            const badge = document.getElementById('status-badge');
            try {
                const json = JsonPreview.getJson(state);
                await NinjadogApi.saveConfig(json);
                badge.textContent = 'Saved';
                badge.className = 'text-xs px-2 py-0.5 rounded-full bg-emerald-900 text-emerald-300';
                badge.classList.remove('hidden');
                setTimeout(() => badge.classList.add('hidden'), 2000);
            } catch (err) {
                badge.textContent = 'Save failed';
                badge.className = 'text-xs px-2 py-0.5 rounded-full bg-red-900 text-red-300';
                badge.classList.remove('hidden');
                setTimeout(() => badge.classList.add('hidden'), 3000);
            }
        });

        document.getElementById('btn-build').addEventListener('click', async () => {
            const badge = document.getElementById('status-badge');
            try {
                // Save first
                const json = JsonPreview.getJson(state);
                await NinjadogApi.saveConfig(json);
                const result = await NinjadogApi.build();
                badge.textContent = result.success ? 'Build succeeded' : 'Build failed';
                badge.className = result.success
                    ? 'text-xs px-2 py-0.5 rounded-full bg-emerald-900 text-emerald-300'
                    : 'text-xs px-2 py-0.5 rounded-full bg-red-900 text-red-300';
                badge.classList.remove('hidden');
                setTimeout(() => badge.classList.add('hidden'), 3000);
            } catch (err) {
                badge.textContent = 'Build failed';
                badge.className = 'text-xs px-2 py-0.5 rounded-full bg-red-900 text-red-300';
                badge.classList.remove('hidden');
                setTimeout(() => badge.classList.add('hidden'), 3000);
            }
        });
    }

    function setupRawToggle() {
        const btn = document.getElementById('btn-toggle-raw');
        const preview = document.getElementById('json-preview');
        const raw = document.getElementById('json-raw');

        btn.addEventListener('click', () => {
            rawEditMode = !rawEditMode;
            if (rawEditMode) {
                preview.classList.add('hidden');
                raw.classList.remove('hidden');
                raw.value = JSON.stringify(JsonPreview.getJson(state), null, 2);
                btn.textContent = 'Preview';
            } else {
                // Try to parse raw edit
                try {
                    state = JSON.parse(raw.value);
                    renderActiveTab();
                    scheduleValidation();
                } catch {
                    // invalid JSON, stay in raw mode
                    rawEditMode = true;
                    return;
                }
                preview.classList.remove('hidden');
                raw.classList.add('hidden');
                btn.textContent = 'Raw Edit';
                JsonPreview.update(state);
            }
        });
    }

    function renderActiveTab() {
        const container = document.getElementById(`tab-${activeTab}`);
        switch (activeTab) {
            case 'config':
                ConfigEditor.render(container, state);
                break;
            case 'entities':
                EntityEditor.render(container, state);
                break;
            case 'enums':
                EnumEditor.render(container, state);
                break;
            case 'seed':
                SeedEditor.render(container, state);
                break;
        }
    }

    function onStateChanged() {
        JsonPreview.update(state);
        scheduleValidation();
    }

    function scheduleValidation() {
        if (validateTimer) clearTimeout(validateTimer);
        validateTimer = setTimeout(async () => {
            try {
                const json = JsonPreview.getJson(state);
                const result = await NinjadogApi.validate(json);
                ValidationDisplay.render(result);
            } catch {
                ValidationDisplay.clear();
            }
        }, 500);
    }

    // Boot
    document.addEventListener('DOMContentLoaded', init);

    return { onStateChanged };
})();
