# N5 Permissions API

A comprehensive employee permissions management API built with .NET 8, implementing Clean Architecture, CQRS, and modern development patterns.

## Project Status

**CHALLENGE COMPLETED 100%**

- Application fully operational
- SQL Server connected with Docker
- All endpoints working
- Clean Architecture implemented
- CQRS with MediatR functioning
- Elasticsearch and Kafka configured
- Unit Tests passing

## Implemented Features

### Main Services
- **Request Permission** - `POST /api/permissions/request`
- **Modify Permission** - `PUT /api/permissions/modify/{id}`
- **Get Permissions** - `GET /api/permissions`

### Technical Features
- Web API with .NET Core 8
- SQL Server with Entity Framework (Docker)
- Elasticsearch for document indexing
- Apache Kafka for messaging
- Repository Pattern + Unit of Work
- CQRS pattern with MediatR
- Clean Architecture (Domain, Application, Infrastructure, API)
- Logging with Serilog on all endpoints
- Complete Unit Testing
- Full Docker containerization

## Project Architecture

```
N5.PermissionsAPI/
├── src/
│   ├── N5.Permissions.Domain/        # Domain entities, interfaces
│   ├── N5.Permissions.Application/   # CQRS, DTOs, Handlers
│   ├── N5.Permissions.Infrastructure/ # Repositories, EF, External services
│   └── N5.Permissions.API/          # Controllers, Program.cs, Middleware
├── tests/
│   ├── N5.Permissions.UnitTests/     # Unit tests
│   └── N5.Permissions.IntegrationTests/ # Integration tests
└── docker-compose.yml               # Complete infrastructure
```

## Technology Stack

- **Framework**: .NET 8
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core
- **Patterns**: CQRS with MediatR
- **Logging**: Serilog
- **Search**: Elasticsearch 7.17
- **Messaging**: Apache Kafka
- **Containerization**: Docker & Docker Compose
- **Documentation**: Swagger/OpenAPI
- **Testing**: xUnit

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Installation and Execution

1. **Clone the repository**
```bash
git clone [REPOSITORY_URL]
cd N5.PermissionsAPI
```

2. **Start the infrastructure with Docker Compose**

**Option A: Start all services at once**
```bash
docker-compose up -d
```

**Option B: Start services individually**
```bash
# Start SQL Server
docker-compose up -d sqlserver

# Start Elasticsearch
docker-compose up -d elasticsearch

# Start Kafka (includes Zookeeper dependency)
docker-compose up -d kafka
```

3. **Verify services are running**
```bash
docker-compose ps
```

4. **Run the application**
```bash
dotnet run --project src/N5.Permissions.API
```

5. **Access the application**
- **Swagger UI**: https://localhost:7051/swagger
- **Health Check**: https://localhost:7051/health
- **API Base**: https://localhost:7051/api

## Docker Services Configuration

The project includes a complete `docker-compose.yml` with the following containerized services:

### SQL Server
```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  container_name: n5-sqlserver
  environment:
    - ACCEPT_EULA=Y
    - SA_PASSWORD=Password123!
    - MSSQL_PID=Express
  ports:
    - "1433:1433"
  volumes:
    - sqlserver_data:/var/opt/mssql
  networks:
    - n5-network
  restart: unless-stopped
```

### Elasticsearch
```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:7.17.0
  container_name: n5-elasticsearch
  environment:
    - discovery.type=single-node
    - xpack.security.enabled=false
    - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
  ports:
    - "9200:9200"
  volumes:
    - elasticsearch_data:/usr/share/elasticsearch/data
  networks:
    - n5-network
```

### Apache Kafka
```yaml
kafka:
  image: confluentinc/cp-kafka:7.4.0
  container_name: n5-kafka
  depends_on:
    - zookeeper
  ports:
    - "9092:9092"
  environment:
    KAFKA_BROKER_ID: 1
    KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
    KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
    KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
    KAFKA_AUTO_CREATE_TOPICS_ENABLE: true
  networks:
    - n5-network
```

### Zookeeper (Kafka Coordinator)
```yaml
zookeeper:
  image: confluentinc/cp-zookeeper:7.4.0
  container_name: n5-zookeeper
  environment:
    ZOOKEEPER_CLIENT_PORT: 2181
    ZOOKEEPER_TICK_TIME: 2000
  networks:
    - n5-network
```

### Individual Docker Commands

You can choose between three approaches to run the services:

#### Option 1: Docker Compose - All Services
```bash
docker-compose up -d
```

#### Option 2: Docker Compose - Individual Services
```bash
# Start SQL Server only
docker-compose up -d sqlserver

# Start Elasticsearch only
docker-compose up -d elasticsearch

# Start Kafka and Zookeeper
docker-compose up -d kafka
```

#### Option 3: Pure Docker Commands
If you prefer to run services with individual Docker commands:

**SQL Server:**
```bash
docker run -d \
  --name n5-sqlserver \
  -e ACCEPT_EULA=Y \
  -e SA_PASSWORD=Password123! \
  -e MSSQL_PID=Express \
  -p 1433:1433 \
  --network n5-network \
  mcr.microsoft.com/mssql/server:2022-latest
```

**Elasticsearch:**
```bash
docker run -d \
  --name n5-elasticsearch \
  -e discovery.type=single-node \
  -e xpack.security.enabled=false \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  -p 9200:9200 \
  --network n5-network \
  docker.elastic.co/elasticsearch/elasticsearch:7.17.0
```

**Zookeeper:**
```bash
docker run -d \
  --name n5-zookeeper \
  -e ZOOKEEPER_CLIENT_PORT=2181 \
  -e ZOOKEEPER_TICK_TIME=2000 \
  --network n5-network \
  confluentinc/cp-zookeeper:7.4.0
```

**Kafka:**
```bash
docker run -d \
  --name n5-kafka \
  -e KAFKA_BROKER_ID=1 \
  -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
  -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
  -e KAFKA_AUTO_CREATE_TOPICS_ENABLE=true \
  -p 9092:9092 \
  --network n5-network \
  --depends-on n5-zookeeper \
  confluentinc/cp-kafka:7.4.0
```

**Create Docker Network:**
```bash
docker network create n5-network
```

## Available Endpoints

| Method | Endpoint | Description | Status |
|--------|----------|-------------|---------|
| POST | `/api/permissions/request` | Request new permission | Working |
| PUT | `/api/permissions/modify/{id}` | Modify existing permission | Working |
| GET | `/api/permissions` | Get all permissions | Working |
| GET | `/api/permissions/{id}` | Get permission by ID | Working |
| GET | `/api/permissiontypes` | Get permission types | Working |
| GET | `/health` | System health check | Working |

## Usage Examples

### Create a Permission
```json
POST /api/permissions/request
{
  "employeeForename": "John",
  "employeeSurname": "Doe",
  "permissionTypeId": 1,
  "permissionDate": "2024-03-15T00:00:00"
}
```

### Modify a Permission
```json
PUT /api/permissions/modify/1
{
  "employeeForename": "John",
  "employeeSurname": "Smith",
  "permissionTypeId": 2,
  "permissionDate": "2024-03-20T00:00:00"
}
```

## Database

### Main Tables
- **PermissionTypes**: Predefined permission types
- **Permissions**: Employee permission records
- **__EFMigrationsHistory**: Migration control

### Predefined Permission Types
1. Vacation Leave
2. Sick Leave
3. Personal Leave
4. Maternity/Paternity Leave
5. Emergency Leave

## Testing

### Run Unit Tests
```bash
dotnet test tests/N5.Permissions.UnitTests
```

### Run Integration Tests
```bash
dotnet test tests/N5.Permissions.IntegrationTests
```

### Run All Tests
```bash
dotnet test
```

## Monitoring and Logging

- **Logs**: Automatically generated in the `logs/` folder
- **Health Checks**: Available at `/health`
- **Metrics**: Elasticsearch automatically indexes operations
- **Events**: Kafka records all permission operations

## Configuration

### Main Environment Variables

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=N5PermissionsDB;User Id=sa;Password=Password123!;TrustServerCertificate=true"
  },
  "ElasticsearchSettings": {
    "Uri": "http://localhost:9200",
    "DefaultIndex": "permissions"
  },
  "KafkaSettings": {
    "BootstrapServers": "localhost:9092",
    "Topic": "permissions-operations"
  }
}
```

## Troubleshooting

### Issue: SQL Server connection error
**Solution**: Verify Docker is running and SQL Server container is active:
```bash
docker-compose ps
docker-compose logs sqlserver
```

### Issue: Port already in use
**Solution**: Change ports in `docker-compose.yml` or stop the service using the port.

### Issue: Database not created
**Solution**: Database is created automatically. Check application logs for more details.

### Issue: Elasticsearch not responding
**Solution**: Check Elasticsearch container status and logs:
```bash
docker logs n5-elasticsearch
curl -X GET "localhost:9200/_cluster/health"
```

### Issue: Kafka connection problems
**Solution**: Verify Kafka and Zookeeper are running:
```bash
docker logs n5-kafka
docker logs n5-zookeeper
```

## Contributing

1. Fork the project
2. Create a feature branch (`git checkout -b feature/new-functionality`)
3. Commit your changes (`git commit -am 'Add new functionality'`)
4. Push to the branch (`git push origin feature/new-functionality`)
5. Create a Pull Request

### Technical References

- **Elasticsearch**: 
  - [Docker Setup Guide](https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html)
  - [.NET Client (NEST)](https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.x/nest.html)
- **SQL Server Express**: [Docker Hub - Microsoft SQL Server](https://hub.docker.com/_/microsoft-mssql-server)
- **Kafka**: [N5 Kafka Configuration Guide](https://www.notion.so/n5now/Kafka-242a5fd883bf49ffa77190fb16eb4ecf#74a1076feed24ea482c804f54483773d)
- **Serilog**: [Official Documentation](https://serilog.net/)
- **CQRS Pattern**: [Microsoft Azure Architecture Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Completion Status

**Last Update**: July 26, 2025  
**Status**: COMPLETED AND WORKING  
**Version**: FINAL

---

*Developed as part of the N5 Challenge - Complete implementation of a permission management API with modern architecture and development best practices.*
