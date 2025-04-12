# P4

## Getting Started (Local)
### Open in Visual Studio 2022 and run using HTTPS

---

## Docker Instructions

### 1. Build Docker Image (using Dockerfile)

```bash
docker build -t p4-image .
```

### 2. Create Docker Network (if not yet created)

```bash
docker network create p4-network
```

### 3. Run All Nodes Except Frontend in Docker

```bash
docker run --network p4-network \
           --name p4-backend \
           --add-host=host.docker.internal:host-gateway \
           -p 5000:8080 \
           p4-image
```

### 4. Run Frontend Locally

#### Run your frontend normally (e.g., npm run dev or dotnet run)

### 5. Start Existing Container

```bash
docker start p4-backend
```
