# Frontend

React + TypeScript + Vite. Данные загружаются из `/api/dashboard` и `/api/sales` через TanStack Query. График строится Recharts. Расчёты показателей и сортировка рейтинга выполняются backend.

## Локальная разработка

```sh
npm ci
npm run dev
```

Vite проксирует `/api` на `http://localhost:8080`. Для этого backend должен быть запущен. Production-сборка: `npm run build`.

## Запуск в Compose

```sh
docker compose up --build
```

Dashboard доступен на `http://localhost:3000`. Nginx в frontend-контейнере проксирует `/api` к backend.

Реализованы выбор периода, сортировка рейтинга, KPI, динамика, лидер периода, категории, товары и история продаж. Запросы отменяются при смене параметров; ошибки и пустые выборки показываются отдельно для аналитики и истории.
