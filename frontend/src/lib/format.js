const language = () => localStorage.getItem('so17.language') === 'en' ? 'en-US' : 'uk-UA';
export const money = value => new Intl.NumberFormat(language(), { style: 'currency', currency: 'USD', currencyDisplay: 'narrowSymbol', maximumFractionDigits: 2 }).format(value);
export const date = value => new Intl.DateTimeFormat(language(), { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
export const statusLabels = locale => locale === 'en'
  ? { Pending: 'New', Processing: 'In progress', Completed: 'Completed', Cancelled: 'Cancelled' }
  : { Pending: 'Нове', Processing: 'У роботі', Completed: 'Завершено', Cancelled: 'Скасовано' };
export const nextStatuses = status => ({ Pending: ['Processing', 'Cancelled'], Processing: ['Completed', 'Cancelled'], Completed: [], Cancelled: [] })[status] || [];
export const orderTotal = lines => lines.reduce((sum, line) => sum + Math.round(Number(line.item.price) * 100) * Number(line.quantity), 0) / 100;
