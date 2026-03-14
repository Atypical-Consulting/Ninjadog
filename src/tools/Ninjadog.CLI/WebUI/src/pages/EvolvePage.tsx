import { useState, useEffect, useCallback } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';
import { getEvolveStatus, evolvePreview, evolveApply, evolveBaseline } from '../api/ninjadog-api';

/* eslint-disable @typescript-eslint/no-explicit-any */

interface PropertyChange {
  name: string;
  kind: string;
  typeChanged: boolean;
  beforeType: string | null;
  afterType: string | null;
}

interface EntityChange {
  key: string;
  kind: string;
  properties: PropertyChange[];
  relationships: { name: string; kind: string }[];
}

interface EnumChange {
  name: string;
  kind: string;
  addedValues: string[];
  removedValues: string[];
}

interface ConfigChanges {
  hasChanges: boolean;
  softDeleteChanged: boolean;
  softDeleteEnabled: boolean | null;
  auditingChanged: boolean;
  auditingEnabled: boolean | null;
  databaseProviderChanged: boolean;
  oldDatabaseProvider: string | null;
  newDatabaseProvider: string | null;
  aotChanged: boolean;
  corsChanged: boolean;
  authChanged: boolean;
  rateLimitChanged: boolean;
  versioningChanged: boolean;
}

interface MigrationOp {
  description: string;
  sql: string;
  isWarning: boolean;
}

interface PreviewResult {
  hasBaseline: boolean;
  hasChanges?: boolean;
  entities?: EntityChange[];
  config?: ConfigChanges;
  enums?: EnumChange[];
  operations?: MigrationOp[];
}

const KIND_STYLES: Record<string, { label: string; className: string }> = {
  Added:    { label: '+ Added',    className: 'evolve-badge-added' },
  Removed:  { label: '- Removed',  className: 'evolve-badge-removed' },
  Modified: { label: '~ Modified', className: 'evolve-badge-modified' },
};

export default function EvolvePage() {
  const state = useConfigStore((s) => s.state);
  const showToast = useUiStore((s) => s.showToast);

  const [hasBaseline, setHasBaseline] = useState<boolean | null>(null);
  const [preview, setPreview] = useState<PreviewResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [migrationName, setMigrationName] = useState('');
  const [applying, setApplying] = useState(false);
  const [lastMigration, setLastMigration] = useState<string | null>(null);

  useEffect(() => {
    getEvolveStatus()
      .then((r) => setHasBaseline(r.hasBaseline))
      .catch(() => setHasBaseline(false));
  }, []);

  const runPreview = useCallback(async () => {
    setLoading(true);
    setLastMigration(null);
    try {
      const result = await evolvePreview(state);
      setPreview(result);
      if (!result.hasBaseline) {
        setHasBaseline(false);
      }
    } catch (e: any) {
      showToast(e.message || 'Preview failed', 'error');
    } finally {
      setLoading(false);
    }
  }, [state, showToast]);

  const handleSaveBaseline = async () => {
    setLoading(true);
    try {
      await evolveBaseline(state);
      setHasBaseline(true);
      setPreview(null);
      showToast('Baseline saved', 'success');
    } catch (e: any) {
      showToast(e.message || 'Failed to save baseline', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleApply = async () => {
    setApplying(true);
    try {
      const result = await evolveApply(state, migrationName || undefined);
      if (result.firstRun) {
        setHasBaseline(true);
        showToast('Baseline saved', 'success');
      } else if (result.hasChanges) {
        setLastMigration(result.migrationFile);
        setPreview(null);
        showToast(`Migration generated: ${result.migrationFile}`, 'success');
      } else {
        showToast('No changes to apply', 'info');
      }
    } catch (e: any) {
      showToast(e.message || 'Apply failed', 'error');
    } finally {
      setApplying(false);
    }
  };

  if (hasBaseline === null) {
    return (
      <div id="tab-evolve" className="tab-content-active">
        <div className="section-card text-center py-8 text-secondary">Loading...</div>
      </div>
    );
  }

  return (
    <div id="tab-evolve" className="tab-content-active">
      <div className="flex items-center justify-between mb-4">
        <div className="section-title mb-0">Schema Evolution</div>
        <div className="flex items-center gap-2">
          {!hasBaseline && (
            <button className="btn-sm btn-primary" onClick={handleSaveBaseline} disabled={loading}>
              Save Baseline
            </button>
          )}
          <button className="btn-sm btn-primary" onClick={runPreview} disabled={loading}>
            {loading ? 'Scanning...' : 'Preview Changes'}
          </button>
        </div>
      </div>

      {!hasBaseline && !preview && (
        <div className="section-card text-center py-8">
          <div className="text-lg font-medium mb-2">No baseline found</div>
          <div className="text-secondary text-sm mb-4">
            Save your current configuration as the baseline to start tracking schema changes.
          </div>
          <button className="btn-sm btn-primary" onClick={handleSaveBaseline} disabled={loading}>
            Save Current Schema as Baseline
          </button>
        </div>
      )}

      {preview && !preview.hasBaseline && (
        <div className="section-card text-center py-8">
          <div className="text-lg font-medium mb-2">No baseline found</div>
          <div className="text-secondary text-sm mb-4">
            Save your current configuration as the baseline first.
          </div>
          <button className="btn-sm btn-primary" onClick={handleSaveBaseline} disabled={loading}>
            Save Baseline
          </button>
        </div>
      )}

      {preview && preview.hasBaseline && !preview.hasChanges && (
        <div className="section-card text-center py-8">
          <div className="text-lg font-medium" style={{ color: 'var(--success)' }}>No schema changes detected</div>
          <div className="text-secondary text-sm mt-1">
            Your current configuration matches the saved baseline.
          </div>
        </div>
      )}

      {preview && preview.hasBaseline && preview.hasChanges && (
        <>
          <DiffSummary
            entities={preview.entities || []}
            config={preview.config!}
            enums={preview.enums || []}
          />

          {preview.operations && preview.operations.length > 0 && (
            <OperationsTable operations={preview.operations} />
          )}

          <div className="section-card mt-4">
            <div className="section-title text-sm mb-2">Apply Evolution</div>
            <div className="flex items-center gap-2">
              <input
                className="field-input text-sm py-1 flex-1"
                placeholder="Migration name (optional, e.g. add-priority-field)"
                value={migrationName}
                onChange={(e) => setMigrationName(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') handleApply(); }}
              />
              <button
                className="btn-sm btn-primary"
                onClick={handleApply}
                disabled={applying}
              >
                {applying ? 'Applying...' : 'Generate Migration'}
              </button>
            </div>
          </div>
        </>
      )}

      {lastMigration && (
        <div className="section-card mt-4" style={{ borderColor: 'var(--success)', borderWidth: 1 }}>
          <div className="flex items-center gap-2">
            <span style={{ color: 'var(--success)' }}>&#10003;</span>
            <span className="text-sm">Migration generated:</span>
            <code className="text-xs" style={{ color: 'var(--accent)' }}>{lastMigration}</code>
          </div>
        </div>
      )}
    </div>
  );
}

function DiffSummary({ entities, config, enums }: {
  entities: EntityChange[];
  config: ConfigChanges;
  enums: EnumChange[];
}) {
  return (
    <div className="section-card">
      <div className="section-title text-sm mb-3">Changes Detected</div>
      <table className="data-table">
        <thead>
          <tr>
            <th style={{ width: 100 }}>Change</th>
            <th>Target</th>
            <th>Details</th>
          </tr>
        </thead>
        <tbody>
          {entities.map((e) => (
            <EntityRow key={e.key} entity={e} />
          ))}
          <ConfigRows config={config} />
          {enums.map((e) => (
            <EnumRow key={e.name} enumChange={e} />
          ))}
        </tbody>
      </table>
    </div>
  );
}

function EntityRow({ entity }: { entity: EntityChange }) {
  const style = KIND_STYLES[entity.kind] || KIND_STYLES.Modified;
  const details: string[] = [];

  if (entity.kind === 'Added') {
    details.push(`${entity.properties.length} properties`);
  } else if (entity.kind === 'Modified') {
    const added = entity.properties.filter((p) => p.kind === 'Added').length;
    const removed = entity.properties.filter((p) => p.kind === 'Removed').length;
    const modified = entity.properties.filter((p) => p.kind === 'Modified').length;
    if (added > 0) details.push(`+${added} props`);
    if (removed > 0) details.push(`-${removed} props`);
    if (modified > 0) details.push(`~${modified} props`);
    if (entity.relationships.length > 0) details.push(`${entity.relationships.length} rel changes`);
  }

  return (
    <tr>
      <td><span className={`evolve-badge ${style.className}`}>{style.label}</span></td>
      <td className="font-medium">Entity <strong>{entity.key}</strong></td>
      <td className="text-secondary text-sm">{details.join(', ')}</td>
    </tr>
  );
}

function ConfigRows({ config }: { config: ConfigChanges }) {
  if (!config.hasChanges) return null;

  const rows: { label: string; detail: string }[] = [];
  if (config.softDeleteChanged) rows.push({ label: 'Soft Delete', detail: config.softDeleteEnabled ? 'enabled' : 'disabled' });
  if (config.auditingChanged) rows.push({ label: 'Auditing', detail: config.auditingEnabled ? 'enabled' : 'disabled' });
  if (config.databaseProviderChanged) rows.push({ label: 'Database Provider', detail: `${config.oldDatabaseProvider} \u2192 ${config.newDatabaseProvider}` });
  if (config.aotChanged) rows.push({ label: 'AOT', detail: 'changed' });
  if (config.corsChanged) rows.push({ label: 'CORS', detail: 'changed' });
  if (config.authChanged) rows.push({ label: 'Auth', detail: 'changed' });
  if (config.rateLimitChanged) rows.push({ label: 'Rate Limit', detail: 'changed' });
  if (config.versioningChanged) rows.push({ label: 'Versioning', detail: 'changed' });

  return (
    <>
      {rows.map((r) => (
        <tr key={r.label}>
          <td><span className="evolve-badge evolve-badge-modified">~ Config</span></td>
          <td className="font-medium">{r.label}</td>
          <td className="text-secondary text-sm">{r.detail}</td>
        </tr>
      ))}
    </>
  );
}

function EnumRow({ enumChange }: { enumChange: EnumChange }) {
  const style = KIND_STYLES[enumChange.kind] || KIND_STYLES.Modified;
  let detail = '';

  if (enumChange.kind === 'Modified') {
    const parts: string[] = [];
    if (enumChange.addedValues.length > 0) parts.push(`+${enumChange.addedValues.join(', +')}`);
    if (enumChange.removedValues.length > 0) parts.push(`-${enumChange.removedValues.join(', -')}`);
    detail = parts.join('; ');
  }

  return (
    <tr>
      <td><span className={`evolve-badge ${style.className}`}>{style.label}</span></td>
      <td className="font-medium">Enum <strong>{enumChange.name}</strong></td>
      <td className="text-secondary text-sm">{detail}</td>
    </tr>
  );
}

function OperationsTable({ operations }: { operations: MigrationOp[] }) {
  const [expanded, setExpanded] = useState<Record<number, boolean>>({});

  return (
    <div className="section-card mt-4">
      <div className="section-title text-sm mb-3">Migration Operations</div>
      <table className="data-table">
        <thead>
          <tr>
            <th style={{ width: 40 }}>#</th>
            <th>Operation</th>
            <th style={{ width: 70 }}>Warning</th>
            <th style={{ width: 60 }}>SQL</th>
          </tr>
        </thead>
        <tbody>
          {operations.map((op, i) => (
            <OperationRow
              key={i}
              index={i}
              op={op}
              isExpanded={!!expanded[i]}
              onToggle={() => setExpanded((e) => ({ ...e, [i]: !e[i] }))}
            />
          ))}
        </tbody>
      </table>
    </div>
  );
}

function OperationRow({ index, op, isExpanded, onToggle }: {
  index: number;
  op: MigrationOp;
  isExpanded: boolean;
  onToggle: () => void;
}) {
  return (
    <>
      <tr>
        <td className="text-muted">{index + 1}</td>
        <td className="text-sm">{op.description}</td>
        <td>
          {op.isWarning
            ? <span style={{ color: 'var(--warning, #e8a33d)' }}>Yes</span>
            : <span style={{ color: 'var(--success)' }}>No</span>}
        </td>
        <td>
          <button className="btn-sm btn-ghost text-xs" onClick={onToggle}>
            {isExpanded ? 'Hide' : 'Show'}
          </button>
        </td>
      </tr>
      {isExpanded && (
        <tr>
          <td colSpan={4} style={{ padding: 0 }}>
            <pre className="evolve-sql-preview">{op.sql}</pre>
          </td>
        </tr>
      )}
    </>
  );
}
