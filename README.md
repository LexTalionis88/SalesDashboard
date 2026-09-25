# Sales Performance Dashboard

Тестовое задание DJI-Market.ru. **Backend реализован; frontend пока каркас.** .NET 10, ASP.NET Core, EF Core, PostgreSQL; тесты unit/integration/e2e — NUnit по указанию пользователя.

## Запуск

```sh
docker compose up --build
```

Команда поднимает PostgreSQL и backend, автоматически применяет migrations и создаёт seed. Требуется Docker с Compose; .NET SDK на хосте для контейнерного запуска не нужен.

- API: http://localhost:8080/api/dashboard
- Последние продажи: http://localhost:8080/api/sales
- Readiness: http://localhost:8080/api/health/ready
- OpenAPI: http://localhost:8080/openapi/v1.json

Это пока API, готовой страницы dashboard нет. React + TypeScript и frontend proxy — следующий этап. Детали разработки и тестов: [backend/README.md](backend/README.md).

## Документация

- [Функциональные требования](docs/requirements/functional.md) и [нефункциональные требования](docs/requirements/non-functional.md).
- [Архитектура](docs/architecture.md), [бизнес-правила](docs/business-rules.md), [API-контракт](docs/api-contract.md).
- [Реестр решений](docs/decisions/open-decisions.md), [решение .NET 10/NUnit](docs/decisions/ADR-001-backend-stack.md), [анализ ТЗ](docs/spec-review.md).
- [План реализации](docs/implementation-plan.md), [план проверки](docs/verification-plan.md), [memory bank](memory-bank/README.md).
- [AI_PROMPTS](AI_PROMPTS.md), [AI_NOTES](AI_NOTES.md), [исходное ТЗ](Тестовое_задание_Sales_Performance_Dashboard.pdf).

## Реализованный backend

Модель Manager/Customer/Category/Product/Sale/SaleItem; EF миграция с FK, денежными типами, ограничениями и индексами; воспроизводимый seed; KPI, рейтинг по прибыли/среднему чеку, динамика по дням, категории, top-5 товаров, сравнение общих KPI с предыдущим периодом и последние продажи. Вычисления выполняются сервером, основные агрегации — PostgreSQL. Ошибки параметров возвращают ProblemDetails.

## Правила v1

В KPI входят только Paid. Cancelled/Refunded отображаются в истории с исходными суммами и includedInKpis=false. Возврат полный, его статус изменяет исходный период продажи. Revenue = Σ(quantity × unitPrice), Cost = Σ(quantity × unitCost), GrossProfit = Revenue − Cost, Margin = GrossProfit / Revenue, AverageCheck = Revenue / число уникальных Paid-продаж. Нулевые знаменатели дают null.

Бизнес-зона Europe/Moscow, даты включены; SQL использует полуоткрытый интервал в UTC. Предыдущий период — столько же календарных дней перед from. Лучший менеджер определяется по прибыли; ничьи разрешаются по ManagerId, менеджеры без продаж не получают место. Деньги в JSON — строки, доли — числа. Детали — в бизнес-правилах и API-контракте.

Seed: **20 менеджеров, 75 клиентов, 6 категорий, 48 товаров, 3000 продаж за 12 месяцев**. Одинаковые seed/anchorDate/версия дают одинаковые данные. Повторный запуск не дублирует данные. Увеличение объёма в 10 раз отменено пользователем.

## Технические решения

Один backend-проект с модулями Analytics/Sales, EF DTO projections и async I/O. Индексы: Sales(SoldAt DESC, Id DESC), частичный Sales(SoldAt, ManagerId) для Paid, FK-индексы SaleItems(SaleId/ProductId), Products(CategoryId), Sales(ManagerId/CustomerId), уникальный Product.Sku. Миграция хранится в Git. Шесть запросов dashboard, ограниченная выдача истории, без N+1. NUnit integration выполняет EXPLAIN на реальном PostgreSQL.

## Проверка

```sh
dotnet test backend/SalesDashboard.slnx
```

Для тестов нужны .NET 10 SDK и Docker. Unit можно запустить отдельно без Docker. Тестовые контейнеры изолированы от данных Compose. Подробности — в backend/README.md.

## Незавершённое и дальнейшее развитие

Не реализованы frontend, визуальные состояния/анимации, браузерные проверки и сравнение предыдущего периода в строках рейтинга (общие KPI уже сравниваются). Общий ориентир ТЗ — 8 часов фактической работы; точное накопленное время не измерялось, исчерпание бюджета не заявляется.

Для production: история событий возврата, метрики/трассировка, резервное копирование, проверка больших объёмов и нагрузочные цели. Авторизация, mobile, admin panel, Kubernetes и облачное развёртывание исключены из текущего ТЗ.
