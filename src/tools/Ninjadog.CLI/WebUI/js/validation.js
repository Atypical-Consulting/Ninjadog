/**
 * Validation panel — displays validation diagnostics.
 */
const ValidationDisplay = (() => {
    function render(result) {
        const container = document.getElementById('validation-messages');
        if (!result || !result.diagnostics || result.diagnostics.length === 0) {
            container.innerHTML = '<p class="text-xs text-gray-500">No validation issues.</p>';
            return;
        }

        container.innerHTML = result.diagnostics.map(d => {
            const cls = d.severity === 2 ? 'diag-error' :
                        d.severity === 1 ? 'diag-warning' : 'diag-info';
            const icon = d.severity === 2 ? '&#x2716;' :
                         d.severity === 1 ? '&#x26A0;' : '&#x2139;';
            const label = d.severity === 2 ? 'Error' :
                          d.severity === 1 ? 'Warning' : 'Info';
            return `<div class="flex items-start gap-2 text-xs ${cls}">
                <span>${icon}</span>
                <span class="text-gray-400">[${d.path}]</span>
                <span>${esc(d.message)}</span>
            </div>`;
        }).join('');
    }

    function clear() {
        document.getElementById('validation-messages').innerHTML =
            '<p class="text-xs text-gray-500">No validation issues.</p>';
    }

    function esc(s) {
        return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    return { render, clear };
})();
