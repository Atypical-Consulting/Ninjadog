import { Routes, Route, Navigate } from 'react-router';
import { useEffect } from 'react';
import Layout from './components/Layout';
import ConfigPage from './pages/ConfigPage';
import EntitiesPage from './pages/EntitiesPage';
import EnumsPage from './pages/EnumsPage';
import SeedPage from './pages/SeedPage';
import { useConfigStore } from './store/config-store';
import { useKeyboardShortcuts } from './hooks/useKeyboardShortcuts';

export default function App() {
  const loadConfig = useConfigStore((s) => s.loadConfig);

  useEffect(() => {
    loadConfig();
  }, [loadConfig]);

  useKeyboardShortcuts();

  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/config" element={<ConfigPage />} />
        <Route path="/entities" element={<EntitiesPage />} />
        <Route path="/enums" element={<EnumsPage />} />
        <Route path="/seed" element={<SeedPage />} />
        <Route path="/" element={<Navigate to="/config" replace />} />
        <Route path="*" element={<Navigate to="/config" replace />} />
      </Route>
    </Routes>
  );
}
