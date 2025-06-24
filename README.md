# how to generate schedule (without registration)
1. create user using post request on http://localhost:5003/User endpoint
2. create department using post request on http://localhost:5003/Department endpoint
3. add this person to department using http://localhost:5003/Department/addUserToDepartment
request example:
```cs
{
  "userId": "685aa69e582628e2ed3898a0",
  "departmentId": "685aa709582628e2ed3898a2",
  "role": "dentist",
  "status": "working",
  "email": "kovsheranton@gmail.com",
  "phoneNumber": "+375445882123"
}
```
After adding user to department in db schedule_db, Collection: UserScheduleRule new record will appear.


## Manual schedule creating
You can create schedule manually using http://localhost:5002/Schedule endpoints. Example:
request :
```cs
{
  "userId": "685aa69e582628e2ed3898a0",
  "departmentId": "685aa709582628e2ed3898a2",
  "startTime": "2025-06-24T08:00:00.000Z",
  "endTime": "2025-06-24T14:30:00.000Z"
}
```
response : 
```csharp
{
  "day": 24,
  "startTime": "2025-06-24T08:00:00Z",
  "endTime": "2025-06-24T14:30:00Z"
}
```
## How to fill ScheduleRules
```csharp
{
  "scheduleRulesId": "string",
  "hoursPerMonth": 0,
  "maxHoursPerDay": 0,
  "startWorkDayTime": "string",
  "onlyFirstShift": true,
  "onlySecondShift": true,
  "evenDOW": true,
  "unEvenDOW": true,
  "evenDOM": true,
  "unEvenDOM": true
}
```
scheduleRulesId - id from ScheduleRules collection
hoursPerMonth - hours from specific file(хз где, мб у папы на ффлешке либо у меня на винде, надо поискать)
maxHoursPerDay - hours from specific file
startWorkDayTime - 