import { useState } from 'react';
import { Search } from 'lucide-react';
import { useResource } from '../hooks/useResource';
import { queryString } from '../api/client';
import { EmptyState, Loading, ResourceError } from './Feedback';
import { Pagination } from './Pagination';
import { useLocale } from '../i18n/LocaleContext';

export function ResourcePicker({ path, label, render, onSelect, disabled = false }) {
  const { t } = useLocale();
  const [query, setQuery] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const { data, loading, error, refresh } = useResource(path + '?' + queryString({ search, page, pageSize: 5 }));
  return <div className="picker"><div className="picker-search"><Search size={17} /><input aria-label={label} placeholder={label} value={query} maxLength={100} disabled={disabled} onChange={event => setQuery(event.target.value)} onKeyDown={event => { if (event.key === 'Enter') { event.preventDefault(); setSearch(query.trim()); setPage(1); } }} /><button type="button" className="button secondary compact" disabled={disabled} onClick={() => { setSearch(query.trim()); setPage(1); }}>{t('search')}</button></div>
    {loading ? <Loading /> : error ? <ResourceError error={error} refresh={refresh} /> : data.items.length ? <ul className="picker-list">{data.items.map(record => <li key={record.id}><button type="button" disabled={disabled} onClick={() => onSelect(record)}>{render(record)}</button></li>)}</ul> : <EmptyState title={t('noRecords')}>{t('changeQuery')}</EmptyState>}
    <Pagination data={data} page={page} onChange={setPage} />
  </div>;
}
