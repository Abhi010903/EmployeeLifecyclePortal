# Production Deployment Guide

## Sprint 31: Production Hardening
## Sprint 32: Docker & Containerization
## Sprint 33: CI/CD Pipeline
## Sprint 34: Monitoring & Testing

---

## Prerequisites

- Docker & Docker Compose
- .NET 10 SDK (for local builds)
- SQL Server 2019 or later
- Azure App Service (for cloud deployment)
- GitHub with Actions enabled

---

## Local Development

### Using Docker Compose

```bash
# Build and start containers
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop containers
docker-compose down
```

### Database Migration

```bash
# Apply migrations
dotnet ef database update --project EmployeeLifecyclePortal.Infrastructure

# Create new migration
dotnet ef migrations add "MigrationName" --project EmployeeLifecyclePortal.Infrastructure
```

---

## Docker Build & Push

```bash
# Build image
docker build -t employeelifecycleportal:latest .

# Tag for registry
docker tag employeelifecycleportal:latest ghcr.io/yourusername/employeelifecycleportal:latest

# Push to registry
docker push ghcr.io/yourusername/employeelifecycleportal:latest
```

---

## CI/CD Pipeline

### GitHub Actions Workflow

Located at `.github/workflows/build-and-deploy.yml`

**Triggers:**
- Push to main or develop branches
- Pull requests to main or develop

**Jobs:**
1. Build & Test - Restore, Build, Run Tests
2. Push Docker - Build and push to container registry
3. Deploy - Deploy to Azure App Service

### Running Workflow

Workflows trigger automatically on push/PR. View in **Actions** tab.

---

## Deployment to Azure

### Create App Service

```bash
# Create resource group
az group create --name elp-rg --location eastus

# Create App Service Plan
az appservice plan create --name elp-plan --resource-group elp-rg --sku B2 --is-linux

# Create App Service
az webapp create --resource-group elp-rg --plan elp-plan --name elp-api --deployment-container-image-name ghcr.io/yourusername/employeelifecycleportal:latest
```

### Configure Container Registry

```bash
# Set registry credentials
az webapp config container set --name elp-api --resource-group elp-rg --docker-custom-image-name ghcr.io/yourusername/employeelifecycleportal:latest --docker-registry-server-url https://ghcr.io --docker-registry-server-user username --docker-registry-server-password token
```

### Configure App Settings

```bash
# Set environment variables
az webapp config appsettings set --resource-group elp-rg --name elp-api --settings ASPNETCORE_ENVIRONMENT=Production ConnectionStrings__DefaultConnection="connection-string"
```

---

## Security Hardening

### HTTPS

- App Service enforces HTTPS by default
- Update `appsettings.Production.json` for SSL

### Secrets Management

- Use Azure Key Vault for sensitive data
- Never commit secrets to repository
- Use GitHub Secrets for CI/CD

### Database Security

- Use Azure SQL Database with firewall rules
- Enable encryption at rest
- Use strong SA passwords
- Restrict access to internal networks

### API Security

- Rate limiting enabled in middleware
- CORS configured for specific origins
- JWT token validation required
- Input validation on all endpoints

---

## Monitoring & Logging

### Application Insights

```bash
# Create Application Insights resource
az monitor app-insights component create --app-name elp-insights --location eastus --resource-group elp-rg --application-type web
```

### Serilog Configuration

Logs are written to:
- Console (development)
- File system (development & production)
- Application Insights (production)

### Health Checks

Monitor at:
- `/health` - Liveness probe
- `/health/detail` - Readiness probe with component status

---

## Performance Optimization

### Caching

- Implement Redis for distributed caching
- Cache authentication tokens
- Cache frequently accessed data

### Database

- Ensure all indexes are created
- Use connection pooling
- Implement query optimization

### API Response

- Implement gzip compression
- Use pagination for large datasets
- Implement async/await throughout

---

## Testing

### Unit Tests

```bash
dotnet test EmployeeLifecyclePortal.Tests
```

### Integration Tests

```bash
dotnet test EmployeeLifecyclePortal.Tests --filter Category=Integration
```

### Load Testing

Use Azure Load Testing or JMeter to validate performance.

---

## Maintenance

### Regular Backups

- Automated daily backups in Azure SQL
- Retention: 35 days
- Test restore procedures

### Log Retention

- Keep application logs for 30 days
- Archive older logs to cold storage
- Monitor log storage costs

### Dependency Updates

- Review NuGet package updates monthly
- Test in development environment
- Deploy to production after validation

---

## Troubleshooting

### Application Won't Start

1. Check Serilog configuration
2. Verify database connection string
3. Ensure migrations are applied
4. Check application logs

### Database Connection Failed

1. Verify connection string
2. Check SQL Server is running
3. Verify firewall rules
4. Test with sqlcmd

### Performance Issues

1. Check database query performance
2. Monitor Application Insights
3. Check resource utilization
4. Review slow query logs

---

## Support

- Documentation: `/docs`
- GitHub Issues: Report bugs
- GitHub Discussions: Ask questions
