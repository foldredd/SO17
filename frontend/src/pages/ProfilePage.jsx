import { useState } from 'react';
import { LockKeyhole, UserRound } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { api } from '../api/client';
import { ErrorMessage } from '../components/Feedback';
import { PageHeading } from '../components/PageHeading';
import { PhoneInput } from '../components/PhoneInput';
import { useLocale } from '../i18n/LocaleContext';

export function ProfilePage() {
  const { user, setUser } = useAuth();
  const { t } = useLocale();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [phone, setPhone] = useState(user.phone);
  async function save(event, password = false) {
    event.preventDefault(); setBusy(true); setError(''); setNotice('');
    const form = event.currentTarget;
    const body = Object.fromEntries(new FormData(form));
    if (!password) body.phone = phone;
    if (password && body.newPassword !== body.confirmPassword) { setError(t('passwordsMismatch')); setBusy(false); return; }
    delete body.confirmPassword;
    try {
      const result = await api(password ? '/auth/password' : '/auth/profile', { method: 'PUT', body });
      if (password) form.reset(); else setUser(result);
      setNotice(password ? t('passwordChanged') : t('profileUpdated'));
    } catch (e) { setError(e.message); } finally { setBusy(false); }
  }
  return <><PageHeading eyebrow="SO17" title={t('profileTitle')}>{t('profileDescription')}</PageHeading><ErrorMessage message={error} />{notice && <div className="notice" role="status">{notice}</div>}
    <div className="profile-grid"><section className="panel profile-card"><div className="panel-heading"><h2><UserRound size={20} />{t('contactDetails')}</h2></div><form onSubmit={save}><fieldset disabled={busy}><div className="identity-summary"><span className="avatar large">{user.name[0]}{user.surname[0]}</span><div><strong>{user.login}</strong><small>{user.email}</small></div></div><label>{t('name')}<input name="name" defaultValue={user.name} required maxLength={80} autoComplete="given-name" /></label><label>{t('surname')}<input name="surname" defaultValue={user.surname} required maxLength={80} autoComplete="family-name" /></label><PhoneInput value={phone} onChange={setPhone} disabled={busy} /><button className="button primary" type="submit">{busy ? t('saving') : t('save')}</button></fieldset></form></section>
    <section className="panel profile-card"><div className="panel-heading"><h2><LockKeyhole size={20} />{t('changePassword')}</h2></div><form onSubmit={event => save(event, true)}><fieldset disabled={busy}><p className="field-hint">{t('passwordHint')}</p><label>{t('currentPassword')}<input name="currentPassword" type="password" autoComplete="current-password" required maxLength={128} /></label><label>{t('newPassword')}<input name="newPassword" type="password" autoComplete="new-password" required minLength={12} maxLength={128} /></label><label>{t('confirmPassword')}<input name="confirmPassword" type="password" autoComplete="new-password" required minLength={12} maxLength={128} /></label><button className="button secondary" type="submit">{t('changePassword')}</button></fieldset></form></section></div>
  </>;
}
