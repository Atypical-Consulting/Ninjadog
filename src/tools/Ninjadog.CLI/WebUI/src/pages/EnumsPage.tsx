import { useState, useRef, useEffect } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';

/* eslint-disable @typescript-eslint/no-explicit-any */

function getEntityColor(name: string) {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  const hue = Math.abs(hash) % 360;
  return `hsl(${hue}, 65%, 55%)`;
}

function AutoFocusInput({ id, value, onChange, onKeyDown, placeholder, className, style }: {
  id?: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  placeholder: string;
  className?: string;
  style?: React.CSSProperties;
}) {
  const ref = useRef<HTMLInputElement>(null);
  useEffect(() => { ref.current?.focus(); }, []);
  return (
    <input
      ref={ref}
      id={id}
      className={className}
      style={style}
      placeholder={placeholder}
      value={value}
      onChange={onChange}
      onKeyDown={onKeyDown}
    />
  );
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
          {addFormOpen ? (
            <div className="inline-add-form">
              <AutoFocusInput
                id="enum-add-input"
                className="field-input text-sm py-1"
                style={{ width: 200 }}
                placeholder="Enum name (PascalCase)"
                value={addInput}
                onChange={(e) => setAddInput(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') addEnum(); if (e.key === 'Escape') { setAddFormOpen(false); setAddInput(''); } }}
              />
              <button id="enum-add-confirm" className="btn-sm btn-primary" onClick={addEnum}>Create</button>
              <button className="btn-sm btn-ghost" onClick={() => { setAddFormOpen(false); setAddInput(''); }}>Cancel</button>
            </div>
          ) : (
            <button id="btn-add-enum" className="btn-sm btn-primary" onClick={() => setAddFormOpen(true)}>+ Add Enum</button>
          )}
        </div>
      </div>

      {names.length === 0 && (
        <p className="text-sm py-4" style={{ color: 'var(--text-muted)' }}>
          No enums defined. Enums let you define fixed sets of values (e.g. Status, Priority) that can be used as property types.
        </p>
      )}

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
        <div className="flex items-center gap-2">
          <span className="entity-color-dot" style={{ background: getEntityColor(name) }} />
          <span className="font-medium text-sm">{name}</span>
          <span className="text-xs text-gray-500 text-[11px] px-2 py-0.5 rounded-full font-medium" style={{ background: 'var(--accent-secondary-dim)', color: 'var(--accent-secondary)' }}>
            {values.length} value{values.length !== 1 ? 's' : ''}
          </span>
        </div>
        <button
          className={`btn-sm enum-remove ${confirmingDelete ? 'btn-confirm-danger' : 'btn-danger'}`}
          data-enum={name}
          onClick={onRemove}
        >
          {confirmingDelete ? 'Sure?' : 'Remove'}
        </button>
      </div>
      <div className="entity-body">
        {values.length > 0 && (
          <div className="flex flex-wrap gap-2 mb-3">
            {values.map((v, i) => (
              <span key={i} className="enum-value-tag">
                {v}
                <button className="enum-val-remove" data-enum={name} data-index={i} onClick={() => onRemoveValue(i)} title="Remove value">&times;</button>
              </span>
            ))}
          </div>
        )}
        <div className="flex items-center gap-2">
          <input
            className="field-input text-xs py-1 flex-1 enum-val-input"
            data-enum={name}
            placeholder="Add a new value..."
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
