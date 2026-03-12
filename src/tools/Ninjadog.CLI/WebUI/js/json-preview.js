/**
 * JSON Editor panel — Monaco-powered JSON editor with schema validation.
 */
const JsonPreview = (() => {
    let editor = null;
    let isUpdatingFromState = false;
    let onEditCallback = null;
    let debounceTimer = null;

    async function init(initialState) {
        // Configure AMD loader for Monaco
        require.config({
            paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs' }
        });

        return new Promise((resolve) => {
            require(['vs/editor/editor.main'], async function () {
                // Define custom dark theme matching Phantom Protocol
                monaco.editor.defineTheme('ninjadog-dark', {
                    base: 'vs-dark',
                    inherit: true,
                    rules: [
                        { token: 'string.key.json', foreground: 'ff6b8a' },
                        { token: 'string.value.json', foreground: '34d399' },
                        { token: 'number', foreground: 'fbbf24' },
                        { token: 'keyword', foreground: 'a78bfa' },
                    ],
                    colors: {
                        'editor.background': '#08090e',
                        'editor.foreground': '#e4e7f1',
                        'editor.lineHighlightBackground': '#1c203020',
                        'editorLineNumber.foreground': '#5c6075',
                        'editorLineNumber.activeForeground': '#8b90a8',
                        'editor.selectionBackground': '#FF2D5530',
                        'editorCursor.foreground': '#FF2D55',
                        'editorWidget.background': '#151821',
                        'editorWidget.border': '#282d42',
                        'input.background': '#0e1017',
                        'input.border': '#1e2235',
                        'editorSuggestWidget.background': '#151821',
                        'editorSuggestWidget.border': '#282d42',
                        'editorSuggestWidget.selectedBackground': '#242838',
                        'editorGutter.background': '#08090e',
                    }
                });

                // Fetch and register JSON schema for validation + autocomplete
                try {
                    const schema = await fetch('/api/schema').then(r => r.json());
                    monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
                        validate: true,
                        schemas: [{
                            uri: 'https://ninjadog.dev/schemas/ninjadog.schema.json',
                            fileMatch: ['*'],
                            schema: schema
                        }],
                        allowComments: false,
                        trailingCommas: 'error'
                    });
                } catch {
                    // Schema fetch failed — editor still works without validation
                }

                // Create the editor
                const container = document.getElementById('monaco-container');
                const jsonContent = JSON.stringify(buildJson(initialState), null, 2);

                editor = monaco.editor.create(container, {
                    value: jsonContent,
                    language: 'json',
                    theme: 'ninjadog-dark',
                    fontSize: 12,
                    fontFamily: "'JetBrains Mono', monospace",
                    minimap: { enabled: false },
                    lineNumbers: 'on',
                    folding: true,
                    foldingStrategy: 'indentation',
                    scrollBeyondLastLine: false,
                    automaticLayout: true,
                    padding: { top: 12, bottom: 12 },
                    renderLineHighlight: 'line',
                    scrollbar: {
                        verticalScrollbarSize: 6,
                        horizontalScrollbarSize: 6,
                    },
                    tabSize: 2,
                    wordWrap: 'on',
                    suggest: {
                        showKeywords: true,
                        showSnippets: true,
                    },
                });

                // Listen for user edits with debounce
                editor.onDidChangeModelContent(() => {
                    if (isUpdatingFromState) return;
                    if (debounceTimer) clearTimeout(debounceTimer);
                    debounceTimer = setTimeout(() => {
                        try {
                            const parsed = JSON.parse(editor.getValue());
                            if (onEditCallback) onEditCallback(parsed);
                        } catch {
                            // Invalid JSON — Monaco shows squiggles, don't update state
                        }
                    }, 300);
                });

                resolve();
            });
        });
    }

    function update(state) {
        if (!editor) return;
        const json = buildJson(state);
        const text = JSON.stringify(json, null, 2);
        const currentText = editor.getValue();
        if (text === currentText) return;

        isUpdatingFromState = true;
        const scrollTop = editor.getScrollTop();
        editor.setValue(text);
        editor.setScrollTop(scrollTop);
        isUpdatingFromState = false;
    }

    function onExternalEdit(callback) {
        onEditCallback = callback;
    }

    function buildJson(state) {
        const result = {};
        if (state['$schema']) {
            result['$schema'] = state['$schema'];
        }
        if (state.config && Object.keys(state.config).length > 0) {
            result.config = state.config;
        }
        if (state.entities && Object.keys(state.entities).length > 0) {
            result.entities = cleanEntities(state.entities);
        }
        if (state.enums && Object.keys(state.enums).length > 0) {
            result.enums = state.enums;
        }
        return result;
    }

    function cleanEntities(entities) {
        const cleaned = {};
        for (const [name, entity] of Object.entries(entities)) {
            const e = {};
            if (entity.properties && Object.keys(entity.properties).length > 0) {
                e.properties = entity.properties;
            }
            if (entity.relationships && Object.keys(entity.relationships).length > 0) {
                e.relationships = entity.relationships;
            }
            if (entity.seedData && entity.seedData.length > 0) {
                e.seedData = entity.seedData;
            }
            cleaned[name] = e;
        }
        return cleaned;
    }

    function getJson(state) {
        return buildJson(state);
    }

    return { init, update, getJson, onExternalEdit };
})();
