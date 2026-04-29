# User Guide

## Overview
 
Warehouse Inventory System provides a demo-ready interface for managing products, warehouses, stock quantities, dashboard metrics, and reports.
 
The application supports two database modes:
 
- **InMemory mode**: Uses EF Core InMemory database for local development without requiring PostgreSQL
- **PostgreSQL mode**: Uses a PostgreSQL database for production deployment with persistent storage
 
By default, the Development environment uses InMemory mode, and Production uses PostgreSQL mode.

## Dashboard

The Dashboard shows high-level operational metrics.

### KPI Tiles

The tiles display:

- Products
- Warehouses
- Total Stock
- Expiring Soon

These values are calculated from the current persisted database records used by the Products, Warehouses, and Stock pages.

### Expiry Email

The Dashboard also includes an expiry email form.

- Enter a recipient email address
- Submit the form to send a list of products expiring tomorrow
- If no products expire tomorrow, the page will show a no-results message instead of sending an email

## Products Page

The Products page allows you to manage product records.

### Available Actions

- Add product
- Edit product
- Delete product
- Search products
- Filter by warehouse
- Filter by type
- Filter by status
- Filter by expiry state
- Reset filters

### Product Fields

Products currently support these fields:

- Product code
- Product name
- Group
- Barcode
- Catalogue
- Warehouse
- Type
- Condition
- Date
- Expiry date
- Active status

### Notes

- Product updates affect Dashboard and Reports values after they are saved to the database
- Products page no longer exports CSV

## Warehouses Page

The Warehouses page allows you to manage warehouse records.

### Available Actions

- Add warehouse
- Edit warehouse
- Delete warehouse

### Warehouse Fields

- Name
- Address

### Notes

- Warehouse changes affect the Dashboard warehouse count after they are saved to the database

## Stock Page

The Stock page is used to view and adjust inventory quantities.

### Available Actions

- Search stock rows by product code or product name
- Filter by warehouse
- Reset filters
- Open the edit modal for a stock row
- Perform Stock In or Stock Out actions

### Stock Edit Rules

- Quantity must be at least 1
- Stock Out cannot reduce quantity below 0
- Current search and warehouse filter state is preserved after stock edits

### Notes

- The page applies filtering client-side to the currently loaded stock table
- A normal browser refresh returns the page to its default filter state

## Reports Page

The Reports page provides operational reports and exports.

### KPI Cards

The top KPI cards show global totals for:

- Total Products
- Total Stock
- Low Stock Items
- Expiring Items

These KPI values are not reduced by report filters.

### Report Types

Available report tabs:

- Stock Levels
- Low Stock
- Expiry Dates
- Stock Movements

### Filters

Available filters:

- Date Range
- Warehouse
- Product Type

### Filter Behavior

- Warehouse and Product Type filters affect report rows
- Date Range affects date-based reports such as Expiry Dates and Stock Movements
- Reset Filter clears report filters
- A browser refresh returns the report page to default filter values while keeping the current report tab

### Export

- Export CSV downloads the currently selected report
- Export respects the current report type and active filters

## Data Persistence Behavior
 
The application's data persistence depends on the configured database mode.
 
### InMemory Mode (Development)
 
- Data is stored in an in-memory database that exists only while the application is running
- Changes made through the interface are reflected immediately while the app is running
- Restarting the application resets all data to seeded demo values
- No PostgreSQL server is required
- Suitable for local development, testing, and UI iteration without database setup
 
### PostgreSQL Mode (Production)
 
- Data is persisted in a PostgreSQL database
- Changes made through the interface are permanently stored
- Restarting the application preserves all data
- Requires a configured and reachable PostgreSQL server
- Suitable for production deployment with persistent storage
 
### Startup Behavior
 
- InMemory mode: The app seeds demo data on every startup
- PostgreSQL mode: The app initializes schema and seeds demo data on first startup against an empty database
- PostgreSQL mode: If the database is unavailable, the application cannot complete normal startup
 
### While the Application Is Running
 
- InMemory mode: Additions, edits, and deletions are reflected immediately but reset on restart
- PostgreSQL mode: Additions, edits, and deletions are persisted permanently
- Dashboard and reports update from the same data source used by the main pages

## Known Limitations

- PostgreSQL mode requires a reachable PostgreSQL server
- InMemory mode data resets on application restart
- Products page CSV export is not available
- Reports PDF export is not implemented
- A real PostgreSQL environment should still be validated outside the current local IDE workflow
