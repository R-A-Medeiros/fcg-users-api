# FCG - UsersAPI

Microsserviço de **Usuários** da FIAP Cloud Games (FCG).

Responsável por cadastro, autenticação (geração de token JWT) e autorização de usuários.
Ao cadastrar um usuário, publica o evento `UserCreatedEvent` no RabbitMQ, que dispara o
e-mail de boas-vindas no NotificationsAPI.

## Tecnologias

- .NET 10 (ASP.NET Core Web API)
- Arquitetura em camadas (Domain / Application / Infrastructure / Presentation)
- PostgreSQL (via EF Core + Npgsql)
- RabbitMQ (via MassTransit)
- JWT (chave simétrica **compartilhada** entre os serviços)
- BCrypt (hash de senha)
- AutoMapper e FluentValidation

## Estrutura da solução

```
users-api/
├── Dockerfile
├── db/init.sql                      # schema idempotente (fonte usada no compose e no k8s)
├── k8s/                             # Deployment, Service, ConfigMap, Secret
└── src/
    ├── Core/
    │   ├── FIAP.Domain/             # entidades, enums, contratos (repos, security)
    │   ├── FIAP.Application/        # use cases, AutoMapper, eventos (UserCreatedEvent)
    │   └── FIAp.Exception/          # exceções de domínio
    ├── infra/
    │   └── FIAP.Infrastructure/     # EF Core (UsersDbContext + migrations), repositórios, JWT, BCrypt
    └── presentation/
        └── FIAP.UsersAPI/           # Controllers, Program.cs, configurações
```

## Endpoints

| Método | Rota                 | Descrição                                             |
|--------|----------------------|-------------------------------------------------------|
| POST   | `/api/User/register` | Cria um usuário e publica `UserCreatedEvent`          |
| POST   | `/api/Login/login`   | Autentica e retorna um token JWT                      |
| GET    | `/health`            | Health check                                          |
| GET    | `/swagger`           | Swagger UI                                            |
| GET    | `/scalar/v1`         | Documentação interativa da API (OpenAPI)              |

### Exemplo de cadastro

```bash
curl -X POST http://localhost:5001/api/User/register \
  -H "Content-Type: application/json" \
  -d '{ "name": "Alice", "email": "alice@example.com", "password": "Secret@123" }'
```

> A senha exige no mínimo 8 caracteres, com letra maiúscula, minúscula, número e um caractere
> especial entre `! @ # $ % & * .`

### Exemplo de login

```bash
curl -X POST http://localhost:5001/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{ "email": "alice@example.com", "password": "Secret@123" }'
```

## Variáveis de ambiente

A configuração do JWT fica na seção `Settings:Jwt` (lida pelo código). Em contêiner/k8s,
use o separador `__` (ex.: `Settings__Jwt__SigningKey`).

| Variável                        | Descrição                                        | Exemplo                                                                 |
|---------------------------------|--------------------------------------------------|-------------------------------------------------------------------------|
| `ConnectionStrings__Postgres`   | String de conexão do PostgreSQL (**secret**)     | `Host=postgres;Port=5432;Database=fcg_users;Username=postgres;Password=postgres` |
| `Settings__Jwt__SigningKey`     | Chave simétrica do JWT (**secret**, min 32 chars)| `CHANGE_ME_super_secret_shared_key_min_32_chars_long!!`                 |
| `Settings__Jwt__Issuer`         | Emissor do token                                 | `FCG`                                                                   |
| `Settings__Jwt__Audience`       | Audiência do token                               | `FCG.Clients`                                                           |
| `Settings__Jwt__ExpiresMinutes` | Validade do token em minutos                     | `60`                                                                    |
| `Settings__Seed__AdminName`     | Nome do administrador semeado na subida          | `Administrator`                                                         |
| `Settings__Seed__AdminEmail`    | E-mail do administrador semeado                  | `admin@fcg.com`                                                         |
| `Settings__Seed__AdminPassword` | Senha do administrador semeado (**secret**)      | `Admin@123`                                                             |
| `RabbitMq__Host`                | Host do RabbitMQ                                  | `rabbitmq`                                                              |
| `RabbitMq__Username`            | Usuário do RabbitMQ (**secret**)                 | `guest`                                                                 |
| `RabbitMq__Password`            | Senha do RabbitMQ (**secret**)                   | `guest`                                                                 |
| `ASPNETCORE_URLS`               | URL de escuta do Kestrel                          | `http://+:8080`                                                         |

> Na subida, o serviço cria um usuário administrador (idempotente) a partir de `Settings__Seed__*`.
> É ele quem pode cadastrar, editar e remover jogos no CatalogAPI. Se o e-mail ou a senha não
> forem configurados, o seed é ignorado.

> A `Settings__Jwt__SigningKey` **deve ser idêntica** à `Jwt:SecretKey` do CatalogAPI,
> que valida o token emitido aqui. O token é emitido com `Issuer=FCG` e
> `Audience=FCG.Clients` e carrega o `UserIdentifier` no claim `NameIdentifier`.

## Banco de dados (migrations)

Um script SQL idempotente já vem pronto em [`db/init.sql`](db/init.sql) e é aplicado
automaticamente pelo `docker-compose.yml` do repositório
[`fcg-orchestration`](https://github.com/R-A-Medeiros/fcg-orchestration) (ver o README dele). Para aplicar
manualmente:

```bash
# 1. Crie o database (uma vez)
psql -h localhost -U postgres -c "CREATE DATABASE fcg_users;"

# 2. Aplique o schema
psql -h localhost -U postgres -d fcg_users -f db/init.sql
```

Alternativamente, com as ferramentas do EF Core instaladas (o DbContext fica na
Infrastructure):

```bash
dotnet ef database update --project src/infra/FIAP.Infrastructure --startup-project src/presentation/FIAP.UsersAPI
```

Para regenerar o script após alterar o modelo:

```bash
dotnet ef migrations script --idempotent \
  --project src/infra/FIAP.Infrastructure \
  --startup-project src/presentation/FIAP.UsersAPI \
  -o db/init.sql
```

### Tabela `users`

| Coluna           | Tipo                      | Observação                 |
|------------------|---------------------------|----------------------------|
| `Id`             | `bigint` (identity)       | PK                         |
| `Name`           | `varchar(150)`            |                            |
| `Email`          | `varchar(200)`            | índice único               |
| `Password`       | `text`                    | hash BCrypt                |
| `UserIdentifier` | `uuid`                    | índice único; vai no token |
| `Role`           | `varchar(50)`             |                            |
| `CreatedAt`      | `timestamp with time zone`|                            |

## Executando localmente

```bash
dotnet run --project src/presentation/FIAP.UsersAPI
```

Requer um PostgreSQL e um RabbitMQ acessíveis (veja o `docker-compose.yml` do repositório
[`fcg-orchestration`](https://github.com/R-A-Medeiros/fcg-orchestration)).

## Docker

```bash
docker build -t fcg/users-api:latest .
```

## Kubernetes

Os manifestos estão em [`k8s/`](k8s/):

```bash
kubectl apply -f k8s/
kubectl get pods
```

- `configmap.yaml` — configurações não sensíveis (host do RabbitMQ, issuer/audience/expiração do JWT)
- `secret.yaml` — dados sensíveis (connection string, chave de assinatura do JWT, credenciais do RabbitMQ)
- `deployment.yaml` — Deployment do serviço (1 réplica, probes de liveness/readiness em `/health`)
- `service.yaml` — Service ClusterIP (`users-api:80` -> container `:8080`)
