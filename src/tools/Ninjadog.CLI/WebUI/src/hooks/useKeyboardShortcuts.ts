import { useEffect } from 'react';
import { useConfigStore } from '../store/config-store';
import { useUiStore } from '../store/ui-store';

export function useKeyboardShortcuts() {
  const saveConfig = useConfigStore((s) => s.saveConfig);
  const buildConfig = useConfigStore((s) => s.buildConfig);
  const undo = useConfigStore((s) => s.undo);
  const redo = useConfigStore((s) => s.redo);
  const showToast = useUiStore((s) => s.showToast);
  const toggleShortcuts = useUiStore((s) => s.toggleShortcuts);
  const closeAllOverlays = useUiStore((s) => s.closeAllOverlays);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const mod = e.metaKey || e.ctrlKey;
      const tag = (e.target as HTMLElement)?.tagName?.toLowerCase() || '';
      const isInput = tag === 'input' || tag === 'textarea' || (e.target as HTMLElement)?.isContentEditable;

      if (mod && e.key === 's') {
        e.preventDefault();
        saveConfig()
          .then(() => showToast('Configuration saved', 'success'))
          .catch((err: Error) => showToast('Save failed: ' + err.message, 'error'));
        return;
      }

      if (mod && e.key === 'z' && !e.shiftKey) {
        e.preventDefault();
        undo();
        return;
      }

      if (mod && e.key === 'z' && e.shiftKey) {
        e.preventDefault();
        redo();
        return;
      }

      if (mod && e.key === 'y') {
        e.preventDefault();
        redo();
        return;
      }

      if (mod && e.key === 'b') {
        e.preventDefault();
        buildConfig()
          .then((r) => showToast(r.success ? 'Build succeeded' : 'Build failed', r.success ? 'success' : 'error'))
          .catch((err: Error) => showToast('Build failed: ' + err.message, 'error'));
        return;
      }

      if (e.key === '?' && !isInput && !mod) {
        e.preventDefault();
        toggleShortcuts();
        return;
      }

      if (e.key === 'Escape') {
        closeAllOverlays();
        return;
      }
    };

    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [saveConfig, buildConfig, undo, redo, showToast, toggleShortcuts, closeAllOverlays]);
}
