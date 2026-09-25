# Backend

ASP.NET Core / .NET 10, EF Core 10, PostgreSQL. NUnit для unit, integration и backend e2e. Версии закреплены в csproj и packages.lock.json; основание — [ADR-001](../docs/decisions/ADR-001-backend-stack.md).

## Запуск из корня репозитория

```sh
docker compose up --build
```

API: http://localhost:8080/api/dashboard. OpenAPI: http://localhost:8080/openapi/v1.json. Readiness: http://localhost:8080/api/health/ready. Docker автоматически создаёт схему и seed. Frontend пока не реализован.

Для локальной разработки с .NET 10 SDK:

```sh
docker compose up -d postgres
dotnet restore backend/SalesDashboard.slnx --locked-mode
dotnet run --project backend/src/SalesDashboard.Api --no-launch-profile --urls http://localhost:8080
```

Локальный API и контейнерный API используют один порт, запускать один вариант за раз. PostgreSQL доступен на localhost:54329; учётные данные в Compose и appsettings предназначены для локального тестового окружения.

## Тесты

```sh
dotnet test backend/SalesDashboard.slnx
dotnet test backend/SalesDashboard.slnx --filter TestCategory=Unit
dotnet test backend/SalesDashboard.slnx --filter TestCategory=Integration
dotnet test backend/SalesDashboard.slnx --filter TestCategory=E2E
```

Unit не требует Docker. Integration/E2E требуют работающий Docker: Testcontainers создаёт отдельные временные PostgreSQL и удаляет их после проверки; данные Compose не затрагиваются. Backend e2e запускает отдельный процесс Kestrel на свободном порту, проверяет HTTP и рестарт. Браузерного сценария dashboard пока нет, поскольку frontend не реализован.

Integration-проверка полного seed сохраняет фактические SQL и EXPLAIN (ANALYZE, BUFFERS) в analytics-query-plans.txt внутри рабочего каталога тестов. Для dashboard проверяется фиксированное число SQL-запросов — 6.

## Миграции и seed

```sh
dotnet tool restore
dotnet ef migrations add Name --project backend/src/SalesDashboard.Api --output-dir Data/Migrations
```

При обычном запуске миграции применять вручную не нужно. Seed: 20 менеджеров, 75 клиентов, 6 категорий, 48 товаров, 3000 продаж за 12 месяцев. Seed__AnchorDate / SEED_ANCHOR_DATE (Compose) задаёт опорную дату; по умолчанию используется бизнес-дата первого запуска. Seed__RandomSeed по умолчанию 20260925, версия генератора 1. Для одинаковых параметров используется стабильный PRNG. Повторный запуск не меняет сохранённые данные. SeedRuns хранит параметры созданного набора, заполнение транзакционно и защищено advisory lock.

## Структура

- Domain — сущности и статусы.
- Data — EF mapping, миграции и генератор seed.
- Features/Analytics — агрегаты и DTO dashboard.
- Features/Sales — ограниченная история продаж.
- Infrastructure — период, валидация и ProblemDetails.
- tests/SalesDashboard.Tests — NUnit категории Unit/Integration/E2E.

Сервисы подключаются через `IAnalyticsService`, `ISalesService` и `IDatabaseInitializer`; конкретные реализации и доменные сущности имеют внутреннюю область видимости.

Контракт: [api-contract.md](../docs/api-contract.md). Правила: [business-rules.md](../docs/business-rules.md).
