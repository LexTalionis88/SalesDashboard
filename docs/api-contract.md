# API v1

Реализован backend на .NET 10. Машиночитаемая схема доступна через `GET /openapi/v1.json`; её сохранённая копия — [openapi.json](openapi.json). Контракты задаются DTO в `backend/src/SalesDashboard.Application/Features/`.

## Общие правила

- JSON использует camelCase; отсутствующие значения возвращаются явным `null`, коллекции — массивами.
- Деньги — десятичные строки с точкой, минимум двумя знаками, без разделителей тысяч. Выручка и прибыль точны до копейки; средний чек может содержать дополнительные знаки деления decimal. Округление до двух знаков выполняет UI для показа.
- `margin` — число-доля (0.36 = 36%), `*Percent` — проценты, `marginPoints` — процентные пункты.
- `salesCount`, `quantity` — целые; идентификаторы — целые. Валюта RUB, бизнес-зона Europe/Moscow.
- `soldAt` — ISO timestamp UTC с `Z`; `from`, `to`, `date` — YYYY-MM-DD.
- SQL-фильтр: `[начало from в бизнес-зоне; начало дня после to)`, преобразованный в UTC.
- API read-only, авторизация и CRUD отсутствуют по scope ТЗ.

## Период для обоих маршрутов

| Параметр | Правило |
| --- | --- |
| `from`, `to` | Передаются вместе, обе даты включены |
| `preset` | Альтернатива from/to: `today`, `last7Days`, `last30Days`, `thisMonth`, `lastMonth` |
| Без параметров периода | `last30Days` по текущей бизнес-дате сервера |

Нельзя смешивать preset и from/to. Даты строго YYYY-MM-DD, from ≤ to. Технический предел выдачи ежедневного ряда — 3660 дней; допустимые годы 1900–9998. Эти ограничения — решение реализации для ограничения размера ответа, не исходное требование ТЗ. Исторический диапазон не ограничен датами seed: пустые периоды допустимы.

## GET /api/dashboard

Дополнительно `rankingBy=grossProfit|averageCheck`, по умолчанию grossProfit. Все вычисления выполняются backend; материализуются агрегаты, а не тысячи продаж. Одна выдача использует шесть SQL-запросов независимо от количества менеджеров.

| Поле | Содержание |
| --- | --- |
| `period` | `{from, to, timeZone}` — фактически разрешённый период |
| `currency`, `rankingBy` | RUB и применённый режим |
| `kpis` | revenue, cost, grossProfit, salesCount, averageCheck, margin, bestManager |
| `ranking[]` | rank (null без Paid-продаж), manager, metrics |
| `series[]` | date, revenue, grossProfit, salesCount; каждый календарный день, включая нули |
| `categories[]` | id, name, revenue, grossProfit, salesCount, quantity; только категории с Paid-продажами, Revenue DESC / Id ASC |
| `topProducts[]` | id, name, categoryId, revenue, grossProfit, salesCount, quantity; максимум 5, GrossProfit DESC / Id ASC |
| `comparison` | period, metrics, change предыдущего периода такой же календарной длительности |

`metrics`: revenue, cost, grossProfit, salesCount, averageCheck, margin. `manager`: id, name, team, position, isActive, initials, avatarUrl (nullable). `change`: revenuePercent, grossProfitPercent, salesCountPercent, averageCheckPercent, marginPoints; изменения могут быть null при отсутствии базы.

Лучший менеджер определяется по прибыли независимо от rankingBy. Равные результаты разрешаются по ManagerId; менеджеры без продаж внизу без ранга. Неактивные менеджеры с историческими продажами участвуют. Сравнение реализовано для общих KPI; изменение в каждой строке рейтинга пока не выводится.

Пример фрагмента KPI для контрольной выборки:

```json
{
  "revenue": "500.00",
  "cost": "320.00",
  "grossProfit": "180.00",
  "salesCount": 2,
  "averageCheck": "250.00",
  "margin": 0.36
}
```

В пустом периоде суммы и count нулевые, averageCheck/margin/bestManager null; ranking содержит менеджеров без ранга, series — нулевые дни, categories/topProducts — пустые массивы. Общий SalesCount нельзя получать суммированием категориальных counts.

## GET /api/sales

`limit` — целое 1–100, по умолчанию 20. Сортировка SoldAt DESC / Id DESC, все статусы, тот же период.

Ответ: `{period, currency, limit, items}`. Элемент items:

```json
{
  "id": 42,
  "soldAt": "2026-09-10T09:00:00Z",
  "manager": { "id": 1, "name": "Анна", "team": "Продажи", "position": "Менеджер", "isActive": true, "initials": "А", "avatarUrl": null },
  "customer": { "id": 1, "name": "Иван", "company": "Компания", "segment": "SMB" },
  "status": "Refunded",
  "includedInKpis": false,
  "amount": "200.00",
  "grossProfit": "80.00",
  "items": [{ "productId": 1, "productName": "Камера", "quantity": 2, "unitPrice": "100.00", "unitCost": "60.00" }]
}
```

Для Cancelled/Refunded суммы исходные, includedInKpis=false. Вложенные товары загружаются одним ограниченным SQL-запросом, без запроса на каждую продажу. Пагинация кроме ограничения последних строк пока не требуется.

## Ошибки и readiness

Неверные параметры → 400 `application/problem+json`:

```json
{
  "title": "Некорректные параметры запроса",
  "status": 400,
  "instance": "/api/sales",
  "errors": { "limit": ["Допустимы значения от 1 до 100."] },
  "traceId": "идентификатор запроса"
}
```

Непредвиденные исключения → 500 ProblemDetails с OpenTelemetry traceId (или HTTP TraceIdentifier, если tracing отключён), без stack trace/SQL. Поле type может добавляться платформой. Неизвестный маршрут → 404.

`GET /api/health/ready`: 200 `{"status":"ready"}` при доступной БД, 503 при недоступной. HTTP-сервер начинает слушать после успешных migrations/seed; ошибка инициализации завершает процесс и видна в логах.
