import { useState } from 'react';
import { Trash2, UserRound, X } from 'lucide-react';
import { api } from '../api/client';
import { money, orderTotal } from '../lib/format';
import { ErrorMessage } from './Feedback';
import { Modal } from './Modal';
import { ResourcePicker } from './ResourcePicker';
import { useLocale } from '../i18n/LocaleContext';

export function OrderComposer({ onClose, onCreated }) {
  const { t } = useLocale();
  const [customer, setCustomer] = useState(null);
  const [lines, setLines] = useState([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  function add(item) {
    if (!item.quantity) { setError(t('noStock')); return; }
    setError('');
    setLines(previous => {
      const existing = previous.find(line => line.item.id === item.id);
      return existing ? previous.map(line => line.item.id === item.id ? { ...line, quantity: Math.min(Number(line.quantity) + 1, item.quantity) } : line) : [...previous, { item, quantity: 1 }];
    });
  }
  async function submit(event) {
    event.preventDefault(); setError('');
    if (!customer || !lines.length) { setError(t('chooseCustomerAndItems')); return; }
    if (lines.some(line => !Number.isInteger(Number(line.quantity)) || Number(line.quantity) < 1 || Number(line.quantity) > line.item.quantity)) { setError(t('checkQuantities')); return; }
    setBusy(true);
    try {
      const order = await api('/orders', { method: 'POST', body: { customerId: customer.id, lines: lines.map(line => ({ itemId: line.item.id, quantity: Number(line.quantity) })) } });
      onCreated(order);
    } catch (e) { setError(e.message); } finally { setBusy(false); }
  }
  return <Modal title={t('newOrder')} wide onClose={onClose} busy={busy}><form onSubmit={submit}><fieldset disabled={busy}>
    <h3 className="form-section-title"><span>{t('orderCustomer')}</span></h3>
    {customer ? <div className="selected-customer"><UserRound size={22} /><div><strong>{customer.name} {customer.surname}</strong><small>{customer.phone}</small></div><button type="button" className="icon-button" aria-label={t('changeCustomer')} onClick={() => setCustomer(null)}><X size={18} /></button></div> : <ResourcePicker path="/customers" label={t('findCustomer')} disabled={busy} onSelect={setCustomer} render={record => <><strong>{record.name} {record.surname}</strong><small>{record.phone}</small></>} />}
    <h3 className="form-section-title"><span>{t('orderItems')}</span></h3>
    <ResourcePicker path="/items" label={t('findItem')} disabled={busy} onSelect={add} render={record => <><div><strong>{record.title}</strong><small>{record.sku} · {t('stock')}: {record.quantity}</small></div><strong>{money(record.price)}</strong></>} />
    {lines.length > 0 && <div className="cart-lines">{lines.map(line => <div className="cart-line" key={line.item.id}><div><strong>{line.item.title}</strong><small>{money(line.item.price)}</small></div><input type="number" aria-label={t('quantity') + ' ' + line.item.title} min={1} max={line.item.quantity} step={1} required value={line.quantity} onChange={event => setLines(previous => previous.map(entry => entry.item.id === line.item.id ? { ...entry, quantity: event.target.value } : entry))} /><strong>{money(line.item.price * Number(line.quantity))}</strong><button type="button" className="icon-button danger-text" aria-label={t('delete') + ' ' + line.item.title} onClick={() => setLines(previous => previous.filter(entry => entry.item.id !== line.item.id))}><Trash2 size={17} /></button></div>)}</div>}
    <div className="order-total"><span>{t('preliminaryTotal')}</span><strong>{money(orderTotal(lines))}</strong></div><p className="field-hint">{t('serverChecks')}</p><ErrorMessage message={error} />
    <div className="form-actions"><button type="button" className="button secondary" onClick={onClose}>{t('cancel')}</button><button type="submit" className="button primary" disabled={!customer || !lines.length || busy}>{busy ? t('creating') : t('createOrder')}</button></div>
  </fieldset></form></Modal>;
}
