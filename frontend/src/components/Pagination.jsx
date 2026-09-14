import { ChevronLeft, ChevronRight } from 'lucide-react';
import { useLocale } from '../i18n/LocaleContext';
export function Pagination({ data, page, onChange }) {
  const { t } = useLocale();
  if (!data) return null;
  const pages = Math.max(1, Math.ceil(data.total / data.pageSize));
  return <div className="pagination"><span>{t('total', { total: data.total, page, pages })}</span><div>
    <button className="icon-button" aria-label={t('previousPage')} disabled={page <= 1} onClick={() => onChange(page - 1)}><ChevronLeft size={18} /></button>
    <button className="icon-button" aria-label={t('nextPage')} disabled={page >= pages} onClick={() => onChange(page + 1)}><ChevronRight size={18} /></button>
  </div></div>;
}
