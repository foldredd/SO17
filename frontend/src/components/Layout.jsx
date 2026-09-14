import { useState } from 'react';
import { NavLink, Outlet, Link } from 'react-router-dom';
import { Boxes, LayoutDashboard, Users, ClipboardList, UserRound, LogOut, ArrowUpRight, Menu, X } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { ErrorMessage } from './Feedback';
import { LanguageSwitcher } from './LanguageSwitcher';
import { useLocale } from '../i18n/LocaleContext';

export function Layout() {
  const { user, logout } = useAuth();
  const { t } = useLocale();
  const [menu, setMenu] = useState(false);
  const [error, setError] = useState('');
  const links = [
    { to: '/', title: t('dashboard'), icon: LayoutDashboard }, { to: '/orders', title: t('orders'), icon: ClipboardList },
    { to: '/customers', title: t('customers'), icon: Users }, { to: '/items', title: t('items'), icon: Boxes },
  ];
  async function signOut() { try { await logout(); } catch (e) { setError(e.message); } }
  return <div className="app-shell">
    <aside className={'sidebar ' + (menu ? 'open' : '')}>
      <Link to="/" className="brand"><span className="brand-mark">s.</span>SO17<span className="brand-caption">workspace</span></Link>
      <div className="workspace"><span className="workspace-logo"><Boxes size={20} /></span><div><strong>{t('salesManagement')}</strong><small>{t('workspace')}</small></div></div>
      <p className="nav-caption">SO17</p>
      <nav aria-label={t('workspace')}>{links.map(({ to, title, icon: Icon }) => <NavLink key={to} to={to} end={to === '/'} onClick={() => setMenu(false)}><Icon size={19} />{title}</NavLink>)}</nav>
      <div className="sidebar-bottom"><NavLink className="profile-link" to="/profile" onClick={() => setMenu(false)}><span className="avatar">{user.name[0]}{user.surname[0]}</span><div><strong>{user.name} {user.surname}</strong><small>{t('profile')}</small></div><ArrowUpRight size={17} /></NavLink>
      <button className="logout" onClick={signOut}><LogOut size={16} />{t('signOut')}</button></div>
    </aside>
    {menu && <button className="scrim" aria-label={t('closeMenu')} onClick={() => setMenu(false)} />}
    <main className="main">
      <header className="topbar"><button className="icon-button mobile-menu" aria-label={t('openMenu')} onClick={() => setMenu(!menu)}>{menu ? <X /> : <Menu />}</button>
        <span className="topbar-spacer" /><LanguageSwitcher />
        <Link to="/profile" className="top-profile"><UserRound size={16} /><span>{user.login}</span><span className="avatar small">{user.name[0]}</span></Link>
      </header>
      <div className="page"><ErrorMessage message={error} /><Outlet /></div>
      <footer>SO17 <span>{t('salesManagement')}</span></footer>
    </main>
  </div>;
}
