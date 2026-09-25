# ADR-001: .NET 10 и NUnit

Дата: 2026-09-25. Статус: **ACCEPTED**. Уточняет D-10 и D-15.

## Основание

Пользователь: «Для бека используй .net 10. Для тестов (unit, integration, e2e) - NUnit. Зафиксируй это решение и можешь начинать реализовывать бэк».

## Решение

- Backend — ASP.NET Core с target framework `net10.0`, EF Core 10 и PostgreSQL.
- NUnit — единый test framework для unit, integration и e2e. Предыдущее предложение xUnit заменено этим решением.
- В текущем backend тесты организованы в одном проекте с категориями `Unit`, `Integration`, `E2E`.
- Integration использует настоящую PostgreSQL в Testcontainers и HTTP через WebApplicationFactory.
- Backend e2e запускает отдельный процесс ASP.NET Core/Kestrel и PostgreSQL, проверяет HTTP-сценарий и перезапуск. Это не браузерный e2e готового frontend: UI ещё не реализован. Будущие браузерные e2e также должны запускаться NUnit, например с Playwright for .NET.
- `IAnalyticsService`, `ISalesService` и `IDatabaseInitializer` являются публичными контрактами сервисов; реализации и доменные типы имеют внутреннюю область видимости. Это позволяет тестировать зависимости через интерфейсы и не расширять публичную поверхность приложения.

## Зафиксированные зависимости

SDK на машине и в Docker: 10.0.401; runtime Docker: 10.0.12; EF Core/ASP.NET OpenAPI/Mvc.Testing: 10.0.12; Npgsql EF provider: 10.0.3; NUnit: 4.3.2; NUnit3TestAdapter: 5.0.0; Microsoft.NET.Test.Sdk: 17.14.0; Testcontainers.PostgreSql: 4.15.0; PostgreSQL image: 17.6-alpine.

Версии пакетов закреплены в csproj и packages.lock.json; локальный dotnet-ef — 10.0.12 в dotnet-tools.json. `global.json` допускает установленный feature band .NET 10 через latestFeature; контейнер сборки закреплён точнее.

Использованы официальные сведения о [Npgsql EF Core 10](https://www.npgsql.org/efcore/release-notes/10.0.html) и [запуске NUnit через dotnet test](https://docs.nunit.org/articles/nunit/getting-started/dotnet-core-and-dotnet-standard.html).
