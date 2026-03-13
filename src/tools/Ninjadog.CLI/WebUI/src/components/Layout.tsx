import { Outlet } from 'react-router';
import Header from './Header';
import TabBar from './TabBar';
import JsonPanel from './JsonPanel';
import ValidationPanel from './ValidationPanel';
import Toast from './Toast';
import ShortcutsOverlay from './ShortcutsOverlay';
import TemplatePicker from './TemplatePicker';
import ImportModal from './ImportModal';
import BuildConsole from './BuildConsole';
import { useUiStore } from '../store/ui-store';
import { useAutoSave } from '../hooks/useAutoSave';
import { useValidation } from '../hooks/useValidation';
import { useConnectionMonitor } from '../hooks/useConnectionMonitor';
import { useResizablePanel } from '../hooks/useResizablePanel';

export default function Layout() {
  const viewMode = useUiStore((s) => s.viewMode);
  useAutoSave();
  useValidation();
  useConnectionMonitor();
  const { panelWidth, handleMouseDown } = useResizablePanel(420);

  const showLeft = viewMode !== 'json';
  const showRight = viewMode !== 'form';

  return (
    <div className="flex flex-col h-full">
      <Toast />
      <ShortcutsOverlay />
      <ImportModal />
      <TemplatePicker />

      <Header />
      <TabBar />

      <div className="flex flex-1 overflow-hidden">
        <div id="left-panel" className={`flex flex-col flex-1 min-w-0${!showLeft ? ' hidden' : ''}`}>
          <div className="flex-1 overflow-auto p-6 content-area">
            <Outlet />
          </div>
        </div>

        <div
          id="resize-handle"
          className={`resize-handle${!(showLeft && showRight) ? ' hidden' : ''}`}
          onMouseDown={handleMouseDown}
        />

        <div id="json-panel" className={`flex flex-col${!showRight ? ' hidden' : ''}`} style={{ width: viewMode === 'json' ? '100%' : `${panelWidth}px` }}>
          <JsonPanel width={viewMode === 'json' ? '100%' : `${panelWidth}px`} />
        </div>
      </div>

      <BuildConsole />
      <ValidationPanel />
    </div>
  );
}
