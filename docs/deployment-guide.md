# Server Deployment Guide

This guide explains how to deploy the Warehouse Inventory System to a server using Docker.

## Prerequisites

- Docker installed on the server
- Docker Compose installed on the server
- Access to the server (SSH or direct access)
- At least 2GB RAM available
- 10GB disk space available

## Deployment Steps

### 1. Prepare the Files

Copy the entire project folder to your server. The folder should contain:
- `docker/Dockerfile`
- `docker/docker-compose.yml`
- `src/` folder with all project files
- All `.csproj` files
- `.dockerignore` file

### 2. Configure Environment Variables

Edit the `docker/docker-compose.yml` file and update the following environment variables:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection=Host=db;Database=warehouseinventory;Username=postgres;Password=YOUR_SECURE_PASSWORD
```

Update the PostgreSQL password in the `db` service:
```yaml
environment:
  - POSTGRES_DB=warehouseinventory
  - POSTGRES_USER=postgres
  - POSTGRES_PASSWORD=YOUR_SECURE_PASSWORD
```

**IMPORTANT:** Replace `YOUR_SECURE_PASSWORD` with a strong, unique password.

For safer deployments:

- do not commit real passwords to repository files
- use Docker environment variables or a server-side `.env` file
- override values such as:
  - `ConnectionStrings__DefaultConnection`
  - `EmailSettings__Username`
  - `EmailSettings__Password`
  - `EmailSettings__FromEmail`
  - `DB_PASSWORD`

### 3. Configure Email Settings (Optional)

If you want expiry warning emails, create a `docker-compose.override.yml` file:

```yaml
version: '3.8'

services:
  app:
    environment:
      - EmailSettings__SmtpServer=smtp.gmail.com
      - EmailSettings__SmtpPort=587
      - EmailSettings__UseSsl=true
      - EmailSettings__Username=your-email@gmail.com
      - EmailSettings__Password=your-app-password
      - EmailSettings__FromEmail=your-email@gmail.com
      - EmailSettings__FromName=Warehouse Inventory System
```

### 4. Deploy to Server

Upload the entire project folder to your server using SCP, SFTP, or any file transfer method.

Example using SCP:
```bash
scp -r WarehouseInventory user@your-server:/path/to/deployment/
```

### 5. Start the Application

SSH into your server and navigate to the project directory:

```bash
cd /path/to/deployment/WarehouseInventory
```

Start the Docker containers:

```bash
docker-compose -f docker/docker-compose.yml up -d
```

This will:
- Build the application Docker image
- Pull the PostgreSQL Docker image
- Start both containers in the background

### 6. Verify Deployment

Check if the containers are running:

```bash
docker-compose -f docker/docker-compose.yml ps
```

You should see both `app` and `db` containers running with status "Up".

Check the application logs:

```bash
docker-compose -f docker/docker-compose.yml logs app
```

Check the database logs:

```bash
docker-compose -f docker/docker-compose.yml logs db
```

### 7. Access the Application

The application will be available at:
- HTTP: `http://your-server-ip:5000`
- If you configured HTTPS, use the appropriate port

**First Time Startup:**
1. Access the application in your browser
2. The application will attempt to create the schema automatically if the database is empty
3. The application will seed demo warehouses, products, stock, and stock transactions on first startup

### 8. Database Migrations

The application currently initializes schema automatically on first run when PostgreSQL is reachable. In this implementation, the startup path is based on automatic database creation rather than committed EF migrations.

```bash
# Review application startup logs
docker-compose -f docker/docker-compose.yml logs app
```

If database initialization fails, verify the PostgreSQL container is healthy and the configured connection string is correct.

## Managing the Deployment

### View Logs

```bash
# All logs
docker-compose -f docker/docker-compose.yml logs

# App logs only
docker-compose -f docker/docker-compose.yml logs app

# Database logs only
docker-compose -f docker/docker-compose.yml logs db

# Follow logs in real-time
docker-compose -f docker/docker-compose.yml logs -f
```

### Stop the Application

```bash
docker-compose -f docker/docker-compose.yml down
```

### Restart the Application

```bash
docker-compose -f docker/docker-compose.yml restart
```

### Update the Application

1. Upload the new code to the server
2. Navigate to the project directory
3. Rebuild and restart:

```bash
docker-compose -f docker/docker-compose.yml down
docker-compose -f docker/docker-compose.yml up -d --build
```

### Backup the Database

```bash
# Backup to a file
docker-compose -f docker/docker-compose.yml exec db pg_dump -U postgres warehouseinventory > backup.sql

# Restore from backup
docker-compose -f docker/docker-compose.yml exec -T db psql -U postgres warehouseinventory < backup.sql
```

### Access Database Directly

```bash
docker-compose -f docker/docker-compose.yml exec db psql -U postgres warehouseinventory
```

## Security Considerations

1. **Change Default Passwords**: Always change the default PostgreSQL password
2. **Use HTTPS**: Configure SSL/TLS for production use
3. **Firewall**: Configure firewall to only allow necessary ports
4. **Regular Backups**: Set up automated database backups
5. **Monitor Logs**: Regularly check application and database logs
6. **Update Dependencies**: Keep Docker images and dependencies updated

## Troubleshooting

### Container won't start

Check the logs:
```bash
docker-compose -f docker/docker-compose.yml logs app
```

Common issues:
- Port 5000 already in use - change the port mapping in docker-compose.yml
- Database connection failed - check connection string and ensure db container is running
- Build errors - ensure all files are uploaded correctly

### Database connection errors

- Verify the db container is running: `docker-compose ps`
- Check the connection string matches the db container credentials
- Ensure both containers are on the same Docker network

### Application not accessible

- Check if the container is running: `docker-compose ps`
- Verify port mapping in docker-compose.yml
- Check server firewall settings
- Ensure the server's IP is accessible from your network

## Performance Tuning

For higher load, consider:

1. **Increase Database Resources**: Adjust PostgreSQL memory settings
2. **Add Redis**: For caching (not implemented yet)
3. **Load Balancing**: Run multiple app instances behind a load balancer
4. **Database Optimization**: Add indexes, optimize queries
5. **CDN**: Use CDN for static assets

## Support

For issues or questions, refer to:
- Main README.md
- Architecture documentation
- Database schema documentation
