import { useUiStore } from '../store/ui-store';

export default function BuildConsole() {
  const { open, lines } = useUiStore((s) => s.buildConsole);
  const close = useUiStore((s) => s.closeBuildConsole);

  return (
    <div id="build-console" className={`build-console${!open ? ' hidden' : ''}`}>
      <div className="build-console-header">
        <span>Build Output</span>
        <button
          id="build-console-close"
          className="text-gray-400 hover:text-white"
          style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.25rem' }}
          onClick={close}
        >
          &times;
        </button>
      </div>
      <div className="build-console-body">
        {lines.map((line, i) => (
          <div
            key={i}
            style={{
              color: line.startsWith('[ERROR]') ? '#f87171' : '#d1d5db',
              fontSize: 12,
              fontFamily: 'var(--font-mono)',
            }}
          >
            {line}
          </div>
        ))}
      </div>
    </div>
  );
}
