/**
 * Ninjadog Config Builder — main application state and orchestration.
 *
 * Manages app lifecycle, undo/redo, toasts, tab badges, breadcrumbs,
 * keyboard shortcuts, auto-save, view modes, split panel, build console,
 * template picker, skeleton loading, export, copy, and import modal.
 */
const App = (() => {
    // ── State ────────────────────────────────────────────────────────────
    let state = {};
    let savedSnapshot = '';
    let activeTab = 'config';
    let validateTimer = null;

    // Undo / Redo
    const MAX_UNDO = 50;
    let undoStack = [];
    let redoStack = [];

    // Auto-save
    let autoSaveEnabled = localStorage.getItem('ninjadog:autoSave') === 'true';
    let autoSaveTimer = null;

    // Split panel resize
    let isResizing = false;

    // ── Templates ────────────────────────────────────────────────────────
    const TEMPLATES = {
        blank: {
            config: { name: '', version: '1.0.0', rootNamespace: '', outputPath: '.' },
            entities: {}
        },
        todo: {
            config: {
                name: 'TodoApi', version: '1.0.0', rootNamespace: 'TodoApi',
                outputPath: '.', cors: { origins: ['http://localhost:3000'] }
            },
            entities: {
                TodoItem: {
                    properties: {
                        id: { type: 'Guid', isKey: true },
                        title: { type: 'string', required: true, maxLength: 200 },
                        description: { type: 'string', maxLength: 1000 },
                        isCompleted: { type: 'bool' },
                        createdAt: { type: 'DateTime' },
                        dueDate: { type: 'DateTime' }
                    }
                }
            }
        },
        blog: {
            config: {
                name: 'BlogApi', version: '1.0.0', rootNamespace: 'BlogApi',
                outputPath: '.', features: { softDelete: true, auditing: true }
            },
            entities: {
                Post: {
                    properties: {
                        id: { type: 'Guid', isKey: true },
                        title: { type: 'string', required: true, maxLength: 200 },
                        content: { type: 'string' },
                        slug: { type: 'string', maxLength: 200 },
                        isPublished: { type: 'bool' }
                    },
                    relationships: {
                        comments: { type: 'hasMany', targetEntity: 'Comment', foreignKey: 'postId' },
                        tags: { type: 'hasMany', targetEntity: 'Tag', foreignKey: 'postId' }
                    }
                },
                Comment: {
                    properties: {
                        id: { type: 'Guid', isKey: true },
                        content: { type: 'string', required: true },
                        author: { type: 'string', maxLength: 100 }
                    },
                    relationships: {
                        post: { type: 'belongsTo', targetEntity: 'Post', foreignKey: 'postId' }
                    }
                },
                Tag: {
                    properties: {
                        id: { type: 'int', isKey: true },
                        name: { type: 'string', required: true, maxLength: 50 }
                    }
                }
            }
        },
        ecommerce: {
            config: {
                name: 'ShopApi', version: '1.0.0', rootNamespace: 'ShopApi',
                outputPath: '.',
                database: { provider: 'postgres' },
                features: { softDelete: true, auditing: true },
                cors: {
                    origins: ['http://localhost:3000'],
                    methods: ['GET', 'POST', 'PUT', 'DELETE'],
                    headers: ['Content-Type', 'Authorization']
                }
            },
            entities: {
                Product: {
                    properties: {
                        id: { type: 'Guid', isKey: true },
                        name: { type: 'string', required: true, maxLength: 200 },
                        description: { type: 'string' },
                        price: { type: 'decimal', required: true, min: 0 },
                        stock: { type: 'int', min: 0 },
                        sku: { type: 'string', maxLength: 50 }
                    }
                },
                Customer: {
                    properties: {
                        id: { type: 'Guid', isKey: true },
                        email: { type: 'string', required: true, maxLength: 255 },
                        firstName: { type: 'string', required: true, maxLength: 100 },
                        lastName: { type: 'string', required: true, maxLength: 100 }
                    },
                    relationships: {
                        orders: { type: 'hasMany', targetEntity: 'Order', foreignKey: 'customerId' }
                    }
                },
                Order: {
                    properties: {
                        id: { type: 'Guid', isKey: true },
                        orderDate: { type: 'DateTime' },
                        totalAmount: { type: 'decimal' },
                        status: { type: 'string', maxLength: 50 }
                    },
                    relationships: {
                        customer: { type: 'belongsTo', targetEntity: 'Customer', foreignKey: 'customerId' },
                        items: { type: 'hasMany', targetEntity: 'OrderItem', foreignKey: 'orderId' }
                    }
                },
                OrderItem: {
                    properties: {
                        id: { type: 'Guid', isKey: true },
                        quantity: { type: 'int', required: true, min: 1 },
                        unitPrice: { type: 'decimal', required: true }
                    },
                    relationships: {
                        order: { type: 'belongsTo', targetEntity: 'Order', foreignKey: 'orderId' },
                        product: { type: 'belongsTo', targetEntity: 'Product', foreignKey: 'productId' }
                    }
                }
            },
            enums: {
                OrderStatus: ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled']
            }
        }
    };

    // ── Toast icons ──────────────────────────────────────────────────────
    const TOAST_ICONS = {
        success: '<svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/></svg>',
        error: '<svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>',
        warning: '<svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M12 3l9.66 16.5H2.34L12 3z"/></svg>',
        info: '<svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01"/></svg>'
    };

    const TOAST_COLORS = {
        success: 'toast-success',
        error: 'toast-error',
        warning: 'toast-warning',
        info: 'toast-info'
    };

    // ── Tab label map ────────────────────────────────────────────────────
    const TAB_LABELS = {
        config: 'Configuration',
        entities: 'Entities',
        enums: 'Enums',
        seed: 'Seed Data'
    };

    // ── Initialization ───────────────────────────────────────────────────
    async function init() {
        setupConnectionListeners();
        setupKeyboardShortcuts();
        setupResizablePanel();
        setupViewModeButtons();

        // Show skeleton while loading
        showSkeletons(true);

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
        setupAutoSaveCheckbox();

        // Remove skeletons and render
        showSkeletons(false);
        renderActiveTab();

        // Initialize Monaco editor and register bidirectional sync
        await JsonPreview.init(state);
        JsonPreview.onExternalEdit((parsed) => {
            pushUndo();
            state = parsed;
            renderActiveTab();
            scheduleValidation();
            updateDirtyIndicator();
            updateTabBadges();
        });

        scheduleValidation();
        updateDirtyIndicator();
        updateTabBadges();
        setBreadcrumb([{ label: 'Config Builder' }, { label: TAB_LABELS[activeTab] }]);
    }

    // ── Connection listeners ─────────────────────────────────────────────
    function setupConnectionListeners() {
        document.addEventListener('ninjadog:disconnected', () => {
            const badge = document.getElementById('status-badge');
            if (badge) {
                badge.textContent = 'Server disconnected';
                badge.className = 'text-xs px-2 py-0.5 rounded-full bg-red-900 text-red-300';
                badge.classList.remove('hidden');
            }
        });

        document.addEventListener('ninjadog:connected', () => {
            const badge = document.getElementById('status-badge');
            if (badge) {
                badge.textContent = 'Reconnected';
                badge.className = 'text-xs px-2 py-0.5 rounded-full bg-emerald-900 text-emerald-300';
                badge.classList.remove('hidden');
                setTimeout(() => badge.classList.add('hidden'), 2000);
            }
            scheduleValidation();
        });
    }

    // ── Tab management ───────────────────────────────────────────────────
    function setupTabs() {
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                switchTab(btn.dataset.tab);
            });
        });
    }

    function switchTab(tabName) {
        const previousTab = activeTab;
        activeTab = tabName;

        // Update tab button states
        document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
        const activeBtn = document.querySelector(`.tab-btn[data-tab="${tabName}"]`);
        if (activeBtn) activeBtn.classList.add('active');

        // Animated tab transition
        const previousContent = document.getElementById(`tab-${previousTab}`);
        const nextContent = document.getElementById(`tab-${tabName}`);

        if (previousContent && previousContent !== nextContent) {
            previousContent.classList.remove('tab-content-active');
            previousContent.classList.add('hidden');
        }

        if (nextContent) {
            nextContent.classList.remove('hidden');
            nextContent.classList.add('tab-content-active');
        }

        renderActiveTab();
        setBreadcrumb([{ label: 'Config Builder' }, { label: TAB_LABELS[tabName] || tabName }]);
    }

    // ── Header buttons ───────────────────────────────────────────────────
    function setupHeaderButtons() {
        const btnSave = document.getElementById('btn-save');
        const btnBuild = document.getElementById('btn-build');
        const btnUndo = document.getElementById('btn-undo');
        const btnRedo = document.getElementById('btn-redo');
        const btnExport = document.getElementById('btn-export');
        const btnCopyJson = document.getElementById('btn-copy-json');
        const btnShortcuts = document.getElementById('btn-shortcuts');

        if (btnSave) btnSave.addEventListener('click', saveConfig);
        if (btnBuild) btnBuild.addEventListener('click', buildConfig);
        if (btnUndo) btnUndo.addEventListener('click', undo);
        if (btnRedo) btnRedo.addEventListener('click', redo);
        if (btnExport) btnExport.addEventListener('click', exportConfig);
        if (btnCopyJson) btnCopyJson.addEventListener('click', copyJsonToClipboard);
        if (btnShortcuts) btnShortcuts.addEventListener('click', toggleShortcutOverlay);

        updateUndoRedoButtons();
    }

    // ── Auto-save ────────────────────────────────────────────────────────
    function setupAutoSaveCheckbox() {
        const checkbox = document.getElementById('auto-save-checkbox');
        if (!checkbox) return;

        checkbox.checked = autoSaveEnabled;
        checkbox.addEventListener('change', () => {
            autoSaveEnabled = checkbox.checked;
            localStorage.setItem('ninjadog:autoSave', autoSaveEnabled);
            if (!autoSaveEnabled && autoSaveTimer) {
                clearTimeout(autoSaveTimer);
                autoSaveTimer = null;
            }
        });
    }

    function scheduleAutoSave() {
        if (!autoSaveEnabled) return;
        if (autoSaveTimer) clearTimeout(autoSaveTimer);
        autoSaveTimer = setTimeout(async () => {
            autoSaveTimer = null;
            if (!dirtyFlag) return;
            try {
                const jsonText = JSON.stringify(JsonPreview.getJson(state), null, 2);
                await NinjadogApi.saveConfig(jsonText);
                markClean();
                updateDirtyIndicator();
                showToast('Auto-saved', 'success', 2000);
            } catch {
                // Silent failure for auto-save; user can manually save
            }
        }, 3000);
    }

    // ── Save & Build ─────────────────────────────────────────────────────
    async function saveConfig() {
        try {
            const jsonText = JSON.stringify(JsonPreview.getJson(state), null, 2);
            await NinjadogApi.saveConfig(jsonText);
            markClean();
            updateDirtyIndicator();
            showToast('Configuration saved', 'success');
        } catch (err) {
            showToast('Save failed: ' + (err.message || 'Unknown error'), 'error');
        }
    }

    async function buildConfig() {
        try {
            // Save first
            const jsonText = JSON.stringify(JsonPreview.getJson(state), null, 2);
            await NinjadogApi.saveConfig(jsonText);
            markClean();
            updateDirtyIndicator();

            const result = await NinjadogApi.build();
            const success = result.success;

            // Show build console
            showBuildConsole(result);

            if (success) {
                showToast('Build succeeded', 'success');
            } else {
                showToast('Build failed', 'error');
            }
        } catch (err) {
            showToast('Build failed: ' + (err.message || 'Unknown error'), 'error');
        }
    }

    // ── Build Console ────────────────────────────────────────────────────
    function showBuildConsole(result) {
        const console_ = document.getElementById('build-console');
        const body = document.getElementById('build-console-body');
        const closeBtn = document.getElementById('build-console-close');
        if (!console_ || !body) return;

        console_.classList.remove('hidden');

        const lines = [];
        if (result.output) {
            lines.push(...(Array.isArray(result.output) ? result.output : [result.output]));
        }
        if (result.errors && result.errors.length > 0) {
            lines.push(...result.errors.map(e => '[ERROR] ' + e));
        }
        if (result.filesGenerated !== undefined) {
            lines.push(`--- ${result.filesGenerated} file(s) generated ---`);
        }
        if (lines.length === 0) {
            lines.push(result.success ? 'Build completed successfully.' : 'Build failed with no output.');
        }

        body.innerHTML = lines.map(line => {
            const color = line.startsWith('[ERROR]') ? '#f87171' : '#d1d5db';
            return `<div style="color:${color};font-size:12px;font-family:var(--font-mono);">${escHtml(line)}</div>`;
        }).join('');

        if (closeBtn) {
            closeBtn.onclick = () => console_.classList.add('hidden');
        }
    }

    // ── Render ────────────────────────────────────────────────────────────
    function renderActiveTab() {
        const container = document.getElementById(`tab-${activeTab}`);
        if (!container) return;

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

    // ── State change handler ─────────────────────────────────────────────
    let dirtyFlag = false;

    function onStateChanged() {
        dirtyFlag = true;
        JsonPreview.update(state);
        scheduleValidation();
        updateDirtyIndicator();
        updateTabBadges();
        scheduleAutoSave();
    }

    function markClean() {
        savedSnapshot = JSON.stringify(state);
        dirtyFlag = false;
    }

    // ── Dirty indicator ──────────────────────────────────────────────────
    function updateDirtyIndicator() {
        const isDirty = dirtyFlag;
        const dot = document.getElementById('dirty-indicator');
        const saveBtn = document.getElementById('btn-save');

        if (dot) {
            if (isDirty) {
                dot.classList.remove('hidden');
            } else {
                dot.classList.add('hidden');
            }
        }
        if (saveBtn) {
            if (isDirty) {
                saveBtn.classList.add('header-btn-dirty');
            } else {
                saveBtn.classList.remove('header-btn-dirty');
            }
        }
    }

    // ── Validation ───────────────────────────────────────────────────────
    function scheduleValidation() {
        if (validateTimer) clearTimeout(validateTimer);
        validateTimer = setTimeout(async () => {
            if (NinjadogApi.isServerDown()) return;
            try {
                const json = JsonPreview.getJson(state);
                const result = await NinjadogApi.validate(json);
                ValidationDisplay.render(result);
            } catch {
                ValidationDisplay.clear();
            }
        }, 500);
    }

    // ── Undo / Redo ──────────────────────────────────────────────────────
    function pushUndo() {
        undoStack.push(JSON.stringify(state));
        if (undoStack.length > MAX_UNDO) {
            undoStack.shift();
        }
        // Any new edit clears the redo stack
        redoStack = [];
        updateUndoRedoButtons();
    }

    function undo() {
        if (undoStack.length === 0) return;
        redoStack.push(JSON.stringify(state));
        state = JSON.parse(undoStack.pop());
        afterUndoRedo();
    }

    function redo() {
        if (redoStack.length === 0) return;
        undoStack.push(JSON.stringify(state));
        state = JSON.parse(redoStack.pop());
        afterUndoRedo();
    }

    function afterUndoRedo() {
        updateUndoRedoButtons();
        renderActiveTab();
        onStateChanged();
    }

    function updateUndoRedoButtons() {
        const btnUndo = document.getElementById('btn-undo');
        const btnRedo = document.getElementById('btn-redo');
        if (btnUndo) btnUndo.disabled = undoStack.length === 0;
        if (btnRedo) btnRedo.disabled = redoStack.length === 0;
    }

    // ── Toast notifications ──────────────────────────────────────────────
    function showToast(message, type, durationMs) {
        type = type || 'info';
        durationMs = durationMs || 3000;

        const container = document.getElementById('toast-container');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `toast ${TOAST_COLORS[type] || TOAST_COLORS.info}`;

        const icon = TOAST_ICONS[type] || TOAST_ICONS.info;
        toast.innerHTML = `<span style="flex-shrink:0;">${icon}</span><span>${escHtml(message)}</span>`;

        container.appendChild(toast);

        // Auto-remove after duration
        const removeTimer = setTimeout(() => dismissToast(toast), durationMs);
        toast.addEventListener('click', () => {
            clearTimeout(removeTimer);
            dismissToast(toast);
        });
    }

    function dismissToast(toast) {
        toast.classList.add('toast-exit');
        setTimeout(() => {
            if (toast.parentNode) toast.parentNode.removeChild(toast);
        }, 250);
    }

    // ── Tab badges ───────────────────────────────────────────────────────
    function updateTabBadges() {
        const entityCount = state.entities ? Object.keys(state.entities).length : 0;
        const enumCount = state.enums ? Object.keys(state.enums).length : 0;

        let seedCount = 0;
        if (state.entities) {
            for (const entity of Object.values(state.entities)) {
                if (entity.seedData && entity.seedData.length > 0) {
                    seedCount += entity.seedData.length;
                }
            }
        }

        setBadge('badge-entities', entityCount);
        setBadge('badge-enums', enumCount);
        setBadge('badge-seed', seedCount);
    }

    function setBadge(id, count) {
        const badge = document.getElementById(id);
        if (!badge) return;
        if (count > 0) {
            badge.textContent = count;
            badge.classList.remove('hidden');
        } else {
            badge.classList.add('hidden');
        }
    }

    // ── Breadcrumb ───────────────────────────────────────────────────────
    function setBreadcrumb(segments) {
        const bar = document.getElementById('breadcrumb-bar');
        if (!bar) return;

        if (!segments || segments.length <= 1) {
            bar.classList.add('hidden');
            return;
        }

        bar.classList.remove('hidden');
        bar.innerHTML = segments.map((seg, i) => {
            const isLast = i === segments.length - 1;
            const separator = i > 0
                ? '<svg width="12" height="12" style="color:#4b5563;margin:0 6px;flex-shrink:0;" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/></svg>'
                : '';
            const clickable = seg.tab && !isLast
                ? `class="text-gray-400 hover:text-gray-200 cursor-pointer transition-colors" data-breadcrumb-tab="${seg.tab}"`
                : `class="${isLast ? 'text-gray-200' : 'text-gray-400'}"`;
            return `${separator}<span ${clickable}>${escHtml(seg.label)}</span>`;
        }).join('');

        // Bind click handlers for navigable breadcrumb segments
        bar.querySelectorAll('[data-breadcrumb-tab]').forEach(el => {
            el.addEventListener('click', () => switchTab(el.dataset.breadcrumbTab));
        });
    }

    // ── Keyboard shortcuts ───────────────────────────────────────────────
    function setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            const mod = e.metaKey || e.ctrlKey;
            const tag = (e.target.tagName || '').toLowerCase();
            const isInput = tag === 'input' || tag === 'textarea' || e.target.isContentEditable;

            // Ctrl+S / Cmd+S — Save
            if (mod && e.key === 's') {
                e.preventDefault();
                saveConfig();
                return;
            }

            // Ctrl+Z / Cmd+Z — Undo
            if (mod && e.key === 'z' && !e.shiftKey) {
                e.preventDefault();
                undo();
                return;
            }

            // Ctrl+Shift+Z / Cmd+Shift+Z — Redo
            if (mod && e.key === 'z' && e.shiftKey) {
                e.preventDefault();
                redo();
                return;
            }

            // Ctrl+Y / Cmd+Y — Redo (alternative)
            if (mod && e.key === 'y') {
                e.preventDefault();
                redo();
                return;
            }

            // Ctrl+B / Cmd+B — Build
            if (mod && e.key === 'b') {
                e.preventDefault();
                buildConfig();
                return;
            }

            // ? — Toggle shortcut overlay (only when not typing)
            if (e.key === '?' && !isInput && !mod) {
                e.preventDefault();
                toggleShortcutOverlay();
                return;
            }

            // Escape — Close overlays
            if (e.key === 'Escape') {
                closeAllOverlays();
                return;
            }
        });
    }

    // ── Shortcut overlay ─────────────────────────────────────────────────
    function toggleShortcutOverlay() {
        const overlay = document.getElementById('shortcut-overlay');
        if (!overlay) return;

        if (overlay.classList.contains('hidden')) {
            overlay.classList.remove('hidden');
            // Close on backdrop click
            overlay.addEventListener('click', onOverlayBackdropClick);
            // Close button
            const closeBtn = overlay.querySelector('#shortcut-overlay-close');
            if (closeBtn) closeBtn.onclick = () => toggleShortcutOverlay();
        } else {
            overlay.classList.add('hidden');
            overlay.removeEventListener('click', onOverlayBackdropClick);
        }
    }

    function onOverlayBackdropClick(e) {
        const overlay = document.getElementById('shortcut-overlay');
        if (e.target === overlay) {
            overlay.classList.add('hidden');
            overlay.removeEventListener('click', onOverlayBackdropClick);
        }
    }

    function closeAllOverlays() {
        const shortcutOverlay = document.getElementById('shortcut-overlay');
        if (shortcutOverlay) shortcutOverlay.classList.add('hidden');

        const templatePicker = document.getElementById('template-picker');
        if (templatePicker) templatePicker.classList.add('hidden');

        const importModal = document.getElementById('import-modal');
        if (importModal) importModal.classList.add('hidden');

        const buildConsole = document.getElementById('build-console');
        if (buildConsole) buildConsole.classList.add('hidden');
    }

    // ── View mode toggle ─────────────────────────────────────────────────
    function setupViewModeButtons() {
        document.querySelectorAll('[data-view]').forEach(btn => {
            btn.addEventListener('click', () => {
                const mode = btn.dataset.view;
                applyViewMode(mode);

                // Update active state on buttons
                document.querySelectorAll('[data-view]').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
            });
        });
    }

    function applyViewMode(mode) {
        const leftPanel = document.getElementById('left-panel');
        const jsonPanel = document.getElementById('json-panel');
        const resizeHandle = document.getElementById('resize-handle');

        if (!leftPanel || !jsonPanel) return;

        switch (mode) {
            case 'form':
                leftPanel.classList.remove('hidden');
                jsonPanel.classList.add('hidden');
                if (resizeHandle) resizeHandle.classList.add('hidden');
                // Reset JSON panel sizing
                jsonPanel.style.width = '';
                jsonPanel.style.flexBasis = '';
                jsonPanel.style.flexGrow = '';
                jsonPanel.style.flexShrink = '';
                break;
            case 'json':
                leftPanel.classList.add('hidden');
                jsonPanel.classList.remove('hidden');
                if (resizeHandle) resizeHandle.classList.add('hidden');
                // Expand JSON panel to fill all available width
                jsonPanel.style.width = '100%';
                jsonPanel.style.flexBasis = '100%';
                jsonPanel.style.flexGrow = '1';
                jsonPanel.style.flexShrink = '1';
                break;
            case 'split':
            default:
                leftPanel.classList.remove('hidden');
                jsonPanel.classList.remove('hidden');
                if (resizeHandle) resizeHandle.classList.remove('hidden');
                // Reset JSON panel to default fixed width
                jsonPanel.style.width = '420px';
                jsonPanel.style.flexBasis = '';
                jsonPanel.style.flexGrow = '0';
                jsonPanel.style.flexShrink = '0';
                break;
        }
    }

    // ── Resizable split panel ────────────────────────────────────────────
    function setupResizablePanel() {
        const handle = document.getElementById('resize-handle');
        if (!handle) return;

        handle.addEventListener('mousedown', (e) => {
            e.preventDefault();
            isResizing = true;
            document.body.style.cursor = 'col-resize';
            document.body.style.userSelect = 'none';

            document.addEventListener('mousemove', onResizeMove);
            document.addEventListener('mouseup', onResizeEnd);
        });
    }

    function onResizeMove(e) {
        if (!isResizing) return;

        const jsonPanel = document.getElementById('json-panel');
        if (!jsonPanel) return;

        const viewportWidth = window.innerWidth;
        const panelRight = viewportWidth - e.clientX;
        const minWidth = 250;
        const maxWidth = viewportWidth * 0.6;

        const clampedWidth = Math.min(Math.max(panelRight, minWidth), maxWidth);
        jsonPanel.style.width = clampedWidth + 'px';
        jsonPanel.style.flexBasis = clampedWidth + 'px';
        jsonPanel.style.flexGrow = '0';
        jsonPanel.style.flexShrink = '0';
    }

    function onResizeEnd() {
        isResizing = false;
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
        document.removeEventListener('mousemove', onResizeMove);
        document.removeEventListener('mouseup', onResizeEnd);
    }

    // ── Export / Download ────────────────────────────────────────────────
    function exportConfig() {
        const json = JsonPreview.getJson(state);
        const text = JSON.stringify(json, null, 2);
        const blob = new Blob([text], { type: 'application/json' });
        const url = URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = 'ninjadog.json';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);

        showToast('Configuration exported', 'success');
    }

    // ── Copy JSON ────────────────────────────────────────────────────────
    async function copyJsonToClipboard() {
        const json = JsonPreview.getJson(state);
        const text = JSON.stringify(json, null, 2);
        try {
            await navigator.clipboard.writeText(text);
            showToast('Copied!', 'success', 2000);
        } catch {
            showToast('Copy failed', 'error');
        }
    }

    // ── Template picker ──────────────────────────────────────────────────
    function showTemplatePicker() {
        const picker = document.getElementById('template-picker');
        if (!picker) return;

        picker.classList.remove('hidden');

        // Bind template buttons
        picker.querySelectorAll('[data-template]').forEach(btn => {
            btn.onclick = () => {
                const key = btn.dataset.template;
                const template = TEMPLATES[key];
                if (template) {
                    pushUndo();
                    state = JSON.parse(JSON.stringify(template));
                    onStateChanged();
                    renderActiveTab();
                    showToast('Template applied: ' + (key.charAt(0).toUpperCase() + key.slice(1)), 'success');
                }
                picker.classList.add('hidden');
            };
        });

        // Close on backdrop click or cancel button
        picker.addEventListener('click', (e) => {
            if (e.target === picker) picker.classList.add('hidden');
        }, { once: true });
        const closeBtn = picker.querySelector('#template-picker-close');
        if (closeBtn) closeBtn.onclick = () => picker.classList.add('hidden');
    }

    // ── Import modal ─────────────────────────────────────────────────────
    let importModalCallback = null;

    function showImportModal(entityName, callback) {
        const modal = document.getElementById('import-modal');
        if (!modal) return;

        importModalCallback = callback;

        const title = modal.querySelector('.import-modal-title');
        if (title) title.textContent = 'Import data for ' + entityName;

        const textarea = modal.querySelector('#import-textarea');
        if (textarea) textarea.value = '';

        const confirmBtn = modal.querySelector('#import-confirm');
        const cancelBtn = modal.querySelector('#import-cancel');

        modal.classList.remove('hidden');

        if (confirmBtn) {
            confirmBtn.onclick = () => {
                const raw = textarea ? textarea.value.trim() : '';
                if (!raw) {
                    modal.classList.add('hidden');
                    return;
                }

                const parsed = parseImportData(raw);
                if (parsed === null) {
                    showToast('Could not parse import data. Use JSON array or CSV format.', 'error', 4000);
                    return;
                }

                modal.classList.add('hidden');
                if (importModalCallback) {
                    importModalCallback(parsed);
                    importModalCallback = null;
                }
            };
        }

        if (cancelBtn) {
            cancelBtn.onclick = () => {
                modal.classList.add('hidden');
                importModalCallback = null;
            };
        }

        // Close on backdrop
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                modal.classList.add('hidden');
                importModalCallback = null;
            }
        }, { once: true });
    }

    function parseImportData(raw) {
        // Try JSON array first
        try {
            const parsed = JSON.parse(raw);
            if (Array.isArray(parsed)) return parsed;
            // Single object — wrap in array
            if (typeof parsed === 'object' && parsed !== null) return [parsed];
        } catch {
            // Not JSON, try CSV
        }

        // Try CSV
        const lines = raw.split('\n').map(l => l.trim()).filter(l => l.length > 0);
        if (lines.length < 2) return null; // Need at least header + 1 row

        const headers = lines[0].split(',').map(h => h.trim());
        const rows = [];

        for (let i = 1; i < lines.length; i++) {
            const values = parseCSVLine(lines[i]);
            if (values.length !== headers.length) continue;

            const obj = {};
            for (let j = 0; j < headers.length; j++) {
                const val = values[j].trim();
                // Auto-detect types
                if (val === 'true' || val === 'false') {
                    obj[headers[j]] = val === 'true';
                } else if (val !== '' && !isNaN(Number(val))) {
                    obj[headers[j]] = Number(val);
                } else {
                    obj[headers[j]] = val;
                }
            }
            rows.push(obj);
        }

        return rows.length > 0 ? rows : null;
    }

    function parseCSVLine(line) {
        const result = [];
        let current = '';
        let inQuotes = false;

        for (let i = 0; i < line.length; i++) {
            const ch = line[i];
            if (ch === '"') {
                if (inQuotes && i + 1 < line.length && line[i + 1] === '"') {
                    current += '"';
                    i++;
                } else {
                    inQuotes = !inQuotes;
                }
            } else if (ch === ',' && !inQuotes) {
                result.push(current);
                current = '';
            } else {
                current += ch;
            }
        }
        result.push(current);
        return result;
    }

    // ── Skeleton loading ─────────────────────────────────────────────────
    function showSkeletons(show) {
        document.querySelectorAll('.skeleton-loader').forEach(el => {
            if (show) {
                el.classList.remove('hidden');
            } else {
                el.classList.add('hidden');
            }
        });

        // Hide actual content containers during skeleton display
        document.querySelectorAll('.tab-content').forEach(el => {
            if (show) {
                el.classList.add('skeleton-hidden');
            } else {
                el.classList.remove('skeleton-hidden');
            }
        });
    }

    // ── Entity color coding ──────────────────────────────────────────────
    function getEntityColor(name) {
        let hash = 0;
        for (let i = 0; i < name.length; i++) {
            hash = name.charCodeAt(i) + ((hash << 5) - hash);
        }
        const hue = Math.abs(hash) % 360;
        return `hsl(${hue}, 65%, 55%)`;
    }

    // ── Accessor ─────────────────────────────────────────────────────────
    function getState() {
        return state;
    }

    // ── Utility ──────────────────────────────────────────────────────────
    function escHtml(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    // ── Boot ─────────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', init);

    // ── Public API ───────────────────────────────────────────────────────
    return {
        onStateChanged,
        showToast,
        pushUndo,
        undo,
        redo,
        getState,
        updateTabBadges,
        setBreadcrumb,
        showImportModal,
        showTemplatePicker,
        getEntityColor
    };
})();
