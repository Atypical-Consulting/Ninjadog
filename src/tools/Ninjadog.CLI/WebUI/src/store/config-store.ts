import { create } from 'zustand';
import * as api from '../api/ninjadog-api';

/* eslint-disable @typescript-eslint/no-explicit-any */
export interface NinjadogState {
  config?: Record<string, any>;
  entities?: Record<string, any>;
  enums?: Record<string, string[]>;
  '$schema'?: string;
  [key: string]: any;
}

interface ConfigStore {
  state: NinjadogState;
  dirty: boolean;
  undoStack: string[];
  redoStack: string[];
  savedSnapshot: string;
  autoSaveEnabled: boolean;
  autoSaveTimer: ReturnType<typeof setTimeout> | null;

  setState: (s: NinjadogState) => void;
  loadConfig: () => Promise<void>;
  saveConfig: () => Promise<void>;
  buildConfig: () => Promise<{ success: boolean; message?: string }>;
  pushUndo: () => void;
  undo: () => void;
  redo: () => void;
  markClean: () => void;
  onStateChanged: () => void;
  setAutoSave: (enabled: boolean) => void;
  getJson: () => Record<string, any>;
}

const MAX_UNDO = 50;

function buildJson(state: NinjadogState): Record<string, any> {
  const result: Record<string, any> = {};
  if (state['$schema']) result['$schema'] = state['$schema'];
  if (state.config && Object.keys(state.config).length > 0) result.config = state.config;
  if (state.entities && Object.keys(state.entities).length > 0) {
    result.entities = cleanEntities(state.entities);
  }
  if (state.enums && Object.keys(state.enums).length > 0) result.enums = state.enums;
  return result;
}

function cleanEntities(entities: Record<string, any>): Record<string, any> {
  const cleaned: Record<string, any> = {};
  for (const [name, entity] of Object.entries(entities)) {
    const e: Record<string, any> = {};
    if (entity.properties && Object.keys(entity.properties).length > 0) {
      e.properties = entity.properties;
    }
    if (entity.relationships && Object.keys(entity.relationships).length > 0) {
      e.relationships = entity.relationships;
    }
    if (entity.seedData && entity.seedData.length > 0) {
      e.seedData = entity.seedData;
    }
    cleaned[name] = e;
  }
  return cleaned;
}

export const useConfigStore = create<ConfigStore>((set, get) => ({
  state: {},
  dirty: false,
  undoStack: [],
  redoStack: [],
  savedSnapshot: '',
  autoSaveEnabled: localStorage.getItem('ninjadog:autoSave') === 'true',
  autoSaveTimer: null,

  setState: (s) => set({ state: s }),

  loadConfig: async () => {
    try {
      const config = await api.getConfig();
      const snapshot = JSON.stringify(config || {});
      set({ state: (config || {}) as NinjadogState, savedSnapshot: snapshot, dirty: false });
    } catch {
      set({ state: {}, savedSnapshot: '{}', dirty: false });
    }
  },

  saveConfig: async () => {
    const { state: s, markClean } = get();
    const jsonText = JSON.stringify(buildJson(s), null, 2);
    await api.saveConfig(jsonText);
    markClean();
  },

  buildConfig: async () => {
    const { saveConfig } = get();
    await saveConfig();
    const result = await api.build();
    return result as { success: boolean; message?: string };
  },

  pushUndo: () => {
    const { state: s, undoStack } = get();
    const newStack = [...undoStack, JSON.stringify(s)];
    if (newStack.length > MAX_UNDO) newStack.shift();
    set({ undoStack: newStack, redoStack: [] });
  },

  undo: () => {
    const { state: s, undoStack, redoStack } = get();
    if (undoStack.length === 0) return;
    const newUndo = [...undoStack];
    const prev = newUndo.pop()!;
    set({
      redoStack: [...redoStack, JSON.stringify(s)],
      undoStack: newUndo,
      state: JSON.parse(prev),
      dirty: true,
    });
  },

  redo: () => {
    const { state: s, undoStack, redoStack } = get();
    if (redoStack.length === 0) return;
    const newRedo = [...redoStack];
    const next = newRedo.pop()!;
    set({
      undoStack: [...undoStack, JSON.stringify(s)],
      redoStack: newRedo,
      state: JSON.parse(next),
      dirty: true,
    });
  },

  markClean: () => {
    const { state: s } = get();
    set({ savedSnapshot: JSON.stringify(s), dirty: false });
  },

  onStateChanged: () => {
    set({ dirty: true });
    // Schedule auto-save
    const { autoSaveEnabled, autoSaveTimer } = get();
    if (autoSaveTimer) clearTimeout(autoSaveTimer);
    if (autoSaveEnabled) {
      const timer = setTimeout(async () => {
        const store = get();
        if (!store.dirty) return;
        try {
          await store.saveConfig();
        } catch {
          // silent failure
        }
      }, 3000);
      set({ autoSaveTimer: timer });
    }
  },

  setAutoSave: (enabled) => {
    localStorage.setItem('ninjadog:autoSave', String(enabled));
    set({ autoSaveEnabled: enabled });
    if (!enabled) {
      const { autoSaveTimer } = get();
      if (autoSaveTimer) {
        clearTimeout(autoSaveTimer);
        set({ autoSaveTimer: null });
      }
    }
  },

  getJson: () => buildJson(get().state),
}));
