# Employee Lifecycle Portal - Quick Start Guide

## 📁 Project Location
```
c:\Coding\Projects\EmployeeLifecyclePortal
```

## 🚀 Start the Project

### Option 1: Run Locally (Development)

**Backend (.NET 10):**
```powershell
cd c:\Coding\Projects\EmployeeLifecyclePortal\EmployeeLifecyclePortal.Api
dotnet run
# Access at: http://localhost:5086
```

**Frontend (React):**
```powershell
cd c:\Coding\Projects\EmployeeLifecyclePortal\frontend
npm install
npm run dev
# Access at: http://localhost:3001
```

### Option 2: Docker (Recommended)

```powershell
cd c:\Coding\Projects\EmployeeLifecyclePortal
docker-compose up -d
# API: http://localhost:5000
# Frontend: http://localhost:3000
# SQL Server: localhost:1433
```

---

## 📊 What's Inside

**81 API Endpoints** across 8 modules:
- ✓ Attendance (11 endpoints)
- ✓ Leave (6 endpoints)
- ✓ Payroll (9 endpoints)
- ✓ Recruitment (14 endpoints)
- ✓ Performance (14 endpoints)
- ✓ Asset (11 endpoints)
- ✓ Reports (6 endpoints)
- ✓ Settings (10 endpoints)

**8 Frontend Pages:**
- Attendance Dashboard
- Leave Management
- Payroll (₹ Currency)
- Recruitment Dashboard
- Performance Reviews
- Asset Tracking
- Reports & Analytics
- Settings (Company, User, Holidays, Shifts, Email)

---

## 🔍 Access Points

| Component | URL | Purpose |
|-----------|-----|---------|
| **Frontend** | http://localhost:3001 | User interface |
| **API** | http://localhost:5086 | REST API |
| **Swagger UI** | http://localhost:5086/swagger | API documentation |
| **Health Check** | http://localhost:5086/health | Service status |

---

## 🛠️ Build & Verify

**Backend Build:**
```powershell
cd c:\Coding\Projects\EmployeeLifecyclePortal\EmployeeLifecyclePortal.Api
dotnet build --no-restore
# Should show: 0 Error(s), 0 Warning(s)
```

**Frontend Type-Check:**
```powershell
cd c:\Coding\Projects\EmployeeLifecyclePortal\frontend
npm run type-check
# Should complete without errors
```

---

## 📁 Key Files

**Backend:**
- `EmployeeLifecyclePortal.Api/Program.cs` - Startup config
- `EmployeeLifecyclePortal.Api/Controllers/` - API endpoints
- `EmployeeLifecyclePortal.Domain/Entities/` - Database models

**Frontend:**
- `frontend/src/pages/` - All 8 pages
- `frontend/src/components/` - Reusable components
- `frontend/src/clients/` - API clients

**DevOps:**
- `Dockerfile` - Container definition
- `docker-compose.yml` - Local development stack
- `.github/workflows/build-and-deploy.yml` - CI/CD pipeline

**Config:**
- `appsettings.Development.json`
- `appsettings.Staging.json`
- `appsettings.Production.json`

---

## 🔐 Authentication

Default auth setup:
- JWT Bearer tokens
- Role-based access (Admin/Manager/HR/Employee)
- CORS enabled for localhost:3001

---

## 📚 Documentation

Check the `docs/` folder for:
- ARCHITECTURE.md - System design
- DATABASE_DESIGN.md - Entity relationships
- API_DESIGN.md - REST conventions
- DEPLOYMENT.md - Production deployment
- SECURITY.md - Security best practices

---

## ⚙️ Configuration

**Development:** Uses local SQL Server, Swagger UI, detailed logging
**Staging:** Cloud database, rate limiting 100 req/min
**Production:** HTTPS required, error-level logging, 60 req/min limit

---

## 🧪 Testing

**Test the API:**
1. Visit http://localhost:5086/swagger
2. Click on any endpoint
3. Click "Try it out"
4. Click "Execute"

**Test the Frontend:**
1. Visit http://localhost:3001
2. Navigate through all 8 modules
3. Try creating/viewing records

---

## 🐛 Troubleshooting

**Backend won't start:**
```powershell
# Clean and rebuild
cd EmployeeLifecyclePortal.Api
dotnet clean
dotnet build
dotnet run
```

**Frontend shows errors:**
```powershell
# Clean dependencies and reinstall
cd frontend
rm -r node_modules package-lock.json
npm install
npm run dev
```

**CORS errors:**
- Check appsettings.json CORS configuration
- Frontend should be on http://localhost:3001
- Backend should be on http://localhost:5086

**Database connection issues:**
- Ensure SQL Server is running
- Check connection string in appsettings.json
- Run migrations if needed

---

## 📦 Deployment

**To Docker:**
```bash
docker build -t employee-portal .
docker run -p 5000:80 -e ConnectionStrings__DefaultConnection="your-connection-string" employee-portal
```

**To Cloud:**
1. Push to GitHub
2. GitHub Actions CI/CD triggers
3. Builds and pushes to container registry
4. Deploy to Azure/AWS/GCP

---

## ✅ Checklist

- [x] All 81 endpoints implemented
- [x] Backend builds with 0 errors
- [x] Frontend type-checks with 0 errors
- [x] Docker configuration ready
- [x] CI/CD pipeline configured
- [x] Documentation complete
- [x] All modules tested

---

## 📞 Support

**If something doesn't work:**
1. Check logs in console
2. Review docs/ folder
3. Check API response in Swagger
4. Verify database connection
5. Check firewall/port conflicts

---

**Project Status: ✓ PRODUCTION READY**

Start with: `docker-compose up -d`

Then visit: http://localhost:3000 (Frontend)
