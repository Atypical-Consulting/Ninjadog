import { create } from 'zustand';
import * as api from '../api/ninjadog-api';
import { useConfigStore } from './config-store';

export interface ChatMessageEntry {
  role: 'user' | 'assistant';
  content: string;
  generatedJson?: string;
  jsonPreview?: string;
  validationIssues?: Array<{ path: string; message: string; severity: string }>;
}

interface ChatStore {
  open: boolean;
  messages: ChatMessageEntry[];
  loading: boolean;

  toggle: () => void;
  setOpen: (open: boolean) => void;
  sendMessage: (text: string) => Promise<void>;
  applyConfig: (json: string) => void;
  clear: () => void;
}

function formatJsonPreview(json: string): string {
  try {
    return JSON.stringify(JSON.parse(json), null, 2).slice(0, 500);
  } catch {
    return json.slice(0, 500);
  }
}

export const useChatStore = create<ChatStore>((set, get) => ({
  open: false,
  messages: [],
  loading: false,

  toggle: () => set((s) => ({ open: !s.open })),
  setOpen: (open) => set({ open }),

  sendMessage: async (text: string) => {
    const userMsg: ChatMessageEntry = { role: 'user', content: text };
    set((s) => ({
      messages: [...s.messages, userMsg],
      loading: true,
    }));

    try {
      // Build API messages from conversation history
      const allMessages = [...get().messages];
      const apiMessages: api.AiChatMessage[] = allMessages.map((m) => {
        // For assistant messages that had generated JSON, send the JSON as the content
        // so the LLM can build on it in subsequent turns
        const content = m.generatedJson || m.content;
        return { role: m.role, content };
      });

      const result = await api.generate(apiMessages);

      const assistantMsg: ChatMessageEntry = {
        role: 'assistant',
        content: result.json
          ? 'Configuration generated successfully.'
          : result.error || 'Failed to generate configuration.',
        generatedJson: result.json || undefined,
        jsonPreview: result.json ? formatJsonPreview(result.json) : undefined,
        validationIssues: result.validation?.diagnostics,
      };

      set((s) => ({
        messages: [...s.messages, assistantMsg],
        loading: false,
      }));
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Unknown error';
      set((s) => ({
        messages: [
          ...s.messages,
          { role: 'assistant' as const, content: `Error: ${errorMsg}` },
        ],
        loading: false,
      }));
    }
  },

  applyConfig: (json: string) => {
    try {
      const parsed = JSON.parse(json);
      const configStore = useConfigStore.getState();
      configStore.pushUndo();
      configStore.setState(parsed);
      configStore.onStateChanged();
    } catch {
      // Invalid JSON — shouldn't happen since we validated it
    }
  },

  clear: () => set({ messages: [] }),
}));
