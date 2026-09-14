export class ApiError extends Error {
  constructor(message, status, errors = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
  }
}
const english = () => typeof localStorage !== 'undefined' && localStorage.getItem('so17.language') === 'en';

async function readResponse(response) {
  if (response.status === 204) return null;
  const payload = await response.json().catch(() => null);
  if (!response.ok) {
    const details = payload?.errors ? Object.values(payload.errors).flat().join(' ') : '';
    throw new ApiError(response.status === 429 ? (english() ? 'Too many requests. Wait a minute.' : 'Забагато запитів. Зачекайте хвилину.') :
      details || payload?.title || (response.status === 401 ? (english() ? 'Sign in to continue.' : 'Увійдіть до акаунта.') : (english() ? 'The request could not be completed.' : 'Не вдалося виконати запит.')), response.status, payload?.errors);
  }
  return payload;
}

export async function api(path, { method = 'GET', body, signal } = {}) {
  const headers = { Accept: 'application/json' };
  try {
    if (method !== 'GET') {
      // Refresh per mutation: login and password changes rotate the user's CSRF identity.
      const csrf = await readResponse(await fetch('/api/auth/csrf', { credentials: 'same-origin', signal }));
      headers['X-CSRF-TOKEN'] = csrf.token;
      if (body !== undefined) headers['Content-Type'] = 'application/json';
    }
    const response = await fetch('/api' + path, { method, headers, credentials: 'same-origin',
      body: body === undefined ? undefined : JSON.stringify(body), signal });
    if (response.status === 401 && !['/auth/login', '/auth/me'].includes(path))
      window.dispatchEvent(new Event('so17:session-expired'));
    return await readResponse(response);
  } catch (error) {
    if (error.name === 'AbortError' || error instanceof ApiError) throw error;
    throw new ApiError(english() ? 'No connection to the server. Check it and try again.' : 'Немає з’єднання із сервером. Перевірте його та повторіть спробу.', 0);
  }
}

export function queryString(values) {
  return new URLSearchParams(Object.entries(values).filter(([, value]) => value !== '' && value != null)).toString();
}
