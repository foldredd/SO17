import { useState } from 'react';
import { ArrowRight, Check, Layers3 } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { ErrorMessage } from '../components/Feedback';
import { LanguageSwitcher } from '../components/LanguageSwitcher';
import { PhoneInput } from '../components/PhoneInput';
import { useLocale } from '../i18n/LocaleContext';

export function AuthPage() {
  const { authenticate } = useAuth();
  const { t } = useLocale();
  const [mode, setMode] = useState('login');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [phone, setPhone] = useState('');
  const register = mode === 'register';
  async function submit(event) {
    event.preventDefault(); setBusy(true); setError('');
    try { const body = Object.fromEntries(new FormData(event.currentTarget)); if (register) body.phone = phone; await authenticate(mode, body); }
    catch (e) { setError(e.message); } finally { setBusy(false); }
  }
  return <main className="auth-shell"><section className="auth-story">
    <a className="brand" href="/"><span className="brand-mark">s.</span>SO17</a>
    <div className="auth-copy"><span className="eyebrow"><span className="live-dot" />SO17</span>
      <h1>{t('salesManagement')}<br /><em>{t('workspace')}</em></h1>
      <p>{t('customers')} · {t('items')} · {t('orders')}</p>
      <div className="auth-feature"><Check size={17} />{t('customers')}</div>
      <div className="auth-feature"><Check size={17} />{t('items')}</div>
      <div className="auth-feature"><Check size={17} />{t('orders')}</div>
    </div><div className="auth-art" aria-hidden="true"><Layers3 size={100} strokeWidth={0.7} /><span>SIMPLY ORGANIZED.</span></div>
    <div className="auth-footer">SO17 / SALES OPERATIONS</div>
  </section><section className="auth-form-area"><div className="auth-form">
    <LanguageSwitcher /><span className="eyebrow dark">SO17</span><h2>{register ? t('joinTeam') : t('welcomeBack')}</h2>
    <p>{register ? t('registerDescription') : t('loginDescription')}</p>
    <div className="auth-tabs"><button className={!register ? 'selected' : ''} onClick={() => { setMode('login'); setError(''); }}>{t('signIn')}</button><button className={register ? 'selected' : ''} onClick={() => { setMode('register'); setError(''); setPhone(''); }}>{t('registration')}</button></div>
    <form onSubmit={submit} key={mode}><fieldset disabled={busy}>
      {register && <div className="form-grid"><label>{t('name')}<input name="name" autoComplete="given-name" required maxLength={80} /></label><label>{t('surname')}<input name="surname" autoComplete="family-name" required maxLength={80} /></label></div>}
      <label>{t('login')}<input name="login" autoComplete="username" required minLength={register ? 3 : 1} maxLength={80} pattern={register ? '[a-zA-Z0-9._\\-]+' : undefined} /></label>
      {register && <><label>Email<input name="email" type="email" autoComplete="email" required maxLength={254} placeholder="name@company.com" /></label><PhoneInput value={phone} onChange={setPhone} disabled={busy} /></>}
      <label>{t('password')}<input name="password" type="password" autoComplete={register ? 'new-password' : 'current-password'} required minLength={register ? 12 : 1} maxLength={128} /></label>
      {register && <><p className="field-hint">{t('passwordHint')}</p><label>{t('invitationCode')}<input name="invitationCode" type="password" autoComplete="off" required maxLength={256} /></label></>}
      <ErrorMessage message={error} /><button className="button primary auth-submit" type="submit">{busy ? t('wait') : register ? t('createAccount') : t('signIn')}<ArrowRight size={18} /></button>
    </fieldset></form><p className="auth-help">{t('accessOnly')}</p>
  </div></section></main>;
}
