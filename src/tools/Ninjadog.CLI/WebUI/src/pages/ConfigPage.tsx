import { useRef, useState, useCallback } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';
import { useNavigate } from 'react-router';
import * as api from '../api/ninjadog-api';

/* eslint-disable @typescript-eslint/no-explicit-any */

function Tooltip({ text }: { text: string }) {
  return (
    <span className="tooltip-wrapper">
      <span className="tooltip-icon">?</span>
      <span className="tooltip-content">{text}</span>
    </span>
  );
}

export default function ConfigPage() {
  const state = useConfigStore((s) => s.state);
  const setState = useConfigStore((s) => s.setState);
  const pushUndo = useConfigStore((s) => s.pushUndo);
  const onStateChanged = useConfigStore((s) => s.onStateChanged);
  const setTemplatePickerOpen = useUiStore((s) => s.setTemplatePickerOpen);
  const navigate = useNavigate();

  const [dirBrowserOpen, setDirBrowserOpen] = useState(false);
  const [dirData, setDirData] = useState<{ current: string; absolute: string; parent: string | null; directories: string[] } | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const interacted = useRef(new Set<string>());
  const undoPushedRef = useRef(false);

  const c = (state.config || {}) as Record<string, any>;
  const cors = (c.cors || {}) as Record<string, any>;
  const features = (c.features || {}) as Record<string, any>;
  const database = (c.database || {}) as Record<string, any>;

  const updateConfig = useCallback(
    (updater: (config: Record<string, any>) => void) => {
      const newState = JSON.parse(JSON.stringify(state));
      newState.config = newState.config || {};
      updater(newState.config);
      setState(newState);
      onStateChanged();
    },
    [state, setState, onStateChanged],
  );

  const handleFocus = () => {
    undoPushedRef.current = false;
  };

  const ensureUndo = () => {
    if (!undoPushedRef.current) {
      pushUndo();
      undoPushedRef.current = true;
    }
  };

  const validateField = (field: string, value: string) => {
    const errors: Record<string, string> = { ...fieldErrors };
    if (field === 'name') {
      if (!value) errors.name = 'Project name is required';
      else if (!/^[a-zA-Z0-9.\-]+$/.test(value)) errors.name = 'Use only letters, numbers, dots, and hyphens';
      else delete errors.name;
    }
    if (field === 'rootNamespace') {
      if (value && !/^[A-Z][a-zA-Z0-9]*(\.[A-Z][a-zA-Z0-9]*)*$/.test(value))
        errors.rootNamespace = 'Invalid namespace format. Use PascalCase with dots';
      else delete errors.rootNamespace;
    }
    if (field === 'corsOrigins') {
      if (value) {
        const origins = value.split(',').map((s) => s.trim()).filter(Boolean);
        const allValid = origins.every((o) => o === '*' || /^https?:\/\//.test(o));
        if (!allValid) errors.corsOrigins = 'Origins must be URLs (http/https) or *';
        else delete errors.corsOrigins;
      } else {
        delete errors.corsOrigins;
      }
    }
    setFieldErrors(errors);
  };

  const handleChange = (field: string, value: string | boolean) => {
    interacted.current.add(field);
    ensureUndo();
    if (typeof value === 'string' && interacted.current.has(field)) {
      validateField(field, value);
    }

    updateConfig((cfg) => {
      switch (field) {
        case 'name': cfg.name = value; break;
        case 'version': cfg.version = value; break;
        case 'description': cfg.description = value; break;
        case 'rootNamespace': cfg.rootNamespace = value; break;
        case 'outputPath': cfg.outputPath = value || '.'; break;
        case 'databaseProvider':
          if (value && value !== 'sqlite') cfg.database = { provider: value };
          else delete cfg.database;
          break;
        case 'corsOrigins': {
          const origins = (value as string).split(',').map((s) => s.trim()).filter(Boolean);
          if (origins.length > 0) {
            cfg.cors = cfg.cors || {};
            cfg.cors.origins = origins;
          } else {
            delete cfg.cors;
          }
          break;
        }
        case 'corsMethods': {
          const methods = (value as string).split(',').map((s) => s.trim()).filter(Boolean);
          if (methods.length > 0) { cfg.cors = cfg.cors || {}; cfg.cors.methods = methods; }
          else if (cfg.cors) delete cfg.cors.methods;
          break;
        }
        case 'corsHeaders': {
          const headers = (value as string).split(',').map((s) => s.trim()).filter(Boolean);
          if (headers.length > 0) { cfg.cors = cfg.cors || {}; cfg.cors.headers = headers; }
          else if (cfg.cors) delete cfg.cors.headers;
          break;
        }
        case 'softDelete': {
          const auditing = cfg.features?.auditing;
          if (value || auditing) {
            cfg.features = cfg.features || {};
            if (value) cfg.features.softDelete = true; else delete cfg.features.softDelete;
          } else {
            delete cfg.features;
          }
          break;
        }
        case 'auditing': {
          const sd = cfg.features?.softDelete;
          if (value || sd) {
            cfg.features = cfg.features || {};
            if (value) cfg.features.auditing = true; else delete cfg.features.auditing;
          } else {
            delete cfg.features;
          }
          break;
        }
      }
    });
  };

  const loadDirectories = async (path: string) => {
    try {
      const data = await api.getDirectories(path);
      setDirData(data);
    } catch {
      setDirData(null);
    }
  };

  const toggleDirBrowser = () => {
    if (!dirBrowserOpen) {
      loadDirectories(c.outputPath || '.');
    }
    setDirBrowserOpen(!dirBrowserOpen);
  };

  const selectDir = (dir: string) => {
    handleChange('outputPath', dir);
    setDirBrowserOpen(false);
  };

  const entityCount = state.entities ? Object.keys(state.entities).length : 0;

  return (
    <div id="tab-config" className="tab-content-active space-y-4">
      <div className="section-card">
        <div className="section-title">General</div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="field-label">Name</label>
            <input
              data-field="name"
              className={`field-input${fieldErrors.name ? ' field-error' : ''}`}
              value={c.name || ''}
              onFocus={handleFocus}
              onChange={(e) => handleChange('name', e.target.value)}
            />
            {fieldErrors.name && <div className="field-error-msg" data-error-for="name" style={{ display: 'block' }}>{fieldErrors.name}</div>}
          </div>
          <div>
            <label className="field-label">Version</label>
            <input data-field="version" className="field-input" value={c.version || ''} onFocus={handleFocus} onChange={(e) => handleChange('version', e.target.value)} />
          </div>
          <div className="col-span-2">
            <label className="field-label">Description</label>
            <input data-field="description" className="field-input" value={c.description || ''} onFocus={handleFocus} onChange={(e) => handleChange('description', e.target.value)} />
          </div>
          <div>
            <label className="field-label">
              Root Namespace <Tooltip text="The root C# namespace for generated code. Example: MyCompany.Api" />
            </label>
            <input
              data-field="rootNamespace"
              className={`field-input${fieldErrors.rootNamespace ? ' field-error' : ''}`}
              value={c.rootNamespace || ''}
              onFocus={handleFocus}
              onChange={(e) => handleChange('rootNamespace', e.target.value)}
            />
            {fieldErrors.rootNamespace && <div className="field-error-msg" data-error-for="rootNamespace" style={{ display: 'block' }}>{fieldErrors.rootNamespace}</div>}
          </div>
          <div>
            <label className="field-label">
              Output Path <Tooltip text="Directory where generated files will be written, relative to ninjadog.json" />
            </label>
            <div className="flex gap-2">
              <input
                data-field="outputPath"
                className="field-input flex-1"
                value={c.outputPath || '.'}
                onFocus={handleFocus}
                onChange={(e) => handleChange('outputPath', e.target.value)}
              />
              <button className="btn-sm btn-ghost flex items-center gap-1 whitespace-nowrap" onClick={toggleDirBrowser}>
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z" /></svg>
                Browse
              </button>
            </div>
            {dirBrowserOpen && dirData && (
              <div className="dir-browser mt-2">
                <div className="dir-current">
                  <span>{dirData.absolute || dirData.current}</span>
                  <button className="btn-sm btn-ghost" style={{ padding: '2px 8px', fontSize: 10 }} onClick={() => setDirBrowserOpen(false)}>Close</button>
                </div>
                {dirData.parent && (
                  <button className="dir-item dir-item-parent" onClick={() => loadDirectories(dirData.parent!)}>
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6" /></svg>
                    ..
                  </button>
                )}
                {dirData.directories.map((d) => (
                  <button key={d} className="dir-item" onClick={() => loadDirectories(dirData.current === '.' ? d : `${dirData.current}/${d}`)}>
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z" /></svg>
                    {d}
                  </button>
                ))}
                <button className="dir-item dir-item-select" onClick={() => selectDir(dirData.current)}>
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="20 6 9 17 4 12" /></svg>
                  Select this folder
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="section-card">
        <div className="section-title">Database</div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="field-label">
              Provider <Tooltip text="The database engine for generated repository code" />
            </label>
            <select data-field="databaseProvider" className="field-select" value={database.provider || 'sqlite'} onChange={(e) => handleChange('databaseProvider', e.target.value)}>
              <option value="sqlite">SQLite</option>
              <option value="postgres">PostgreSQL</option>
              <option value="sqlserver">SQL Server</option>
            </select>
          </div>
        </div>
      </div>

      <div className="section-card">
        <div className="section-title">CORS</div>
        <div>
          <label className="field-label">
            Origins (comma-separated) <Tooltip text="Allowed origins for cross-origin requests. Use * for all origins (not recommended for production)" />
          </label>
          <input
            data-field="corsOrigins"
            className={`field-input${fieldErrors.corsOrigins ? ' field-error' : ''}`}
            value={(cors.origins || []).join(', ')}
            onFocus={handleFocus}
            onChange={(e) => handleChange('corsOrigins', e.target.value)}
          />
          {fieldErrors.corsOrigins && <div className="field-error-msg" style={{ display: 'block' }}>{fieldErrors.corsOrigins}</div>}
        </div>
        <div className="grid grid-cols-2 gap-4 mt-3">
          <div>
            <label className="field-label">Methods (comma-separated)</label>
            <input className="field-input" value={(cors.methods || []).join(', ')} onFocus={handleFocus} onChange={(e) => handleChange('corsMethods', e.target.value)} />
          </div>
          <div>
            <label className="field-label">Headers (comma-separated)</label>
            <input className="field-input" value={(cors.headers || []).join(', ')} onFocus={handleFocus} onChange={(e) => handleChange('corsHeaders', e.target.value)} />
          </div>
        </div>
      </div>

      <div className="section-card">
        <div className="section-title">Features</div>
        <div className="flex items-center gap-6">
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" data-field="softDelete" className="field-checkbox" checked={!!features.softDelete} onChange={(e) => handleChange('softDelete', e.target.checked)} />
            Soft Delete <Tooltip text="Adds IsDeleted and DeletedAt columns instead of permanently deleting records" />
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" data-field="auditing" className="field-checkbox" checked={!!features.auditing} onChange={(e) => handleChange('auditing', e.target.checked)} />
            Auditing <Tooltip text="Adds CreatedAt and UpdatedAt timestamp columns to all entities" />
          </label>
        </div>
      </div>

      {entityCount === 0 && (
        <div className="empty-state">
          <div className="empty-state-icon">
            <svg viewBox="0 0 24 24" width="64" height="64" fill="none" stroke="currentColor" strokeWidth="1.5">
              <path d="M12 2L9 9L2 12L9 15L12 22L15 15L22 12L15 9L12 2Z" />
            </svg>
          </div>
          <div className="empty-state-title">Ready to build your API</div>
          <div className="empty-state-text">Configure your project settings above, then add entities to generate a complete CRUD Web API.</div>
          <div className="empty-state-actions">
            <button id="btn-start-template" className="btn-sm btn-primary" onClick={() => setTemplatePickerOpen(true)}>Start from Template</button>
            <button id="btn-start-scratch" className="btn-sm btn-ghost" onClick={() => navigate('/entities')}>Add First Entity</button>
          </div>
        </div>
      )}
    </div>
  );
}
