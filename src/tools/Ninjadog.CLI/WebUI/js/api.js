/**
 * Ninjadog API client — thin wrapper around fetch for the backend endpoints.
 */
const NinjadogApi = (() => {
    let serverDown = false;
    let healthCheckTimer = null;

    async function request(url, options) {
        try {
            const res = await fetch(url, options);
            if (serverDown) {
                serverDown = false;
                stopHealthCheck();
                document.dispatchEvent(new CustomEvent('ninjadog:connected'));
            }
            return res;
        } catch (err) {
            if (!serverDown) {
                serverDown = true;
                startHealthCheck();
                document.dispatchEvent(new CustomEvent('ninjadog:disconnected'));
            }
            throw err;
        }
    }

    function startHealthCheck() {
        if (healthCheckTimer) return;
        healthCheckTimer = setInterval(async () => {
            try {
                const res = await fetch('/api/schema', { method: 'GET' });
                if (res.ok) {
                    serverDown = false;
                    stopHealthCheck();
                    document.dispatchEvent(new CustomEvent('ninjadog:connected'));
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

    function isServerDown() {
        return serverDown;
    }

    async function getConfig() {
        const res = await request('/api/config');
        if (!res.ok) throw new Error(`GET /api/config failed: ${res.status}`);
        return res.json();
    }

    async function saveConfig(config) {
        const body = typeof config === 'string' ? config : JSON.stringify(config, null, 2);
        const res = await request('/api/config', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: body
        });
        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            throw new Error(err.error || `POST /api/config failed: ${res.status}`);
        }
        return res.json();
    }

    async function validate(config) {
        if (serverDown) throw new Error('Server unavailable');
        const body = typeof config === 'string' ? config : JSON.stringify(config, null, 2);
        const res = await request('/api/validate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: body
        });
        if (!res.ok) throw new Error(`POST /api/validate failed: ${res.status}`);
        return res.json();
    }

    async function build() {
        const res = await request('/api/build', { method: 'POST' });
        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            throw new Error(err.error || `POST /api/build failed: ${res.status}`);
        }
        return res.json();
    }

    async function getSchema() {
        const res = await request('/api/schema');
        if (!res.ok) throw new Error(`GET /api/schema failed: ${res.status}`);
        return res.json();
    }

    async function getDirectories(path) {
        const res = await request(`/api/directories?path=${encodeURIComponent(path || '.')}`);
        if (!res.ok) throw new Error(`GET /api/directories failed: ${res.status}`);
        return res.json();
    }

    return { getConfig, saveConfig, validate, build, getSchema, getDirectories, isServerDown };
})();
