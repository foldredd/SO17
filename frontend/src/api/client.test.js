import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, ApiError } from './client';

afterEach(() => vi.unstubAllGlobals());
describe('HTTP client security and failures', () => {
  it('requests a CSRF token before writing and includes cookies', async () => {
    const fetch = vi.fn().mockResolvedValueOnce(new Response(JSON.stringify({ token: 'test-token' })))
      .mockResolvedValueOnce(new Response(null, { status: 204 }));
    vi.stubGlobal('fetch', fetch);
    await expect(api('/auth/logout', { method: 'POST' })).resolves.toBeNull();
    expect(fetch.mock.calls[1][1].headers['X-CSRF-TOKEN']).toBe('test-token');
    expect(fetch.mock.calls[1][1].credentials).toBe('same-origin');
  });
  it('does not retry failed writes', async () => {
    const fetch = vi.fn().mockResolvedValueOnce(new Response(JSON.stringify({ token: 'token' })))
      .mockRejectedValueOnce(new TypeError('network'));
    vi.stubGlobal('fetch', fetch);
    await expect(api('/orders', { method: 'POST', body: {} })).rejects.toBeInstanceOf(ApiError);
    expect(fetch).toHaveBeenCalledTimes(2);
  });
  it('keeps cancellation distinguishable from network errors', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new DOMException('cancelled', 'AbortError')));
    await expect(api('/orders')).rejects.toHaveProperty('name', 'AbortError');
  });
  it('shows validation details from the server', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ errors: { phone: ['Неверный телефон'] } }), { status: 400 })));
    await expect(api('/customers')).rejects.toThrow('Неверный телефон');
  });
});
