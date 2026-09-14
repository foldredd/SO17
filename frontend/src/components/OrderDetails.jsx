import { useState } from 'react';
import { api } from '../api/client';
import { date, money, nextStatuses, statusLabels } from '../lib/format';
import { ErrorMessage } from './Feedback';
import { Modal } from './Modal';
import { StatusBadge } from './StatusBadge';
import { useLocale } from '../i18n/LocaleContext';

export function OrderDetails({ order, onClose, onChanged }) {
  const { t, language } = useLocale();
  const [current, setCurrent] = useState(order);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [confirmCancel, setConfirmCancel] = useState(false);
  async function change(status) {
    setBusy(true); setError('');
    try {
      setCurrent(await api('/orders/' + current.id + '/status', { method: 'PATCH', body: { status, version: current.version } }));
      setConfirmCancel(false); onChanged();
    } catch (e) { setError(e.message); } finally { setBusy(false); }
  }
  return <Modal title={t('orderDetails', { id: current.id.toString().padStart(5, '0') })} onClose={onClose} busy={busy} wide>
    <div className="order-summary"><div><strong>{current.customerName}</strong><p>{current.customerPhone}</p><small>{date(current.createdAt)}</small></div><StatusBadge status={current.status} /></div>
    <div className="table-scroll"><table><thead><tr><th>{t('product')}</th><th className="number">{t('quantity')}</th><th className="number">{t('price')}</th><th className="number">{t('amount')}</th></tr></thead><tbody>
      {current.lines.map(line => <tr key={line.itemId}><td><strong>{line.title}</strong><small>{line.sku}</small></td><td className="number">{line.quantity}</td><td className="number">{money(line.unitPrice)}</td><td className="number">{money(line.total)}</td></tr>)}
    </tbody></table></div><div className="order-total"><span>{t('totalAmount')}</span><strong>{money(current.total)}</strong></div>
    <ErrorMessage message={error} />
    {confirmCancel ? <div className="confirm-panel"><p>{t('confirmCancellation')}</p><div className="form-actions"><button className="button secondary" disabled={busy} onClick={() => setConfirmCancel(false)}>{t('back')}</button><button className="button danger" disabled={busy} onClick={() => change('Cancelled')}>{t('confirmCancel')}</button></div></div> :
      <div className="form-actions">{nextStatuses(current.status).map(status => <button key={status} disabled={busy} className={'button ' + (status === 'Cancelled' ? 'secondary danger-text' : 'primary')} onClick={() => status === 'Cancelled' ? setConfirmCancel(true) : change(status)}>{busy ? t('saving') : status === 'Cancelled' ? t('cancelOrder') : status === 'Processing' ? t('takeToWork') : t('complete')}</button>)}</div>}
    <p className="field-hint">{t('status')}: {statusLabels(language)[current.status]}.</p>
  </Modal>;
}
