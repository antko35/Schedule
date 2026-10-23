// Mock data lifted from the schema shapes in interfaces/*.ts
window.MOCK = {
  departments: [
    { id: "d1", departmentName: "хирургия" },
    { id: "d2", departmentName: "терапия" },
    { id: "d3", departmentName: "ортодонтия" }
  ],
  doctorsByDept: {
    d1: [
      { id: "u1", firstName: "Иван", lastName: "Иванов", patronymic: "Иванович", gender: "мужской", dateOfBirth: "01.04.1985", email: "ivanov@clinic.by", phoneNumber: "+375 44 588 21 23" },
      { id: "u2", firstName: "Ольга", lastName: "Иваненко", patronymic: "Петровна", gender: "женский", dateOfBirth: "22.11.1990", email: "o.ivanenko@clinic.by", phoneNumber: "+375 29 511 44 02" },
      { id: "u3", firstName: "Светлана", lastName: "Андрейчук", patronymic: "Васильевна", gender: "женский", dateOfBirth: "14.06.1979", email: "andreychuk@clinic.by", phoneNumber: "+375 25 322 19 87" }
    ],
    d2: [
      { id: "u4", firstName: "Дмитрий", lastName: "Ковшер", patronymic: "Антонович", gender: "мужской", dateOfBirth: "07.02.1988", email: "kovsher@clinic.by", phoneNumber: "+375 44 588 21 23" },
      { id: "u5", firstName: "Елена", lastName: "Бельская", patronymic: "Сергеевна", gender: "женский", dateOfBirth: "30.09.1992", email: "belskaya@clinic.by", phoneNumber: "+375 29 712 09 14" }
    ],
    d3: [
      { id: "u6", firstName: "Артём", lastName: "Петров", patronymic: "Олегович", gender: "мужской", dateOfBirth: "18.12.1981", email: "petrov@clinic.by", phoneNumber: "+375 33 401 18 92" }
    ]
  },
  // shift: 1 = morning (green), 2 = afternoon (orange), 0 = weekend
  scheduleByDoctor: {
    u1: { /* день: shift */ 1:1, 2:2, 3:0, 4:1, 5:2, 6:0, 7:0, 8:1, 9:2, 10:1, 11:0, 12:1, 13:2, 14:0, 15:1, 16:2, 17:0, 18:1, 19:2, 20:0, 21:1, 22:2, 23:0, 24:1, 25:2, 26:0, 27:0, 28:1, 29:2, 30:1, 31:0 },
    u2: { 1:2, 2:1, 3:0, 4:2, 5:1, 6:0, 7:0, 8:2, 9:1, 10:2, 11:0, 12:2, 13:1, 14:0, 15:2, 16:1, 17:0, 18:2, 19:1, 20:0, 21:2, 22:1, 23:0, 24:2, 25:1, 26:0, 27:0, 28:2, 29:1, 30:2, 31:0 },
    u3: { 1:1, 4:1, 5:1, 6:0, 7:0, 11:2, 12:2, 14:0, 15:1, 18:2, 19:1, 25:0, 26:1, 28:0, 30:2 },
    u4: { 1:1, 2:1, 6:0, 7:0, 8:1, 10:2, 14:0, 15:1, 20:0, 22:2, 28:0, 30:1 },
    u5: {},
    u6: { 5:1, 10:1, 15:2, 20:1, 25:2 }
  },
  doctorFuncList: [
    { id: "1", text: "Добавить работника", route: "addUser", style: "add" },
    { id: "2", text: "Удалить работника", route: "deleteUser", style: "delete" },
    { id: "3", text: "Изменить информацию", route: "redactUserInfo", style: "redact" }
  ],
  departmentFuncList: [
    { id: "1", text: "Добавить отделение", route: "createDepartment", style: "add" },
    { id: "2", text: "Добавить работника", route: "addUserToDepartment", style: "add" },
    { id: "3", text: "Изменить информацию работника", route: "redactUserInfoFromDepartment", style: "redact" },
    { id: "4", text: "Удалить работника", route: "deleteUserFromDepartment", style: "delete" },
    { id: "5", text: "Удалить отделение", route: "deleteDepartment", style: "delete" }
  ],
  scheduleFuncList: [
    { id: "1", text: "Составить расписание работнику", route: "createUserSchedule", style: "add" },
    { id: "2", text: "Добавить официальный выходной", route: "transferDay", style: "add" },
    { id: "3", text: "Удалить официальный выходной", route: "deleteHoliday", style: "delete" }
  ],
  days: [
    { id: '1', day: 'Пн' }, { id: '2', day: 'Вт' }, { id: '3', day: 'Ср' },
    { id: '4', day: 'Чт' }, { id: '5', day: 'Пт' }, { id: '6', day: 'Сб' }, { id: '7', day: 'Вс' }
  ],
  months: [
    "январь","февраль","март","апрель","май","июнь",
    "июль","август","сентябрь","октябрь","ноябрь","декабрь"
  ]
};
