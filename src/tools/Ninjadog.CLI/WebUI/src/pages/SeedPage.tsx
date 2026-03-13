import { useRef, useCallback } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';

/* eslint-disable @typescript-eslint/no-explicit-any */

const GUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function getEntityColor(name: string) {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  return `hsl(${Math.abs(hash) % 360}, 65%, 55%)`;
}

function generateGuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
  });
}

function findKeyProp(entity: any): { name: string; type: string } | null {
  const props = entity.properties || {};
  for (const [name, def] of Object.entries(props) as Array<[string, any]>) {
    if (def.isKey) return { name, type: def.type };
  }
  return null;
}

function nextIntKey(entity: any, keyName: string): number {
  const rows = entity.seedData || [];
  let max = 0;
  rows.forEach((row: any) => {
    const v = Number(row[keyName]);
    if (!isNaN(v) && v > max) max = v;
  });
  return max + 1;
}

function generateKeyValue(entity: any, keyProp: { name: string; type: string }): string | number {
  const t = (keyProp.type || '').toLowerCase();
  if (t === 'guid') return generateGuid();
  if (t === 'int' || t === 'long') return nextIntKey(entity, keyProp.name);
  return '';
}

function validateCell(value: unknown, propType: string): string | null {
  if (value === '' || value === undefined || value === null) return null;
  const v = String(value).trim();
  if (v === '') return null;
  const t = (propType || 'string').toLowerCase();
  if (t === 'int' || t === 'long') { if (!/^-?\d+$/.test(v)) return 'Expected an integer value'; return null; }
  if (t === 'decimal' || t === 'double' || t === 'float') { if (isNaN(Number(v))) return 'Expected a numeric value'; return null; }
  if (t === 'bool') { if (v !== 'true' && v !== 'false') return 'Expected "true" or "false"'; return null; }
  if (t === 'guid') { if (!GUID_RE.test(v)) return 'Expected a valid GUID'; return null; }
  if (t === 'datetime' || t === 'dateonly' || t === 'timeonly') { if (isNaN(new Date(v).getTime())) return 'Expected a valid date/time value'; return null; }
  return null;
}

export default function SeedPage() {
  const state = useConfigStore((s) => s.state);
  const setState = useConfigStore((s) => s.setState);
  const pushUndo = useConfigStore((s) => s.pushUndo);
  const onStateChanged = useConfigStore((s) => s.onStateChanged);
  const openImportModal = useUiStore((s) => s.openImportModal);

  const undoPushedRef = useRef(false);

  const entities = (state.entities || {}) as Record<string, any>;
  const entityNames = Object.keys(entities);

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

  if (entityNames.length === 0) {
    return <div id="tab-seed" className="tab-content-active"><p className="text-sm text-gray-500">No entities defined. Add entities first.</p></div>;
  }

  const addRow = (entityName: string) => {
    pushUndo();
    mutate((s: any) => {
      const ent = s.entities[entityName];
      ent.seedData = ent.seedData || [];
      const row: any = {};
      Object.keys(ent.properties || {}).forEach((p) => { row[p] = ''; });
      const keyProp = findKeyProp(ent);
      if (keyProp && ['guid', 'int', 'long'].includes((keyProp.type || '').toLowerCase())) {
        row[keyProp.name] = generateKeyValue(ent, keyProp);
      }
      ent.seedData.push(row);
    });
  };

  const removeRow = (entityName: string, index: number) => {
    pushUndo();
    mutate((s: any) => {
      s.entities[entityName].seedData.splice(index, 1);
      if (s.entities[entityName].seedData.length === 0) delete s.entities[entityName].seedData;
    });
  };

  const generateKey = (entityName: string, rowIdx: number) => {
    pushUndo();
    mutate((s: any) => {
      const ent = s.entities[entityName];
      const keyProp = findKeyProp(ent);
      if (keyProp) ent.seedData[rowIdx][keyProp.name] = generateKeyValue(ent, keyProp);
    });
  };

  const fillKeys = (entityName: string) => {
    pushUndo();
    mutate((s: any) => {
      const ent = s.entities[entityName];
      const keyProp = findKeyProp(ent);
      if (!keyProp || !ent.seedData) return;
      ent.seedData.forEach((row: any) => {
        const val = row[keyProp.name];
        if (val === '' || val === undefined || val === null) {
          row[keyProp.name] = generateKeyValue(ent, keyProp);
        }
      });
    });
  };

  const handleCellChange = (entityName: string, rowIdx: number, prop: string, value: string) => {
    if (!undoPushedRef.current) {
      pushUndo();
      undoPushedRef.current = true;
    }
    mutate((s: any) => {
      const ent = s.entities[entityName];
      ent.seedData = ent.seedData || [];
      ent.seedData[rowIdx] = ent.seedData[rowIdx] || {};
      let parsed: unknown = value;
      if (value === 'true') parsed = true;
      else if (value === 'false') parsed = false;
      else if (value !== '' && !isNaN(Number(value))) parsed = Number(value);
      ent.seedData[rowIdx][prop] = parsed;
    });
  };

  const handleImport = (entityName: string) => {
    openImportModal(entityName, (importedRows) => {
      if (!Array.isArray(importedRows) || importedRows.length === 0) return;
      pushUndo();
      mutate((s: any) => {
        const ent = s.entities[entityName];
        ent.seedData = ent.seedData || [];
        importedRows.forEach((row) => ent.seedData.push(row));
      });
    });
  };

  return (
    <div id="tab-seed" className="tab-content-active">
      <div className="section-title">Seed Data</div>
      {entityNames.map((name) => {
        const entity = entities[name];
        const props = Object.keys(entity.properties || {});
        const seedData = entity.seedData || [];
        const keyProp = findKeyProp(entity);
        const canAutoKey = keyProp && ['guid', 'int', 'long'].includes((keyProp.type || '').toLowerCase());

        return (
          <div key={name} className="entity-card" data-seed-entity={name}>
            <div className="entity-header">
              <span className="font-medium text-sm">
                <span className="entity-color-dot" style={{ background: getEntityColor(name) }} />
                {name}
              </span>
              <div className="flex items-center gap-2">
                <span className="text-xs text-gray-500">{seedData.length} rows</span>
                {canAutoKey && seedData.length > 0 && (
                  <button className="btn-sm btn-ghost" onClick={() => fillKeys(name)} title={`Auto-fill empty ${keyProp!.name} values`}>Fill Keys</button>
                )}
                <button className="btn-sm btn-ghost seed-import" data-entity={name} onClick={() => handleImport(name)}>Import</button>
                <button className="btn-sm btn-ghost seed-add-row" data-entity={name} onClick={() => addRow(name)}>+ Row</button>
              </div>
            </div>
            {(seedData.length > 0 || props.length > 0) && (
              <div className="entity-body" style={{ overflowX: 'auto' }}>
                <table className="data-table">
                  <thead>
                    <tr>
                      {props.map((p) => {
                        const isKey = entity.properties[p]?.isKey;
                        return <th key={p}>{p}{isKey ? <span style={{ color: '#eab308' }} title="Key field"> &#x1f511;</span> : ''}</th>;
                      })}
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {seedData.map((row: any, i: number) => (
                      <tr key={i} data-entity={name} data-row={i}>
                        {props.map((p) => {
                          const propDef = entity.properties[p] || {};
                          const cellValue = row[p] ?? '';
                          const error = validateCell(cellValue, propDef.type);
                          const isKey = propDef.isKey;
                          const canGen = isKey && ['guid', 'int', 'long'].includes((propDef.type || '').toLowerCase());
                          return (
                            <td key={p} className={`seed-cell${error ? ' cell-error' : ''}`} title={error || undefined}>
                              <input
                                className={`field-input seed-field${isKey ? ' font-mono' : ''}`}
                                data-prop={p}
                                style={{ paddingTop: 4, paddingBottom: 4, fontSize: 12 }}
                                defaultValue={String(cellValue)}
                                onFocus={() => { undoPushedRef.current = false; }}
                                onBlur={(e) => handleCellChange(name, i, p, e.target.value)}
                              />
                              {canGen && (
                                <button className="seed-gen-key" data-entity={name} data-row={i} title={`Generate ${propDef.type} key`} onClick={() => generateKey(name, i)}>&#x21bb;</button>
                              )}
                            </td>
                          );
                        })}
                        <td><button className="btn-sm btn-danger seed-remove-row" data-entity={name} data-row={i} onClick={() => removeRow(name, i)}>X</button></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
