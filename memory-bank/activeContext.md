# Active context

Обновлено: 2026-09-25.

Пользователь принял решения по умолчанию для v1, затем явно выбрал .NET 10 для backend и NUnit для unit/integration/e2e и поручил начать backend. Решение записано в ADR-001; xUnit заменён на NUnit. Исходный seed 20/75/6/48/3000 за 12 месяцев сохранён.

Реализованы backend, EF migration, PostgreSQL seed, dashboard/sales API, валидация/ошибки, OpenAPI и Docker Compose для backend/БД. Frontend пока каркас. Бизнес-правила реализованы; сравнение общих KPI включено, изменения в строках рейтинга пока нет.

Unit/integration/backend e2e написаны на NUnit. Integration — PostgreSQL Testcontainers, e2e — отдельный Kestrel и PostgreSQL. Фактические проверки и границы — progress.md. Для API есть docs/api-contract.md и endpoint /openapi/v1.json.

Следующий этап: React + TypeScript dashboard с реальными API, все loading/error/empty states, frontend proxy в Compose и браузерные e2e на NUnit. Перед этим прочитать API-контракт и принятые правила. Точные версии frontend/UI ещё не выбраны. Не запрашивать повторное согласование уже принятых backend-решений.

Работа ведётся в main, коммиты на русском языке. Удалённого репозитория нет. Фактический общий timebox не измерялся; не выдавать оценку за учёт времени.
