let serverDown = false;
let healthCheckTimer: ReturnType<typeof setInterval> | null = null;

type ConnectionHandler = (connected: boolean) => void;
const connectionHandlers: ConnectionHandler[] = [];

export function onConnectionChange(handler: ConnectionHandler) {
  connectionHandlers.push(handler);
  return () => {
    const idx = connectionHandlers.indexOf(handler);
    if (idx >= 0) connectionHandlers.splice(idx, 1);
  };
}

function notifyConnection(connected: boolean) {
  connectionHandlers.forEach((h) => h(connected));
}

function startHealthCheck() {
  if (healthCheckTimer) return;
  healthCheckTimer = setInterval(async () => {
    try {
      const res = await fetch('/api/schema', { method: 'GET' });
      if (res.ok) {
        serverDown = false;
        stopHealthCheck();
        notifyConnection(true);
      }
    } catch {
      // still down
    }
  }, 3000);
}

function stopHealthCheck() {
  if (healthCheckTimer) {
    clearInterval(healthCheckTimer);
    healthCheckTimer = null;
  }
}

async function request(url: string, options?: RequestInit): Promise<Response> {
  try {
    const res = await fetch(url, options);
    if (serverDown) {
      serverDown = false;
      stopHealthCheck();
      notifyConnection(true);
    }
    return res;
  } catch (err) {
    if (!serverDown) {
      serverDown = true;
      startHealthCheck();
      notifyConnection(false);
    }
    throw err;
  }
}

export function isServerDown() {
  return serverDown;
}

export async function getConfig(): Promise<Record<string, unknown>> {
  const res = await request('/api/config');
  if (!res.ok) throw new Error(`GET /api/config failed: ${res.status}`);
  return res.json();
}

export async function saveConfig(config: string | object) {
  const body = typeof config === 'string' ? config : JSON.stringify(config, null, 2);
  const res = await request('/api/config', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { error?: string }).error || `POST /api/config failed: ${res.status}`);
  }
  return res.json();
}

export async function validate(config: string | object) {
  if (serverDown) throw new Error('Server unavailable');
  const body = typeof config === 'string' ? config : JSON.stringify(config, null, 2);
  const res = await request('/api/validate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body,
  });
  if (!res.ok) throw new Error(`POST /api/validate failed: ${res.status}`);
  return res.json();
}

export async function build() {
  const res = await request('/api/build', { method: 'POST' });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { error?: string }).error || `POST /api/build failed: ${res.status}`);
  }
  return res.json();
}

export async function getSchema() {
  const res = await request('/api/schema');
  if (!res.ok) throw new Error(`GET /api/schema failed: ${res.status}`);
  return res.json();
}

export interface AiChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface AiGenerationResult {
  success: boolean;
  json?: string;
  error?: string;
  validation?: {
    isValid: boolean;
    diagnostics: Array<{ path: string; message: string; severity: string }>;
  };
}

export async function generate(messages: AiChatMessage[]): Promise<AiGenerationResult> {
  const res = await request('/api/generate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ messages }),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { error?: string }).error || `POST /api/generate failed: ${res.status}`);
  }
  return res.json();
}

export async function getDirectories(path?: string) {
  const res = await request(`/api/directories?path=${encodeURIComponent(path || '.')}`);
  if (!res.ok) throw new Error(`GET /api/directories failed: ${res.status}`);
  return res.json() as Promise<{
    current: string;
    absolute: string;
    parent: string | null;
    directories: string[];
  }>;
}
