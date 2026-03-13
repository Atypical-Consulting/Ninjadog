import { useState, useCallback, useEffect, useRef } from 'react';

export function useResizablePanel(initialWidth: number) {
  const [panelWidth, setPanelWidth] = useState(initialWidth);
  const isResizing = useRef(false);

  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
    isResizing.current = true;
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  }, []);

  useEffect(() => {
    const onMove = (e: MouseEvent) => {
      if (!isResizing.current) return;
      const viewportWidth = window.innerWidth;
      const panelRight = viewportWidth - e.clientX;
      const min = 250;
      const max = viewportWidth * 0.6;
      setPanelWidth(Math.min(Math.max(panelRight, min), max));
    };

    const onUp = () => {
      if (isResizing.current) {
        isResizing.current = false;
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
      }
    };

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup', onUp);
    return () => {
      document.removeEventListener('mousemove', onMove);
      document.removeEventListener('mouseup', onUp);
    };
  }, []);

  return { panelWidth, handleMouseDown };
}
