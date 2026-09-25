# Sales Performance Dashboard

Тестовое задание DJI-Market.ru. Реализованы dashboard на React + TypeScript, API на .NET 10, PostgreSQL и локальный запуск через Docker Compose.

## Запуск

```sh
docker compose up --build
```

- Dashboard: http://localhost:3000
- API: http://localhost:8080/api/dashboard
- Последние продажи: http://localhost:8080/api/sales
- Readiness: http://localhost:8080/api/health/ready
- OpenAPI: http://localhost:8080/openapi/v1.json
- dotnet-monitor: http://localhost:52323
- Jaeger UI: http://localhost:16686

Backend применяет миграции и создаёт seed при первом запуске. Frontend проксирует `/api` через Nginx, поэтому браузеру нужен один origin. Backend экспортирует traces по OTLP/gRPC в Jaeger; каждый HTTP-запрос и EF Core запрос получают связанные spans.

## Возможности

Выбор периода, KPI, ежедневная динамика, рейтинг менеджеров по валовой прибыли или среднему чеку, лучший менеджер, категории, пять наиболее прибыльных товаров и история продаж. Запросы при смене фильтров отменяются; ошибки и пустые выборки отображаются в интерфейсе.

В KPI входят только Paid. Cancelled и Refunded остаются в истории с исходными суммами. Деньги рассчитываются сервером из позиций продаж. Бизнес-зона — Europe/Moscow. Предыдущий период содержит столько же календарных дней перед текущим.

Seed: 20 менеджеров, 75 клиентов, 6 категорий, 48 товаров и 3000 продаж за 12 месяцев. Повторный старт не дублирует данные.

## Структура

- `backend/src/SalesDashboard.Api` — HTTP-контроллеры и композиция.
- `backend/src/SalesDashboard.Application` — бизнес-логика и DTO.
- `backend/src/SalesDashboard.DataAccess` — EF Core, миграции и seed.
- `backend/tests` — отдельные NUnit-сборки unit, integration и E2E, включая браузерные сценарии frontend.
- `frontend` — React, Vite, TanStack Query, Recharts и Nginx.

## Проверка

```sh
dotnet test backend/SalesDashboard.slnx
npm --prefix frontend run build
```

Integration и backend E2E используют Testcontainers с PostgreSQL и требуют Docker. Браузерные E2E используют Playwright for .NET и требуют установленный Chromium; адрес frontend задаётся переменной `FRONTEND_E2E_URL`.

## Документы

- [Требования](docs/requirements/functional.md), [архитектура](docs/architecture.md), [бизнес-правила](docs/business-rules.md), [API-контракт](docs/api-contract.md).
- [Решения](docs/decisions/open-decisions.md), [план проверки](docs/verification-plan.md), [memory bank](memory-bank/README.md).
- [Руководство пользователя](docs/user-guide.md) — отдельная инструкция для работы с dashboard.
- [QA-артефакты](docs/qa/README.md) — план, сценарии, матрица трассируемости, чек-лист и отчёт.
- [CI/CD](docs/ci-cd.md) — GitHub Actions для проверок и публикации образов в GHCR.
- [Frontend](frontend/README.md), [backend](backend/README.md), [история запросов](AI_PROMPTS.md).

Авторизация, admin panel и production deployment не входят в текущую версию. В строках рейтинга нет сравнения с прошлым периодом; общие KPI сравниваются.
