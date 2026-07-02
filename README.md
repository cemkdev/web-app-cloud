> [!IMPORTANT]
> ## 🚧 Active Refactoring & Modernization
>
> This repository is currently undergoing a major refactoring and modernization effort.
>
> The current focus is improving architecture, maintainability, consistency, code quality, and aligning the solution with modern .NET engineering practices.
>
> Additional platform improvements, including observability and deployment enhancements, are planned after this phase is completed.

---


# Project Overview
This project is a modern **e-commerce web application** built with a robust **.NET 6 (Onion Architecture)** API and an **Angular 19** frontend.  

**Purpose:** to provide a structure where users can view products, build a cart, and place orders, while advanced role-based visibility and access management ensures full control on the admin side.

## Highlights
- **Architecture:** Onion Architecture + CQRS (Command/Query separation)  
- **Authentication:** JWT (HttpOnly cookie)  
- **Authorization:** Role & permission management (dynamic endpoint scanning / role-based visibility & access)  
- **Application Flows:** Cart & order; admin panel (customers, products, orders, users/roles)  
- **UI Design:** Bootstrap (user) + Angular Material (admin)  
- **Integrations:** Social login (Google, Facebook), QR code for product/stock ops  
- **File Management:** Azure Blob Storage or Local Storage (optional)  
- **Infrastructure:** PostgreSQL database (containerized)

## Extra Features
- Unique filename generation for uploads  
- Custom authentication design + Silent Refresh mechanism  
- Sample SignalR connection (product-added notifications)  
- Password reset via e-mail link  
- Mail service  
- Angular route guards (role-based)  
- HTTP error handler interceptor (centralized error handling)  
- Dynamic component loading  
- Custom design system: centralized SCSS + HTML class-driven styling (minimal component CSS)

## Next Improvements
- Logging: Serilog + structured JSON, correlation ID  
- Exception handling: global middleware + ProblemDetails; layered exceptions; consistent HTTP status  
- Validation: FluentValidation (partial)  
- Inventory: post-order stock update (Message Queue)  
- Order states: shipping/delivered flow (Message Queue)  
- Guest cart: Redis-based cache  
- UI: Home, product detail, cart quantity indicators, user profile, customers  
- Rating: user rating flow  
- 2FA (optional)

---

# Demo
- A **demo version** of this project will be available at:  [**demo**](https://webappang.azurewebsites.net)  *(Login requires third-party cookies enabled in your browser.)*
- <sub>Usernames: owner, editor, user | Password: 123</sub>
- **Limitations (Demo):**
  - External login (Facebook/Google) is **disabled**. (The Google login button is hidden if no GoogleClientId is provided.)
  - **Reset password** and **update order status** e-mail features are **disabled**.  

---

# Running Locally
> **Prerequisite — Angular HTTPS certificates**  
> This project requires running the Angular client over **HTTPS** during local development.  
> Make sure to complete step **3** in the Required steps section below.

### Required steps
1. Use the repository’s [`docker-compose.yml`](./docker-compose.yml) to start the **PostgreSQL** container.  
2. **Verify client and API port alignment (WebAppAPI.API HTTPS port):**  
   Make sure the client and the API use the same **HTTPS port**. Visual Studio may assign different ports if `launchSettings.json` is local-only, so check and adjust if needed. If the ports already match, no change is required.  
   - `WebAppClient/src/environments/environment.ts` *(used by `ng serve` / local dev)*  
   - `WebAppClient/src/environments/environment.prod.ts` *(used by production build)*  
     - `allowedDomains: ["localhost:<API_PORT>"]`  
     - `baseUrl: "https://localhost:<API_PORT>/api"`  
     - `baseSignalRUrl: "https://localhost:<API_PORT>/"`  
   - `WebAppAPI/Presentation/WebAppAPI.API/appsettings*.json`  
     - `"BaseStorageUrl": "https://localhost:<API_PORT>"`
3. Angular **HTTPS** run guide: see [`docs/angular-https.md`](docs/angular-https.md).  
4. In Visual Studio, set **WebAppAPI.API** as the sole *Startup Project*.

### Optional steps
- To test **external login**, enter your **own Google/Facebook client id and secret** values in `appsettings`.  
- To test the **mail system**, provide your **own e-mail account and settings** in `appsettings`.  
- **Storage** can also be used locally; Azure is not mandatory. You may leave it empty.


---
