# P4

## Getting Started (Local)

1. Open the solution in **Visual Studio 2022**  
2. Run the project using **HTTPS**

---

## Docker Instructions

### 1. Build Docker Image

```bash
docker build -t p4-image .
```

### 2. Create Docker Network (if needed)

```bash
docker network create p4-network
```

### 3. Run Docker Container

```bash
docker run --network p4-network \
           --name p4-container \
           --add-host=host.docker.internal:host-gateway \
           -p 5000:8080 \
           p4-image
```

- `p4-network`: Name of the Docker network  
- `p4-container`: Desired name for the container  
- `5000`: Local host port (you can change this)  
- `8080`: Internal container port  
- `p4-image`: Name of the Docker image you built

### 4. Start Existing Container

If the container already exists, just start it:

```bash
docker start p4-container
```
