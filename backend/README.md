# Backend

Зарезервирован один ASP.NET Core проект `src/SalesDashboard.Api` и проект тестов `tests/SalesDashboard.Tests`. `.csproj`, solution, entry point, зависимости и Dockerfile ещё не созданы. Обязательный стек: C#, .NET 8+, EF Core, PostgreSQL, REST и async I/O.

| Каталог | Назначение |
| --- | --- |
| `Domain` | Manager, Customer, Category, Product, Sale, SaleItem, SaleStatus |
| `Data` | DbContext и конфигурации таблиц/связей |
| `Data/Migrations` | Версионируемые EF Core миграции |
| `Data/Seed` | Автоматическое первоначальное заполнение и сценарии edge cases |
| `Features/Analytics` | DTO, endpoints, валидация периода и aggregate queries |
| `Features/Sales` | Ограниченная выдача последних продаж с позициями |
| `Infrastructure` | Ошибки, время, readiness и запуск migrations/seed |
| `tests/SalesDashboard.Tests` | KPI, даты, статусы, рейтинг и интеграция с PostgreSQL |

Это организация по папкам внутри одного приложения, а не набор отдельных архитектурных слоёв или микросервисов. См. `docs/architecture.md`.
