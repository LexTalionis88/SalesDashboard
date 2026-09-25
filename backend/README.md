# Backend

ASP.NET Core / .NET 10, EF Core 10, PostgreSQL. Р”РѕСЃС‚СѓРї Рє РґР°РЅРЅС‹Рј РІС‹РґРµР»РµРЅ РІ РѕС‚РґРµР»СЊРЅСѓСЋ СЃР±РѕСЂРєСѓ `SalesDashboard.DataAccess`; API Р·Р°РІРёСЃРёС‚ РѕС‚ РЅРµС‘, РѕР±СЂР°С‚РЅРѕР№ Р·Р°РІРёСЃРёРјРѕСЃС‚Рё РЅРµС‚. NUnit РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ РґР»СЏ unit, integration Рё backend e2e. Р’РµСЂСЃРёРё Р·Р°РєСЂРµРїР»РµРЅС‹ РІ csproj Рё packages.lock.json; РѕСЃРЅРѕРІР°РЅРёРµ вЂ” [ADR-001](../docs/decisions/ADR-001-backend-stack.md).

## Р—Р°РїСѓСЃРє РёР· РєРѕСЂРЅСЏ СЂРµРїРѕР·РёС‚РѕСЂРёСЏ

```sh
docker compose up --build
```

API: http://localhost:8080/api/dashboard. OpenAPI: http://localhost:8080/openapi/v1.json. Readiness: http://localhost:8080/api/health/ready. Docker Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРё СЃРѕР·РґР°С‘С‚ СЃС…РµРјСѓ Рё seed. Frontend РїРѕРєР° РЅРµ СЂРµР°Р»РёР·РѕРІР°РЅ.

Р”Р»СЏ Р»РѕРєР°Р»СЊРЅРѕР№ СЂР°Р·СЂР°Р±РѕС‚РєРё СЃ .NET 10 SDK:

```sh
docker compose up -d postgres
dotnet restore backend/SalesDashboard.slnx --locked-mode
dotnet run --project backend/src/SalesDashboard.Api --no-launch-profile --urls http://localhost:8080
```

Р›РѕРєР°Р»СЊРЅС‹Р№ API Рё РєРѕРЅС‚РµР№РЅРµСЂРЅС‹Р№ API РёСЃРїРѕР»СЊР·СѓСЋС‚ РѕРґРёРЅ РїРѕСЂС‚, Р·Р°РїСѓСЃРєР°С‚СЊ РѕРґРёРЅ РІР°СЂРёР°РЅС‚ Р·Р° СЂР°Р·. PostgreSQL РґРѕСЃС‚СѓРїРµРЅ РЅР° localhost:54329; СѓС‡С‘С‚РЅС‹Рµ РґР°РЅРЅС‹Рµ РІ Compose Рё appsettings РїСЂРµРґРЅР°Р·РЅР°С‡РµРЅС‹ РґР»СЏ Р»РѕРєР°Р»СЊРЅРѕРіРѕ С‚РµСЃС‚РѕРІРѕРіРѕ РѕРєСЂСѓР¶РµРЅРёСЏ.

## РўРµСЃС‚С‹

```sh
dotnet test backend/SalesDashboard.slnx
dotnet test backend/SalesDashboard.slnx --filter TestCategory=Unit
dotnet test backend/SalesDashboard.slnx --filter TestCategory=Integration
dotnet test backend/SalesDashboard.slnx --filter TestCategory=E2E
```

Unit РЅРµ С‚СЂРµР±СѓРµС‚ Docker. Integration/E2E С‚СЂРµР±СѓСЋС‚ СЂР°Р±РѕС‚Р°СЋС‰РёР№ Docker: Testcontainers СЃРѕР·РґР°С‘С‚ РѕС‚РґРµР»СЊРЅС‹Рµ РІСЂРµРјРµРЅРЅС‹Рµ PostgreSQL Рё СѓРґР°Р»СЏРµС‚ РёС… РїРѕСЃР»Рµ РїСЂРѕРІРµСЂРєРё; РґР°РЅРЅС‹Рµ Compose РЅРµ Р·Р°С‚СЂР°РіРёРІР°СЋС‚СЃСЏ. Backend e2e Р·Р°РїСѓСЃРєР°РµС‚ РѕС‚РґРµР»СЊРЅС‹Р№ РїСЂРѕС†РµСЃСЃ Kestrel РЅР° СЃРІРѕР±РѕРґРЅРѕРј РїРѕСЂС‚Сѓ, РїСЂРѕРІРµСЂСЏРµС‚ HTTP Рё СЂРµСЃС‚Р°СЂС‚. Р‘СЂР°СѓР·РµСЂРЅРѕРіРѕ СЃС†РµРЅР°СЂРёСЏ dashboard РїРѕРєР° РЅРµС‚, РїРѕСЃРєРѕР»СЊРєСѓ frontend РЅРµ СЂРµР°Р»РёР·РѕРІР°РЅ.

Integration-РїСЂРѕРІРµСЂРєР° РїРѕР»РЅРѕРіРѕ seed СЃРѕС…СЂР°РЅСЏРµС‚ С„Р°РєС‚РёС‡РµСЃРєРёРµ SQL Рё EXPLAIN (ANALYZE, BUFFERS) РІ analytics-query-plans.txt РІРЅСѓС‚СЂРё СЂР°Р±РѕС‡РµРіРѕ РєР°С‚Р°Р»РѕРіР° С‚РµСЃС‚РѕРІ. Р”Р»СЏ dashboard РїСЂРѕРІРµСЂСЏРµС‚СЃСЏ С„РёРєСЃРёСЂРѕРІР°РЅРЅРѕРµ С‡РёСЃР»Рѕ SQL-Р·Р°РїСЂРѕСЃРѕРІ вЂ” 6.

## РњРёРіСЂР°С†РёРё Рё seed

```sh
dotnet tool restore
dotnet ef migrations add Name --project backend/src/SalesDashboard.DataAccess --startup-project backend/src/SalesDashboard.Api --output-dir Data/Migrations
```

РџСЂРё РѕР±С‹С‡РЅРѕРј Р·Р°РїСѓСЃРєРµ РјРёРіСЂР°С†РёРё РїСЂРёРјРµРЅСЏС‚СЊ РІСЂСѓС‡РЅСѓСЋ РЅРµ РЅСѓР¶РЅРѕ. Seed: 20 РјРµРЅРµРґР¶РµСЂРѕРІ, 75 РєР»РёРµРЅС‚РѕРІ, 6 РєР°С‚РµРіРѕСЂРёР№, 48 С‚РѕРІР°СЂРѕРІ, 3000 РїСЂРѕРґР°Р¶ Р·Р° 12 РјРµСЃСЏС†РµРІ. Seed__AnchorDate / SEED_ANCHOR_DATE (Compose) Р·Р°РґР°С‘С‚ РѕРїРѕСЂРЅСѓСЋ РґР°С‚Сѓ; РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ Р±РёР·РЅРµСЃ-РґР°С‚Р° РїРµСЂРІРѕРіРѕ Р·Р°РїСѓСЃРєР°. Seed__RandomSeed РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ 20260925, РІРµСЂСЃРёСЏ РіРµРЅРµСЂР°С‚РѕСЂР° 1. Р”Р»СЏ РѕРґРёРЅР°РєРѕРІС‹С… РїР°СЂР°РјРµС‚СЂРѕРІ РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ СЃС‚Р°Р±РёР»СЊРЅС‹Р№ PRNG. РџРѕРІС‚РѕСЂРЅС‹Р№ Р·Р°РїСѓСЃРє РЅРµ РјРµРЅСЏРµС‚ СЃРѕС…СЂР°РЅС‘РЅРЅС‹Рµ РґР°РЅРЅС‹Рµ. SeedRuns С…СЂР°РЅРёС‚ РїР°СЂР°РјРµС‚СЂС‹ СЃРѕР·РґР°РЅРЅРѕРіРѕ РЅР°Р±РѕСЂР°, Р·Р°РїРѕР»РЅРµРЅРёРµ С‚СЂР°РЅР·Р°РєС†РёРѕРЅРЅРѕ Рё Р·Р°С‰РёС‰РµРЅРѕ advisory lock.

## РЎС‚СЂСѓРєС‚СѓСЂР°

- Domain вЂ” СЃСѓС‰РЅРѕСЃС‚Рё Рё СЃС‚Р°С‚СѓСЃС‹.
- `SalesDashboard.DataAccess` вЂ” РѕС‚РґРµР»СЊРЅР°СЏ СЃР±РѕСЂРєР° EF mapping, РјРёРіСЂР°С†РёР№, РґРѕРјРµРЅРЅРѕР№ РјРѕРґРµР»Рё Рё seed; РµС‘ РїСѓР±Р»РёС‡РЅС‹Р№ РєРѕРЅС‚СЂР°РєС‚ РёРЅРёС†РёР°Р»РёР·Р°С†РёРё РЅР°С…РѕРґРёС‚СЃСЏ РІ `Abstractions/Services`.
- `SalesDashboard.DataAccess` вЂ” РѕС‚РґРµР»СЊРЅР°СЏ СЃР±РѕСЂРєР° СЃР»РѕСЏ РґРѕСЃС‚СѓРїР° Рє РґР°РЅРЅС‹Рј; РµС‘ РїСѓР±Р»РёС‡РЅС‹Р№ РєРѕРЅС‚СЂР°РєС‚ РёРЅРёС†РёР°Р»РёР·Р°С†РёРё РЅР°С…РѕРґРёС‚СЃСЏ РІ `Abstractions/Services`.
- Features/Analytics вЂ” Р°РіСЂРµРіР°С‚С‹ Рё DTO dashboard.
- Features/Sales вЂ” РѕРіСЂР°РЅРёС‡РµРЅРЅР°СЏ РёСЃС‚РѕСЂРёСЏ РїСЂРѕРґР°Р¶.
- Infrastructure вЂ” РїРµСЂРёРѕРґ, РІР°Р»РёРґР°С†РёСЏ Рё ProblemDetails.
- tests/SalesDashboard.Tests вЂ” NUnit РєР°С‚РµРіРѕСЂРёРё Unit/Integration/E2E.

РЎРµСЂРІРёСЃРЅС‹Рµ РєРѕРЅС‚СЂР°РєС‚С‹ РЅР°С…РѕРґСЏС‚СЃСЏ РІ `src/SalesDashboard.Api/Abstractions/Services`; СЂРµР°Р»РёР·Р°С†РёРё вЂ” РІ `Features` Рё `Data/Seed`. РљРѕРЅРєСЂРµС‚РЅС‹Рµ СЂРµР°Р»РёР·Р°С†РёРё Рё РґРѕРјРµРЅРЅС‹Рµ СЃСѓС‰РЅРѕСЃС‚Рё РёРјРµСЋС‚ РІРЅСѓС‚СЂРµРЅРЅСЋСЋ РѕР±Р»Р°СЃС‚СЊ РІРёРґРёРјРѕСЃС‚Рё.

РљРѕРЅС‚СЂР°РєС‚: [api-contract.md](../docs/api-contract.md). РџСЂР°РІРёР»Р°: [business-rules.md](../docs/business-rules.md).


## dotnet monitor

docker compose поднимает sidecar dotnet-monitor на http://localhost:52323; backend и monitor используют общий /diag/port.sock.

