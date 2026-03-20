# SumiFolio - ArtbookStore

## Project description

SumiFolio Bookstore is a database-driven web application built with ASP.NET Core MVC. The application functions as an online bookstore where users can browse products, add items to a cart, and place orders.

The system supports two primary roles:

* Customer – can browse products, add items to a cart, and place orders.

* Admin – can manage products, categories, inventory, and orders through an administrative dashboard.

The application uses Entity Framework Core with Azure SQL Database for persistent data storage and ASP.NET Core Identity for authentication and role-based authorization.

Key architectural patterns include:

- MVC architecture

- Entity Framework Core Code First

- Role-based authorization

- ViewModels for presentation logic

- ViewComponents for reusable UI components

## Features

The application includes the following functionality:

Public features

- Product listing with pagination

- Category filtering

- Product detail pages

- Responsive navigation layout

- Authentication (login / register)

Customer features

- Shopping cart functionality

- Add products to cart

- Update item quantity

- Remove items from cart

- Checkout process

- Order history

- Order detail view

Admin features

- Admin dashboard with statistics

- Product management (CRUD)

- Category management

- Inventory management

- Order management

- Order status updates

- Low stock monitoring

- Revenue visualization chart

## Technologies used

The project uses the following technologies:

- ASP.NET Core MVC

- Entity Framework Core

- Azure SQL Database

- ASP.NET Core Identity

- Razor Views

- Chart.js

- Bootstrap Icons

- Entity Framework Migrations

## Project structure

The project follows a layered MVC architecture:

```sh
Controllers/
    Handles HTTP requests and application flow

Models/
    Domain entities and data models

Models/ViewModels/
    Presentation-specific models for views

Views/
    Razor views for UI rendering

Data/
    DbContext and Identity seed logic

ViewComponents/
    Reusable UI components (CartSummary)

Migrations/
    Entity Framework Core migration files

Areas/
    Identity UI (Login, Register, Logout)

wwwroot/
    Static assets such as CSS, JavaScript and images

Program.cs
    Application startup configuration

appsettings.json
    Application configuration
```

## System architecture

The application is structured into two main layers:

### Backend

The backend is responsible for business logic, database interaction, authentication, and request handling.

#### Controllers

Controllers process HTTP requests and coordinate application logic.

Examples:

- ProductsController – manages product browsing and administration.

- OrdersController – handles shopping cart and order processing.

- CategoriesController – manages product categories.

- AdminController – provides the admin dashboard.

Models

Models represent domain entities stored in the database.

Examples:

- Product

- Category

- Order

- OrderItem

- ApplicationUser

#### ViewModels

ViewModels structure data specifically for presentation.

Examples:

- AdminDashboardViewModel

- CartSummaryViewModel

#### Data layer

The data layer handles database configuration and entity relationships.

Important files:

- ApplicationDbContext.cs – Entity Framework database context

- IdentitySeed.cs – creates default roles and admin user

#### Database

The database is managed using Entity Framework Core Code First migrations.

Key features:

- relational data model

- migration-based schema versioning

- integration with ASP.NET Identity tables

### Frontend

The frontend is responsible for the user interface and interaction.

#### Razor Views

Razor views generate HTML on the server side.

Examples:

- Product listing pages

- Admin dashboard

- Order pages

- Category management pages

#### Layout and navigation

The shared layout \_Layout.cshtml defines:

- navigation menu

- authentication UI

- cart display

- global message handling

#### ViewComponents

Reusable UI logic is implemented using ViewComponents.

Example:

- CartSummary – displays current cart item count and total value.

#### Static assets

Frontend styling and scripts are stored in:

```sh
wwwroot/
```

This includes:

- CSS styles

- JavaScript files

- images

- Bootstrap resources

#### UI Libraries

The frontend uses:

- Bootstrap Icons

- Chart.js (for dashboard visualizations)

## Dependencies

The project uses the following NuGet packages:

- Microsoft.AspNetCore.Identity.EntityFrameworkCore

- Microsoft.AspNetCore.Identity.UI

- Microsoft.EntityFrameworkCore.SqlServer

- Microsoft.EntityFrameworkCore.Tools

- Microsoft.EntityFrameworkCore.Design

- Microsoft.VisualStudio.Web.CodeGeneration.Design

## Database Setup

The application uses Entity Framework Core Code First migrations.

To create and update the database run:

```sh
dotnet ef database update
```

This will create the database schema based on the migrations included in the repository.

Existing migrations include:

- InitialCreate

- UpdateModels

- AddIdentity

- FixProductModel

- AddOrders

- AddAuthorFieldToProduct

- AddCategoryImage

- AddOrderUserNavigation

- AddSlugToCategory

## Database seeding

The application automatically seeds initial data during startup through the IdentitySeed class.

The seed process performs the following actions:

- Creates the roles Admin and Customer

- Creates an administrator account if it does not exist

- Assigns the Admin role to the administrator user

Seeding only runs in the Development environment to avoid modifying production data.

Admin credentials are read from configuration:

```sh
SeedAdmin:Email
SeedAdmin:Password
```

These values should be stored in **User Secrets** and seeding runs only in Development environment to prevent modifying production data.

## Configuration

The application requires the following configuration values.

### Database Connection

Defined in appsettings.json:

```sh
ConnectionStrings:DefaultConnection
```

This connection string points to the Azure SQL database used by the application.

### Inventory Settings

Inventory configuration is defined using the InventorySettings model:

```sh
InventorySettings:
  LowStockThreshold: 5
```

This value determines when products are considered low in stock in the admin dashboard.

## Running the project

1. Clone the repository

```sh
git clone https://github.com/SaraM47/ArtbookStore.git
```

2. Navigate to the project folder

```sh
cd ArtbookStore.Web
```

3. Restore dependencies

```sh
dotnet restore
```

4. Apply database migrations

```sh
dotnet ef database update
```

5. Run the application

```sh
dotnet run
```

The application will start and can be accessed in the browser.

## Security

Authentication and authorization are implemented using ASP.NET Core Identity.

Security features include:

- Password hashing

- Role-based authorization

- Anti-forgery tokens for form submissions

- Restricted admin endpoints

- Ownership checks for order access

## Future improvements

If the application is further developed, several improvements and additional features could be implemented to enhance usability, functionality, and overall system quality.

## Potential enhancements

* Search functionality to allow users to quickly find products

* Sorting options, for example by price, to improve browsing experience

* Image upload functionality for products instead of manually entering ImageURL paths

* Clear stock status indicators on product detail pages, for example showing whether a product is in stock or sold out

* Wishlist or favorites feature for logged-in users to save artbooks

* Discount code system for customers, enabling percentage-based price reductions

* Deployment to a cloud environment for better scalability, accessibility, and production readiness