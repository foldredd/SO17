import { AlertCircle, Inbox, LoaderCircle } from 'lucide-react';
import { useLocale } from '../i18n/LocaleContext';

export function ErrorMessage({ message }) { return message ? <div className="alert" role="alert"><AlertCircle size={18} /><span>{message}</span></div> : null; }
export function Loading() { const { t } = useLocale(); return <div className="loading" role="status"><LoaderCircle className="spin" size={24} />{t('loading')}</div>; }
export function EmptyState({ title, children }) {
  const { t } = useLocale();
  return <div className="empty"><span className="empty-icon"><Inbox size={28} /></span><h3>{title || t('noRecords')}</h3><p>{children}</p></div>;
}
export function ResourceError({ error, refresh }) { const { t } = useLocale(); return <div><ErrorMessage message={error} /><button className="button secondary" onClick={refresh}>{t('retry')}</button></div>; }
