# SalesDashboard.Application

Сборка прикладной бизнес-логики: аналитика, продажи, DTO и интерфейсы сервисов.

Зависимости направлены в одну сторону: `SalesDashboard.Api` использует Application для композиции и HTTP-адаптеров, Application использует `SalesDashboard.DataAccess` для чтения данных. DataAccess не зависит от Application.
