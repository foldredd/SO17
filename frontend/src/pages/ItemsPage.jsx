import { CatalogPage } from '../components/CatalogPage';
import { money } from '../lib/format';
import { useLocale } from '../i18n/LocaleContext';

export function ItemsPage() {
  const { t } = useLocale();
  return <CatalogPage config={{
    path: '/items', title: t('items'), eyebrow: 'SO17', add: t('addItem'), edit: t('editItem'), description: t('itemDescription'), search: t('itemSearch'), empty: t('itemEmpty'), emptyText: t('itemEmptyText'), label: record => record.title,
    fields: [{ name: 'title', label: t('product'), maxLength: 160 }, { name: 'sku', label: t('sku'), maxLength: 50, pattern: '[A-Za-z0-9][A-Za-z0-9._\\-]*', placeholder: 'SKU-001' }, { name: 'quantity', label: t('stock'), type: 'number', min: 0, max: 1000000, step: 1 }, { name: 'price', label: t('price'), type: 'number', min: 0.01, max: 99999999.99, step: 0.01 }],
    columns: [{ title: t('product'), render: record => <strong>{record.title}</strong> }, { title: t('sku'), render: record => <span className="sku">{record.sku}</span> }, { title: t('stock'), number: true, render: record => <span className={record.quantity === 0 ? 'stock-empty' : ''}>{record.quantity}</span> }, { title: t('price'), number: true, render: record => <strong>{money(record.price)}</strong> }]
  }} />;
}
