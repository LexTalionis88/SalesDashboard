# System patterns

Обновлено: 2026-09-25. Backend, frontend, proxy и observability v1 реализованы.

- Модульный монолит: ASP.NET Core API, React frontend и PostgreSQL.
- Сборки backend разделены по ответственности: `SalesDashboard.Api` → `SalesDashboard.Application` → `SalesDashboard.DataAccess`.
- Server-side фильтрация и агрегация обязательны; браузер получает агрегаты и ограниченную историю.
- EF Core использует DTO projections, `AsNoTracking`, async I/O и единые правила дат/статусов.
- `SaleItem` хранит исторические цену и себестоимость. Денежные расчёты выполняются через `decimal`.
- Frontend отдаётся Nginx с `/api` proxy и единым origin. Proxy использует Docker DNS resolver, чтобы переживать пересоздание backend.
- OpenTelemetry tracing включает ASP.NET Core, HttpClient и EF Core spans. Compose экспортирует их по OTLP/gRPC в Jaeger.
- Приняты Paid-only аналитика, календарные даты с полуоткрытым SQL-диапазоном, deterministic ranking и бизнес-зона Europe/Moscow.
- Источники правил: `docs/requirements/`, `docs/business-rules.md`, `docs/architecture.md` и `docs/decisions/open-decisions.md`.

При изменении принятого решения обновлять соответствующие документы и memory bank. Не выдавать предложения за реализованное поведение.
