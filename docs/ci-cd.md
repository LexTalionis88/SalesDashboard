# CI/CD в GitHub Actions

Workflow находится в `.github/workflows/ci-cd.yml`.

## CI

CI запускается для каждого pull request, push в `main` и вручную через
`workflow_dispatch`.

Проверки выполняются отдельными job:

1. Frontend TypeScript и Vite build.
2. Backend unit-тесты.
3. Backend integration-тесты с PostgreSQL Testcontainers.
4. Backend E2E-тесты.
5. Compose smoke-test с readiness, API и frontend proxy.
6. Browser E2E через Playwright и Chromium.

Для integration/E2E нужен Docker daemon. GitHub-hosted runner предоставляет
Docker и доступ к Docker Hub для тестового PostgreSQL.

## CD

После успешного CI для push в `main` job `publish-images` публикует образы в
GitHub Container Registry:

```text
ghcr.io/<owner>/<repository>/backend:latest
ghcr.io/<owner>/<repository>/frontend:latest
```

Также создаётся тег с коротким SHA коммита. Для публикации используется
встроенный `GITHUB_TOKEN`; дополнительные secrets не нужны.

Workflow не выполняет deployment в конкретное облако или кластер, потому что
deployment target и его credentials не определены в проекте. Публикация образов
является подготовленным CD-этапом для последующего deployment job.

## Настройка репозитория

В GitHub repository settings проверьте:

1. Actions разрешены для репозитория.
2. Workflow permissions позволяют GitHub Actions читать содержимое.
3. Для package publishing разрешено `Read and write permissions` либо workflow
   использует заданное в job разрешение `packages: write`.
4. Ветка `main` защищена обязательным успешным workflow перед merge.

## Локальный повтор

Команды из workflow можно повторить локально:

```sh
npm --prefix frontend ci
npm --prefix frontend run build
dotnet restore backend/SalesDashboard.slnx --locked-mode
dotnet test backend/SalesDashboard.slnx --no-restore
docker compose up -d --build
```
