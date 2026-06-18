/* global React, FormHeader, FormInput, GenderPicker, SubmitButton, Toast */
const { useState } = React;

const FORM_META = {
  // Работники
  addUser:              { group: "работники", label: "Добавить работника", style: "add",    desc: "Создать карточку нового работника в системе." },
  deleteUser:           { group: "работники", label: "Удалить работника",  style: "delete", desc: "Удалить работника из системы безвозвратно." },
  redactUserInfo:       { group: "работники", label: "Изменить информацию", style: "redact", desc: "Обновить персональные данные работника." },
  // Отделения
  createDepartment:     { group: "отделения", label: "Добавить отделение", style: "add",    desc: "Создать новое отделение в клинике." },
  addUserToDepartment:  { group: "отделения", label: "Добавить работника в отделение", style: "add", desc: "Прикрепить работника к отделению с указанием роли." },
  redactUserInfoFromDepartment: { group: "отделения", label: "Изменить информацию работника", style: "redact", desc: "Обновить роль, статус или контакты внутри отделения." },
  deleteUserFromDepartment: { group: "отделения", label: "Удалить работника из отделения", style: "delete", desc: "Снять работника с отделения." },
  deleteDepartment:     { group: "отделения", label: "Удалить отделение", style: "delete",  desc: "Удалить отделение и снять всех его работников." },
  // Расписание
  createUserSchedule:   { group: "расписание", label: "Составить расписание", style: "add",   desc: "Автогенерация расписания по правилам для всего отделения." },
  transferDay:          { group: "расписание", label: "Добавить выходной",     style: "add",   desc: "Добавить официальный выходной или перенос рабочей субботы." },
  deleteHoliday:        { group: "расписание", label: "Удалить выходной",      style: "delete",desc: "Удалить ранее добавленный официальный выходной." },
};

function Sidebar({ data, activeForm, onPick }) {
  const Group = ({ title, list }) => (
    <div className="group">
      <div className="group-label">{title}</div>
      <ul>
        {list.map(f => (
          <li key={f.id} className={f.style + (activeForm === f.route ? " is-active" : "")}
              onClick={()=>onPick(f.route)}>
            <span className="dot"></span>
            <a onClick={(e)=>e.preventDefault()} href="#">{f.text}</a>
          </li>
        ))}
      </ul>
    </div>
  );
  return (
    <div className="sidebar">
      <Group title="Работники" list={data.doctorFuncList}/>
      <div className="divider"></div>
      <Group title="Отделения" list={data.departmentFuncList}/>
      <div className="divider"></div>
      <Group title="Расписание" list={data.scheduleFuncList}/>
    </div>
  );
}

function AddUserForm({ onDone }) {
  const [form, setForm] = useState({ lastName:"", firstName:"", patronymic:"", dateOfBirth:"", gender:"" });
  const set = (k) => (v) => setForm(f => ({...f, [k]:v}));
  const submit = (e) => {
    e.preventDefault();
    if (!form.lastName || !form.firstName) return;
    onDone(`${form.lastName} ${form.firstName} ${form.patronymic} успешно добавлен(а)`);
    setForm({ lastName:"", firstName:"", patronymic:"", dateOfBirth:"", gender:"" });
  };
  return (
    <form className="form" onSubmit={submit}>
      <FormInput id="ln" label="Фамилия"      value={form.lastName}    onChange={set("lastName")}/>
      <FormInput id="fn" label="Имя"          value={form.firstName}   onChange={set("firstName")}/>
      <FormInput id="pn" label="Отчество"     value={form.patronymic}  onChange={set("patronymic")}/>
      <FormInput id="db" label="Дата рожд."   placeholder="ДД.ММ.ГГГГ" maxLength="10"
                 value={form.dateOfBirth}     onChange={set("dateOfBirth")}/>
      <GenderPicker value={form.gender} onChange={set("gender")}/>
      <div className="form-actions">
        <SubmitButton text="Добавить работника" disabled={false}/>
        <button type="button" className="btn-secondary" onClick={()=>setForm({ lastName:"", firstName:"", patronymic:"", dateOfBirth:"", gender:"" })}>Очистить</button>
      </div>
    </form>
  );
}

function CreateDepartmentForm({ onDone }) {
  const [name, setName] = useState("");
  const submit = (e) => {
    e.preventDefault();
    if (!name) return;
    onDone(`Отделение «${name}» успешно добавлено`);
    setName("");
  };
  return (
    <form className="form" onSubmit={submit}>
      <FormInput id="dept" label="Название" value={name} onChange={setName} placeholder="Например, ортодонтия"/>
      <div className="form-actions">
        <SubmitButton text="Создать отделение" disabled={false}/>
      </div>
    </form>
  );
}

function Placeholder({ route }) {
  return (
    <div className="form-placeholder">
      <div className="tag">В разработке</div>
      <p>Форма «<b>{(FORM_META[route]||{}).label || route}</b>» откроется здесь.<br/>
         В прототипе показаны только две основные формы — добавление работника и создание отделения.</p>
    </div>
  );
}

function WelcomeDashboard({ data, onPick }) {
  const allFuncs = [
    ...data.doctorFuncList,
    ...data.departmentFuncList,
    ...data.scheduleFuncList,
  ];
  return (
    <div>
      <h1 className="welcome-heading">Панель управления</h1>
      <p className="welcome-sub">Управление работниками, отделениями и расписанием. Выберите действие.</p>
      <div className="welcome-grid">
        {allFuncs.map(f => {
          const meta = FORM_META[f.route] || {};
          const symbol = f.style === "add" ? "+" : f.style === "delete" ? "−" : "✎";
          return (
            <div key={f.id} className={"welcome-card " + f.style} onClick={()=>onPick(f.route)}>
              <div className="icon">{symbol}</div>
              <div className="ttl">{f.text}</div>
              <div className="sub">{meta.group}</div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function FormShell({ route, onDone }) {
  const meta = FORM_META[route] || { group: "панель", label: route, desc: "" };
  return (
    <>
      <FormHeader
        text={meta.label}
        desc={meta.desc}
        breadcrumb={["Панель управления", meta.group, meta.label]}
      />
      {route === "addUser" && <AddUserForm onDone={onDone}/>}
      {route === "createDepartment" && <CreateDepartmentForm onDone={onDone}/>}
      {route !== "addUser" && route !== "createDepartment" && <Placeholder route={route}/>}
    </>
  );
}

function ControlPanelPage({ data }) {
  const [active, setActive] = useState(null);
  const [toast, setToast] = useState("");
  const handleDone = (msg) => {
    setToast(msg);
    setTimeout(()=>setToast(""), 3500);
  };
  return (
    <div className="control-panel">
      <Sidebar data={data} activeForm={active} onPick={setActive}/>
      <div className="control-options">
        {active ? <FormShell route={active} onDone={handleDone}/> : <WelcomeDashboard data={data} onPick={setActive}/>}
      </div>
      <Toast message={toast}/>
    </div>
  );
}

Object.assign(window, { Sidebar, AddUserForm, ControlPanelPage });
