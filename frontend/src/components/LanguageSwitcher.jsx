import { useLocale } from '../i18n/LocaleContext';

export function LanguageSwitcher() {
  const { language, setLanguage, t } = useLocale();
  const flag = language === 'en' ? '/flags/gb.svg' : '/flags/ua.svg';
  return <label className="language-switch"><img src={flag} alt="" /><select value={language} onChange={event => setLanguage(event.target.value)} aria-label={t('language')}><option value="uk">Українська</option><option value="en">English</option></select></label>;
}
