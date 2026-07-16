# Login Instructions

## Quick Login

1. **Go to**: http://localhost:3000

2. **Use these credentials**:
   ```
   Email: admin@example.com
   Password: Admin@123456
   ```

3. **Click**: Sign In

## If Login Still Fails

### Step 1: Clear Browser Cache
```
Press: Ctrl + Shift + Delete
Select: All time
Check: Cookies and site data
Click: Clear data
Close browser
```

### Step 2: Restart Browser
```
Start fresh browser completely
Go to: http://localhost:3000
```

### Step 3: Check Services
**Backend should be running**:
- Terminal shows: "Now listening on: http://localhost:5086"

**Frontend should be running**:
- Terminal shows: "VITE v5.4.21 ready"
- "Local: http://localhost:3000/"

### Step 4: Try Different Browser
If Chrome doesn't work, try Firefox, Edge, or Safari.

## Alternative Test Accounts

If admin account doesn't work, try these:

```
Email: manager@example.com
Password: Manager@123456

Email: employee@example.com
Password: Employee@123456
```

## What You Should See After Login

✅ Dashboard page with employee statistics
✅ Charts and graphs
✅ Navigation menu on left
✅ Employee, Department, Attendance data
✅ All HRMS features accessible

## Common Issues

### "Cannot find server at localhost:3000"
- Frontend not running
- Start frontend: `cd frontend && npm run dev`

### "Cannot connect to API"
- Backend not running
- Start backend: `dotnet run --project EmployeeLifecyclePortal.Api/EmployeeLifecyclePortal.Api.csproj`

### "Invalid credentials"
- Wrong password
- Double check: admin@example.com / Admin@123456
- No spaces before/after

### Blank page after login
- Wait 5 seconds for data to load
- Refresh page (F5)
- Check browser console (F12)

## Contact

If issues persist, check:
1. `LOGIN_FIXED_COMPLETE.md` - Detailed troubleshooting
2. Backend logs in `Logs/` directory
3. Browser console (F12)
4. Network tab (F12) for API errors

---

**Ready to try?** Go to http://localhost:3000 now! 🚀
