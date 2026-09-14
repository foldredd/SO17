import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, ArrowUpRight, Boxes, ClipboardList, Plus, Users, Wallet } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { useResource } from '../hooks/useResource';
import { money } from '../lib/format';
import { EmptyState, Loading, ResourceError } from '../components/Feedback';
import { PageHeading } from '../components/PageHeading';
import { OrderTable } from '../components/OrderTable';
import { OrderDetails } from '../components/OrderDetails';
import { useLocale } from '../i18n/LocaleContext';

export function DashboardPage() {
  const { user } = useAuth();
  const { t } = useLocale();
  const { data, error, loading, refresh } = useResource('/dashboard');
  const [selected, setSelected] = useState(null);
  const cards = data ? [
    { title: t('completedSales'), value: money(data.revenue), note: t('completedSalesNote'), icon: Wallet, tone: 'purple' },
    { title: t('activeOrders'), value: data.activeOrders, note: t('activeOrdersNote'), icon: ClipboardList, tone: 'orange' },
    { title: t('customers'), value: data.customers, note: t('customersNote'), icon: Users, tone: 'blue' },
    { title: t('items'), value: data.items, note: t('itemsNote'), icon: Boxes, tone: 'green' },
  ] : [];
  return <><PageHeading eyebrow="SO17" title={t('dashboardTitle', { name: user.name })} action={<Link className="button primary" to="/orders?new=1"><Plus size={18} />{t('newOrder')}</Link>}>{t('dashboardText')}</PageHeading>
    <section className="overview-banner"><div><span className="eyebrow">SO17</span><h2>{t('salesManagement')}<br />{t('workspace')}</h2><p>{t('dashboardText')}</p><Link to="/orders">{t('orders')} <ArrowRight size={17} /></Link></div><div className="banner-art" aria-hidden="true"><div className="art-card back" /><div className="art-card front"><span className="art-check">✓</span><i /><i /><i /><span className="art-pill">SO17</span></div><span className="art-orbit" /></div></section>
    {loading ? <Loading /> : error ? <ResourceError error={error} refresh={refresh} /> : <>
      <section className="metrics" aria-label={t('dashboard')}>{cards.map(({ title, value, note, icon: Icon, tone }) => <article className="metric" key={title}><div className="metric-heading"><span>{title}</span><span className={'metric-icon ' + tone}><Icon size={19} /></span></div><strong>{value}</strong><small>{note}</small></article>)}</section>
      <div className="dashboard-grid"><section className="panel"><div className="panel-heading"><div><h2>{t('recentOrders')}</h2><p>{t('recentOrdersNote')}</p></div><Link className="text-link" to="/orders">{t('allOrdersLink')} <ArrowUpRight size={16} /></Link></div>{data.recentOrders.length ? <OrderTable orders={data.recentOrders} onOpen={setSelected} /> : <EmptyState title={t('firstOrder')}>{t('firstOrderText')}</EmptyState>}</section>
      <aside className="panel quick-actions"><div className="panel-heading"><h2>{t('quickActions')}</h2></div>{[{ to: '/customers?new=1', icon: Users, title: t('addCustomerAction'), text: t('addCustomerText') }, { to: '/items?new=1', icon: Boxes, title: t('addItemAction'), text: t('addItemText') }, { to: '/orders?new=1', icon: ClipboardList, title: t('createOrderAction'), text: t('createOrderText') }].map(({ to, icon: Icon, title, text }) => <Link key={to} to={to}><span className="quick-icon"><Icon size={20} /></span><div><strong>{title}</strong><small>{text}</small></div><ArrowUpRight size={17} /></Link>)}</aside></div>
    </>}{selected && <OrderDetails order={selected} onClose={() => setSelected(null)} onChanged={refresh} />}</>;
}
