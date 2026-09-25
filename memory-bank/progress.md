# Progress

Обновлено: 2026-09-25.

## Готово

- Анализ 11 страниц ТЗ, требования, принятые бизнес-правила, архитектура, memory bank.
- Git-репозиторий, main; сообщения коммитов на русском.
- .NET 10 backend, EF Core/PostgreSQL, начальная миграция и автоматическая инициализация.
- Seed 20 менеджеров / 75 клиентов / 6 категорий / 48 товаров / 3000 продаж за 12 месяцев. Транзакционность, повторный старт без дублей, сохранение параметров генератора.
- API dashboard/sales/readiness, серверные агрегаты, статусы, периоды, рейтинг, динамика, категории/top-5, общие сравнения.
- DTO, OpenAPI endpoint, документированный контракт, ProblemDetails.
- Dockerfile и Compose для frontend, backend, PostgreSQL и dotnet-monitor.
- NUnit unit, integration и backend e2e. Тесты используют изолированные PostgreSQL-контейнеры.
- Сервисы представлены интерфейсами `IAnalyticsService`, `ISalesService`, `IDatabaseInitializer` в отдельном каталоге `Abstractions/Services`; реализации и доменные сущности переведены во внутреннюю область видимости. Публичные методы интерфейсов и инфраструктурных контрактов имеют русские XML-комментарии.
- Слой доступа к данным вынесен в отдельную сборку `SalesDashboard.DataAccess`; API ссылается на неё без обратной зависимости. DbContext, миграции, seed и доменная модель находятся в DataAccess.

## Проверки

Основной запуск: 34 теста прошли, 0 пропущено (21 unit, 12 integration, 1 backend e2e). Сборка без предупреждений. После добавления SQL-проверки integration запускается повторно; итог зафиксирован в docs/backend-validation.md. E2E проверяет HTTP, параметры/пустые периоды, OpenAPI и сохранение данных после рестарта процесса.

## Не готово

Браузерные автоматические тесты и визуальная проверка ещё не выполнены. Дельта в каждой строке рейтинга пока отсутствует. Production deployment/авторизация вне scope. Общий фактический бюджет 8 часов не измерен.

- Бизнес-логика вынесена в отдельную сборку SalesDashboard.Application; интерфейсы находятся в Application/Abstractions/Services, реализации и DTO — в Application/Features.

- Minimal API заменён на контроллеры DashboardController, SalesController и HealthController; проверка готовности использует IDatabaseReadiness.

- AnalyticsService.GetAsync декомпозирован на отдельные этапы загрузки и сборки результата; вынесены тестируемые методы валидации и формирования DTO.

- Методы AnalyticsService сделаны internal для доступа из тестовой сборки; добавлены unit-тесты в BusinessRulesTests, всего 38 тестов проходят.

- Параметры ограничения выборки EF-запросов передаются переменными (Take(limit), Take(topProductsLimit)), константных offsets в запросах нет.

- SalesService.GetAsync декомпозирован на валидацию лимита, EF-загрузку и чистую сборку DTO; добавлены unit-тесты, всего 43 теста в проекте.

- Unit-тесты структурированы по ответственности: отдельные классы для AnalyticsService и SalesService, а также для DateRange и SeedGenerator.

- Unit, integration и e2e тесты разнесены в отдельные сборки SalesDashboard.UnitTests, SalesDashboard.IntegrationTests и SalesDashboard.E2ETests.

- Очищены пустые каталоги API, оставшиеся после разделения Application/DataAccess и переноса тестов.

- Testcontainers проверены: integration 12 тестов и backend e2e 1 тест проходят при доступном Docker daemon.

- Frontend dashboard реализован на React + TypeScript, Vite, TanStack Query и Recharts. Подключены оба API, фильтры периода и рейтинга, KPI, график, категории, топ товаров и история продаж. Frontend и Nginx proxy работают в Compose на localhost:3000.
- `npm run build` и `docker compose build frontend` проходят; страница и `/api/dashboard` через frontend proxy отвечают HTTP 200.
