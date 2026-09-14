import { ArrowUpRight } from 'lucide-react';
import { date, money } from '../lib/format';
import { StatusBadge } from './StatusBadge';
import { useLocale } from '../i18n/LocaleContext';

export function OrderTable({ orders, onOpen }) {
  const { t } = useLocale();
  return <div className="table-scroll"><table><thead><tr><th>{t('order')}</th><th>{t('customer')}</th><th>{t('status')}</th><th className="number">{t('amount')}</th><th><span className="sr-only">{t('actions')}</span></th></tr></thead>
    <tbody>{orders.map(order => <tr key={order.id}>
      <td><button className="text-button order-number" onClick={() => onOpen(order)}>№ {order.id.toString().padStart(5, '0')}</button><small>{date(order.createdAt)}</small></td>
      <td><strong>{order.customerName}</strong><small>{order.customerPhone}</small></td>
      <td><StatusBadge status={order.status} /></td><td className="number amount">{money(order.total)}</td>
      <td className="actions"><button className="icon-button" aria-label={t('openOrder', { id: order.id })} onClick={() => onOpen(order)}><ArrowUpRight size={18} /></button></td>
    </tr>)}</tbody></table></div>;
}
