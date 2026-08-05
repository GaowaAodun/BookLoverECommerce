# BookLoverECommerce

A microservices-based e-commerce platform built with ASP.NET Core 10, Clean Architecture, PostgreSQL, YARP API Gateway, and Docker.

## Tech Stack

- ASP.NET Core 10
- Clean Architecture
- PostgreSQL
- Entity Framework Core
- YARP API Gateway
- Docker & Docker Compose
- JWT Authentication

## Solution Structure

```
BookLoverECommerce
│
├── ApiGateway
├── BuildingBlocks
├── Services
│   ├── Auth
│   ├── Products
│   ├── Cart
│   └── Price
└── docs
```


# BookLoverECommerce

A microservices-based e-commerce backend built with **ASP.NET Core 10**, **PostgreSQL**, **RabbitMQ**, **MassTransit**, **YARP API Gateway**, and **Docker Compose**.

The project currently includes:

- Auth Service
- Products Service
- Price Service
- Cart Service
- API Gateway
- PostgreSQL database per service
- RabbitMQ messaging
- Swagger UI
- Health checks
- Automatic database migrations and seed data

---

## Architecture

```text
Client
  |
  v
API Gateway
  |
  +--> Auth API ------> Auth PostgreSQL
  |
  +--> Products API --> Products PostgreSQL
  |         |
  |         +--> RabbitMQ --> Price API
  |
  +--> Price API -----> Price PostgreSQL
  |
  +--> Cart API ------> Cart PostgreSQL
```

Docker Compose manages the containers. RabbitMQ is used for asynchronous communication between services.

---

## Prerequisites

Install the following software before running the project:

- Git
- Docker Desktop
- .NET 10 SDK

Verify the installation:

```bash
git --version
docker --version
docker compose version
dotnet --version
```

The .NET SDK version must support `net10.0`.

---

## 1. Clone the repository

```bash
git clone https://github.com/GaowaAodun/BookLoverECommerce.git
cd BookLoverECommerce
```

Switch to the shared development branch:

```bash
git checkout dev
git pull origin dev
```

<!-- If the latest Docker work has not yet been merged into `dev`, switch to the container configuration branch instead:

```bash
git checkout feature/container_configuration
git pull origin feature/container_configuration
``` -->

---

## 2. Create the environment file

The real `.env` file is not committed because it contains local passwords and secrets.

Copy the example file:

```bash
cp .env.example .env
```

Open `.env` and provide local development values:

```env
AUTH_DB_PASSWORD=auth_password
PRODUCTS_DB_PASSWORD=products_password
PRICE_DB_PASSWORD=price_password
CART_DB_PASSWORD=cart_password

RABBITMQ_USERNAME=booklover
RABBITMQ_PASSWORD=booklover_password

ADMIN_USER_PASSWORD=Admin123!BookLover

JWT_KEY=replace_with_a_long_random_key
```

Generate a JWT key:

```bash
openssl rand -base64 64
```

Copy the generated value into:

```env
JWT_KEY=PASTE_THE_GENERATED_VALUE_HERE
```

Do not commit `.env`.

---

## 3. Build the .NET solution

From the repository root:

```bash
dotnet restore
dotnet build BookLoverECommerce.slnx
```

Expected result:

```text
Build succeeded.
0 Error(s)
```

---

## 4. Validate Docker Compose

Run:

```bash
docker compose config
```

This checks:

- YAML indentation
- missing environment variables
- invalid service configuration
- Dockerfile paths

If the command prints the resolved configuration without an error, the Compose file is valid.

---

## 5. Build and start the complete system

Run:

```bash
docker compose up -d --build
```

On the first run, Docker will:

- download the .NET 10 SDK and runtime images
- download PostgreSQL 18
- download RabbitMQ with the management UI
- build all API images
- create the Docker network
- create persistent volumes
- start all databases
- start RabbitMQ
- start the APIs
- apply EF Core migrations
- seed initial data
- start the API Gateway

The first build may take several minutes.

---

## 6. Check container status

```bash
docker compose ps
```

Expected containers:

```text
booklover-rabbitmq
booklover-auth-db
booklover-products-db
booklover-price-db
booklover-cart-db
booklover-auth-api
booklover-products-api
booklover-price-api
booklover-cart-api
booklover-api-gateway
```

The databases and RabbitMQ should eventually show `healthy`.

To include stopped or failed containers:

```bash
docker compose ps -a
```

---

## 7. View logs

All services:

```bash
docker compose logs --tail=100
```

Follow logs continuously:

```bash
docker compose logs -f
```

One service:

```bash
docker compose logs -f auth-api
docker compose logs -f products-api
docker compose logs -f price-api
docker compose logs -f cart-api
docker compose logs -f api-gateway
docker compose logs -f rabbitmq
```

Press `Ctrl+C` to stop following logs.

---

## Service URLs

| Service | URL |
|---|---|
| API Gateway | `http://localhost:5092` |
| Auth API | `http://localhost:5001` |
| Products API | `http://localhost:5002` |
| Price API | `http://localhost:5003` |
| Cart API | `http://localhost:5004` |
| RabbitMQ Management UI | `http://localhost:15672` |

---

## Swagger UI

Swagger is enabled in the `Development` and `Docker` environments.

Open:

```text
http://localhost:5001/swagger/
http://localhost:5002/swagger/
http://localhost:5003/swagger/
http://localhost:5004/swagger/
```

The API Gateway does not currently require its own Swagger UI.

---

## Health checks

Test each API:

```bash
curl -i http://localhost:5001/health
curl -i http://localhost:5002/health
curl -i http://localhost:5003/health
curl -i http://localhost:5004/health
curl -i http://localhost:5092/health
```

Expected result:

```text
HTTP/1.1 200 OK
Healthy
```

---

## RabbitMQ

Open the management interface:

```text
http://localhost:15672
```

Use the credentials from `.env`:

```text
Username: value of RABBITMQ_USERNAME
Password: value of RABBITMQ_PASSWORD
```

Check RabbitMQ from Terminal:

```bash
docker compose exec rabbitmq rabbitmq-diagnostics -q ping
```

Expected:

```text
Ping succeeded
```

The Products service publishes product events through RabbitMQ, and the Price service consumes the configured product event queue.

---

## Database access

### Auth database

```bash
docker compose exec auth-db \
  psql -U auth_user -d booklover_auth
```

### Products database

```bash
docker compose exec products-db \
  psql -U products_user -d booklover_products
```

### Price database

```bash
docker compose exec price-db \
  psql -U price_user -d booklover_price
```

### Cart database

```bash
docker compose exec cart-db \
  psql -U cart_user -d booklover_cart
```

Inside PostgreSQL:

```sql
\dt
```

Exit:

```sql
\q
```

---

## API Gateway testing

Test Products through the Gateway:

```bash
curl -i http://localhost:5092/api/products
```

Without a valid JWT, a protected endpoint may return:

```text
401 Unauthorized
```

This is expected and confirms that the Gateway route reached the protected service.

If the direct API returns `401` but the Gateway returns `404`, check the YARP route and path transform configuration.

---

## Stopping the project

Stop containers while keeping database and RabbitMQ data:

```bash
docker compose down
```

Start again later:

```bash
docker compose up -d
```

---

## Rebuilding after pulling new code

```bash
git checkout dev
git pull origin dev

dotnet restore
dotnet build BookLoverECommerce.slnx

docker compose up -d --build
```

Docker will reuse cached layers and rebuild only what changed.

To rebuild one service:

```bash
docker compose up -d \
  --build \
  --force-recreate \
  products-api
```

Replace `products-api` with another service name when needed.

---

## Resetting the complete development environment

This removes all containers, databases, RabbitMQ queues, users, messages, and persistent volumes:

```bash
docker compose down -v
```

Then recreate everything:

```bash
docker compose up -d --build
```

Use `down -v` only when the existing development data can be deleted.

---

## Common problems

### A service is not reachable

Check its status:

```bash
docker compose ps -a
```

Read its logs:

```bash
docker compose logs --tail=200 SERVICE_NAME
```

Example:

```bash
docker compose logs --tail=200 auth-api
```

### Database tables do not exist

Confirm that the service has:

```text
Database__ApplyMigrationsOnStartup=true
```

in `compose.yaml`.

Then inspect the API logs for migration errors.

### Auth API reports missing admin configuration

Confirm that `.env` contains:

```env
ADMIN_USER_PASSWORD=Admin123!BookLover
```

and that `compose.yaml` passes the required `AdminUser__...` values to the Auth container.

### RabbitMQ does not accept the configured user

RabbitMQ credentials are created only when its volume is initialized. If an old RabbitMQ volume contains different credentials, reset only RabbitMQ:

```bash
docker compose stop rabbitmq
docker compose rm -f rabbitmq
docker volume rm booklover-rabbitmq-data
docker compose up -d rabbitmq
```

This removes existing RabbitMQ users, queues, and messages.

### Port already in use

Check the port:

```bash
lsof -i :5001
lsof -i :5002
lsof -i :5003
lsof -i :5004
lsof -i :5092
lsof -i :5432
lsof -i :5433
lsof -i :5434
lsof -i :5435
lsof -i :5672
lsof -i :15672
```

Stop the conflicting process or change the host port in `compose.yaml`.

---

## Important repository files

The repository should contain:

```text
compose.yaml
.env.example
.dockerignore
.gitignore
Directory.Packages.props
BookLoverECommerce.slnx
README.md
src/
```

Each web project should contain its own Dockerfile:

```text
src/ApiGateway/BookLoverECommerce.ApiGateway/Dockerfile
src/Services/Auth/BookLoverECommerce.Auth.Api/Dockerfile
src/Services/Products/BookLoverECommerce.Products.Api/Dockerfile
src/Services/Price/BookLoverECommerce.Price.Api/Dockerfile
src/Services/Cart/BookLoverECommerce.Cart.Api/Dockerfile
```

Do not commit:

```text
.env
bin/
obj/
real passwords
JWT secrets
local database files
```

---

## Quick start

For a new developer:

```bash
git clone https://github.com/GaowaAodun/BookLoverECommerce.git
cd BookLoverECommerce

git checkout dev
git pull origin dev

cp .env.example .env
# Edit .env and generate JWT_KEY

docker compose config
docker compose up -d --build
docker compose ps
```

Then open:

```text
http://localhost:5001/swagger/
http://localhost:5002/swagger/
http://localhost:5003/swagger/
http://localhost:5004/swagger/
http://localhost:15672
http://localhost:5092
```

## Current Status

- ✅ Project Structure
- ✅ Solution Architecture
- ✅ Clean Architecture
- ✅ BuildingBlocks
- ✅ Central Package Management
- 🟡 (Almost Complete) Auth Service Skeleton 
- ✅ Products Service Skeleton
- ✅ Cart Service Skeleton
- ✅ Price Service Skeleton
- ✅ API Gateway(YARP)
- 🟡 (Almost Complete)Docker & Docker Compose
- 🟡 (Initial Complete)RabbitMQ & MassTransit


## License

MIT
