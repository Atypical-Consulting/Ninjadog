import { create } from 'zustand';

export type ViewMode = 'split' | 'form' | 'json';
export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
  exiting?: boolean;
}

interface UiStore {
  viewMode: ViewMode;
  connected: boolean;
  showReconnected: boolean;
  toasts: Toast[];
  shortcutsOpen: boolean;
  templatePickerOpen: boolean;
  importModal: { open: boolean; entityName: string; callback: ((rows: Record<string, unknown>[]) => void) | null };
  buildConsole: { open: boolean; lines: string[] };

  setViewMode: (mode: ViewMode) => void;
  setConnected: (c: boolean) => void;
  showToast: (message: string, type?: ToastType, durationMs?: number) => void;
  dismissToast: (id: number) => void;
  toggleShortcuts: () => void;
  setShortcutsOpen: (open: boolean) => void;
  setTemplatePickerOpen: (open: boolean) => void;
  openImportModal: (entityName: string, callback: (rows: Record<string, unknown>[]) => void) => void;
  closeImportModal: () => void;
  showBuildConsole: (lines: string[]) => void;
  closeBuildConsole: () => void;
  closeAllOverlays: () => void;
}

let toastCounter = 0;

export const useUiStore = create<UiStore>((set, get) => ({
  viewMode: 'split',
  connected: true,
  showReconnected: false,
  toasts: [],
  shortcutsOpen: false,
  templatePickerOpen: false,
  importModal: { open: false, entityName: '', callback: null },
  buildConsole: { open: false, lines: [] },

  setViewMode: (mode) => set({ viewMode: mode }),

  setConnected: (c) => {
    set({ connected: c });
    if (c) {
      set({ showReconnected: true });
      setTimeout(() => set({ showReconnected: false }), 2000);
    }
  },

  showToast: (message, type = 'info', durationMs = 3000) => {
    const id = ++toastCounter;
    const toast: Toast = { id, message, type };
    set((s) => ({ toasts: [...s.toasts, toast] }));
    setTimeout(() => {
      set((s) => ({
        toasts: s.toasts.map((t) => (t.id === id ? { ...t, exiting: true } : t)),
      }));
      setTimeout(() => {
        set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) }));
      }, 250);
    }, durationMs);
  },

  dismissToast: (id) => {
    set((s) => ({
      toasts: s.toasts.map((t) => (t.id === id ? { ...t, exiting: true } : t)),
    }));
    setTimeout(() => {
      set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) }));
    }, 250);
  },

  toggleShortcuts: () => set((s) => ({ shortcutsOpen: !s.shortcutsOpen })),
  setShortcutsOpen: (open) => set({ shortcutsOpen: open }),
  setTemplatePickerOpen: (open) => set({ templatePickerOpen: open }),

  openImportModal: (entityName, callback) =>
    set({ importModal: { open: true, entityName, callback } }),
  closeImportModal: () =>
    set({ importModal: { open: false, entityName: '', callback: null } }),

  showBuildConsole: (lines) => set({ buildConsole: { open: true, lines } }),
  closeBuildConsole: () => set({ buildConsole: { open: false, lines: [] } }),

  closeAllOverlays: () => {
    const s = get();
    if (s.shortcutsOpen) set({ shortcutsOpen: false });
    if (s.templatePickerOpen) set({ templatePickerOpen: false });
    if (s.importModal.open) set({ importModal: { open: false, entityName: '', callback: null } });
    if (s.buildConsole.open) set({ buildConsole: { open: false, lines: [] } });
  },
}));
