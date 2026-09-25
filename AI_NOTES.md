# Заметки об использовании AI

1. Инструмент: Codex / GPT-6; работа начата с анализа 11 страниц PDF.
2. AI выделил требования, противоречия, архитектуру и открытые решения, создал memory bank.
3. Пользователь принял варианты по умолчанию для v1; временное увеличение seed отменил.
4. Пользователь отдельно выбрал .NET 10 и NUnit для unit/integration/e2e; этим заменено предложение xUnit.
5. Запросы пользователя записаны дословно в AI_PROMPTS.md, без служебных сообщений и секретов.
6. AI реализовал backend, миграции, seed, серверные агрегаты, API, Docker и тесты.
7. PostgreSQL-запросы проверены на реальной БД, а не EF InMemory.
8. Проверяются денежные формулы, статусы, даты, ранжирование, пустые данные и seed.
9. E2E запускает отдельный ASP.NET Core процесс и PostgreSQL, проверяет HTTP и рестарт.
10. Первоначальная сериализация ProblemDetails теряла errors производного типа; integration-тесты выявили это, сериализация исправлена.
11. Первый набор NuGet дал конфликт версий EF и уязвимую зависимость OpenAPI; пакеты обновлены и выровнены, сборка проходит без предупреждений.
12. Доступ к dotnet-ef из sandbox потребовал разрешённого запуска инструмента вне sandbox.
13. Результаты автоматических проверок и ограничения отражены в docs/backend-validation.md.
14. Человек ещё не проводил ручной review кода; автоматические проверки не выданы за ручную проверку.
15. Frontend и браузерный e2e не реализованы; backend e2e не объявлен тестом готового dashboard.
16. Общий фактический timebox не измерен; не заявляется соблюдение восьми часов.
17. По замечанию пользователя сервисные зависимости вынесены в интерфейсы, реализации и доменные типы сужены до internal, публичные методы дополнены русскими XML-комментариями.
18. По дополнительному замечанию интерфейсы перенесены из каталогов реализаций в отдельный `Abstractions/Services`; namespaces, DI, тесты и документация обновлены.
19. По запросу пользователя слой доступа к данным вынесен в отдельную сборку `SalesDashboard.DataAccess`; API ссылается на неё без обратной зависимости, миграции перенастроены на эту сборку.

- 2026-09-25: бизнес-логика вынесена в SalesDashboard.Application; зависимости направлены Api -> Application -> DataAccess.

- 2026-09-25: HTTP endpoints переведены с Minimal API на контроллеры DashboardController, SalesController и HealthController; readiness предоставляется через IDatabaseReadiness.

- 2026-09-25: в Compose добавлен sidecar mcr.microsoft.com/dotnet/monitor:10; backend и monitor используют общий diagnostic socket /diag/port.sock, endpoint monitor опубликован на localhost:52323.

- 2026-09-25: проверена кодировка русских строк и XML-комментариев; исправлены mojibake в комментариях и сообщениях валидации сервисов Analytics/Sales.

- 2026-09-25: AnalyticsService.GetAsync разделён на валидацию, загрузку менеджеров, серию, категории, продукты и сборку DashboardDto; чистые этапы ValidateRanking и BuildDashboard доступны для unit-тестов.

- 2026-09-25: методы AnalyticsService декомпозированы и переведены в internal; добавлены NUnit unit-тесты ValidateRanking и BuildDashboard, общий набор тестов вырос до 38.

- 2026-09-25: константный Take(5) для top products заменён на Take(topProductsLimit), где limit передаётся в метод как переменная и параметризуется EF; Sales уже использовал Take(limit). Skip в backend отсутствует.

- 2026-09-25: SalesService.GetAsync разделён на ValidateLimit, LoadSalesAsync и BuildSalesDto; добавлены NUnit unit-тесты лимита и преобразования продаж.

- 2026-09-25: BusinessRulesTests разделён на AnalyticsServiceTests, SalesServiceTests, DateRangeTests и SeedGeneratorTests; добавлен общий FixedClock.

- 2026-09-25: тесты разнесены по сборкам SalesDashboard.UnitTests, SalesDashboard.IntegrationTests и SalesDashboard.E2ETests; старый объединённый тестовый проект удалён из solution.

- 2026-09-25: проверены каталоги backend без bin/obj; удалены пустые остаточные папки API Domain, Abstractions, Data и Features после выноса слоёв.

- 2026-09-25: Testcontainers проверены с доступом к Docker daemon: IntegrationTests 12/12, E2ETests 1/1; временные PostgreSQL контейнеры завершены тестовым lifecycle.

- 2026-09-25: реализован frontend dashboard с реальными API на React, TypeScript, Vite, TanStack Query и Recharts; контейнер Nginx проксирует API. Успешны сборки npm и Docker, HTTP-проверка страницы и proxy. Встроенный браузер для визуальной проверки оказался недоступен.

- 2026-09-25: по запросу «делай» добавлены браузерные E2E на NUnit и Playwright for .NET: загрузка dashboard, смена периода/рейтинга и empty state. Chromium установлен локально, 3/3 browser E2E проходят; полный solution прогон: Unit 25, Integration 12, E2E 4.
