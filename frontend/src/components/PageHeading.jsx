export function PageHeading({ eyebrow, title, children, action }) {
  return <div className="page-heading"><div><span className="eyebrow dark">{eyebrow}</span><h1>{title}</h1><p>{children}</p></div>{action}</div>;
}
