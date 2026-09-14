import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Plus, Search } from 'lucide-react';
import { queryString } from '../api/client';
import { useResource } from '../hooks/useResource';
import { statusLabels } from '../lib/format';
import { EmptyState, Loading, ResourceError } from '../components/Feedback';
import { PageHeading } from '../components/PageHeading';
import { Pagination } from '../components/Pagination';
import { OrderTable } from '../components/OrderTable';
import { OrderDetails } from '../components/OrderDetails';
import { OrderComposer } from '../components/OrderComposer';
import { useLocale } from '../i18n/LocaleContext';

export function OrdersPage() {
  const { t, language } = useLocale();
  const [params, setParams] = useSearchParams();
  const [page, setPage] = useState(1);
  const [draft, setDraft] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [selected, setSelected] = useState(null);
  const [notice, setNotice] = useState('');
  const { data, loading, error, refresh } = useResource('/orders?' + queryString({ page, search, status }));
  return <><PageHeading eyebrow="SO17" title={t('orders')} action={<button className="button primary" onClick={() => setParams({ new: '1' })}><Plus size={18} />{t('newOrder')}</button>}>{t('dashboardText')}</PageHeading>
    {notice && <div className="notice" role="status">{notice}</div>}
    <section className="panel"><div className="filter-tabs" role="group" aria-label={t('orders')}>{[['', t('allOrders')], ...Object.entries(statusLabels(language))].map(([value, label]) => <button key={value} className={status === value ? 'selected' : ''} onClick={() => { setStatus(value); setPage(1); }}>{label}</button>)}</div>
      <div className="list-toolbar"><form className="search-form" onSubmit={event => { event.preventDefault(); setSearch(draft.trim()); setPage(1); }}><Search size={18} /><input aria-label={t('orders')} placeholder={t('orderSearch')} value={draft} onChange={event => setDraft(event.target.value)} maxLength={100} /><button className="button secondary compact" type="submit">{t('search')}</button></form><span className="list-count">{data?.total ?? '—'} {t('ordersCount')}</span></div>
      {loading ? <Loading /> : error ? <ResourceError error={error} refresh={refresh} /> : data.items.length ? <OrderTable orders={data.items} onOpen={setSelected} /> : <EmptyState title={t('ordersEmpty')}>{t('orderEmptyText')}</EmptyState>}
      <Pagination data={data} page={page} onChange={setPage} /></section>
    {params.get('new') === '1' && <OrderComposer onClose={() => setParams({})} onCreated={order => { setParams({}); setNotice(t('itemAddedMessage')); setSelected(order); refresh(); }} />}
    {selected && <OrderDetails order={selected} onClose={() => setSelected(null)} onChanged={refresh} />}
  </>;
}
