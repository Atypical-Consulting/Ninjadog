import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';

export default function Header() {
  const dirty = useConfigStore((s) => s.dirty);
  const undoStack = useConfigStore((s) => s.undoStack);
  const redoStack = useConfigStore((s) => s.redoStack);
  const undo = useConfigStore((s) => s.undo);
  const redo = useConfigStore((s) => s.redo);
  const saveConfig = useConfigStore((s) => s.saveConfig);
  const buildConfig = useConfigStore((s) => s.buildConfig);
  const getJson = useConfigStore((s) => s.getJson);
  const autoSaveEnabled = useConfigStore((s) => s.autoSaveEnabled);
  const setAutoSave = useConfigStore((s) => s.setAutoSave);
  const showToast = useUiStore((s) => s.showToast);
  const toggleShortcuts = useUiStore((s) => s.toggleShortcuts);
  const connected = useUiStore((s) => s.connected);
  const showReconnected = useUiStore((s) => s.showReconnected);
  const showBuildConsole = useUiStore((s) => s.showBuildConsole);

  const handleSave = async () => {
    try {
      await saveConfig();
      showToast('Configuration saved', 'success');
    } catch (err) {
      showToast('Save failed: ' + ((err as Error).message || 'Unknown error'), 'error');
    }
  };

  const handleBuild = async () => {
    try {
      const result = await buildConfig();
      showBuildConsole([result.message || (result.success ? 'Build completed.' : 'Build failed.')]);
      showToast(result.success ? 'Build succeeded' : 'Build failed', result.success ? 'success' : 'error');
    } catch (err) {
      showToast('Build failed: ' + ((err as Error).message || 'Unknown error'), 'error');
    }
  };

  const handleExport = () => {
    const json = getJson();
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
  };

  return (
    <header className="header-bar flex items-center justify-between px-6 py-3 shrink-0">
      <div className="flex items-center gap-3">
        <div className="ninja-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24" width="28" height="28" fill="currentColor">
            <path d="M12 2L9 9L2 12L9 15L12 22L15 15L22 12L15 9L12 2Z" />
          </svg>
        </div>
        <div className="flex flex-col leading-tight">
          <div className="flex items-center gap-2">
            <h1 className="text-lg font-display tracking-wide text-white">NINJADOG</h1>
            <span id="dirty-indicator" className={`dirty-dot${!dirty ? ' hidden' : ''}`} title="Unsaved changes" />
          </div>
          <span className="text-[10px] uppercase tracking-[0.2em] text-gray-500 -mt-0.5">
            Config Builder
          </span>
        </div>
        {!connected && (
          <span className="status-badge text-xs px-2 py-0.5 rounded-full bg-red-900 text-red-300 ml-2">
            Server disconnected
          </span>
        )}
        {connected && showReconnected && (
          <span className="status-badge text-xs px-2 py-0.5 rounded-full bg-emerald-900 text-emerald-300 ml-2">
            Reconnected
          </span>
        )}
      </div>

      <div className="flex items-center gap-2">
        <button
          id="btn-undo"
          className="header-btn-icon"
          title="Undo (Ctrl+Z)"
          disabled={undoStack.length === 0}
          onClick={undo}
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="1 4 1 10 7 10" /><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" /></svg>
        </button>
        <button
          id="btn-redo"
          className="header-btn-icon"
          title="Redo (Ctrl+Y)"
          disabled={redoStack.length === 0}
          onClick={redo}
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="23 4 23 10 17 10" /><path d="M20.49 15a9 9 0 1 1-2.13-9.36L23 10" /></svg>
        </button>
        <div className="header-separator" />

        <label className="auto-save-toggle" title="Auto-save">
          <input
            type="checkbox"
            checked={autoSaveEnabled}
            onChange={(e) => setAutoSave(e.target.checked)}
          />
          <span className="auto-save-label">Auto</span>
        </label>

        <button id="btn-export" className="header-btn-icon" title="Download JSON" onClick={handleExport}>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" /><polyline points="7 10 12 15 17 10" /><line x1="12" y1="15" x2="12" y2="3" /></svg>
        </button>

        <button id="btn-shortcuts" className="header-btn-icon" title="Keyboard Shortcuts (?)" onClick={toggleShortcuts}>
          <span style={{ fontSize: '14px', fontWeight: 700 }}>?</span>
        </button>
        <div className="header-separator" />

        <button id="btn-save" className={`header-btn header-btn-secondary${dirty ? ' header-btn-dirty' : ''}`} onClick={handleSave}>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" /><polyline points="17 21 17 13 7 13 7 21" /><polyline points="7 3 7 8 15 8" /></svg>
          Save
        </button>
        <button id="btn-build" className="header-btn header-btn-primary" onClick={handleBuild}>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" /></svg>
          Build
        </button>
      </div>
    </header>
  );
}
