import { useEffect, useRef } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';

export function useAutoSave() {
  const dirty = useConfigStore((s) => s.dirty);
  const autoSaveEnabled = useConfigStore((s) => s.autoSaveEnabled);
  const saveConfig = useConfigStore((s) => s.saveConfig);
  const showToast = useUiStore((s) => s.showToast);
  const timer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (!dirty || !autoSaveEnabled) return;

    if (timer.current) clearTimeout(timer.current);

    timer.current = setTimeout(async () => {
      try {
        await saveConfig();
        showToast('Auto-saved', 'success', 2000);
      } catch {
        // silent
      }
    }, 3000);

    return () => {
      if (timer.current) clearTimeout(timer.current);
    };
  }, [dirty, autoSaveEnabled, saveConfig, showToast]);
}
