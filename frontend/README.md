# Employee Lifecycle Portal - Frontend

Enterprise HRMS (Human Resource Management System) frontend built with React, TypeScript, and Tailwind CSS.

## Technology Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Fast build tool and dev server
- **Tailwind CSS** - Utility-first styling
- **React Router v6** - Client-side routing
- **Zustand** - State management
- **Axios** - HTTP client
- **React Hook Form** - Form handling
- **Recharts** - Charts and visualizations
- **Lucide React** - Icon library
- **React Hot Toast** - Notifications

## Features

- 🎨 Modern, responsive UI design
- 🔐 JWT-based authentication
- 📊 Interactive dashboards with charts
- 👥 Employee management (CRUD operations)
- 🏢 Department management
- ⏰ Attendance tracking
- 📅 Leave management
- 💰 Payroll processing
- 🎯 Role-based access control
- 📱 Mobile-responsive design
- ♿ Accessibility-focused components

## Project Structure

```
frontend/
├── src/
│   ├── api/              # API clients and endpoints
│   ├── components/       # Reusable components
│   │   ├── Common/      # Button, Input, Card, Table, Modal, Badge
│   │   └── Layout/      # Header, Sidebar, MainLayout, AuthLayout
│   ├── pages/           # Page components
│   ├── store/           # Zustand state management
│   ├── types/           # TypeScript type definitions
│   ├── utils/           # Utility functions
│   ├── App.tsx          # Main App component
│   ├── main.tsx         # Entry point
│   └── index.css        # Global styles
├── index.html           # HTML template
├── package.json         # Dependencies
├── vite.config.ts       # Vite configuration
├── tsconfig.json        # TypeScript configuration
└── tailwind.config.ts   # Tailwind CSS configuration
```

## Setup & Installation

### Prerequisites

- Node.js 18+
- npm or yarn

### Installation Steps

1. Navigate to frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Create `.env.local` from `.env.example`:
```bash
cp .env.example .env.local
```

4. Update `.env.local` with your API endpoint:
```
VITE_API_URL=http://localhost:5000/api
```

## Development

Start the development server:
```bash
npm run dev
```

The application will be available at `http://localhost:3000`

## Build

Create production build:
```bash
npm run build
```

Preview production build:
```bash
npm run preview
```

## Features Details

### Authentication
- JWT-based authentication
- Automatic token refresh
- Protected routes with ProtectedRoute component
- Role-based access control

### Dashboard
- Employee statistics
- Attendance metrics
- Leave overview
- Charts and visualizations
- Department headcount analysis

### Employee Management
- View all employees with pagination
- Search and filter employees
- Create new employees
- Edit employee details
- Delete employees
- View employee profiles with history

### Departments
- View all departments
- Department statistics
- Budget and salary information
- Create and manage departments

### Attendance
- Daily attendance overview
- Check-in/check-out tracking
- Attendance statistics
- Filter by status (Present, Absent, Late, etc.)

### Leave Management
- Leave balance tracking
- Leave request submission
- Request approvals workflow
- Leave history
- Multiple leave types support

### Payroll
- Monthly payroll processing
- Salary calculations
- Allowances and deductions
- Payroll export
- Payment status tracking

## UI/UX Design

### Color Scheme
- **Primary**: Sky Blue (#0284c7)
- **Secondary**: Amber (#f59e0b)
- **Neutral**: Gray scale
- **Status**: Green (success), Red (danger), Yellow (warning), Blue (info)

### Typography
- **Font Family**: Inter (sans-serif)
- **Font Weights**: 400, 500, 600, 700

### Components
- Responsive grid layouts
- Reusable card components
- Interactive tables with actions
- Modal dialogs
- Form inputs with validation
- Status badges
- Progress bars
- Charts and graphs

### Responsive Design
- Mobile-first approach
- Breakpoints: md (768px), lg (1024px)
- Sidebar collapse on mobile
- Touch-friendly interactions
- Optimized for all screen sizes

## Best Practices

- TypeScript for type safety
- Component composition and reusability
- Separation of concerns (API, store, components)
- Error handling and user feedback
- Loading states
- Form validation
- Accessibility considerations
- Performance optimization

## API Integration

### Authentication Endpoints
- `POST /auth/login` - User login
- `POST /auth/register` - User registration

### Employee Endpoints
- `GET /employees` - Get all employees (paginated)
- `GET /employees/:id` - Get employee by ID
- `POST /employees` - Create employee
- `PUT /employees/:id` - Update employee
- `DELETE /employees/:id` - Delete employee

### Department Endpoints
- `GET /departments` - Get all departments
- `GET /departments/:id` - Get department by ID
- `POST /departments` - Create department
- `PUT /departments/:id` - Update department

### Attendance Endpoints
- `GET /attendance` - Get attendance records
- `POST /attendance/checkin` - Check in
- `POST /attendance/checkout` - Check out

### Leave Endpoints
- `GET /leave` - Get leave requests
- `POST /leave` - Submit leave request
- `PUT /leave/:id/approve` - Approve leave
- `PUT /leave/:id/reject` - Reject leave

### Payroll Endpoints
- `GET /payroll` - Get payroll records
- `POST /payroll/process` - Process payroll
- `GET /payroll/export` - Export payroll

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Performance Tips

- Code splitting and lazy loading
- Image optimization
- CSS minification
- JavaScript minification
- Build optimization with Vite

## Contributing

Follow these guidelines:
1. Use TypeScript for all new files
2. Follow the existing component structure
3. Use Tailwind CSS for styling
4. Test components thoroughly
5. Document complex logic

## License

Proprietary - Employee Lifecycle Portal HRMS
