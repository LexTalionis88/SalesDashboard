# Протокол проверки

Дата последней проверки: 2026-09-25.

## Автоматические тесты

Команда: `dotnet test backend/SalesDashboard.slnx --no-restore`.

- Unit: 25/25.
- Integration: 12/12, PostgreSQL Testcontainers.
- Backend E2E: 1/1, отдельный Kestrel и PostgreSQL.
- Browser E2E: 3/3, Playwright for .NET, frontend Compose на `http://localhost:3000`.

## Сборка и Compose

- `npm run build` — успешно.
- `docker compose build frontend` — успешно.
- `docker compose ps` — PostgreSQL healthy, backend, frontend и dotnet-monitor запущены.
- `GET http://localhost:3000/` — HTTP 200.
- `GET http://localhost:3000/api/dashboard?from=2026-09-01&to=2026-09-25&rankingBy=grossProfit` — данные dashboard получены через Nginx proxy.
- `GET http://localhost:3000/api/sales?from=2026-09-01&to=2026-09-25&limit=20` — 20 записей получены через Nginx proxy.

## Ограничения

Ручная визуальная проверка на размере 1440×900 не выполнена в текущей сессии: встроенный браузер был недоступен. Автоматические браузерные сценарии выполнены локальным Chromium.
