import { describe, expect, it } from 'vitest';
import { nextStatuses, orderTotal } from './format';
import { queryString } from '../api/client';

describe('Order calculations and transitions', () => {
  it('sums decimal prices without fractional-cent drift', () => {
    expect(orderTotal([{ item: { price: 0.1 }, quantity: 1 }, { item: { price: 0.2 }, quantity: 1 }])).toBe(0.3);
    expect(orderTotal([{ item: { price: 125.5 }, quantity: '2' }])).toBe(251);
  });
  it('does not offer changes to final orders', () => {
    expect(nextStatuses('Completed')).toEqual([]);
    expect(nextStatuses('Cancelled')).toEqual([]);
    expect(nextStatuses('Pending')).toEqual(['Processing', 'Cancelled']);
  });
  it('encodes phone searches and excludes empty filters', () => {
    expect(queryString({ page: 1, search: '+7 900', status: '' })).toBe('page=1&search=%2B7+900');
  });
});
