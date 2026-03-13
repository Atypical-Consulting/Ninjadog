import { useState } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';

/* eslint-disable @typescript-eslint/no-explicit-any */

function getEntityColor(name: string) {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  const hue = Math.abs(hash) % 360;
  return `hsl(${hue}, 65%, 55%)`;
}

export default function EnumsPage() {
  const state = useConfigStore((s) => s.state);
  const setState = useConfigStore((s) => s.setState);
  const pushUndo = useConfigStore((s) => s.pushUndo);
  const onStateChanged = useConfigStore((s) => s.onStateChanged);
  const showToast = useUiStore((s) => s.showToast);

  const [addFormOpen, setAddFormOpen] = useState(false);
  const [addInput, setAddInput] = useState('');
  const [confirmDelete, setConfirmDelete] = useState<Record<string, boolean>>({});

  const enums = (state.enums || {}) as Record<string, string[]>;
  const names = Object.keys(enums);

  const mutate = (updater: (s: any) => void) => {
    const newState = JSON.parse(JSON.stringify(state));
    newState.enums = newState.enums || {};
    updater(newState);
    setState(newState);
    onStateChanged();
  };

  const addEnum = () => {
    const name = addInput.trim();
    if (!name) return;
    pushUndo();
    mutate((s: any) => { s.enums[name] = []; });
    setAddFormOpen(false);
    setAddInput('');
  };

  const removeEnum = (name: string) => {
    if (!confirmDelete[name]) {
      setConfirmDelete({ ...confirmDelete, [name]: true });
      setTimeout(() => setConfirmDelete((cd) => { const n = { ...cd }; delete n[name]; return n; }), 3000);
      return;
    }
    pushUndo();
    mutate((s: any) => {
      delete s.enums[name];
      if (Object.keys(s.enums).length === 0) delete s.enums;
    });
    setConfirmDelete((cd) => { const n = { ...cd }; delete n[name]; return n; });
    showToast(`Removed enum "${name}"`, 'success');
  };

  const removeValue = (enumName: string, index: number) => {
    pushUndo();
    mutate((s: any) => { s.enums[enumName].splice(index, 1); });
  };

  const addValue = (enumName: string, value: string) => {
    if (!value.trim()) return;
    pushUndo();
    mutate((s: any) => { s.enums[enumName].push(value.trim()); });
  };

  return (
    <div id="tab-enums" className="tab-content-active">
      <div className="flex items-center justify-between mb-4">
        <div className="section-title mb-0">Enums ({names.length})</div>
        <div>
          <button id="btn-add-enum" className={`btn-sm btn-primary${addFormOpen ? ' hidden' : ''}`} onClick={() => setAddFormOpen(true)}>+ Add Enum</button>
          <div className={`inline-add-form${!addFormOpen ? ' hidden' : ''}`}>
            <input
              id="enum-add-input"
              className="field-input text-sm py-1"
              style={{ width: 200 }}
              placeholder="Enum name (PascalCase)"
              value={addInput}
              onChange={(e) => setAddInput(e.target.value)}
              onKeyDown={(e) => { if (e.key === 'Enter') addEnum(); if (e.key === 'Escape') setAddFormOpen(false); }}
              autoFocus={addFormOpen}
            />
            <button id="enum-add-confirm" className="btn-sm btn-primary" onClick={addEnum}>Create</button>
            <button className="btn-sm btn-ghost" onClick={() => setAddFormOpen(false)}>Cancel</button>
          </div>
        </div>
      </div>

      {names.map((name) => (
        <EnumCard
          key={name}
          name={name}
          values={enums[name]}
          confirmingDelete={!!confirmDelete[name]}
          onRemove={() => removeEnum(name)}
          onRemoveValue={(i) => removeValue(name, i)}
          onAddValue={(v) => addValue(name, v)}
        />
      ))}
    </div>
  );
}

function EnumCard({
  name, values, confirmingDelete, onRemove, onRemoveValue, onAddValue,
}: {
  name: string;
  values: string[];
  confirmingDelete: boolean;
  onRemove: () => void;
  onRemoveValue: (index: number) => void;
  onAddValue: (value: string) => void;
}) {
  const [input, setInput] = useState('');

  const handleAdd = () => {
    if (!input.trim()) return;
    onAddValue(input);
    setInput('');
  };

  return (
    <div className="entity-card" data-enum={name}>
      <div className="entity-header">
        <span className="font-medium text-sm">
          <span className="entity-color-dot" style={{ background: getEntityColor(name) }} />
          {name}
        </span>
        <div className="flex items-center gap-2">
          <span className="text-xs text-gray-500">{values.length} values</span>
          <button
            className={`btn-sm enum-remove ${confirmingDelete ? 'btn-confirm-danger' : 'btn-danger'}`}
            data-enum={name}
            onClick={onRemove}
          >
            {confirmingDelete ? 'Sure?' : 'Remove'}
          </button>
        </div>
      </div>
      <div className="entity-body">
        <div className="flex flex-wrap gap-2 mb-2">
          {values.map((v, i) => (
            <span key={i} className="enum-value-tag">
              {v}
              <button className="enum-val-remove" data-enum={name} data-index={i} onClick={() => onRemoveValue(i)}>&times;</button>
            </span>
          ))}
        </div>
        <div className="flex items-center gap-2">
          <input
            className="field-input text-xs py-1 flex-1 enum-val-input"
            data-enum={name}
            placeholder="New value..."
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => { if (e.key === 'Enter') handleAdd(); }}
          />
          <button className="btn-sm btn-ghost enum-val-add" data-enum={name} onClick={handleAdd}>Add</button>
        </div>
      </div>
    </div>
  );
}
