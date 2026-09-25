# Tech context

Обновлено: 2026-09-25.

## Обязательная технологическая основа

C#, .NET 10, ASP.NET Core, EF Core, REST, async/await; React + TypeScript; PostgreSQL; Docker и Docker Compose. Frontend использует Vite, TanStack Query и Recharts; версии закреплены в package-lock.json. NUnit применяется для unit/integration/e2e backend. Бизнес-правила и архитектурные решения по умолчанию приняты пользователем 2026-09-25.

## Текущее окружение и артефакты

- Рабочая папка: `D:\Develop\AI_dev\!TestForJob`; shell PowerShell, Windows.
- На начало работы в папке находился только исходный PDF; локальных инструкций AGENTS.md не было.
- Для чтения PDF использованы Python с pdfplumber и PyMuPDF/Pillow. Текст извлечён со всех 11 страниц, просмотрены три обзорных изображения страниц.
- Добавлен AGENTS.md для следующих сессий; каталоги сохранены через `.gitkeep`.
- Compose запускает PostgreSQL, .NET 10 backend на localhost:8080, frontend на localhost:3000 и dotnet-monitor на localhost:52323.
- Есть csproj, solution, lock-файлы backend, отдельные сборки Application/DataAccess, Dockerfile и EF migration. Frontend имеет package.json, package-lock.json и Dockerfile.
- Локальный Git-репозиторий инициализирован 2026-09-25, ветка `main`. Материалы фиксируются тематическими коммитами на русском языке. Удалённый репозиторий не настроен, публикация не выполнялась.
- Временные изображения PDF находятся в `tmp/pdfs`, каталог игнорируется Git и не является частью поставки.

Команда запуска: `docker compose up --build`. Dashboard доступен на localhost:3000. Архитектура описана в `docs/architecture.md`.

- Архитектура backend: SalesDashboard.Api -> SalesDashboard.Application -> SalesDashboard.DataAccess; бизнес-логика изолирована в Application.

- Compose включает sidecar dotnet-monitor:10 для локальной диагностики через общий /diag/port.sock; HTTP endpoint monitor — localhost:52323.
