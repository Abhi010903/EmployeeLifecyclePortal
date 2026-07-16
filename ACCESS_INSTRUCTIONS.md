# How to Access the Running HRMS Application

## 🎯 Quick Access

### Frontend Application
```
URL: http://localhost:3000
Status: ✅ Running
Port: 3000
```

### Backend API
```
URL: http://localhost:5086/api
Status: ✅ Running
Port: 5086
```

---

## 🔐 Login Steps

### Step 1: Open Browser
Go to: **http://localhost:3000**

You will see the **HRMS Login Page** with:
- Email input field
- Password input field
- Sign In button
- Demo credentials hint

### Step 2: Enter Credentials
Choose a test account:

**Option A: Admin Account (Full Access)**
```
Email: admin@example.com
Password: Admin@123456
```

**Option B: Manager Account (Manager Features)**
```
Email: manager@example.com
Password: Manager@123456
```

**Option C: Employee Account (Basic Access)**
```
Email: employee@example.com
Password: Employee@123456
```

### Step 3: Click "Sign In"
- System will validate credentials
- JWT token will be generated
- You'll be redirected to Dashboard

---

## 🗺️ Main Application Layout

### Header (Top Bar)
- **Left**: HRMS Logo
- **Center**: Current page title
- **Right**: 
  - 🔔 Notifications bell
  - 👤 User profile menu
  - User name
  - Logout option

### Sidebar (Left Menu)
Navigation items include:
- 🏠 Dashboard
- 👥 Employees
- 🏢 Departments
- ⏰ Attendance
- 📅 Leave
- 💰 Payroll
- 🏆 Performance
- 📊 Reports
- ⚙️ Settings (Admin only)

### Main Content Area
- Page-specific content
- Charts, tables, forms
- Action buttons
- Search/filter options

---

## 📋 Pages & Features Overview

### 1. Dashboard Page
**URL**: http://localhost:3000/dashboard

**What You See**:
- 📊 4 stat cards (Employees, Present, Pending Leave, Payroll)
- 📈 Employee & Attendance Trend chart
- 📉 Department Headcount chart
- 3 Quick stats cards (Retention, Attrition, New Hires)

**Actions**:
- View real-time statistics
- Analyze trends with charts
- See department breakdowns

### 2. Employees Page
**URL**: http://localhost:3000/employees

**What You See**:
- Search bar for employee search
- Employee list table with columns:
  - Name
  - Email
  - Phone
  - Status (badge)
  - Actions (Edit, Delete)
- Add Employee button

**Actions You Can Perform**:
1. ✅ **Search**: Type name or email to filter
2. ✅ **Create**: Click "Add Employee" button
   - Fill form: First Name, Last Name, Email, Phone, Department
   - Click "Save"
3. ✅ **Edit**: Click pencil icon
   - Update details
   - Click "Save"
4. ✅ **Delete**: Click trash icon
   - Confirm deletion

### 3. Departments Page
**URL**: http://localhost:3000/departments

**What You See**:
- Search bar
- Department cards in grid layout
- Each card shows:
  - Department name
  - Description
  - Total Budget
  - Average Salary
  - Edit/Delete buttons

**Actions**:
- Search departments
- View department details
- See budget information
- Add new departments

### 4. Attendance Page
**URL**: http://localhost:3000/attendance

**What You See**:
- 4 stat cards:
  - 🟢 Present Today
  - 🔴 Absent Today
  - 🟡 Late Arrivals
  - 🔵 On Leave
- Today's Attendance table with columns:
  - Employee Name
  - Check In (with icon)
  - Check Out (with icon)
  - Duration
  - Status
  - Actions

**Actions**:
- View daily attendance
- Check-in/out employees
- Edit attendance records
- Filter by status

### 5. Leave Management Page
**URL**: http://localhost:3000/leave

**What You See**:
- 3 leave balance cards:
  - Annual Leave (with progress bar)
  - Sick Leave (with progress bar)
  - Casual Leave (with progress bar)
- Leave Requests table with columns:
  - Employee Name
  - Leave Type
  - Period
  - Days
  - Status

**Actions**:
- ✅ **Request Leave**:
  - Click "Request Leave" button
  - Select leave type
  - Choose dates
  - Enter reason
  - Submit
- View leave balance
- See request history
- Track approval status

### 6. Payroll Page
**URL**: http://localhost:3000/payroll

**What You See**:
- 3 summary cards:
  - Total Payroll
  - Salaries Paid
  - Pending Payroll
- Payroll table with columns:
  - Employee Name
  - Base Salary
  - Allowances
  - Deductions
  - Net Salary
  - Status
  - View details button
- Export Payroll button

**Actions**:
- View payroll records
- See salary breakdown
- Track payment status
- Export payroll data

---

## 🎨 UI Elements Guide

### Status Badges
- 🟢 **Green** = Active/Present/Approved/Paid
- 🔴 **Red** = Inactive/Absent/Rejected/Danger
- 🟡 **Yellow** = Warning/Pending/Late
- 🔵 **Blue** = Info/Processing

### Buttons
- **Primary (Blue)**: Main actions
- **Secondary (Orange)**: Alternative actions
- **Outline (Gray)**: Neutral actions
- **Danger (Red)**: Delete/Remove actions

### Cards
- Clickable cards highlight on hover
- Shadow effects for depth
- Color-coded backgrounds

### Tables
- Hover to highlight rows
- Click actions on right
- Sortable columns (prepared)
- Paginated data

---

## ⌨️ Keyboard Shortcuts & Tips

### General
- `Ctrl+F` - Search in table
- `Tab` - Move between form fields
- `Enter` - Submit form/Login
- `Escape` - Close modals

### Search Tips
- Search by **name**: "John"
- Search by **email**: "john@"
- Search by **status**: "Active"

### Table Tips
- Click row to see details
- Click edit icon to modify
- Click delete icon to remove
- Use search to filter

---

## 🔄 Common Workflows

### Workflow 1: Create New Employee
1. Click "Employees" in sidebar
2. Click "Add Employee" button
3. Fill in form:
   - First Name: "John"
   - Last Name: "Doe"
   - Email: "john@company.com"
   - Phone: "555-1234"
   - Department: Select from dropdown
4. Click "Save"
5. See success toast notification
6. Employee appears in list

### Workflow 2: Request Leave
1. Click "Leave" in sidebar
2. View your current leave balance
3. Click "Request Leave" button
4. Fill form:
   - Leave Type: "Annual Leave"
   - Start Date: Pick date
   - End Date: Pick date
   - Reason: "Vacation"
5. Click "Submit Request"
6. Request shows as "Pending"

### Workflow 3: Check Attendance
1. Click "Attendance" in sidebar
2. See today's stats in cards at top
3. View detailed attendance table
4. Look for your name or search
5. Check-in/out times shown
6. Duration calculated automatically

### Workflow 4: View Payroll
1. Click "Payroll" in sidebar
2. See total payroll in summary cards
3. Find your employee record in table
4. See breakdown:
   - Base Salary
   - Allowances
   - Deductions
   - Net Salary
5. Click eye icon for details
6. Click Export to download

---

## 🐛 Troubleshooting

### If Login Fails
**Problem**: "Login failed" error message
**Solution**:
- Check credentials are correct
- Make sure backend is running on port 5086
- Clear browser cache
- Try different browser

### If Page Doesn't Load
**Problem**: Blank page or 404 error
**Solution**:
- Check frontend is running on port 3000
- Wait 5 seconds for page to load
- Refresh browser (F5)
- Check browser console for errors

### If Charts Don't Show
**Problem**: Empty chart area
**Solution**:
- Wait for data to load
- Check network tab in dev tools
- Verify API is responding
- Restart frontend server

### If Can't Edit/Delete
**Problem**: Buttons disabled or not working
**Solution**:
- Check user role has permission
- Try with admin account
- Check network connection
- Verify API endpoint is working

---

## 📊 Data You'll See

### Sample Dashboard Stats
- Total Employees: 2,543
- Present Today: 2,187 (86%)
- Pending Leave: 124
- Payroll Due: $2.4M
- Retention Rate: 94.2%
- Attrition This Month: 2
- New Hires This Year: 187

### Sample Employees
- John Doe (ID: EMP001) - Active
- Jane Smith (ID: EMP002) - Active
- Bob Johnson (ID: EMP003) - Active
- And many more...

### Sample Departments
- Engineering (Budget: $500K, Avg Salary: $95K)
- Human Resources (Budget: $200K, Avg Salary: $65K)
- Sales (Budget: $300K, Avg Salary: $75K)

---

## 🔐 User Roles & Permissions

### Admin Role
✅ Can:
- View all pages
- Create/Edit/Delete all records
- Access Settings
- View Reports
- Manage Users
- See Audit Logs

❌ Cannot:
- (Unrestricted access)

### Manager Role
✅ Can:
- View Dashboard
- Manage employees
- Approve leave requests
- View team attendance
- See payroll
- View Performance

❌ Cannot:
- Access Settings
- Delete employees
- Modify payroll
- See other managers' data

### Employee Role
✅ Can:
- View Dashboard
- See own profile
- Request leave
- View own attendance
- See own payroll

❌ Cannot:
- Manage other employees
- Approve leave
- Manage departments
- Access Settings

---

## 💡 Tips & Tricks

### Dashboard
- Charts are interactive - hover to see exact values
- Cards show trends with percentages
- Refresh to get latest data

### Employees
- Use search to find quickly
- Email is required field
- Phone is optional
- Department assignment is important for leaves

### Attendance
- Early departure shows "Early Leave"
- Late arrival shows "Late"
- Full day present shows "Present"
- Absent shown in red

### Leave
- Balance shows used/total days
- Progress bar fills as you use leave
- Pending requests show in yellow
- Approved show in green

### Payroll
- Net Salary = Base + Allowances - Deductions
- Status shows if paid or pending
- Export creates downloadable file
- Export works in spreadsheet format

---

## 🎯 First 5 Minutes

1. **Login** (30 seconds)
   - Go to http://localhost:3000
   - Enter admin credentials
   - Click Sign In

2. **Explore Dashboard** (1 minute)
   - See statistics
   - Look at charts
   - Check HR metrics

3. **Browse Employees** (1 minute)
   - View employee list
   - Try searching
   - See employee details

4. **Check Attendance** (1 minute)
   - See today's attendance
   - Check presence stats
   - View attendance table

5. **Try a Feature** (1.5 minutes)
   - Request leave
   - Or create employee
   - Or view payroll
   - See success message

**Total**: ~5 minutes to get familiar!

---

## 📞 Need Help?

**Backend Issues**:
- Check logs: Look in terminal where backend runs
- Check port: http://localhost:5086/api
- Restart backend if needed

**Frontend Issues**:
- Check browser console: F12 → Console tab
- Check network tab for API errors
- Clear browser cache and retry

**Feature Questions**:
- Read docs in `/docs` folder
- Check `API_DESIGN.md` for endpoints
- See `MODULES.md` for feature details

---

## ✨ Enjoy the HRMS System!

You now have full access to a **production-ready enterprise HRMS application** with:
- ✅ Modern, responsive UI
- ✅ Complete employee management
- ✅ Attendance tracking
- ✅ Leave management
- ✅ Payroll processing
- ✅ Interactive dashboards
- ✅ Professional design

**Happy exploring! 🚀**

---

**Quick Links**:
- Frontend: http://localhost:3000
- Backend: http://localhost:5086/api
- Admin: admin@example.com / Admin@123456
