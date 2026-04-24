# User Guide

## Overview

Warehouse Inventory System provides a demo-ready interface for managing products, warehouses, stock quantities, dashboard metrics, and reports.

The current implementation uses a PostgreSQL database through Entity Framework Core for the main UI workflows. Changes made through the interface are persisted when the application can connect to the configured database.

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

The current application now persists operational data in the configured database.

### Startup Behavior

- On first startup against an empty database, the app initializes schema and seeds demo data
- If the configured PostgreSQL database is unavailable, the application cannot complete normal startup

### While the Application Is Running

- Additions, edits, and deletions are persisted to the database
- Dashboard and reports update from the same persisted records used by the main pages

## Known Limitations

- Main UI CRUD flows require a reachable PostgreSQL database
- Products page CSV export is not available
- Reports PDF export is not implemented
- A real PostgreSQL environment should still be validated outside the current local IDE workflow
