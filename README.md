# PharmaPro - Factory Management System (DEPI Project)

PharmaPro is a comprehensive 3-Tier ASP.NET Core MVC application designed for managing pharmaceutical manufacturing facilities. The platform automates administrative, operational, and scheduling workflows, ensuring seamless coordination between administrators, managers, and employees.

---

## 🏗️ System Architecture

The project is structured following clean architecture principles using a **3-Tier Architecture**:

1. **`DEPI.DAL` (Data Access Layer)**:
   * **Models**: Contains domain entities (`Employee`, `Department`, `ProductionLine`, `Shift`, `Schedule`, `Attendance`, `Mission`, `VacationRequest`, `SwapRequest`, `JopDescription`).
   * **DbContext**: `ApplicationDbContext` managing EF Core configurations, composite keys (e.g., `EmployeeDepartment`), relationships, and cascade delete restrictions (`DeleteBehavior.Restrict`).
   * **Identity**: Customized `ApplicationUser` extending `IdentityUser`.
   * **Repositories**: Data access logic for Departments, Production Lines, Shifts, Employees, and Users.

2. **`DEPI.BLL` (Business Logic Layer)**:
   * **Services**: Encapsulates business logic and orchestration (`AdminService`, `DepartmentService`, `ProductionLineService`, `ShiftService`, `AccountService`).
   * **DTOs**: Data Transfer Objects to decoupled layers and secure API responses (e.g., `UserManagementDto`, `ProductionLineDto`, `AdminApprovalDto`).

3. **`DEPI.PL` / `DEPI_Pro` (Presentation Layer)**:
   * ASP.NET Core MVC project handling controllers (`Account`, `Admin`, `Home`), layouts (`_Layout`, `_AdminLayout`), views (Razor Templates), and public assets.

---

## 📈 Current Project Status

### ✅ Completed Features (Done)

1. **User Authentication & Identity Management**:
   * Secure Sign Up, Log In, and Log Out functionality using ASP.NET Core Identity.
   * Role-Based Access Control (RBAC) with three primary roles: `Admin`, `Manager`, and `Employee`.
   * Automated database role seeding on startup.

2. **Employee Registration & Approval Workflow**:
   * New employees sign up with state tracking (`Pending` status).
   * Dedicated Admin dashboard view to review, **Approve**, or **Reject** pending employee registrations.
   * Linked one-to-one relationship between Identity `ApplicationUser` and the `Employee` entity.

3. **Admin Control Panel**:
   * Interactive Dashboard displaying real-time system overview statistics (Total Users, Active Departments, Active Production Lines, Shifts, and Pending Registrations).

4. **Department Management (CRUD)**:
   * Full capability to Create, Read, Update, and Deactivate/Delete departments.

5. **Production Line Management (CRUD)**:
   * Create, Read, Update, and Delete production lines.
   * Production lines are mapped to their respective Departments, displaying names dynamically instead of raw IDs.

6. **Shift Management (CRUD)**:
   * Create, Read, Update, and Delete shift schedules (e.g., Morning, Night) for facility employees.

7. **Weekly/Monthly Scheduling System**:
   * Assigning specific employees to production lines, departments, and shifts for given timeframes (`Schedule` entity).
   * Interactive views for managers to build and publish schedules.

8. **Vacation & Leave Management**:
   * Submission of vacation requests by employees (`VacationRequest` entity).
   * Portal for managers/admins to approve, reject, or comment on submitted leaves.

9. **Shift Swap Requests**:
   * Peer-to-peer shift swapping between employees (`SwapRequest` entity) with request, recipient, and approval tracking.

10. **Attendance & Time Tracking**:
    * Clock-in and Clock-out logging linked directly to schedules (`Attendance` entity) to measure punctuality and hours worked.

11. **Official Missions & Business Trips**:
    * Authorizing and documenting external or specialized task missions (`Mission` entity) for employees.

12. **Job Descriptions & Skill Management**:
    * Mapping specialized operator roles and capabilities to specific production lines (`JopDescription` entity).

13. **Email & Notification System**:
    * Integrating the `IEmailService` to send auto-generated email alerts for status changes (e.g., vacation approval, schedule assignments, shift swaps).

---

## 🛠️ Technologies & Tools

* **Backend Framework**: .NET 8.0 / ASP.NET Core MVC
* **Database**: Microsoft SQL Server
* **ORM**: Entity Framework Core (EF Core)
* **Security & Auth**: ASP.NET Core Identity
* **Frontend**: HTML5, CSS3, Bootstrap 5, Razor Views
