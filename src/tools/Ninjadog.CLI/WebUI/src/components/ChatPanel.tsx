import { useState, useRef, useEffect } from 'react';
import { useChatStore } from '../store/chat-store';

export default function ChatPanel() {
  const open = useChatStore((s) => s.open);
  const messages = useChatStore((s) => s.messages);
  const loading = useChatStore((s) => s.loading);
  const sendMessage = useChatStore((s) => s.sendMessage);
  const applyConfig = useChatStore((s) => s.applyConfig);
  const clear = useChatStore((s) => s.clear);
  const toggle = useChatStore((s) => s.toggle);

  const [input, setInput] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, loading]);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

  if (!open) return null;

  const handleSend = () => {
    const text = input.trim();
    if (!text || loading) return;
    setInput('');
    sendMessage(text);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div
      className="flex flex-col shrink-0 border-l"
      style={{
        width: 380,
        background: 'var(--bg-card)',
        borderColor: 'var(--border)',
      }}
    >
      {/* Header */}
      <div
        className="flex items-center justify-between px-4 py-3 shrink-0 border-b"
        style={{ borderColor: 'var(--border)' }}
      >
        <div className="flex items-center gap-2">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="var(--accent)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
          </svg>
          <span className="text-sm font-semibold" style={{ color: 'var(--text)' }}>
            AI Assistant
          </span>
        </div>
        <div className="flex items-center gap-1">
          {messages.length > 0 && (
            <button
              className="chat-icon-btn"
              title="Clear chat"
              onClick={clear}
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
              </svg>
            </button>
          )}
          <button
            className="chat-icon-btn"
            title="Close"
            onClick={toggle}
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-auto p-4 space-y-3" style={{ minHeight: 0 }}>
        {messages.length === 0 && !loading && (
          <div className="text-center py-8" style={{ color: 'var(--text-muted)' }}>
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" className="mx-auto mb-3 opacity-50">
              <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
            </svg>
            <p className="text-sm mb-1">Describe your API in plain English</p>
            <p className="text-xs opacity-60">
              e.g. &quot;A blog platform with users, posts, tags, and comments&quot;
            </p>
          </div>
        )}

        {messages.map((msg, i) => (
          <div key={i} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div
              className="rounded-lg px-3 py-2 text-sm max-w-[90%]"
              style={{
                background: msg.role === 'user' ? 'var(--accent-dim)' : 'var(--bg-elevated)',
                border: `1px solid ${msg.role === 'user' ? 'rgba(232, 85, 61, 0.2)' : 'var(--border-dim)'}`,
                color: 'var(--text)',
              }}
            >
              <p className="whitespace-pre-wrap">{msg.content}</p>

              {msg.generatedJson && (
                <div className="mt-2 pt-2" style={{ borderTop: '1px solid var(--border-dim)' }}>
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs" style={{ color: 'var(--text-muted)' }}>
                      Generated config
                    </span>
                    {msg.validationIssues && msg.validationIssues.length > 0 && (
                      <span className="text-xs px-1.5 py-0.5 rounded" style={{ background: 'rgba(229, 168, 66, 0.15)', color: 'var(--warning)' }}>
                        {msg.validationIssues.length} issue{msg.validationIssues.length > 1 ? 's' : ''}
                      </span>
                    )}
                  </div>
                  <pre
                    className="text-xs overflow-auto rounded p-2 mb-2 font-mono"
                    style={{
                      background: 'var(--bg-deep)',
                      border: '1px solid var(--border-dim)',
                      maxHeight: 200,
                      color: 'var(--text-secondary)',
                    }}
                  >
                    {msg.jsonPreview}
                    {msg.generatedJson!.length > 500 ? '\n...' : ''}
                  </pre>
                  <button
                    className="chat-apply-btn"
                    onClick={() => applyConfig(msg.generatedJson!)}
                  >
                    Apply to config
                  </button>

                  {msg.validationIssues && msg.validationIssues.length > 0 && (
                    <div className="mt-2 space-y-1">
                      {msg.validationIssues.map((issue, j) => (
                        <p key={j} className="text-xs" style={{ color: issue.severity === 'error' ? 'var(--danger)' : 'var(--warning)' }}>
                          {issue.path}: {issue.message}
                        </p>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        ))}

        {loading && (
          <div className="flex justify-start">
            <div
              className="rounded-lg px-3 py-2 text-sm"
              style={{ background: 'var(--bg-elevated)', border: '1px solid var(--border-dim)' }}
            >
              <div className="flex items-center gap-2" style={{ color: 'var(--text-muted)' }}>
                <span className="inline-flex gap-1">
                  <span className="w-1.5 h-1.5 rounded-full animate-pulse" style={{ background: 'var(--accent)', animationDelay: '0ms' }} />
                  <span className="w-1.5 h-1.5 rounded-full animate-pulse" style={{ background: 'var(--accent)', animationDelay: '150ms' }} />
                  <span className="w-1.5 h-1.5 rounded-full animate-pulse" style={{ background: 'var(--accent)', animationDelay: '300ms' }} />
                </span>
                <span className="text-xs">Generating...</span>
              </div>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <div className="shrink-0 p-3 border-t" style={{ borderColor: 'var(--border)' }}>
        <div
          className="flex items-end gap-2 rounded-lg p-2"
          style={{ background: 'var(--bg-elevated)', border: '1px solid var(--border)' }}
        >
          <textarea
            ref={inputRef}
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Describe your API..."
            rows={1}
            className="flex-1 bg-transparent border-none outline-none resize-none text-sm"
            style={{
              color: 'var(--text)',
              maxHeight: 120,
              minHeight: 24,
              fontFamily: 'var(--font-body)',
            }}
            disabled={loading}
          />
          <button
            onClick={handleSend}
            disabled={!input.trim() || loading}
            className="shrink-0 p-1.5 rounded transition-colors"
            style={{
              color: input.trim() && !loading ? 'var(--accent)' : 'var(--text-muted)',
              opacity: input.trim() && !loading ? 1 : 0.5,
            }}
            title="Send (Enter)"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="22" y1="2" x2="11" y2="13" /><polygon points="22 2 15 22 11 13 2 9 22 2" />
            </svg>
          </button>
        </div>
        <p className="text-xs mt-1.5 px-1" style={{ color: 'var(--text-muted)' }}>
          Enter to send · Shift+Enter for newline · Requires ANTHROPIC_API_KEY
        </p>
      </div>
    </div>
  );
}
