# SAIMRS — Agricultural Inventory Management System

Offline Windows desktop app for the **Provincial Agri-Industrial Center (PAIC), Upper Irasan**.
Built with **C# / .NET 10**, **Blazor Hybrid on WPF** (BlazorWebView), **SQLite** (EF Core),
**QuestPDF** (PDF reports) and **ClosedXML** (Excel reports), following **Clean Architecture**.

---

## 1. Requirements

| What | Notes |
|---|---|
| Windows 10 or 11 | WPF + WebView2 (WebView2 is already installed on Windows 10/11) |
| .NET 10 SDK | https://dotnet.microsoft.com/download |
| Visual Studio 2026 | Workloads: **.NET desktop development** and **ASP.NET and web development** |
| Internet (first build only) | To download NuGet packages. The app itself runs fully offline. |

## 2. Run it

1. Open `SAIMRS.slnx` in Visual Studio.
2. Right-click **SAIMRS.Desktop** → **Set as Startup Project**.
3. Press **F5**.

On the first run you will see the **Setup** screen: create the admin account
(name, username, password, security question and answer). After that, the app always opens on **Login**.

> **Demo data:** Debug builds (F5) load sample items, suppliers, beneficiaries and transactions into an
> empty database so every screen has something to show. Release builds start clean.
> Change this in `src/SAIMRS.Desktop/App.xaml.cs` (`SeedSampleData`).

### Command line

```bash
dotnet build SAIMRS.slnx
dotnet run --project src/SAIMRS.Desktop
dotnet test tests/SAIMRS.Domain.Tests
```

## 3. Database & migrations

The database is a single file: `%LocalAppData%\SAIMRS\saimrs.db`
(profile photos: `%LocalAppData%\SAIMRS\photos`).

The app works immediately without migrations (it creates the tables on first start).
**Before using it for real records, create the initial migration** so future schema changes can be applied
without losing data:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/SAIMRS.Infrastructure --output-dir Persistence/Migrations
```

Then delete the test database (`%LocalAppData%\SAIMRS\saimrs.db`) once, and run the app — it now applies
migrations automatically at startup (`DbInitializer`). For later changes, add a new migration the same way
(e.g. `dotnet ef migrations add AddItemBarcode ...`).

## 4. Package versions

Packages use floating versions (e.g. `10.0.*`) so restore picks the newest patch.
If a package fails to restore, pin a version you can see in NuGet, e.g. in
`SAIMRS.Desktop.csproj` change `Microsoft.AspNetCore.Components.WebView.Wpf` to the latest `10.0.x`.

**QuestPDF license:** the app uses the free *Community* license (allowed for organizations with under
USD 1M annual revenue, which covers government and academic use). See `QuestPdfReportGenerator.cs`.

---

## 5. Architecture

```
┌──────────────────────────────────────────────┐
│  SAIMRS.Desktop  (WPF + BlazorWebView, UI)   │  composition root
└───────────────┬──────────────────────────────┘
                │ depends on
┌───────────────▼──────────────┐   ┌─────────────────────────────┐
│  SAIMRS.Application          │◄──│  SAIMRS.Infrastructure      │
│  use cases, DTOs, interfaces │   │  SQLite/EF Core, QuestPDF,  │
└───────────────┬──────────────┘   │  ClosedXML, hashing, backup │
                │                  └─────────────────────────────┘
┌───────────────▼──────────────┐
│  SAIMRS.Domain               │   depends on nothing
│  entities, business rules    │
└──────────────────────────────┘
```

**Dependency rule:** references point inward only. The Domain has no packages at all.
The Application layer declares interfaces (e.g. `IInventoryItemRepository`, `IPdfReportGenerator`,
`IDatabaseBackupService`); the Infrastructure layer implements them; the Desktop layer wires everything
together in `App.xaml.cs`.

| Layer | Contains | Key idea |
|---|---|---|
| **Domain** | `InventoryItem`, `StockTransaction`, `Beneficiary`, `Supplier`, `Category`, `PerishableBatch`, `AdminAccount`, `SystemSettings` | Business rules live *inside* the entities. Stock can only change through `Receive()`, `Issue()` and `AdjustToCount()`, which also create the transaction record — so quantity and history can never get out of sync. |
| **Application** | One folder per feature (`Features/Inventory`, `Features/Transactions`, …) with a service, DTOs and requests | Services return `Result` / `Result<T>` instead of throwing, so the UI just shows `result.Error`. |
| **Infrastructure** | `AppDbContext`, entity configurations, repositories, `Pbkdf2PasswordHasher`, `SqliteBackupService`, `QuestPdfReportGenerator`, `ClosedXmlReportGenerator` | Swappable technology. Decimals are stored as REAL so SQLite can filter/sort quantities. |
| **Desktop** | Razor pages and components, layouts, WPF host, UI services (toasts, dialogs, file pickers) | Each service call runs in its **own DI scope** (`Use<TService>().Run(...)` in `AppComponentBase`) so a DbContext never lives longer than one operation. |

### Folder structure

```
SAIMRS/
├── SAIMRS.slnx
├── Directory.Build.props
├── src/
│   ├── SAIMRS.Domain/
│   │   ├── Common/        BaseEntity, IArchivable, DomainException, Guard, Units
│   │   ├── Entities/      AdminAccount, Category, Supplier, Beneficiary, InventoryItem,
│   │   │                  StockTransaction, PerishableBatch, SystemSettings
│   │   └── Enums/         TransactionType, StockStatus, ExpiryStatus, BeneficiaryStatus,
│   │                      CropType, SecurityQuestion
│   ├── SAIMRS.Application/
│   │   ├── Common/        Result, PagedResult, PasswordPolicy, Formatting, LookupItem,
│   │   │   └── Interfaces/  IUnitOfWork, IPasswordHasher, IDateTimeProvider, ICurrentUserService,
│   │   │                    IProfilePhotoStorage, IPdf/IExcelReportGenerator, IDatabaseBackupService
│   │   ├── Repositories/  repository interfaces + filters
│   │   ├── Features/      Auth, Profile, Inventory, Transactions, Perishables, Categories,
│   │   │                  Suppliers, Beneficiaries, Alerts, Dashboard, Reports, Settings, Backup
│   │   └── DependencyInjection.cs
│   ├── SAIMRS.Infrastructure/
│   │   ├── Persistence/   AppDbContext, UnitOfWork, Configurations/, Repositories/, Seed/DbInitializer
│   │   ├── Reports/       Pdf/QuestPdfReportGenerator, Excel/ClosedXmlReportGenerator
│   │   ├── Security/      Pbkdf2PasswordHasher
│   │   ├── Storage/       LocalProfilePhotoStorage
│   │   ├── Backup/        SqliteBackupService
│   │   ├── Services/      AppPaths, SystemDateTimeProvider
│   │   └── DependencyInjection.cs
│   └── SAIMRS.Desktop/
│       ├── App.xaml(.cs), MainWindow.xaml(.cs)
│       ├── Services/      AuthStateService, ToastService, ConfirmService, AppEvents, PageState,
│       │                  FileDialogService, AppLifetimeService
│       ├── wwwroot/       index.html, css/app.css, lib/ (fonts + Font Awesome, offline)
│       └── Components/
│           ├── Routes.razor, _Imports.razor
│           ├── Layout/    MainLayout, AuthLayout, EmptyLayout, Sidebar, TopBar
│           ├── Shared/    AppComponentBase, Modal, ToastHost, ConfirmHost, StatCard, Badge,
│           │              Pagination, EmptyState, ProgressBar, BarChart, DonutChart, Avatar, BrandLogo
│           └── Pages/     Index, Dashboard, Auth/ (Setup, Login, ForgotPassword), Inventory/,
│                          Transactions/, Perishables/, Categories/, Suppliers/, Beneficiaries/,
│                          Reports/, Alerts/, Settings/ (General, Account, Data Backup)
└── tests/
    └── SAIMRS.Domain.Tests/   stock rules, weighted cost, expiry status, login lockout
```

---

## 6. Business rules (quick reference)

**Account**
- One admin account, created on the first-run Setup screen. There is no Users page.
- Passwords and the security answer are hashed with PBKDF2-SHA256 (100,000 iterations).
- Security answers ignore capital letters and extra spaces.
- 5 wrong login attempts (or 5 wrong security answers) lock that screen for 5 minutes.
- Forgot password: username → security question → new password.
- Settings → Account: change name, username, photo, password and security question. The current password is required for username, password and security-question changes.

**Inventory**
- A new item's opening quantity is recorded as the first Stock IN.
- After that, quantity changes only through Transactions.
- Status is calculated, not stored: **Out of Stock** (qty 0), **Low Stock** (qty ≤ threshold), **In Stock**.
- Unit cost is an optional acquisition cost. Each Stock IN with a cost updates it using the weighted average. Reports show "value at cost". There are no sales.

**Transactions**
- **IN:** optional supplier, which updates the supplier's "last delivery".
- **OUT:** a beneficiary is required. The beneficiary must be active. You cannot issue more than is on hand.
- **ADJUST:** enter the physically counted quantity and a reason. The system records the difference.
- Transactions are never edited or deleted (audit trail).

**Records**
- Items, suppliers and beneficiaries are **archived** instead of deleted. Their history stays in reports, and they can be restored.
- A category cannot be deleted while it has items (including archived items).

**Perishables and alerts**
- Perishable batches are independent from inventory items.
- Batch status: Expired, Critical (≤ 7 days), Near Expiry (≤ the Settings value, default 30), Safe.
- Alerts are generated automatically from low stock and expiry dates. Stock received in the last 24 hours shows as information only.

**Reports**
- Six reports: Inventory Summary, Stock Movement, Low Stock, Expiry, Distribution, Beneficiary Masterlist.
- Each report is previewed on screen and exported to PDF or Excel. The Save dialog lets you choose the location.

**Backup and restore**
- Backup copies the database to any location you choose (e.g. a USB drive), using SQLite's online backup API.
- Restore validates the file, keeps a safety copy in `%LocalAppData%\SAIMRS\backups\before-restore`, replaces the data, then restarts the app.

## 7. Replacing the logo

Put your logo in `src/SAIMRS.Desktop/wwwroot/img/logo.png` and edit
`Components/Shared/BrandLogo.razor` to use `<img src="img/logo.png" />` instead of the seedling icon.
It is used by the sidebar and the login/setup screens.

## 8. Third-party licenses

- Font Awesome Free (icons CC BY 4.0, fonts SIL OFL 1.1): `wwwroot/lib/fontawesome/LICENSE.txt`
- DM Sans / DM Mono (SIL OFL 1.1): `wwwroot/lib/fonts/OFL-LICENSE.txt`
- QuestPDF (Community license), ClosedXML (MIT), EF Core (MIT)
