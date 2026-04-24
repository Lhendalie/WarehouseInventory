# Quick Server Deployment Checklist

## Pre-Deployment Checklist

- [ ] Docker is installed on the server
- [ ] Docker Compose is installed on the server
- [ ] Server has at least 2GB RAM
- [ ] Server has at least 10GB disk space
- [ ] You have SSH access to the server
- [ ] You have a strong PostgreSQL password ready
- [ ] You have SMTP credentials ready (if using email features)

## Deployment Steps

1. **Copy the entire project folder to the server**
   - Use SCP, SFTP, or any file transfer method
   - Ensure all files are transferred including:
     - `docker/` folder
     - `src/` folder
     - All `.csproj` files
     - `.dockerignore` file

2. **Configure passwords in `docker/docker-compose.prod.yml`**
   - Replace `CHANGE_THIS_PASSWORD` with your secure PostgreSQL password
   - Update the connection string to match

   Recommended approach:
   - supply the password through environment variables or a server-side `.env` file
   - avoid committing real passwords into repository files

3. **Configure email (optional)**
   - Copy `docker/docker-compose.override.yml.example` to `docker/docker-compose.override.yml`
   - Update with your SMTP credentials

   Recommended environment variable names:
   - `ConnectionStrings__DefaultConnection`
   - `EmailSettings__Username`
   - `EmailSettings__Password`
   - `EmailSettings__FromEmail`
   - `DB_PASSWORD`

4. **Start the application**
   ```bash
   docker-compose -f docker/docker-compose.prod.yml up -d
   ```

5. **Verify deployment**
   ```bash
   docker-compose -f docker/docker-compose.prod.yml ps
   docker-compose -f docker/docker-compose.prod.yml logs app
   ```

6. **Access the application**
   - Open browser: `http://your-server-ip`
   - The application should load the dashboard

## Post-Deployment

- [ ] Configure firewall to allow port 80
- [ ] Set up SSL certificate (if using HTTPS)
- [ ] Configure automated database backups
- [ ] Set up log monitoring
- [ ] Verify the application initialized the schema and seeded demo records on first startup
- [ ] Test basic functionality (products, warehouses, stock operations, dashboard, reports)

## Troubleshooting

If containers won't start:
```bash
docker-compose -f docker/docker-compose.prod.yml logs
```

If database connection fails:
- Ensure db container is running
- Check connection string matches db credentials
- Verify both containers are on the same Docker network

## Important Notes

- The application will automatically create the database schema on first run if the configured PostgreSQL database is reachable
- The application will seed demo warehouse, product, stock, and stock transaction data on first startup when the database is empty
- PostgreSQL data is persisted in a Docker volume
- To stop: `docker-compose -f docker/docker-compose.prod.yml down`
- To restart: `docker-compose -f docker/docker-compose.prod.yml restart`
- To update: Upload new code and run `docker-compose -f docker/docker-compose.prod.yml up -d --build`

## Support

See `docs/deployment-guide.md` for detailed deployment instructions.
