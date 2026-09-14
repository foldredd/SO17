import { useCallback, useEffect, useState } from 'react';
import { api } from '../api/client';
import { AuthContext } from './AuthContext';

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [connectionError, setConnectionError] = useState('');
  const [revision, setRevision] = useState(0);
  const reload = useCallback(() => {
    setLoading(true);
    setRevision(value => value + 1);
  }, []);
  useEffect(() => {
    const controller = new AbortController();
    api('/auth/me', { signal: controller.signal })
      .then(profile => { if (!controller.signal.aborted) { setUser(profile); setConnectionError(''); } })
      .catch(error => { if (!controller.signal.aborted) { setUser(null); setConnectionError(error.status === 401 ? '' : error.message); } })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [revision]);
  useEffect(() => { const expired = () => setUser(null);
    window.addEventListener('so17:session-expired', expired);
    return () => window.removeEventListener('so17:session-expired', expired);
  }, []);
  const authenticate = async (mode, body) => { const profile = await api('/auth/' + mode, { method: 'POST', body }); setUser(profile); };
  const logout = async () => { await api('/auth/logout', { method: 'POST' }); setUser(null); };
  return <AuthContext.Provider value={{ user, setUser, loading, connectionError, reload, authenticate, logout }}>{children}</AuthContext.Provider>;
}
