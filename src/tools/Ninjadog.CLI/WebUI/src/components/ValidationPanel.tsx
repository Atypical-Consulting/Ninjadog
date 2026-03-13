import { useState, useEffect } from 'react';
import { getValidationResult, onValidationChange, type Diagnostic } from '../hooks/useValidation';

function escHtml(s: string) {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function diagClass(severity: number) {
  if (severity === 2) return 'diag-error';
  if (severity === 1) return 'diag-warning';
  return 'diag-info';
}

function diagIcon(severity: number) {
  if (severity === 2) return '\u2716';
  if (severity === 1) return '\u26A0';
  return '\u2139';
}

export default function ValidationPanel() {
  const [diagnostics, setDiagnostics] = useState<Diagnostic[]>([]);

  useEffect(() => {
    const update = () => {
      const result = getValidationResult();
      setDiagnostics(result?.diagnostics || []);
    };
    update();
    return onValidationChange(update);
  }, []);

  return (
    <div id="validation-panel" className="shrink-0 border-t border-bdr bg-surface-base max-h-40 overflow-auto">
      <div className="validation-header px-4 py-2 text-[11px] font-medium text-gray-500 border-b border-bdr-dim uppercase tracking-widest font-mono">
        Validation
      </div>
      <div className="px-4 py-2 space-y-1">
        {diagnostics.length === 0 ? (
          <p className="text-xs text-gray-500">No validation issues.</p>
        ) : (
          diagnostics.map((d, i) => (
            <div key={i} className={`flex items-start gap-2 text-xs ${diagClass(d.severity)}`}>
              <span>{diagIcon(d.severity)}</span>
              <span className="text-gray-400">[{d.path}]</span>
              <span dangerouslySetInnerHTML={{ __html: escHtml(d.message) }} />
            </div>
          ))
        )}
      </div>
    </div>
  );
}
