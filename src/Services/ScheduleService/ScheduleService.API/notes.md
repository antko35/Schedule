PATCH setRules for setting rules 
Flow:
On schedule page for each user in "Редактировать расписание" mode we can set rules for schedule generation.
In "Редактировать расписание" module under "Выберите день в календаре" block user must have opportunity to enter such values as:   

"Норма часов в месяц": (пользователь вводит часы и минуты, в скобочках автоматически переводится в число с запятой)
```
0.1 часа = 6 мин
0.2 часа = 12 мин
0.3 часа = 18 мин
0.4 часа = 24 мин
0.5 часа = 30 мин
0.6 часа = 36 мин
0.7 часа = 42 мин
0.8 часа = 48 мин
0.9 часа = 54 мин
```

"Периодичность смен" (dropdown):
```
первая пн, ср, пт, вторая вт, чт ("evenDOW": true")
вторая пн, ср, пт, первая вт, чт ("unEvenDOW": true")
первая по четным числам месяца, вторая по нечетным ("evenDOM": true")
вторая по четным числам месяца, первая по нечетным ("unEvenDOM": true")
всегда первая ("onlyFirstShift" : true")
всегда вторая ("onlySecondShift" : true")
ручное создание (везде false) 
```

"Продолжительность рабочего дня" (пользователь вводит часы и минуты, в скобочках автоматически переводится в число с запятой)

"Начало рабочего дня" ("startWorkDayTime")

"Сгенерировать" button. Request for  [HttpPost] [Route("{userId}/generate")]
