# Tech context

Обновлено: 2026-09-25.

## Обязательная технологическая основа

C#, .NET 8+, ASP.NET Core, EF Core, REST, async/await; React + TypeScript; PostgreSQL; Docker и Docker Compose. Для v1 принята основа Vite + TanStack Query + Recharts и NUnit для unit/integration/e2e по прямому указанию пользователя; точные версии и конкретный UI-набор ещё не выбраны (D-10, PARTIALLY ACCEPTED). Бизнес-правила и архитектурные решения по умолчанию приняты пользователем 2026-09-25; backend реализован.

## Текущее окружение и артефакты

- Рабочая папка: `D:\Develop\AI_dev\!TestForJob`; shell PowerShell, Windows.
- На начало работы в папке находился только исходный PDF; локальных инструкций AGENTS.md не было.
- Для чтения PDF использованы Python с pdfplumber и PyMuPDF/Pillow. Текст извлечён со всех 11 страниц, просмотрены три обзорных изображения страниц.
- Добавлен AGENTS.md для следующих сессий; каталоги сохранены через `.gitkeep`.
- Compose запускает PostgreSQL и .NET 10 backend на localhost:8080; frontend пока отсутствует.
- Есть csproj, solution, lock-файлы backend, отдельная сборка `SalesDashboard.DataAccess`, Dockerfile и EF migration. .NET SDK 10.0.401 и Docker доступны; версии — ADR-001. Frontend package.json ещё отсутствует.
- Локальный Git-репозиторий инициализирован 2026-09-25, ветка `main`. Материалы фиксируются тематическими коммитами на русском языке. Удалённый репозиторий не настроен, публикация не выполнялась.
- Временные изображения PDF находятся в `tmp/pdfs`, каталог игнорируется Git и не является частью поставки.

Будущая команда запуска по ТЗ: `docker compose up --build`. Проверка backend-запуска описана в docs/backend-validation.md; полный UI ещё не реализован. Архитектурные предложения — `docs/architecture.md`.

- ����������� backend: SalesDashboard.Api -> SalesDashboard.Application -> SalesDashboard.DataAccess; ������-������ ����������� � Application.

- Compose �������� sidecar dotnet-monitor:10 ��� ��������� ����������� ����� ����� /diag/port.sock; HTTP endpoint monitor � localhost:52323.
