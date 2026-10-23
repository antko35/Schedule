/* global React */
const { useState, useMemo } = React;

function initials(doc) {
  const a = (doc.lastName || "")[0] || "";
  const b = (doc.firstName || "")[0] || "";
  return (a + b).toUpperCase();
}

/* DepartmentList — picker on the schedule page */
function DepartmentPicker({ departments, doctorsByDept, selectedDoctorId, onSelectDoctor, searchTerm = "" }) {
  const [openId, setOpenId] = useState(null);
  const q = searchTerm.trim().toLowerCase();

  // Open department automatically when search matches a doctor inside it
  const autoOpen = useMemo(() => {
    if (!q) return null;
    for (const d of departments) {
      const docs = doctorsByDept[d.id] || [];
      if (docs.some(doc =>
        `${doc.lastName} ${doc.firstName} ${doc.patronymic||""}`.toLowerCase().includes(q)
      )) return d.id;
    }
    return null;
  }, [q, departments, doctorsByDept]);

  const currentOpen = autoOpen || openId;

  return (
    <div>
      {departments.map(d => {
        const open = currentOpen === d.id;
        let docs = doctorsByDept[d.id] || [];
        if (q) docs = docs.filter(doc =>
          `${doc.lastName} ${doc.firstName} ${doc.patronymic||""}`.toLowerCase().includes(q)
        );
        const sorted = [...docs].sort((a,b)=> (a.lastName||"").localeCompare(b.lastName||""));
        const groups = {};
        sorted.forEach(doc => {
          const L = (doc.lastName||"").charAt(0).toUpperCase();
          (groups[L] = groups[L] || []).push(doc);
        });
        const total = (doctorsByDept[d.id] || []).length;
        return (
          <div className="choose-department" key={d.id}>
            <div className={"department " + (open ? "active" : "")} onClick={()=>setOpenId(open ? null : d.id)}>
              <p>{d.departmentName}</p>
              <span className="count">{total}</span>
              <img className={"chevron " + (open ? "rotated" : "")} src="../../assets/icons/chevron.svg"/>
            </div>
            {open && (
              <div className="doctors-list">
                {sorted.length === 0 && (
                  <div className="empty-doctors">Ничего не найдено</div>
                )}
                {Object.entries(groups).map(([letter, list]) => (
                  <div key={letter}>
                    <div className="letter-divider">{letter}</div>
                    {list.map(doc => (
                      <div key={doc.id}
                           className={"doctor " + (doc.id === selectedDoctorId ? "selected" : "")}
                           onClick={()=>onSelectDoctor(doc, d.id)}>
                        <p>{doc.lastName} {doc.firstName} {doc.patronymic}</p>
                      </div>
                    ))}
                  </div>
                ))}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}

/* DayCell — one calendar day */
function DayCell({ day, shift, isToday }) {
  const time = shift === 1 ? "08:00 — 14:30"
             : shift === 2 ? "14:30 — 20:00"
             : "";
  const cls = shift === 1 ? "shift-1"
            : shift === 2 ? "shift-2"
            : shift === 0 ? "weekend" : "no-schedule";
  return (
    <div className={"day-cell " + cls + (isToday ? " today" : "")}>
      <p className="day-number">{day}</p>
      {time ? <p className="work-time">{time}</p>
            : shift === 0 ? <img className="weekend-icon" src="../../assets/icons/weekend.svg"/> : null}
    </div>
  );
}

/* Calendar — month grid with toolbar + stats */
function Calendar({ months, days, year, month, onPrevMonth, onNextMonth, onPrevYear, onNextYear, onJumpToday, schedule, todayDay }) {
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const firstWeekday = (new Date(year, month, 1).getDay() + 6) % 7; // Mon=0
  const total = firstWeekday + daysInMonth;
  const trailing = Math.max(0, 42 - total);

  // Stats
  const stats = useMemo(() => {
    let s1 = 0, s2 = 0, wk = 0, hours = 0;
    for (let d = 1; d <= daysInMonth; d++) {
      const v = schedule[d];
      if (v === 1) { s1++; hours += 6.5; }
      else if (v === 2) { s2++; hours += 5.5; }
      else if (v === 0) { wk++; }
    }
    return { s1, s2, wk, hours };
  }, [schedule, daysInMonth]);

  return (
    <div className="schedule-container">
      <div className="calendar-toolbar">
        <div className="nav-cluster">
          <button className="nav-btn" onClick={onPrevYear} title="Предыдущий год">
            <img src="../../assets/icons/left-arrow.svg"/><img src="../../assets/icons/left-arrow.svg" style={{marginLeft:-8}}/>
          </button>
          <button className="nav-btn" onClick={onPrevMonth} title="Предыдущий месяц">
            <img src="../../assets/icons/left-arrow.svg"/>
          </button>
        </div>
        <div className="label-stack">
          <div className="month">{months[month]}</div>
          <div className="year">{year}</div>
        </div>
        <button className="today-btn" onClick={onJumpToday}>Сегодня</button>
        <div className="nav-cluster">
          <button className="nav-btn" onClick={onNextMonth} title="Следующий месяц">
            <img src="../../assets/icons/right-arrow.svg"/>
          </button>
          <button className="nav-btn" onClick={onNextYear} title="Следующий год">
            <img src="../../assets/icons/right-arrow.svg"/><img src="../../assets/icons/right-arrow.svg" style={{marginLeft:-8}}/>
          </button>
        </div>
      </div>

      <div className="calendar-stats">
        <div className="stat-pill first">
          <div className="lbl">1 смена</div>
          <div className="val">{stats.s1}</div>
          <div className="sub">дней</div>
        </div>
        <div className="stat-pill second">
          <div className="lbl">2 смена</div>
          <div className="val">{stats.s2}</div>
          <div className="sub">дней</div>
        </div>
        <div className="stat-pill weekend">
          <div className="lbl">выходных</div>
          <div className="val">{stats.wk}</div>
          <div className="sub">дней</div>
        </div>
        <div className="stat-pill hours">
          <div className="lbl">часы</div>
          <div className="val">{stats.hours.toFixed(1)}</div>
          <div className="sub">за месяц</div>
        </div>
      </div>

      <div className="days-grid">
        {days.map(d => <div key={d.id} className="day-header">{d.day}</div>)}
        {Array.from({length: firstWeekday}).map((_,i)=>(
          <div key={"e"+i} className="empty-cell"/>
        ))}
        {Array.from({length: daysInMonth}).map((_,i)=>{
          const day = i + 1;
          return <DayCell key={day} day={day} shift={schedule[day]} isToday={day === todayDay}/>;
        })}
        {Array.from({length: trailing}).map((_,i)=>(
          <div key={"t"+i} className="empty-cell"/>
        ))}
      </div>
    </div>
  );
}

function SelectedDoctorPill({ doctor, deptName, onClear }) {
  if (!doctor) return null;
  return (
    <div className="selected-doctor-pill">
      <div className="avatar">{initials(doctor)}</div>
      <div className="meta">
        <div className="name">{doctor.lastName} {doctor.firstName}</div>
        <div className="role">{deptName}</div>
      </div>
      <button className="clear" onClick={onClear} title="Снять выбор">✕</button>
    </div>
  );
}

function ScheduleLegend() {
  return (
    <div className="legend-card">
      <h2>обозначения</h2>
      <div className="legend-row"><div className="chip" style={{background:"var(--shift-first)"}}></div>1 смена · 08:00 — 14:30</div>
      <div className="legend-row"><div className="chip" style={{background:"var(--shift-second)"}}></div>2 смена · 14:30 — 20:00</div>
      <div className="legend-row"><div className="chip" style={{background:"var(--shift-weekend)"}}></div>выходной</div>
      <hr/>
      <div className="legend-row"><div className="chip" style={{background:"var(--bg-day-today)", outline:"2px solid var(--accent-default)", outlineOffset:"-2px"}}></div>текущий день</div>
    </div>
  );
}

/* HoursSummary — featured "scheduled hours this month" panel for the right rail */
function HoursSummary({ schedule, year, month, norm = 168 }) {
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const today = new Date();
  const isCurrentMonth = (year === today.getFullYear() && month === today.getMonth());
  const todayDay = isCurrentMonth ? today.getDate() : null;

  const stats = useMemo(() => {
    let s1 = 0, s2 = 0, wk = 0, plannedDays = 0;
    let hoursTotal = 0, hoursElapsed = 0;
    for (let d = 1; d <= daysInMonth; d++) {
      const v = schedule[d];
      let h = 0;
      if (v === 1) { s1++; h = 6.5; }
      else if (v === 2) { s2++; h = 5.5; }
      else if (v === 0) { wk++; }
      hoursTotal += h;
      if (h > 0) plannedDays++;
      if (todayDay && d <= todayDay && h > 0) hoursElapsed += h;
    }
    const hoursS1 = s1 * 6.5;
    const hoursS2 = s2 * 5.5;
    const avg = plannedDays ? (hoursTotal / plannedDays) : 0;
    return { s1, s2, wk, plannedDays, hoursTotal, hoursS1, hoursS2, avg, hoursElapsed };
  }, [schedule, daysInMonth, todayDay]);

  const pct = Math.min(100, Math.round((stats.hoursTotal / norm) * 100));
  const elapsedPct = Math.min(100, Math.round((stats.hoursElapsed / norm) * 100));
  const diff = stats.hoursTotal - norm;
  const fmt = (n) => Number.isInteger(n) ? n.toString() : n.toFixed(1);

  // Choose a status tag color based on plan-vs-norm
  let statusTag = null;
  if (stats.hoursTotal === 0) {
    statusTag = { label: "не запланировано", color: "var(--fg-quiet)", bg: "rgba(255,255,255,.06)" };
  } else if (diff === 0) {
    statusTag = { label: "точно в норму", color: "var(--action-success)", bg: "rgba(76,175,80,.14)" };
  } else if (diff > 0) {
    statusTag = { label: `+${fmt(diff)} ч сверх нормы`, color: "var(--shift-second)", bg: "rgba(185,91,24,.18)" };
  } else {
    statusTag = { label: `${fmt(diff)} ч до нормы`, color: "var(--accent-control)", bg: "rgba(55,123,211,.18)" };
  }

  return (
    <div className="hours-card">
      <div className="hours-card__head">
        <h2>часы в этом месяце</h2>
        <span className="hours-card__tag" style={{color: statusTag.color, background: statusTag.bg}}>
          {statusTag.label}
        </span>
      </div>

      <div className="hours-card__big">
        <span className="num">{fmt(stats.hoursTotal)}</span>
        <span className="unit">ч</span>
        <span className="of">/ {norm}</span>
      </div>

      <div className="hours-card__bar" title={`${pct}% от нормы`}>
        {isCurrentMonth && stats.hoursElapsed > 0 && (
          <div className="hours-card__bar-elapsed" style={{width: elapsedPct + "%"}}></div>
        )}
        <div className="hours-card__bar-fill" style={{width: pct + "%"}}>
          <span className="hours-card__bar-split"
                style={{
                  background: "var(--shift-second)",
                  width: stats.hoursTotal ? ((stats.hoursS2 / stats.hoursTotal) * 100) + "%" : 0
                }}></span>
        </div>
      </div>
      <div className="hours-card__bar-meta">
        <span>{pct}% от месячной нормы</span>
        {isCurrentMonth && stats.hoursElapsed > 0 && <span>пройдено {fmt(stats.hoursElapsed)} ч</span>}
      </div>

      <div className="hours-card__breakdown">
        <div className="row">
          <span className="chip" style={{background:"var(--shift-first)"}}></span>
          <span className="label">1 смена</span>
          <span className="value">{fmt(stats.hoursS1)} ч</span>
          <span className="muted">· {stats.s1} дн.</span>
        </div>
        <div className="row">
          <span className="chip" style={{background:"var(--shift-second)"}}></span>
          <span className="label">2 смена</span>
          <span className="value">{fmt(stats.hoursS2)} ч</span>
          <span className="muted">· {stats.s2} дн.</span>
        </div>
        <div className="row">
          <span className="chip" style={{background:"var(--shift-weekend)"}}></span>
          <span className="label">выходные</span>
          <span className="value">—</span>
          <span className="muted">· {stats.wk} дн.</span>
        </div>
      </div>

      <div className="hours-card__foot">
        <div>
          <span className="k">средняя смена</span>
          <span className="v">{fmt(stats.avg)} ч</span>
        </div>
        <div>
          <span className="k">рабочих дней</span>
          <span className="v">{stats.plannedDays}</span>
        </div>
      </div>
    </div>
  );
}

Object.assign(window, { DepartmentPicker, DayCell, Calendar, SelectedDoctorPill, ScheduleLegend, HoursSummary, initials });
