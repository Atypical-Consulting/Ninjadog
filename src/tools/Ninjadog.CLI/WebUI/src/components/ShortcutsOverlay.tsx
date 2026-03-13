import { useUiStore } from '../store/ui-store';

const SHORTCUTS = [
  { keys: ['Ctrl', 'S'], desc: 'Save configuration' },
  { keys: ['Ctrl', 'B'], desc: 'Build project' },
  { keys: ['Ctrl', 'Z'], desc: 'Undo' },
  { keys: ['Ctrl', 'Y'], desc: 'Redo' },
  { keys: ['Ctrl', 'E'], desc: 'Add entity' },
  { keys: ['1', '\u2013', '4'], desc: 'Switch tabs' },
  { keys: ['Ctrl', 'Shift', 'A'], desc: 'AI Assistant' },
  { keys: ['?'], desc: 'Show this overlay' },
];

export default function ShortcutsOverlay() {
  const open = useUiStore((s) => s.shortcutsOpen);
  const setOpen = useUiStore((s) => s.setShortcutsOpen);

  return (
    <div id="shortcut-overlay" className={`modal-overlay${!open ? ' hidden' : ''}`} onClick={(e) => e.target === e.currentTarget && setOpen(false)}>
      <div className="modal-panel shortcut-panel">
        <div className="flex items-center justify-between mb-4">
          <h2 className="shortcut-title" style={{ marginBottom: 0 }}>Keyboard Shortcuts</h2>
          <button id="shortcut-overlay-close" className="header-btn-icon" style={{ width: 28, height: 28 }} onClick={() => setOpen(false)}>&times;</button>
        </div>
        <div className="shortcut-grid">
          {SHORTCUTS.map((s, i) => (
            <div key={i} className="shortcut-row">
              <span className="shortcut-key">
                {s.keys.map((k, j) => <kbd key={j}>{k}</kbd>)}
              </span>
              <span className="shortcut-desc">{s.desc}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
