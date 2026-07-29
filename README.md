# FintechSolution

.NET microservices platform for auth, accounts, transactions, payments, notifications, and wallets, fronted by a YARP API gateway and integrated with RabbitMQ (MassTransit).

## Architecture

```text
Client → APIGateway (YARP)
           ├─ AuthService
           ├─ AccountService
           ├─ TransactionService ──HTTP──► AccountService
           ├─ PaymentService ──outbox──► RabbitMQ ──► AccountService / NotificationService
           ├─ WalletService ──HTTP──► AccountService
           └─ NotificationService ◄── RabbitMQ (auth / tx / payment events)
```

- **AuthService** — register, login, refresh, email verify (ASP.NET Identity + JWT)
- **AccountService** — accounts and balances (canonical ledger)
- **TransactionService** — deposit / withdraw / transfer; settles balances via AccountService
- **PaymentService** — payments with idempotency, retries, and transactional outbox
- **NotificationService** — email / SMS / push / in-app (event consumers)
- **WalletService** — thin JWT facade over AccountService (no second ledger)
- **APIGateway** — reverse proxy + JWT validation
- **SharedKernel** — shared events, middleware, auth constants

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (PaymentService targets **.NET 9**)
- SQL Server (LocalDB / full instance with Trusted Connection)
- [RabbitMQ](https://www.rabbitmq.com/) on `localhost:5672` (default `guest` / `guest`)

## Quick start

```bash
# Restore & build
dotnet restore FintechSolution.sln
dotnet build FintechSolution.sln

# Run tests
dotnet test FintechSolution.sln

# Start a service (example)
dotnet run --project src/AuthService/AuthService.csproj --launch-profile http
```

Or use the VS Code / Cursor compound launch config **Run All Services**.

### Local ports (http profiles)

| Service | URL | Swagger |
|---------|-----|---------|
| APIGateway | http://localhost:5086 | http://localhost:5086/ |
| AuthService | http://localhost:5258 | http://localhost:5258/swagger |
| AccountService | http://localhost:5018 | http://localhost:5018/swagger |
| TransactionService | http://localhost:5050 | http://localhost:5050/swagger |
| PaymentService | http://localhost:5150 | http://localhost:5150/swagger |
| NotificationService | http://localhost:5132 | http://localhost:5132/swagger |
| WalletService | http://localhost:5219 | http://localhost:5219/swagger |

Gateway routes (examples):

- `/api/auth/**` → AuthService  
- `/api/accounts/**` → AccountService  
- `/api/transactions/**` → TransactionService  
- `/api/payments/**` → PaymentService (`/api/v1/Payments/...`)  
- `/api/wallets/**` → WalletService  
- `/api/notifications/**` → NotificationService  

## Configuration

Shared JWT (dev):

- Issuer: `FintechAuthServer`
- Audience: `FintechApiClient`
- Key: see each service `appsettings.json` (same value across services)

RabbitMQ section (Auth, Account, Transaction, Payment, Notification):

```json
"RabbitMq": {
  "Host": "localhost",
  "Username": "guest",
  "Password": "guest",
  "VirtualHost": "/"
}
```

SQL connection strings are per-service in `appsettings.json` / `appsettings.Development.json`. Apply EF migrations as needed for Auth, Account, Transaction, and Payment databases.

## Smoke flow

See [docs/e2e-smoke.http](docs/e2e-smoke.http):

1. Register / login (Auth)  
2. Create account  
3. Create + process payment  
4. Confirm balance debit (Account consumer on `PaymentSucceeded`)  
5. Notifications via RabbitMQ consumers  

## Tests

| Project | Notes |
|---------|--------|
| `AccountService.Tests` | Unit + integration; OBN live tests need Auth on `:5258` |
| `PaymentService.Tests` | Unit + integration (in-memory EF) |

```bash
dotnet test FintechSolution.sln
```

## Solution layout

```text
FintechSolution.sln
src/
  APIGateway/
  AuthService/
  AccountService/
  AccountService.Tests/
  TransactionService/
  PaymentService/
  PaymentService.Tests/
  NotificationService/
  WalletService/
  SharedKernel/
docs/
  e2e-smoke.http
```

## Security note

JWT keys and connection strings in appsettings are for **local development only**. Rotate secrets and use proper configuration/secret stores before deploying.
