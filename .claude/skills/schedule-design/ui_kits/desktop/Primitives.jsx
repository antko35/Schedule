/* global React */
const { useState } = React;

function NavPill({ route, onNavigate }) {
  const cls = route === "/" ? "is-schedule"
    : route === "/departments" ? "is-departments"
    : route === "/control" ? "is-control" : "is-schedule";
  const Item = ({ to, children }) => (
    <a className={"nav-link " + (route === to ? "active" : "")}
       onClick={(e)=>{e.preventDefault(); onNavigate(to);}} href="#">
      <p>{children}</p>
    </a>
  );
  return (
    <div className="header">
      <div className={"header-list " + cls}>
        <Item to="/">расписание</Item>
        <Item to="/departments">отделения</Item>
        <Item to="/control">панель управления</Item>
      </div>
    </div>
  );
}

function KitChrome({ route, onNavigate }) {
  const routes = [
    { to: "/",            label: "Расписание",       swatch: "var(--accent-schedule)" },
    { to: "/departments", label: "Отделения",        swatch: "var(--accent-departments)" },
    { to: "/control",     label: "Панель",           swatch: "var(--accent-control)" },
  ];
  const current = routes.find(r => r.to === route) || routes[0];
  return (
    <div className="kit-chrome" data-route={route}>
      <div className="brand">
        <div className="dot"></div>
        <span>UI Kit · Desktop</span>
      </div>
      <div className="crumbs">
        <span>Schedule</span>
        <span className="sep">/</span>
        <span className="now">{current.label.toLowerCase()}</span>
      </div>
      <div className="quick-jump">
        {routes.map(r => (
          <button key={r.to}
                  className={route === r.to ? "active" : ""}
                  onClick={()=>onNavigate(r.to)}>
            <span className="swatch" style={{background: r.swatch}}></span>
            {r.label}
          </button>
        ))}
      </div>
    </div>
  );
}

function FormHeader({ text, desc, breadcrumb }) {
  return (
    <>
      {breadcrumb && (
        <div className="form-breadcrumb">
          {breadcrumb.map((b, i) => (
            <React.Fragment key={i}>
              {i > 0 && <span className="sep">/</span>}
              <span className={i === breadcrumb.length - 1 ? "now" : ""}>{b}</span>
            </React.Fragment>
          ))}
        </div>
      )}
      <div className="form-header">
        <h1>{text}</h1>
        {desc && <div className="desc">{desc}</div>}
      </div>
    </>
  );
}

function FormInput({ label, id, value, onChange, placeholder, maxLength, type = "text" }) {
  return (
    <div className="form-line">
      <label htmlFor={id}>{label}</label>
      <input id={id} type={type} value={value}
             placeholder={placeholder} maxLength={maxLength}
             onChange={(e)=>onChange(e.target.value)}/>
    </div>
  );
}

function SelectInput({ label, id, value, onChange, options }) {
  return (
    <div className="form-line">
      <label htmlFor={id}>{label}</label>
      <select id={id} value={value} onChange={(e)=>onChange(e.target.value)}>
        <option value="" disabled></option>
        {options.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
      </select>
    </div>
  );
}

function GenderPicker({ value, onChange }) {
  return (
    <div className="form-line">
      <label>Пол</label>
      <div className="gender-group">
        {[["мужской","Мужской"],["женский","Женский"]].map(([k,l])=>(
          <span key={k} className={"pill " + (value===k ? "active" : "")}
                onClick={()=>onChange(k)}>{l}</span>
        ))}
      </div>
    </div>
  );
}

function SubmitButton({ text, disabled }) {
  return (
    <button className="btn-submit" disabled={disabled}>{disabled ? "Сохранение…" : text}</button>
  );
}

function Toast({ message }) {
  if (!message) return null;
  return (
    <div className="toast">
      <div className="check">✓</div>
      <p>{message}</p>
    </div>
  );
}

Object.assign(window, { NavPill, KitChrome, FormHeader, FormInput, SelectInput, GenderPicker, SubmitButton, Toast });
