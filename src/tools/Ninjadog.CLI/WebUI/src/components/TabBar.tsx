import { NavLink } from 'react-router';
import { useConfigStore } from '../store/config-store';
import { useUiStore, type ViewMode } from '../store/ui-store';

const TABS = [
  { path: '/config', label: 'Config' },
  { path: '/entities', label: 'Entities', badgeKey: 'entities' as const },
  { path: '/enums', label: 'Enums', badgeKey: 'enums' as const },
  { path: '/seed', label: 'Seed Data', badgeKey: 'seed' as const },
];

const VIEW_MODES: { mode: ViewMode; title: string; icon: React.ReactNode }[] = [
  {
    mode: 'split',
    title: 'Split View',
    icon: (
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><rect x="3" y="3" width="18" height="18" rx="2" /><line x1="12" y1="3" x2="12" y2="21" /></svg>
    ),
  },
  {
    mode: 'form',
    title: 'Form Only',
    icon: (
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><rect x="3" y="3" width="18" height="18" rx="2" /><line x1="9" y1="9" x2="15" y2="9" /><line x1="9" y1="13" x2="15" y2="13" /></svg>
    ),
  },
  {
    mode: 'json',
    title: 'JSON Only',
    icon: (
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="16 18 22 12 16 6" /><polyline points="8 6 2 12 8 18" /></svg>
    ),
  },
];

function useBadgeCounts() {
  const state = useConfigStore((s) => s.state);
  const entityCount = state.entities ? Object.keys(state.entities).length : 0;
  const enumCount = state.enums ? Object.keys(state.enums).length : 0;
  let seedCount = 0;
  if (state.entities) {
    for (const entity of Object.values(state.entities) as Array<{ seedData?: unknown[] }>) {
      if (entity.seedData && entity.seedData.length > 0) seedCount += entity.seedData.length;
    }
  }
  return { entities: entityCount, enums: enumCount, seed: seedCount };
}

export default function TabBar() {
  const viewMode = useUiStore((s) => s.viewMode);
  const setViewMode = useUiStore((s) => s.setViewMode);
  const badges = useBadgeCounts();

  return (
    <nav className="tab-bar flex items-center border-b border-bdr bg-surface-base shrink-0" style={{ height: 42 }}>
      {TABS.map((tab) => (
        <NavLink
          key={tab.path}
          to={tab.path}
          data-tab={tab.path.slice(1)}
          className={({ isActive }) =>
            `tab-btn px-5 text-sm font-medium${isActive ? ' active' : ''}`
          }
        >
          {tab.label}
          {tab.badgeKey && (
            <span id={`badge-${tab.badgeKey}`} className={`tab-badge${badges[tab.badgeKey] > 0 ? '' : ' hidden'}`}>{badges[tab.badgeKey]}</span>
          )}
        </NavLink>
      ))}

      <div className="ml-auto flex items-center gap-1 px-3">
        <div className="view-toggle">
          {VIEW_MODES.map((v) => (
            <button
              key={v.mode}
              className={`view-toggle-btn${viewMode === v.mode ? ' active' : ''}`}
              data-view={v.mode}
              title={v.title}
              onClick={() => setViewMode(v.mode)}
            >
              {v.icon}
            </button>
          ))}
        </div>
      </div>
    </nav>
  );
}
