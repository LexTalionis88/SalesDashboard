# Active context

Обновлено: 2026-09-25.

Пользователь принял решения по умолчанию для v1, затем явно выбрал .NET 10 для backend и NUnit для unit/integration/e2e и поручил начать backend. Решение записано в ADR-001; xUnit заменён на NUnit. Исходный seed 20/75/6/48/3000 за 12 месяцев сохранён.

Реализованы backend, EF migration, PostgreSQL seed, dashboard/sales API, валидация/ошибки, OpenAPI и Docker Compose. Frontend dashboard подключён к API через Nginx proxy; добавлены фильтры, KPI, график, рейтинг, категории, товары, история и состояния загрузки/ошибки/пустой выборки. Бизнес-правила реализованы; сравнение общих KPI включено, изменения в строках рейтинга пока нет.

OpenTelemetry tracing подключён в API: ASP.NET Core, HttpClient и EF Core spans экспортируются по OTLP/gRPC при заданном endpoint. Compose поднимает Jaeger на `localhost:16686`; проверены traces сервиса `sales-dashboard-api`. Nginx использует Docker DNS resolver, чтобы переживать пересоздание backend без 502.

Unit/integration/backend e2e и браузерные frontend e2e написаны на NUnit. Integration — PostgreSQL Testcontainers, backend e2e — отдельный Kestrel и PostgreSQL, browser e2e — Playwright for .NET против Compose frontend. Фактические проверки и границы — progress.md. Для API есть docs/api-contract.md и endpoint /openapi/v1.json.

Браузерные E2E реализованы в `FrontendJourneyTests`: загрузка dashboard, смена периода/рейтинга и пустой период. Chromium установлен локально, три сценария проходят; визуальный интерактивный просмотр через встроенный браузер остаётся отдельным ручным шагом.

Работа ведётся в main, коммиты на русском языке. Удалённого репозитория нет. Фактический общий timebox не измерялся; не выдавать оценку за учёт времени.
