/* global React, initials */
const { useState, useMemo } = React;

function ProfileRow({ doc }) {
  const [open, setOpen] = useState(false);
  return (
    <div className={"profile-row " + (open ? "expanded" : "")}
         onClick={(e)=>{e.stopPropagation(); setOpen(o=>!o);}}>
      <div className="avatar-circle">{initials(doc)}</div>
      <div className="info">
        <p className="name">{doc.lastName} {doc.firstName} {doc.patronymic}</p>
        <p className="date">{doc.dateOfBirth} · {doc.gender}</p>
        {open && (
          <dl className="additional">
            <dt>Почта</dt><dd>{doc.email}</dd>
            <dt>Телефон</dt><dd>{doc.phoneNumber}</dd>
            <dt>Отчество</dt><dd>{doc.patronymic}</dd>
            <dt>Дата рожд.</dt><dd>{doc.dateOfBirth}</dd>
          </dl>
        )}
      </div>
      <img className={"chevron " + (open ? "rotated" : "")} src="../../assets/icons/chevron.svg"/>
    </div>
  );
}

function DepartmentCard({ dept, doctors, defaultOpen = false }) {
  const [open, setOpen] = useState(defaultOpen);
  const sorted = [...doctors].sort((a,b)=> (a.lastName||"").localeCompare(b.lastName||""));
  const groups = {};
  sorted.forEach(doc => {
    const L = (doc.lastName||"").charAt(0).toUpperCase();
    (groups[L] = groups[L] || []).push(doc);
  });
  return (
    <div className="department-card">
      <div className="header" onClick={()=>setOpen(o=>!o)}>
        <div className="meta">
          <h1>{dept.departmentName}</h1>
        </div>
        <span className="count-badge">{doctors.length} {doctors.length === 1 ? "работник" : "работников"}</span>
        <img className={"chevron " + (open ? "rotated" : "")} src="../../assets/icons/chevron.svg"/>
      </div>
      {open && doctors.length === 0 && <div className="empty">Нет добавленных работников</div>}
      {open && Object.entries(groups).map(([letter, list]) => (
        <div className="group" key={letter}>
          <div className="letter-divider">{letter}</div>
          {list.map(doc => <ProfileRow key={doc.id} doc={doc}/>)}
        </div>
      ))}
    </div>
  );
}

function DepartmentsPage({ data }) {
  const [q, setQ] = useState("");
  const term = q.trim().toLowerCase();

  const filteredDepts = useMemo(() => {
    if (!term) return data.departments.map(d => ({ d, docs: data.doctorsByDept[d.id] || [] }));
    return data.departments.map(d => {
      const docs = (data.doctorsByDept[d.id] || []).filter(doc =>
        `${doc.lastName} ${doc.firstName} ${doc.patronymic||""}`.toLowerCase().includes(term) ||
        (d.departmentName||"").toLowerCase().includes(term)
      );
      return { d, docs };
    }).filter(g => g.docs.length > 0 || (g.d.departmentName||"").toLowerCase().includes(term));
  }, [term, data]);

  const totalDoctors = Object.values(data.doctorsByDept).reduce((a, l) => a + l.length, 0);

  return (
    <div className="departments-container">
      <div className="departments-toolbar">
        <div className="summary"><b>{data.departments.length}</b> отделений · <b>{totalDoctors}</b> работников</div>
        <div className="search">
          <img src="../../assets/icons/search.svg" alt=""/>
          <input
            type="text"
            placeholder="Поиск по работнику или отделению"
            value={q}
            onChange={(e)=>setQ(e.target.value)}
          />
        </div>
      </div>
      {filteredDepts.length === 0 && (
        <div style={{padding:"60px 24px", color:"var(--fg-quiet)", textAlign:"center"}}>
          <img src="../../assets/icons/no-result.svg" style={{width:90, opacity:.6, filter:"invert(1)"}}/>
          <p style={{marginTop:18, fontSize:14}}>Ничего не найдено</p>
        </div>
      )}
      {filteredDepts.map(({d, docs}) => (
        <DepartmentCard key={d.id} dept={d} doctors={docs} defaultOpen={!!term}/>
      ))}
    </div>
  );
}

Object.assign(window, { ProfileRow, DepartmentCard, DepartmentsPage });
