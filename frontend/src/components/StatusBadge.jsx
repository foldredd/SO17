import { statusLabels } from '../lib/format';
import { useLocale } from '../i18n/LocaleContext';
export function StatusBadge({ status }) { const { language } = useLocale(); return <span className={'badge ' + status.toLowerCase()}><i />{statusLabels(language)[status]}</span>; }
