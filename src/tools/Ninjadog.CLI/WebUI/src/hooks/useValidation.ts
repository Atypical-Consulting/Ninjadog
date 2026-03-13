import { useEffect, useRef } from 'react';
import { useConfigStore } from '../store/config-store';
import * as api from '../api/ninjadog-api';

export interface Diagnostic {
  severity: number;
  path: string;
  message: string;
}

export interface ValidationResult {
  diagnostics: Diagnostic[];
}

let latestResult: ValidationResult | null = null;
const listeners: (() => void)[] = [];

export function getValidationResult() {
  return latestResult;
}

export function onValidationChange(fn: () => void) {
  listeners.push(fn);
  return () => {
    const idx = listeners.indexOf(fn);
    if (idx >= 0) listeners.splice(idx, 1);
  };
}

export function useValidation() {
  const state = useConfigStore((s) => s.state);
  const getJson = useConfigStore((s) => s.getJson);
  const timer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (timer.current) clearTimeout(timer.current);

    timer.current = setTimeout(async () => {
      if (api.isServerDown()) return;
      try {
        const json = getJson();
        const result = await api.validate(json);
        latestResult = result as ValidationResult;
      } catch {
        latestResult = null;
      }
      listeners.forEach((fn) => fn());
    }, 500);

    return () => {
      if (timer.current) clearTimeout(timer.current);
    };
  }, [state, getJson]);
}
