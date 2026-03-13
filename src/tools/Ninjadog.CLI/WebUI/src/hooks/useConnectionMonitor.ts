import { useEffect } from 'react';
import { onConnectionChange } from '../api/ninjadog-api';
import { useUiStore } from '../store/ui-store';

export function useConnectionMonitor() {
  const setConnected = useUiStore((s) => s.setConnected);

  useEffect(() => {
    return onConnectionChange(setConnected);
  }, [setConnected]);
}
