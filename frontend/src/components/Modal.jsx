import { useEffect, useRef } from 'react';
import { X } from 'lucide-react';
import { useLocale } from '../i18n/LocaleContext';

export function Modal({ title, children, onClose, busy = false, wide = false }) {
  const { t } = useLocale();
  const dialog = useRef(null);
  useEffect(() => { const element = dialog.current; element.showModal(); return () => element.close(); }, []);
  return <dialog ref={dialog} className={wide ? 'modal wide' : 'modal'} aria-labelledby="modal-title"
    onCancel={event => { event.preventDefault(); if (!busy) onClose(); }}>
    <div className="modal-heading"><h2 id="modal-title">{title}</h2><button type="button" className="icon-button" aria-label={t('close')} disabled={busy} onClick={onClose}><X size={21} /></button></div>
    {children}
  </dialog>;
}
