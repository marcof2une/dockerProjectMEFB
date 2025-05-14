# Fibonacci Sequence Application

This project is a web application that renders Fibonacci sequences using React for the frontend and Nginx as a server. The application is designed to allow performance comparisons between a virtual machine setup and a Dockerized environment.

## Project Structure

The project consists of the following main components:

- **Frontend**: A React application that calculates and displays Fibonacci sequences.
  - `src/App.js`: Main component that imports and renders the FibonacciRenderer component.
  - `src/components/FibonacciRenderer.js`: Functional component that generates and displays the Fibonacci sequence based on user input.
  - `src/index.js`: Entry point for the React application.
  - `src/styles/styles.css`: CSS styles for the application.
  - `public/index.html`: HTML template for the React application.
  - `package.json`: Configuration file for npm, listing dependencies and scripts.
  - `README.md`: Documentation for the frontend part of the project.

- **Nginx**: Configuration for serving the React application.
  - `default.conf`: Nginx configuration file to set up the server and handle routing.

- **Docker**: Docker setup for containerizing the application.
  - `Dockerfile.frontend`: Dockerfile for building the frontend application.
  - `Dockerfile.nginx`: Dockerfile for setting up the Nginx server.

- **Docker Compose**: Configuration for running multi-container Docker applications.
  - `docker-compose.yml`: Defines services for the frontend and Nginx.

- **VM Setup**: Instructions for setting up the project in a virtual machine environment.
  - `setup-instructions.md`: Detailed steps to run the application in a VM.

## Setup Instructions

### Prerequisites

- Node.js and npm installed for the frontend setup.
- Docker and Docker Compose installed for the containerized setup.

### Running the Application

#### Frontend

1. Navigate to the `frontend` directory.
2. Install dependencies:
   ```
   npm install
   ```
3. Start the application:
   ```
   npm start
   ```

#### Dockerized Environment

1. Ensure Docker is running.
2. Build and run the application using Docker Compose:
   ```
   docker-compose up --build
   ```

### Performance Comparison

To compare performance, run the application in both the virtual machine and Dockerized environments, and monitor the response times and resource usage.

## License

This project is licensed under the MIT License.