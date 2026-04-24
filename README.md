# Warehouse Inventory System

Warehouse Inventory System is an ASP.NET Core 8 Razor Pages application for managing products, warehouses, stock movements, expiry tracking, dashboard metrics, and operational reports.

## Current Application Status

The current implementation focuses on a fully navigable demo application with database-backed CRUD and reporting on the main operational pages.

This means:

- Changes made in the UI are persisted in the configured PostgreSQL database.
- Dashboard and reporting tiles are wired to the same EF Core data used by Products, Warehouses, and Stock pages.
- The app can automatically create its schema and seed initial demo data when the database is available.
- The app is suitable for demo, prototyping, UI iteration, and workflow validation with persisted data.

## Implemented Features

- **Dashboard**
  - KPI tiles for Products, Warehouses, Total Stock, and Expiring Soon
  - Live counts based on the current persisted database records
  - Expiry email form for products expiring tomorrow

- **Products Page**
  - Add, edit, and delete products
  - Product fields include group, barcode, catalogue, warehouse, type, condition, date, expiry date, and active status
  - Search and client-side filters
  - Reset filter workflow

- **Warehouses Page**
  - Add, edit, and delete warehouses
  - Name and address management
  - Warehouse totals reflected on the dashboard

- **Stock Page**
  - Warehouse filter and search
  - Stock edit modal with Stock In / Stock Out actions
  - Quantity validation and negative-stock prevention
  - Filter state preserved after stock edits

- **Reports Page**
  - KPI cards for total products, total stock, low stock items, and expiring items
  - Report tabs for Stock Levels, Low Stock, Expiry Dates, and Stock Movements
  - Filters for date range, warehouse, and product type
  - CSV export for the currently selected report

- **Email**
  - Expiry email sending via configured SMTP settings

## Tech Stack

- **Framework**: ASP.NET Core 8 Razor Pages
- **Language**: C#
- **UI**: Bootstrap 5
- **Testing**: xUnit, FluentAssertions, Moq
- **Deployment**: Docker and Docker Compose
- **Email**: MailKit / configured email service

## Important Data Behavior

The current UI pages now use the configured database through Entity Framework Core.

This is important to understand:

- Adding, editing, and deleting data persists to the configured database.
- On first successful startup against an empty database, the app auto-creates schema and seeds demo records.
- Dashboard and Reports are aligned with the same persisted records used by the operational pages.

## Main Pages

- `/` - Dashboard
- `/Products` - Product management
- `/Warehouses` - Warehouse management
- `/Stock` - Stock management
- `/Reports` - Reports and CSV export

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or another .NET-capable IDE
- Docker Desktop (optional, for Docker-based runs)

## Running the Application

### Local Development

1. Clone the repository
2. Open `WarehouseInventory.sln`
3. Review `src/WarehouseInventory.Web/appsettings.json`
4. Ensure PostgreSQL is reachable with the configured `DefaultConnection`
5. Run the web project
6. Open `http://localhost:5000`

### Docker

1. From the project root, run:

```bash
docker-compose -f docker/docker-compose.yml up -d
```

2. Open:

```text
http://localhost:5000
```

## Project Structure

```text
WarehouseInventory.sln
├── src/
│   ├── WarehouseInventory.Domain/
│   ├── WarehouseInventory.Application/
│   ├── WarehouseInventory.Infrastructure/
│   └── WarehouseInventory.Web/
├── tests/
│   ├── WarehouseInventory.UnitTests/
│   └── WarehouseInventory.IntegrationTests/
├── docs/
│   └── deployment-guide.md
├── docker/
└── README.md
```

## Configuration

### Database Connection

Configure PostgreSQL in `src/WarehouseInventory.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=warehouseinventory;Username=postgres;Password=yourpassword"
  }
}
```

At startup, the application will attempt to initialize the database and seed demo data if the schema is empty.

### Secret Management

Do not keep real secrets in `appsettings.json`.

For local development, use `dotnet user-secrets` from `src/WarehouseInventory.Web`:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=warehouseinventory;Username=postgres;Password=YOUR_REAL_PASSWORD"
dotnet user-secrets set "EmailSettings:Username" "your-email@example.com"
dotnet user-secrets set "EmailSettings:Password" "YOUR_SMTP_PASSWORD"
dotnet user-secrets set "EmailSettings:FromEmail" "your-email@example.com"
```

For Docker or server deployments, provide secrets through environment variables instead of editing tracked files.

Common examples:

```text
ConnectionStrings__DefaultConnection
EmailSettings__Username
EmailSettings__Password
EmailSettings__FromEmail
DB_PASSWORD
```

### Email Settings

Configure SMTP in `src/WarehouseInventory.Web/appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Warehouse Inventory System"
  }
}
```

If email settings are not valid, expiry email actions will fail gracefully with an error message in the UI.

## Testing Status

Test projects are present in the solution, but automated test coverage should be treated as an actively evolving area alongside the current UI implementation.

## Known Limitations

- A reachable PostgreSQL instance is required for the app to start successfully.
- CSV export is currently implemented for Reports, not Products.
- PDF export is not implemented.
- Automatic schema creation is implemented, but a real PostgreSQL environment should still be validated outside the current local IDE workflow.

## Documentation

- [Deployment Checklist](DEPLOYMENT.md)
- [Deployment Guide](docs/deployment-guide.md)
- [User Guide](docs/user-guide.md)

## License

This project is proprietary software.
