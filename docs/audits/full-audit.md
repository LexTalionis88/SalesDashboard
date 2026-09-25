# Полный аудит проекта

Дата аудита: 2026-09-25.

## Объём

Проверены исходный код и структура репозитория, требования и решения,
backend на .NET 10, frontend на React/Vite, PostgreSQL и seed,
контейнеризация, OpenTelemetry, тестовые сборки, API-контракты,
длина строк исходного кода и production-риски.

## Итог

Локальная версия v1 собирается и запускается. Критических дефектов,
блокирующих демонстрационный сценарий, не обнаружено.

Подтверждённые проверки:

- `dotnet test backend/SalesDashboard.slnx --no-restore`:
  45 тестов пройдено, 0 ошибок, 0 пропусков.
- `npm run build` во frontend: TypeScript проверен, Vite-сборка успешна.
- `docker compose config`: конфигурация валидна.
- `docker compose up -d --build`: backend, frontend, PostgreSQL,
  Jaeger и dotnet-monitor запустились.
- `/api/health/ready`, `/api/dashboard` и frontend возвращают HTTP 200.
- Jaeger содержит сервис `sales-dashboard-api`, трассировка подключена.
- В исходниках frontend/backend нет строк длиннее 120 символов.
- Проверка уязвимых NuGet-пакетов не обнаружила advisories.
- `npm audit` не завершился: registry advisory endpoint недоступен
  из текущего окружения. Это не является подтверждением безопасности npm.

## Сильные стороны

1. Слои API, Application и DataAccess разделены сборками.
2. Сервисы представлены интерфейсами и могут заменяться в тестах.
3. Read-only аналитика использует серверные агрегаты, проекции и
   `AsNoTracking`; frontend не агрегирует полный набор продаж.
4. Для dashboard отсутствует N+1: integration-тест фиксирует шесть SQL-команд.
5. Период проверяется с полуоткрытым интервалом `[from, to + 1 день)`.
6. Денежные значения передаются строками, расчёты выполняются на сервере.
7. Seed идемпотентен, защищён advisory lock и маркером `SeedRun`.
8. Unit, integration и E2E тесты находятся в отдельных сборках NUnit.
9. Compose использует healthcheck PostgreSQL и привязку портов к loopback.
10. Ошибки API скрывают stack trace и возвращают trace id.

## Найденные риски

### P0 при внешней публикации

**Открытый dotnet-monitor.** В Compose используется `--no-auth`.
Сейчас порт опубликован только на `127.0.0.1`, поэтому риск ограничен
локальной машиной. При публикации порта наружу monitor позволит выполнять
диагностические операции без аутентификации.

Решение: production-профиль без `--no-auth`, с авторизацией и TLS,
либо отключение monitor вне локальной разработки.

### P1 перед production

**Dev-пароль базы в tracked-файлах.** `sales_local` присутствует в
`docker-compose.yml` и `appsettings.json`. Для локального v1 это допустимо,
но конфигурация не должна использоваться за пределами development.

Решение: `.env` вне Git, Docker secrets или secret manager, отдельный
пользователь БД с минимальными правами и ротация пароля.

**Нет authentication/authorization.** API открыт любому процессу,
получившему доступ к порту. Это соответствует ограничениям текущего ТЗ,
но неприемлемо для внешнего сервиса.

Решение: identity provider, read-only scope/роль, аудит доступа и TLS.

**Нет security headers и rate limiting.** Nginx не задаёт CSP,
`X-Content-Type-Options`, `Referrer-Policy` и frame policy. На API нет
ограничения частоты запросов.

Решение: добавить headers на ingress, настроить rate limiting,
connection limits и request timeouts перед внешним доступом.

**Неполная защита telemetry.** Jaeger UI и OTLP endpoint доступны на
loopback без аутентификации. При публикации сети это раскрывает трассы и
приёмник telemetry.

Решение: не публиковать telemetry-порты наружу либо закрыть их reverse proxy,
auth и TLS.

### P1 по производительности

**Крупный initial frontend bundle.** Production-сборка сообщает JavaScript
около 639 KB до сжатия и предупреждение Vite о chunk больше 500 KB.
Основной вклад даёт Recharts.

Решение: lazy-load графика, code splitting, performance budget в CI и
проверка Lighthouse на целевых устройствах.

**Нет compression и cache policy в Nginx.** Хешированные Vite-ассеты не
получают долгий immutable cache, gzip/Brotli не настроены.

Решение: включить compression для JS/CSS/JSON и cache-control для `/assets`.

### P2

**Нет подтверждённого npm audit.** Проверка завершилась ошибкой доступа к
registry advisory endpoint. Нужен повтор в CI или сети с доступом к npm.

**Нет нагрузочного профиля.** Текущие тесты подтверждают корректность и
план запросов на seed из 3000 продаж, но не задают p95, capacity и SLA.

**Readiness проверяет только подключение к БД.** На текущем запуске
инициализатор выполняется до `app.Run`, поэтому ошибка миграции/seed не даёт
поднять API. Если lifecycle изменится, endpoint нужно связать с состоянием
инициализации.

## Аудит требований

Обязательные требования v1 покрыты: выбор периода, KPI, динамика по дням,
рейтинг, лучший менеджер, категории, top-5 товаров, история продаж,
серверные расчёты, PostgreSQL, Docker Compose и OpenTelemetry.

Зафиксированные решения также согласованы с реализацией: .NET 10,
NUnit для всех тестовых контуров, отдельные сборки слоёв и контроллеры
вместо Minimal API.

Сравнение с предыдущим периодом реализовано как дополнительный блок.
Production deployment, авторизация, SLA и полноценная мобильная версия
остаются за пределами текущего ТЗ и должны быть отдельными решениями.

## Рекомендованный порядок работ

1. Разделить development и production Compose-профили.
2. Убрать секреты из tracked-конфигурации.
3. Закрыть monitor, Jaeger и OpenAPI при внешнем доступе.
4. Добавить security headers, TLS и rate limiting на ingress.
5. Уменьшить initial bundle и включить compression/cache.
6. Запустить npm audit в CI и добавить dependency scanning.
7. Согласовать SLA и выполнить нагрузочное тестирование.

## Вердикт

Проект готов к локальной демонстрации и проверке требований v1.
Для production-публикации сначала требуется закрыть P0/P1-пункты
безопасности и производительности, перечисленные выше.


## Повторная проверка bundle

После аудита график вынесен в lazy chunk SalesChart. Initial JavaScript уменьшился с 639 KB до 266 KB, gzip-размер — с примерно 189 KB до 82 KB. Recharts загружается отдельным chunk 374 KB и не входит в initial загрузку приложения. Frontend build и 7 browser E2E тестов прошли.
