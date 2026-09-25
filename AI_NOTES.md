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

- 2026-09-25: ������-������ �������� � SalesDashboard.Application; ����������� ���������� Api -> Application -> DataAccess.

- 2026-09-25: HTTP endpoints ���������� � Minimal API �� ����������� DashboardController, SalesController � HealthController; readiness ��������������� ����� IDatabaseReadiness.

- 2026-09-25: � Compose �������� sidecar mcr.microsoft.com/dotnet/monitor:10; backend � monitor ���������� ����� diagnostic socket /diag/port.sock, endpoint monitor ����������� �� localhost:52323.

- 2026-09-25: ��������� ��������� ������� ����� � XML-������������; ���������� mojibake � ������������ � ���������� ��������� �������� Analytics/Sales.

- 2026-09-25: AnalyticsService.GetAsync ������� �� ���������, �������� ����������, �����, ���������, �������� � ������ DashboardDto; ������ ����� ValidateRanking � BuildDashboard �������� ��� unit-������.

- 2026-09-25: ������ AnalyticsService ��������������� � ���������� � internal; ��������� NUnit unit-����� ValidateRanking � BuildDashboard, ����� ����� ������ ����� �� 38.

- 2026-09-25: ����������� Take(5) ��� top products ������� �� Take(topProductsLimit), ��� limit ��������� � ����� ��� ���������� � ��������������� EF; Sales ��� ����������� Take(limit). Skip � backend �����������.

- 2026-09-25: SalesService.GetAsync ������� �� ValidateLimit, LoadSalesAsync � BuildSalesDto; ��������� NUnit unit-����� ������ � �������������� ������.

- 2026-09-25: BusinessRulesTests ������� �� AnalyticsServiceTests, SalesServiceTests, DateRangeTests � SeedGeneratorTests; �������� ����� FixedClock.

- 2026-09-25: ����� ��������� �� ������� SalesDashboard.UnitTests, SalesDashboard.IntegrationTests � SalesDashboard.E2ETests; ������ ������������ �������� ������ ����� �� solution.
