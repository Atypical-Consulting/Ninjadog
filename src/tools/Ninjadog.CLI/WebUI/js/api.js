/**
 * Ninjadog API client — thin wrapper around fetch for the backend endpoints.
 */
const NinjadogApi = (() => {
    async function getConfig() {
        const res = await fetch('/api/config');
        if (!res.ok) throw new Error(`GET /api/config failed: ${res.status}`);
        return res.json();
    }

    async function saveConfig(config) {
        const res = await fetch('/api/config', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(config, null, 2)
        });
        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            throw new Error(err.error || `POST /api/config failed: ${res.status}`);
        }
        return res.json();
    }

    async function validate(config) {
        const res = await fetch('/api/validate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(config, null, 2)
        });
        if (!res.ok) throw new Error(`POST /api/validate failed: ${res.status}`);
        return res.json();
    }

    async function build() {
        const res = await fetch('/api/build', { method: 'POST' });
        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            throw new Error(err.error || `POST /api/build failed: ${res.status}`);
        }
        return res.json();
    }

    async function getSchema() {
        const res = await fetch('/api/schema');
        if (!res.ok) throw new Error(`GET /api/schema failed: ${res.status}`);
        return res.json();
    }

    return { getConfig, saveConfig, validate, build, getSchema };
})();
