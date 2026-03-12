/**
 * Ninjadog Config Builder — main application state and orchestration.
 */
const App = (() => {
    let state = {};
    let savedSnapshot = '';
    let activeTab = 'config';
    let validateTimer = null;

    async function init() {
        // Load config from server
        try {
            const config = await NinjadogApi.getConfig();
            state = config || {};
        } catch {
            state = {};
        }

        savedSnapshot = JSON.stringify(state);

        setupTabs();
        setupHeaderButtons();
        renderActiveTab();

        // Initialize Monaco editor and register bidirectional sync
        await JsonPreview.init(state);
        JsonPreview.onExternalEdit((parsed) => {
            state = parsed;
            renderActiveTab();
            scheduleValidation();
            updateDirtyIndicator();
        });

        scheduleValidation();
        updateDirtyIndicator();
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
                savedSnapshot = JSON.stringify(state);
                updateDirtyIndicator();
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
                savedSnapshot = JSON.stringify(state);
                updateDirtyIndicator();
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
        updateDirtyIndicator();
    }

    function updateDirtyIndicator() {
        const isDirty = JSON.stringify(state) !== savedSnapshot;
        const dot = document.getElementById('dirty-indicator');
        const saveBtn = document.getElementById('btn-save');

        if (isDirty) {
            dot.classList.remove('hidden');
            saveBtn.classList.add('header-btn-dirty');
        } else {
            dot.classList.add('hidden');
            saveBtn.classList.remove('header-btn-dirty');
        }
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
