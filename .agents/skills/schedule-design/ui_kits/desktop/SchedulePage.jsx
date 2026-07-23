/* global React, DepartmentPicker, Calendar, SelectedDoctorPill, ScheduleLegend */
const { useState } = React;

function SchedulePage({ data, onNavigate }) {
  const [selectedDoctor, setSelectedDoctor] = useState(null);
  const [selectedDept, setSelectedDept] = useState(null);
  const [search, setSearch] = useState("");
  const today = new Date();
  const [year, setYear] = useState(today.getFullYear());
  const [month, setMonth] = useState(today.getMonth());
  const schedule = selectedDoctor ? (data.scheduleByDoctor[selectedDoctor.id] || {}) : {};
  const deptName = selectedDept ? (data.departments.find(d=>d.id===selectedDept)||{}).departmentName : "";
  const jumpToday = () => { setYear(today.getFullYear()); setMonth(today.getMonth()); };

  return (
    <div className="schedule">
      <div className="choose-container">
        {selectedDoctor ? (
          <>
            <SelectedDoctorPill
              doctor={selectedDoctor}
              deptName={deptName}
              onClear={()=>{ setSelectedDoctor(null); setSelectedDept(null); }}
            />
            <div className="reduct" onClick={()=>onNavigate("/redact")}>
              редактировать расписание
              <img src="../../assets/icons/redact-schedule.svg"/>
            </div>
          </>
        ) : (
          <h1>выбрать работника</h1>
        )}

        <div className="sidebar-search">
          <img src="../../assets/icons/search.svg" alt=""/>
          <input
            type="text"
            placeholder="Найти работника или отделение"
            value={search}
            onChange={(e)=>setSearch(e.target.value)}
          />
        </div>

        <DepartmentPicker
          departments={data.departments}
          doctorsByDept={data.doctorsByDept}
          selectedDoctorId={selectedDoctor && selectedDoctor.id}
          searchTerm={search}
          onSelectDoctor={(doc, dId)=>{ setSelectedDoctor(doc); setSelectedDept(dId); }}
        />
      </div>

      {selectedDoctor ? (
        <Calendar
          months={data.months} days={data.days}
          year={year} month={month}
          onPrevYear={()=>setYear(y=>y-1)} onNextYear={()=>setYear(y=>y+1)}
          onPrevMonth={()=>{ if(month===0){setMonth(11);setYear(y=>y-1);} else setMonth(m=>m-1); }}
          onNextMonth={()=>{ if(month===11){setMonth(0);setYear(y=>y+1);} else setMonth(m=>m+1); }}
          onJumpToday={jumpToday}
          schedule={schedule}
          todayDay={year===today.getFullYear() && month===today.getMonth() ? today.getDate() : -1}
        />
      ) : (
        <div className="schedule-empty">
          <div className="ill"></div>
          <h3>Выберите работника</h3>
          <p>Выберите отделение и работника в боковой панели слева, чтобы открыть его месячный график.</p>
        </div>
      )}

      <div className="right-rail">
        {selectedDoctor && (
          <HoursSummary schedule={schedule} year={year} month={month}/>
        )}
        <ScheduleLegend/>
      </div>
    </div>
  );
}

Object.assign(window, { SchedulePage });
