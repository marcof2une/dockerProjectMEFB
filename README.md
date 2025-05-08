---
# 🐳 Introduction to Docker

Welcome to this beginner-friendly guide to **Docker**! This repository serves as an introduction to containerization using Docker, covering basic concepts, commands, and tools like Dockerfiles and Docker Compose.

---

## 📚 What is Docker?

**Docker** is an open-source platform that automates the deployment, scaling, and management of applications inside **containers**. Containers are lightweight, isolated environments that include everything needed to run an application: code, runtime, system tools, system libraries, and settings.

Containers are different from virtual machines (VMs) because they share the host operating system’s kernel and do not require full OS per application, making them more efficient in terms of resource use.

---

## 🧰 Basic Docker Commands

| Command | Description |
|--------|-------------|
| `docker --version` | Check Docker version |
| `docker info` | Display system-wide information |
| `docker run hello-world` | Run your first Docker container |
| `docker pull <image>` | Pull an image from Docker Hub |
| `docker images` | List all local Docker images |
| `docker ps` | List running containers |
| `docker ps -a` | List all containers (including stopped ones) |
| `docker stop <container_id>` | Stop a running container |
| `docker rm <container_id>` | Remove a stopped container |
| `docker rmi <image_name>` | Remove a Docker image |
| `docker build -t <name>:<tag> .` | Build an image from a Dockerfile |
| `docker run -d -p 80:80 nginx` | Run a container in detached mode and map ports |

---

## 📄 Dockerfile vs. Docker Compose

### 📝 Dockerfile

A **Dockerfile** is a text document that contains all the commands a user could call on the command line to assemble an image. Using `docker build`, you can create an image from it.

**Example:**
```Dockerfile
FROM ubuntu:latest
RUN apt-get update
CMD ["echo", "Hello from Ubuntu!"]
```

### 📋 Docker Compose

**Docker Compose** is a tool for defining and running multi-container Docker applications. With a `docker-compose.yml` file, you can configure your application’s services, networks, and volumes.

**Example:**
```yaml
version: '3'
services:
  web:
    image: nginx
    ports:
      - "80:80"
  db:
    image: mysql:5.7
    environment:
      MYSQL_ROOT_PASSWORD: example
```

**Run with:**  
```bash
docker-compose up
```

| Feature | Dockerfile | Docker Compose |
|--------|------------|----------------|
| Purpose | Build a single image | Orchestrate multiple containers |
| File Type | `Dockerfile` | `docker-compose.yml` |
| Use Case | Creating custom images | Managing multi-service apps |

---

## 🖼️ Useful Docker Images to Try

You can pull these images using `docker pull <image-name>`:

| Image Name | Description |
|------------|-------------|
| `hello-world` | A simple test image |
| `nginx` | Lightweight web server |
| `redis` | In-memory data structure store |
| `mysql` | Popular open-source relational database |
| `postgres` | Powerful open-source object-relational database |
| `mongo` | NoSQL document-oriented database |
| `alpine` | Minimal Linux distribution |
| `httpd` | Apache HTTP Server |
| `library/ubuntu` | Official Ubuntu image |
| `node` | JavaScript runtime for building apps |
| `python` | Python programming language image |

### Example Usage:

```bash
docker pull ubuntu
docker run -it ubuntu bash
```

This will start a new interactive shell session inside an Ubuntu container.

---

## 📁 Repository Structure

```
.
├── README.md
├── Dockerfile.example
├── docker-compose.example.yml
└── examples/
    └── simple-app/
```

Each folder contains hands-on examples to get you started quickly.

---

## ✅ Getting Started

1. Install Docker from [https://www.docker.com/get-started](https://www.docker.com/get-started)
2. Clone this repo:  
   ```bash
   git clone https://github.com/your-username/intro-to-docker.git
   ```
3. Explore the examples and try running some containers!
---

