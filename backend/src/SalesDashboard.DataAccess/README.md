# SalesDashboard.DataAccess

Отдельная сборка слоя доступа к данным для backend.

Содержит EF Core `SalesDbContext`, PostgreSQL-миграции, доменные сущности, воспроизводимый seed и инициализацию базы. Публичный контракт инициализации находится в `Abstractions/Services`; реализация и доменные типы внутренние.

Сборка не зависит от `SalesDashboard.Api`. API зависит от неё через ссылку на проект.

```sh
dotnet ef migrations add Name --project backend/src/SalesDashboard.DataAccess --startup-project backend/src/SalesDashboard.Api --output-dir Data/Migrations
```
