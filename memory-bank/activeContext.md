# Active context

Обновлено: 2026-09-25.

Пользователь принял решения по умолчанию для v1, затем явно выбрал .NET 10 для backend и NUnit для unit/integration/e2e и поручил начать backend. Решение записано в ADR-001; xUnit заменён на NUnit. Исходный seed 20/75/6/48/3000 за 12 месяцев сохранён.

Реализованы backend, EF migration, PostgreSQL seed, dashboard/sales API, валидация/ошибки, OpenAPI и Docker Compose. Frontend dashboard подключён к API через Nginx proxy; добавлены фильтры, KPI, график, рейтинг, категории, товары, история и состояния загрузки/ошибки/пустой выборки. Бизнес-правила реализованы; сравнение общих KPI включено, изменения в строках рейтинга пока нет.

Unit/integration/backend e2e написаны на NUnit. Integration — PostgreSQL Testcontainers, e2e — отдельный Kestrel и PostgreSQL. Фактические проверки и границы — progress.md. Для API есть docs/api-contract.md и endpoint /openapi/v1.json.

Следующий этап: браузерные E2E на NUnit и визуальная проверка dashboard. Frontend использует React, TypeScript, Vite, TanStack Query и Recharts; версии закреплены в package-lock.json. Встроенный браузер при первой попытке проверки был недоступен.

Работа ведётся в main, коммиты на русском языке. Удалённого репозитория нет. Фактический общий timebox не измерялся; не выдавать оценку за учёт времени.
