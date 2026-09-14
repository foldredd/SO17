import { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { useLocale } from '../i18n/LocaleContext';
import { countryOptions } from '../i18n/phoneOperators';

const defaultCode = countryOptions[0].code;

function splitPhone(value = '') {
  const digits = value.replace(/\D/g, '');
  const selected = countryOptions.find(option => digits.startsWith(option.code.slice(1)));
  return { code: selected?.code || defaultCode, local: selected ? digits.slice(selected.code.length - 1) : '' };
}

export function PhoneInput({ value = '', onChange, disabled = false, required = true }) {
  const { language, t } = useLocale();
  const phone = splitPhone(value);
  const [open, setOpen] = useState(false);
  const country = countryOptions.find(option => option.code === phone.code) || countryOptions[0];
  const update = next => onChange(next.code + next.local);
  return <label className="phone-input">{t('phone')}<div className="phone-combined"><div className="country-picker"><button type="button" className="country-trigger" disabled={disabled} aria-expanded={open} onClick={() => setOpen(!open)}><img src={country.flag} alt="" /><strong>{country.code}</strong><ChevronDown size={16} /></button>{open && <div className="country-options">{countryOptions.map(option => <button type="button" key={option.code} onClick={() => { update({ ...phone, code: option.code }); setOpen(false); }}><img src={option.flag} alt="" /><span>{option[language]}</span><strong>{option.code}</strong></button>)}</div>}</div><input aria-label={t('phone')} type="tel" inputMode="numeric" autoComplete="tel-national" value={phone.local} disabled={disabled} required={required} pattern="[0-9]{7,10}" maxLength={10} placeholder="1234567" onChange={event => update({ ...phone, local: event.target.value.replace(/\D/g, '').slice(0, 10) })} /></div></label>;
}
