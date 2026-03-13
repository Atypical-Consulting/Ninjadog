import { useState } from 'react';
import { useUiStore } from '../store/ui-store';

function parseCSVLine(line: string): string[] {
  const result: string[] = [];
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

function parseImportData(raw: string): Record<string, unknown>[] | null {
  try {
    const parsed = JSON.parse(raw);
    if (Array.isArray(parsed)) return parsed;
    if (typeof parsed === 'object' && parsed !== null) return [parsed];
  } catch {
    // Not JSON
  }

  const lines = raw.split('\n').map((l) => l.trim()).filter((l) => l.length > 0);
  if (lines.length < 2) return null;

  const headers = lines[0].split(',').map((h) => h.trim());
  const rows: Record<string, unknown>[] = [];

  for (let i = 1; i < lines.length; i++) {
    const values = parseCSVLine(lines[i]);
    if (values.length !== headers.length) continue;
    const obj: Record<string, unknown> = {};
    for (let j = 0; j < headers.length; j++) {
      const val = values[j].trim();
      if (val === 'true' || val === 'false') obj[headers[j]] = val === 'true';
      else if (val !== '' && !isNaN(Number(val))) obj[headers[j]] = Number(val);
      else obj[headers[j]] = val;
    }
    rows.push(obj);
  }

  return rows.length > 0 ? rows : null;
}

export default function ImportModal() {
  const { open, entityName, callback } = useUiStore((s) => s.importModal);
  const close = useUiStore((s) => s.closeImportModal);
  const showToast = useUiStore((s) => s.showToast);
  const [text, setText] = useState('');

  const handleImport = () => {
    const raw = text.trim();
    if (!raw) {
      close();
      return;
    }
    const parsed = parseImportData(raw);
    if (!parsed) {
      showToast('Could not parse import data. Use JSON array or CSV format.', 'error', 4000);
      return;
    }
    close();
    setText('');
    if (callback) callback(parsed);
  };

  const handleCancel = () => {
    close();
    setText('');
  };

  return (
    <div id="import-modal" className={`modal-overlay${!open ? ' hidden' : ''}`} onClick={(e) => e.target === e.currentTarget && handleCancel()}>
      <div className="modal-panel import-modal-panel">
        <h2 className="import-modal-title">Import data for {entityName}</h2>
        <p className="text-xs text-gray-400 mb-3">
          Paste CSV or JSON data below. CSV should have a header row matching property names.
        </p>
        <textarea
          id="import-textarea"
          className="import-textarea"
          placeholder={'name,email,age\nJohn,john@example.com,30\nJane,jane@example.com,25'}
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <div className="import-actions">
          <button id="import-cancel" className="btn-sm btn-ghost" onClick={handleCancel}>Cancel</button>
          <button id="import-confirm" className="btn-sm btn-primary" onClick={handleImport}>Import</button>
        </div>
      </div>
    </div>
  );
}
