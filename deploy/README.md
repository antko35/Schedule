# Запуск Schedule на другом компьютере (без Aspire)

Эта папка позволяет собрать всё решение в один переносимый бандл и запустить
его на любой машине, где есть **только Docker Desktop** — без .NET SDK, без
установки Postgres/Mongo/RabbitMQ/Keycloak.

> Aspire (`dotnet run --project Schedule.AppHost`) — только для разработки.
> Для переноса используется этот `docker-compose.yml`, в котором строки
> подключения, которые обычно инжектит Aspire, прописаны явно.

## Что внутри

| Файл | Назначение |
|---|---|
| `docker-compose.yml` | Все сервисы + инфраструктура, self-contained |
| `keycloak/import/schedule-service-realm.json` | Авто-импортируемый realm (клиент + audience + демо-юзер `demo`/`demo`) |
| `build.ps1` | Собирает образы и пакует бандл (запускать на **твоей** машине) |
| `run.ps1` | Запускает всё (запускать на **целевой** машине) |

---

## Вариант A. Перенос без интернета/исходников (рекомендуется)

На **твоей** машине (есть Docker + исходники):

```powershell
cd deploy
.\build.ps1                  # собрать образы и упаковать в deploy\bundle\
# .\build.ps1 -WithFrontend  # + собрать десктоп-приложение Electron
```

Скопируй папку **`deploy\bundle\`** на другой компьютер (флешка/сеть).

На **целевой** машине (нужен только Docker Desktop):

```powershell
cd bundle
.\run.ps1                    # загрузит образы из images.tar и поднимет всё
```

## Вариант B. На целевой машине есть исходники и интернет

Копируешь весь репозиторий, затем:

```powershell
cd deploy
.\run.ps1 -Build             # соберёт образы из исходников и запустит
```

---

## Управление

```powershell
.\run.ps1 -Logs   # смотреть логи
.\run.ps1 -Down   # остановить и удалить контейнеры (данные в volume сохранятся)
```

## Адреса после запуска

| Сервис | URL |
|---|---|
| UserService API | http://localhost:5001/swagger |
| ScheduleService API | http://localhost:5002 (Swagger на `/`) |
| UserManagementService API | http://localhost:5003/swagger |
| Keycloak | http://localhost:8080 (admin/admin) |
| RabbitMQ | http://localhost:15672 (guest/guest) |
| pgAdmin | http://localhost:5050 |
| Mongo Express (schedule) | http://localhost:8082 |
| Mongo Express (management) | http://localhost:8081 |

## Аутентификация (Keycloak)

Защищённые эндпоинты ScheduleService требуют JWT. Realm `schedule-service`
импортируется автоматически. Получить токен для тестов:

```bash
curl -X POST http://localhost:8080/realms/schedule-service/protocol/openid-connect/token \
  -d "client_id=schedule-service" -d "grant_type=password" \
  -d "username=demo" -d "password=demo"
```

Дальше — `Authorization: Bearer <access_token>`.

## Заметки / отличия от исходного compose

- Строки подключения Mongo/RabbitMQ заданы явно (Aspire их больше не инжектит).
- Убран HTTPS-сертификат у `user-service` (папки `certs` не было) — сервисы
  работают по HTTP внутри Docker. При необходимости HTTPS настраивается отдельно.
- Убрана `dotnet ef database update` из команды `user-service`: миграции
  применяются на старте приложения (`MigrationExtensions`).
- Добавлен `ASPNETCORE_URLS=http://+:8080`, т.к. финальный образ собирается на
  `sdk`-базе и порт 8080 иначе не гарантирован.
