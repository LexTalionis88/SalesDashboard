# System patterns

Обновлено: 2026-09-25. Основа v1 принята пользователем; механизмы ещё не реализованы.

- Модульный монолит: один ASP.NET Core проект, React приложение, PostgreSQL. Модули Analytics и Sales внутри backend.
- Server-side фильтрация/агрегация — обязательное требование ТЗ; браузер получает агрегаты и ограниченный список истории.
- EF Core DTO projections, async I/O, единые правила дат/статусов; без универсального Repository и CQRS без необходимости.
- SaleItem хранит исторические цену/себестоимость. Денежные расчёты на decimal. SalesCount не равен числу позиций.
- Принят frontend static server с `/api` proxy и единым origin; migrations/seed автоматически до backend readiness.
- Принята Paid-only аналитика, календарные даты с полуоткрытым SQL-диапазоном, deterministic ranking. Бизнес-зона Europe/Moscow. Эти бизнес-решения ACCEPTED.
- Memory bank — указатель на контекст; источники правил — `docs/requirements/`, `docs/business-rules.md`, `docs/architecture.md` и реестр D-*.

Подробности: `docs/architecture.md`. При смене принятого решения обновить документ и `docs/decisions/open-decisions.md`; не выдавать этот список за факт реализации.
