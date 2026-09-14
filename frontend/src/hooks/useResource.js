import { useCallback, useEffect, useState } from 'react';
import { api } from '../api/client';

export function useResource(path) {
  const [state, setState] = useState({ data: null, path: null, revision: -1, error: '' });
  const [revision, setRevision] = useState(0);
  useEffect(() => {
    const controller = new AbortController();
    api(path, { signal: controller.signal })
      .then(data => { if (!controller.signal.aborted) setState({ data, path, revision, error: '' }); })
      .catch(error => { if (!controller.signal.aborted) setState({ data: null, path, revision, error: error.message }); });
    return () => controller.abort();
  }, [path, revision]);
  const refresh = useCallback(() => setRevision(value => value + 1), []);
  const loading = state.path !== path || state.revision !== revision;
  return { data: loading ? null : state.data, error: loading ? '' : state.error, loading, refresh };
}
