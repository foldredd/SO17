import { CatalogPage } from '../components/CatalogPage';
import { useLocale } from '../i18n/LocaleContext';

export function CustomersPage() {
  const { t } = useLocale();
  return <CatalogPage config={{
    path: '/customers', title: t('customers'), eyebrow: 'SO17', add: t('addCustomer'), edit: t('editCustomer'), description: t('customerDescription'), search: t('customerSearch'), empty: t('customerEmpty'), emptyText: t('customerEmptyText'), phoneField: true,
    label: record => record.name + ' ' + record.surname,
    fields: [{ name: 'name', label: t('name'), maxLength: 80 }, { name: 'surname', label: t('surname'), maxLength: 80 }, { name: 'phone' }],
    columns: [{ title: t('customer'), render: record => <div className="person-cell"><span className="avatar">{record.name[0]}{record.surname[0]}</span><strong>{record.name} {record.surname}</strong></div> }, { title: t('phone'), render: record => <a href={'tel:' + record.phone}>{record.phone}</a> }, { title: t('number'), render: record => <span className="muted">#{record.id}</span> }]
  }} />;
}
