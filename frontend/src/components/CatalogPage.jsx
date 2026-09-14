import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Pencil, Plus, Search, Trash2 } from 'lucide-react';
import { api, queryString } from '../api/client';
import { useResource } from '../hooks/useResource';
import { EmptyState, ErrorMessage, Loading, ResourceError } from './Feedback';
import { PageHeading } from './PageHeading';
import { Pagination } from './Pagination';
import { Modal } from './Modal';
import { PhoneInput } from './PhoneInput';
import { useLocale } from '../i18n/LocaleContext';

export function CatalogPage({ config }) {
  const { t } = useLocale();
  const [params, setParams] = useSearchParams();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [draft, setDraft] = useState('');
  const [edit, setEdit] = useState(null);
  const [remove, setRemove] = useState(null);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [busy, setBusy] = useState(false);
  const [phone, setPhone] = useState('');
  const { data, loading, error: loadError, refresh } = useResource(config.path + '?' + queryString({ page, search }));
  const creating = params.get('new') === '1';
  const close = () => { setParams({}); setEdit(null); setRemove(null); setPhone(''); setError(''); };
  const changed = message => { close(); setNotice(message); refresh(); };
  async function save(event) {
    event.preventDefault(); setBusy(true); setError('');
    const body = Object.fromEntries(new FormData(event.currentTarget));
    if (config.phoneField) body.phone = phone;
    for (const field of config.fields) if (field.type === 'number') body[field.name] = Number(body[field.name]);
    if (edit) body.version = edit.version;
    try { await api(config.path + (edit ? '/' + edit.id : ''), { method: edit ? 'PUT' : 'POST', body }); changed(t('dataSaved')); }
    catch (e) { setError(e.message); } finally { setBusy(false); }
  }
  async function deleteRecord() {
    setBusy(true); setError('');
    try { await api(config.path + '/' + remove.id + '?version=' + remove.version, { method: 'DELETE' }); if (data.items.length === 1 && page > 1) setPage(page - 1); changed(t('recordDeleted')); }
    catch (e) { setError(e.message); } finally { setBusy(false); }
  }
  return <><PageHeading eyebrow={config.eyebrow} title={config.title} action={<button className="button primary" onClick={() => { setError(''); setPhone(''); setParams({ new: '1' }); }}><Plus size={18} />{config.add}</button>}>{config.description}</PageHeading>
    {notice && <div className="notice" role="status">{notice}</div>}
    <section className="panel"><div className="list-toolbar"><form className="search-form" onSubmit={event => { event.preventDefault(); setSearch(draft.trim()); setPage(1); }}><Search size={18} /><input aria-label={config.search} placeholder={config.search} value={draft} maxLength={100} onChange={event => setDraft(event.target.value)} /><button type="submit" className="button secondary compact">{t('search')}</button></form><span className="list-count">{data?.total ?? '—'} {t('records')}</span></div>
      {loading ? <Loading /> : loadError ? <ResourceError error={loadError} refresh={refresh} /> : data.items.length ? <div className="table-scroll"><table><thead><tr>{config.columns.map(column => <th key={column.title} className={column.number ? 'number' : ''}>{column.title}</th>)}<th className="actions">{t('actions')}</th></tr></thead><tbody>{data.items.map(record => <tr key={record.id}>{config.columns.map(column => <td key={column.title} className={column.number ? 'number' : ''}>{column.render(record)}</td>)}<td className="actions"><button className="icon-button" aria-label={t('edit') + ' ' + config.label(record)} onClick={() => { setEdit(record); setPhone(record.phone || ''); setError(''); }}><Pencil size={17} /></button><button className="icon-button danger-text" aria-label={t('delete') + ' ' + config.label(record)} onClick={() => { setRemove(record); setError(''); }}><Trash2 size={17} /></button></td></tr>)}</tbody></table></div> : <EmptyState title={search ? t('nothingFound') : config.empty}>{search ? t('changeQuery') : config.emptyText}</EmptyState>}
      <Pagination data={data} page={page} onChange={setPage} /></section>
    {(creating || edit) && <Modal title={edit ? config.edit : config.add} busy={busy} onClose={close}><form onSubmit={save}><fieldset disabled={busy}>{config.fields.map(field => field.name === 'phone' ? <PhoneInput key="phone" value={phone} onChange={setPhone} disabled={busy} /> : <label key={field.name}>{field.label}<input name={field.name} type={field.type || 'text'} defaultValue={edit?.[field.name] ?? ''} required maxLength={field.maxLength} min={field.min} max={field.max} step={field.step} pattern={field.pattern} placeholder={field.placeholder} autoComplete="off" /></label>)}<ErrorMessage message={error} /><div className="form-actions"><button type="button" className="button secondary" onClick={close}>{t('cancel')}</button><button className="button primary" type="submit">{busy ? t('saving') : t('save')}</button></div></fieldset></form></Modal>}
    {remove && <Modal title={t('deleteRecord')} busy={busy} onClose={close}><p>{t('deleteConfirm', { name: config.label(remove) })}</p><ErrorMessage message={error} /><div className="form-actions"><button className="button secondary" disabled={busy} onClick={close}>{t('cancel')}</button><button className="button danger" disabled={busy} onClick={deleteRecord}>{busy ? t('deleting') : t('delete')}</button></div></Modal>}
  </>;
}
