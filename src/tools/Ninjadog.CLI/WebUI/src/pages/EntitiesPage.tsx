import { useState, useRef, useCallback } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';

/* eslint-disable @typescript-eslint/no-explicit-any */

const PROPERTY_TYPES = ['string', 'int', 'long', 'decimal', 'double', 'float', 'bool', 'DateTime', 'DateOnly', 'TimeOnly', 'Guid'];
const RELATIONSHIP_TYPES = ['hasMany', 'hasOne', 'belongsTo'];

function getEntityColor(name: string) {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  const hue = Math.abs(hash) % 360;
  return `hsl(${hue}, 65%, 55%)`;
}

const PRESETS: Record<string, Array<Record<string, any>>> = {
  id: [{ name: 'id', type: 'Guid', isKey: true }],
  name: [{ name: 'name', type: 'string', required: true, maxLength: 200 }],
  timestamps: [{ name: 'createdAt', type: 'DateTime' }, { name: 'updatedAt', type: 'DateTime' }],
  email: [{ name: 'email', type: 'string', required: true, maxLength: 255, pattern: '^[^@]+@[^@]+$' }],
  description: [{ name: 'description', type: 'string', maxLength: 2000 }],
};

export default function EntitiesPage() {
  const state = useConfigStore((s) => s.state);
  const setState = useConfigStore((s) => s.setState);
  const pushUndo = useConfigStore((s) => s.pushUndo);
  const onStateChanged = useConfigStore((s) => s.onStateChanged);
  const showToast = useUiStore((s) => s.showToast);

  const [focusedEntity, setFocusedEntity] = useState<string | null>(null);
  const [addFormOpen, setAddFormOpen] = useState(false);
  const [addInput, setAddInput] = useState('');
  const [checkedProps, setCheckedProps] = useState<Record<string, Record<string, boolean>>>({});
  const [confirmDelete, setConfirmDelete] = useState<Record<string, boolean>>({});
  const undoPushedRef = useRef(false);

  const entities = (state.entities || {}) as Record<string, any>;
  const names = Object.keys(entities);
  const useSidebar = names.length >= 5;

  const currentFocused = useSidebar
    ? (focusedEntity && entities[focusedEntity] ? focusedEntity : names[0] || null)
    : null;

  const mutate = useCallback(
    (updater: (s: any) => void) => {
      const newState = JSON.parse(JSON.stringify(state));
      newState.entities = newState.entities || {};
      updater(newState);
      setState(newState);
      onStateChanged();
    },
    [state, setState, onStateChanged],
  );

  const addEntity = () => {
    const name = addInput.trim();
    if (!name) return;
    pushUndo();
    mutate((s: any) => {
      s.entities[name] = { properties: {} };
    });
    setFocusedEntity(name);
    setAddFormOpen(false);
    setAddInput('');
  };

  const removeEntity = (name: string) => {
    if (!confirmDelete[name]) {
      setConfirmDelete({ ...confirmDelete, [name]: true });
      setTimeout(() => setConfirmDelete((cd) => { const n = { ...cd }; delete n[name]; return n; }), 3000);
      return;
    }
    pushUndo();
    mutate((s: any) => {
      delete s.entities[name];
    });
    setCheckedProps((cp) => { const n = { ...cp }; delete n[name]; return n; });
    setConfirmDelete((cd) => { const n = { ...cd }; delete n[name]; return n; });
    if (focusedEntity === name) setFocusedEntity(null);
  };

  const cloneEntity = (name: string) => {
    const cloneName = name + 'Copy';
    pushUndo();
    mutate((s: any) => {
      s.entities[cloneName] = JSON.parse(JSON.stringify(s.entities[name]));
    });
    setFocusedEntity(cloneName);
    showToast(`Cloned "${name}" as "${cloneName}"`, 'success');
  };

  const applyPreset = (entityName: string, presetKey: string) => {
    const presetItems = PRESETS[presetKey];
    if (!presetItems) return;
    pushUndo();
    mutate((s: any) => {
      s.entities[entityName].properties = s.entities[entityName].properties || {};
      presetItems.forEach((item: any) => {
        const def = { ...item };
        const pName = def.name;
        delete def.name;
        if (!def.type) def.type = 'string';
        s.entities[entityName].properties[pName] = def;
      });
    });
    showToast(`Added "${presetKey}" preset to ${entityName}`, 'success');
  };

  const handlePropChange = (entityName: string, origPropName: string, key: string, value: any) => {
    if (!undoPushedRef.current) {
      pushUndo();
      undoPushedRef.current = true;
    }
    mutate((s: any) => {
      const entity = s.entities[entityName];
      if (key === 'name') {
        const newName = (value as string).trim() || origPropName;
        if (newName !== origPropName) {
          const prop = entity.properties[origPropName];
          delete entity.properties[origPropName];
          entity.properties[newName] = prop;
          if (entity.seedData) {
            entity.seedData.forEach((row: any) => {
              if (origPropName in row) {
                row[newName] = row[origPropName];
                delete row[origPropName];
              }
            });
          }
        }
      } else if (key === 'isKey' || key === 'required') {
        if (value) entity.properties[origPropName][key] = true;
        else delete entity.properties[origPropName][key];
      } else if (key === 'maxLength' || key === 'minLength' || key === 'min' || key === 'max') {
        const v = (value as string).trim();
        if (v !== '') entity.properties[origPropName][key] = parseInt(v, 10);
        else delete entity.properties[origPropName][key];
      } else if (key === 'pattern') {
        const v = (value as string).trim();
        if (v) entity.properties[origPropName][key] = v;
        else delete entity.properties[origPropName][key];
      } else {
        entity.properties[origPropName][key] = value;
      }
    });
  };

  const removeProp = (entityName: string, propName: string) => {
    pushUndo();
    mutate((s: any) => {
      delete s.entities[entityName].properties[propName];
      if (s.entities[entityName].seedData) {
        s.entities[entityName].seedData.forEach((row: any) => delete row[propName]);
      }
    });
    setCheckedProps((cp) => {
      const n = { ...cp };
      if (n[entityName]) { const e = { ...n[entityName] }; delete e[propName]; n[entityName] = e; }
      return n;
    });
  };

  const addProp = (entityName: string, propName: string) => {
    if (!propName.trim()) return;
    pushUndo();
    mutate((s: any) => {
      s.entities[entityName].properties = s.entities[entityName].properties || {};
      s.entities[entityName].properties[propName.trim()] = { type: 'string' };
    });
  };

  const handleRelChange = (entityName: string, origRelName: string, key: string, value: string) => {
    mutate((s: any) => {
      const entity = s.entities[entityName];
      if (key === 'name') {
        const newName = value.trim() || origRelName;
        if (newName !== origRelName) {
          const rel = entity.relationships[origRelName];
          delete entity.relationships[origRelName];
          entity.relationships[newName] = rel;
        }
      } else {
        entity.relationships[origRelName][key] = value;
      }
    });
  };

  const removeRel = (entityName: string, relName: string) => {
    pushUndo();
    mutate((s: any) => {
      delete s.entities[entityName].relationships[relName];
      if (Object.keys(s.entities[entityName].relationships || {}).length === 0)
        delete s.entities[entityName].relationships;
    });
  };

  const addRel = (entityName: string, relName: string) => {
    if (!relName.trim()) return;
    pushUndo();
    mutate((s: any) => {
      s.entities[entityName].relationships = s.entities[entityName].relationships || {};
      s.entities[entityName].relationships[relName.trim()] = { type: 'hasMany', targetEntity: '' };
    });
  };

  const bulkDelete = (entityName: string) => {
    const checked = checkedProps[entityName] || {};
    const toDelete = Object.keys(checked).filter((p) => checked[p]);
    if (toDelete.length === 0) return;
    pushUndo();
    mutate((s: any) => {
      toDelete.forEach((p) => {
        if (s.entities[entityName].seedData) s.entities[entityName].seedData.forEach((row: any) => delete row[p]);
        delete s.entities[entityName].properties[p];
      });
    });
    setCheckedProps((cp) => { const n = { ...cp }; delete n[entityName]; return n; });
    showToast(`Deleted ${toDelete.length} properties`, 'success');
  };

  const bulkToggleRequired = (entityName: string) => {
    const checked = checkedProps[entityName] || {};
    const toToggle = Object.keys(checked).filter((p) => checked[p]);
    if (toToggle.length === 0) return;
    pushUndo();
    mutate((s: any) => {
      toToggle.forEach((p) => {
        const prop = s.entities[entityName].properties[p];
        if (prop) { if (prop.required) delete prop.required; else prop.required = true; }
      });
    });
  };

  const toggleCheck = (entityName: string, propName: string, checked: boolean) => {
    setCheckedProps((cp) => {
      const n = { ...cp };
      n[entityName] = { ...(n[entityName] || {}), [propName]: checked };
      if (!checked) delete n[entityName][propName];
      return n;
    });
  };

  const displayNames = useSidebar && currentFocused ? [currentFocused] : names;

  return (
    <div id="tab-entities" className="tab-content-active">
      <div className="flex items-center justify-between mb-4">
        <div className="section-title mb-0">Entities ({names.length})</div>
        <div>
          <button id="btn-add-entity" className={`btn-sm btn-primary${addFormOpen ? ' hidden' : ''}`} onClick={() => setAddFormOpen(true)}>+ Add Entity</button>
          <div id="entity-add-form" className={`inline-add-form${!addFormOpen ? ' hidden' : ''}`}>
            <input
              id="entity-add-input"
              className="field-input text-sm py-1"
              style={{ width: 200 }}
              placeholder="Entity name (PascalCase)"
              value={addInput}
              onChange={(e) => setAddInput(e.target.value)}
              onKeyDown={(e) => { if (e.key === 'Enter') addEntity(); if (e.key === 'Escape') setAddFormOpen(false); }}
              autoFocus={addFormOpen}
            />
            <button id="entity-add-confirm" className="btn-sm btn-primary" onClick={addEntity}>Create</button>
            <button id="entity-add-cancel" className="btn-sm btn-ghost" onClick={() => setAddFormOpen(false)}>Cancel</button>
          </div>
        </div>
      </div>

      <div className={useSidebar ? 'flex gap-4' : ''}>
        {useSidebar && (
          <div className="entity-sidebar">
            <div className="entity-sidebar-header">Entities</div>
            {names.map((n) => (
              <button
                key={n}
                className={`entity-sidebar-item${n === currentFocused ? ' active' : ''}`}
                onClick={() => setFocusedEntity(n)}
              >
                <span className="entity-color-dot" style={{ background: getEntityColor(n) }} />
                {n}
              </button>
            ))}
          </div>
        )}

        <div className={useSidebar ? 'flex-1 min-w-0' : ''}>
          {displayNames.map((name) => {
            const entity = entities[name];
            const props = entity.properties || {};
            const rels = entity.relationships || {};
            const propNames = Object.keys(props);
            const relNames = Object.keys(rels);
            const checked = checkedProps[name] || {};
            const checkedCount = propNames.filter((p) => checked[p]).length;

            return (
              <EntityCard
                key={name}
                name={name}
                propNames={propNames}
                relNames={relNames}
                props={props}
                rels={rels}
                checkedCount={checkedCount}
                checked={checked}
                confirmingDelete={!!confirmDelete[name]}
                onRemove={() => removeEntity(name)}
                onClone={() => cloneEntity(name)}
                onPreset={(preset) => applyPreset(name, preset)}
                onPropChange={(prop, key, val) => handlePropChange(name, prop, key, val)}
                onPropFocus={() => { undoPushedRef.current = false; }}
                onRemoveProp={(prop) => removeProp(name, prop)}
                onAddProp={(prop) => addProp(name, prop)}
                onRelChange={(rel, key, val) => handleRelChange(name, rel, key, val)}
                onRemoveRel={(rel) => removeRel(name, rel)}
                onAddRel={(rel) => addRel(name, rel)}
                onBulkDelete={() => bulkDelete(name)}
                onBulkToggleRequired={() => bulkToggleRequired(name)}
                onToggleCheck={(prop, c) => toggleCheck(name, prop, c)}
              />
            );
          })}
        </div>
      </div>
    </div>
  );
}

interface EntityCardProps {
  name: string;
  propNames: string[];
  relNames: string[];
  props: Record<string, any>;
  rels: Record<string, any>;
  checkedCount: number;
  checked: Record<string, boolean>;
  confirmingDelete: boolean;
  onRemove: () => void;
  onClone: () => void;
  onPreset: (key: string) => void;
  onPropChange: (prop: string, key: string, value: any) => void;
  onPropFocus: () => void;
  onRemoveProp: (prop: string) => void;
  onAddProp: (prop: string) => void;
  onRelChange: (rel: string, key: string, value: string) => void;
  onRemoveRel: (rel: string) => void;
  onAddRel: (rel: string) => void;
  onBulkDelete: () => void;
  onBulkToggleRequired: () => void;
  onToggleCheck: (prop: string, checked: boolean) => void;
}

function EntityCard({
  name, propNames, relNames, props, rels, checkedCount, checked, confirmingDelete,
  onRemove, onClone, onPreset, onPropChange, onPropFocus, onRemoveProp, onAddProp,
  onRelChange, onRemoveRel, onAddRel, onBulkDelete, onBulkToggleRequired, onToggleCheck,
}: EntityCardProps) {
  const [propAddOpen, setPropAddOpen] = useState(false);
  const [propAddInput, setPropAddInput] = useState('');
  const [relAddOpen, setRelAddOpen] = useState(false);
  const [relAddInput, setRelAddInput] = useState('');

  return (
    <div className="entity-card" data-entity={name}>
      <div className="entity-header">
        <div className="flex items-center gap-2">
          <span className="entity-color-dot" style={{ background: getEntityColor(name) }} />
          <span className="font-medium text-sm">{name}</span>
          <span className="badge badge-secondary">{propNames.length} prop{propNames.length !== 1 ? 's' : ''}</span>
          {relNames.length > 0 && <span className="badge badge-secondary">{relNames.length} rel{relNames.length !== 1 ? 's' : ''}</span>}
        </div>
        <div className="flex items-center gap-2">
          <button className="btn-sm btn-ghost entity-clone" data-entity={name} onClick={onClone}>Clone</button>
          <button
            className={`btn-sm entity-remove ${confirmingDelete ? 'btn-confirm-danger' : 'btn-danger'}`}
            data-entity={name}
            onClick={onRemove}
          >
            {confirmingDelete ? 'Sure?' : 'Remove'}
          </button>
        </div>
      </div>
      <div className="entity-body">
        {/* Properties */}
        <div className="mb-3">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-medium text-gray-400">Properties</span>
            <div>
              <button className={`btn-sm btn-ghost prop-add-btn${propAddOpen ? ' hidden' : ''}`} data-entity={name} onClick={() => setPropAddOpen(true)}>+ Property</button>
              <div className={`inline-add-form${!propAddOpen ? ' hidden' : ''}`}>
                <input
                  className="field-input text-xs py-1 prop-add-input"
                  data-entity={name}
                  style={{ width: 150 }}
                  placeholder="Property name (camelCase)"
                  value={propAddInput}
                  onChange={(e) => setPropAddInput(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') { onAddProp(propAddInput); setPropAddInput(''); setPropAddOpen(false); }
                    if (e.key === 'Escape') setPropAddOpen(false);
                  }}
                  autoFocus={propAddOpen}
                />
                <button className="btn-sm btn-primary prop-add-confirm" data-entity={name} onClick={() => { onAddProp(propAddInput); setPropAddInput(''); setPropAddOpen(false); }}>Add</button>
                <button className="btn-sm btn-ghost" onClick={() => setPropAddOpen(false)}>Cancel</button>
              </div>
            </div>
          </div>

          <div className="preset-bar">
            <span className="text-xs text-gray-500 mr-1">Quick add:</span>
            {Object.keys(PRESETS).map((k) => (
              <button key={k} className="preset-btn" data-preset={k} data-entity={name} onClick={() => onPreset(k)}>
                {k === 'id' ? 'ID Field' : k.charAt(0).toUpperCase() + k.slice(1)}
              </button>
            ))}
          </div>

          {checkedCount > 0 && (
            <div className="bulk-toolbar">
              <span className="text-xs">{checkedCount} selected</span>
              <button className="btn-sm btn-danger" onClick={onBulkDelete}>Delete Selected</button>
              <button className="btn-sm btn-ghost" onClick={onBulkToggleRequired}>Toggle Required</button>
            </div>
          )}

          {propNames.length > 0 ? (
            <table className="data-table">
              <thead>
                <tr>
                  <th></th><th></th><th>Name</th><th>Type</th><th>Key</th><th>Required</th>
                  <th>MaxLen</th><th>MinLen</th><th>Min</th><th>Max</th><th>Pattern</th><th></th>
                </tr>
              </thead>
              <tbody>
                {propNames.map((p) => (
                  <tr key={p} data-entity={name} data-prop={p}>
                    <td className="drag-handle">&#10303;</td>
                    <td className="text-center">
                      <input
                        type="checkbox"
                        className="field-checkbox"
                        checked={!!checked[p]}
                        onChange={(e) => onToggleCheck(p, e.target.checked)}
                      />
                    </td>
                    <td>
                      <input
                        className="field-input py-1 text-xs"
                        style={{ minWidth: 120 }}
                        defaultValue={p}
                        onFocus={onPropFocus}
                        onBlur={(e) => onPropChange(p, 'name', e.target.value)}
                      />
                    </td>
                    <td>
                      <select
                        className="field-select py-1 text-xs prop-field"
                        data-key="type"
                        style={{ minWidth: 110 }}
                        value={props[p].type || 'string'}
                        onChange={(e) => onPropChange(p, 'type', e.target.value)}
                      >
                        {PROPERTY_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                      </select>
                    </td>
                    <td className="text-center">
                      <input type="checkbox" className="field-checkbox" checked={!!props[p].isKey} onChange={(e) => onPropChange(p, 'isKey', e.target.checked)} />
                    </td>
                    <td className="text-center">
                      <input type="checkbox" className="field-checkbox" checked={!!props[p].required} onChange={(e) => onPropChange(p, 'required', e.target.checked)} />
                    </td>
                    <td><input className="field-input py-1 text-xs w-16" type="number" defaultValue={props[p].maxLength ?? ''} onFocus={onPropFocus} onBlur={(e) => onPropChange(p, 'maxLength', e.target.value)} /></td>
                    <td><input className="field-input py-1 text-xs w-16" type="number" defaultValue={props[p].minLength ?? ''} onFocus={onPropFocus} onBlur={(e) => onPropChange(p, 'minLength', e.target.value)} /></td>
                    <td><input className="field-input py-1 text-xs w-16" type="number" defaultValue={props[p].min ?? ''} onFocus={onPropFocus} onBlur={(e) => onPropChange(p, 'min', e.target.value)} /></td>
                    <td><input className="field-input py-1 text-xs w-16" type="number" defaultValue={props[p].max ?? ''} onFocus={onPropFocus} onBlur={(e) => onPropChange(p, 'max', e.target.value)} /></td>
                    <td><input className="field-input py-1 text-xs w-20" defaultValue={props[p].pattern || ''} onFocus={onPropFocus} onBlur={(e) => onPropChange(p, 'pattern', e.target.value)} /></td>
                    <td><button className="btn-sm btn-danger prop-remove" onClick={() => onRemoveProp(p)}>X</button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <p className="text-xs text-gray-500">No properties defined.</p>
          )}
        </div>

        {/* Relationships */}
        <div>
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-medium text-gray-400">Relationships</span>
            <div>
              <button className={`btn-sm btn-ghost rel-add-btn${relAddOpen ? ' hidden' : ''}`} data-entity={name} onClick={() => setRelAddOpen(true)}>+ Relationship</button>
              <div className={`inline-add-form${!relAddOpen ? ' hidden' : ''}`}>
                <input
                  className="field-input text-xs py-1 rel-add-input"
                  data-entity={name}
                  style={{ width: 150 }}
                  placeholder="Relationship name"
                  value={relAddInput}
                  onChange={(e) => setRelAddInput(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') { onAddRel(relAddInput); setRelAddInput(''); setRelAddOpen(false); }
                    if (e.key === 'Escape') setRelAddOpen(false);
                  }}
                  autoFocus={relAddOpen}
                />
                <button className="btn-sm btn-primary rel-add-confirm" data-entity={name} onClick={() => { onAddRel(relAddInput); setRelAddInput(''); setRelAddOpen(false); }}>Add</button>
                <button className="btn-sm btn-ghost" onClick={() => setRelAddOpen(false)}>Cancel</button>
              </div>
            </div>
          </div>
          {relNames.length > 0 ? (
            <table className="data-table">
              <thead><tr><th>Name</th><th>Type</th><th>Target Entity</th><th>Foreign Key</th><th></th></tr></thead>
              <tbody>
                {relNames.map((r) => (
                  <tr key={r} data-entity={name} data-rel={r}>
                    <td><input className="field-input py-1 text-xs rel-field" data-key="name" defaultValue={r} onBlur={(e) => onRelChange(r, 'name', e.target.value)} /></td>
                    <td>
                      <select className="field-select py-1 text-xs rel-field" data-key="type" value={rels[r].type || 'hasMany'} onChange={(e) => onRelChange(r, 'type', e.target.value)}>
                        {RELATIONSHIP_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                      </select>
                    </td>
                    <td><input className="field-input py-1 text-xs rel-field" data-key="targetEntity" defaultValue={rels[r].targetEntity || ''} onBlur={(e) => onRelChange(r, 'targetEntity', e.target.value)} /></td>
                    <td><input className="field-input py-1 text-xs rel-field" data-key="foreignKey" defaultValue={rels[r].foreignKey || ''} onBlur={(e) => onRelChange(r, 'foreignKey', e.target.value)} /></td>
                    <td><button className="btn-sm btn-danger rel-remove" data-entity={name} onClick={() => onRemoveRel(r)}>X</button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <p className="text-xs text-gray-500">No relationships defined.</p>
          )}
        </div>
      </div>
    </div>
  );
}
