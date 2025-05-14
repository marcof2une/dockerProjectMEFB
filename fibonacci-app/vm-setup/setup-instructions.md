# Virtual Machine Setup Instructions for Fibonacci App

## Prerequisites
- Ensure you have a virtual machine set up with a compatible operating system (e.g., Ubuntu, CentOS).
- Install Node.js and npm on your virtual machine.
- Install Docker and Docker Compose if you plan to run the application in a Dockerized environment.

## Steps to Set Up the Project

1. **Clone the Repository**
   Open your terminal and clone the Fibonacci app repository:
   ```
   git clone <repository-url>
   cd fibonacci-app
   ```

2. **Install Frontend Dependencies**
   Navigate to the frontend directory and install the required dependencies:
   ```
   cd frontend
   npm install
   ```

3. **Build the Frontend Application**
   Build the React application to prepare it for production:
   ```
   npm run build
   ```

4. **Set Up Nginx**
   If you are using Nginx to serve the application, ensure that the Nginx configuration file is correctly set up. You can find the configuration in the `nginx/default.conf` file.

5. **Run the Application**
   You can run the application either directly using npm or through Docker.

   **Directly using npm:**
   ```
   npm start
   ```

   **Using Docker:**
   - Build the Docker images:
     ```
     docker-compose build
     ```
   - Start the services:
     ```
     docker-compose up
     ```

6. **Access the Application**
   Open your web browser and navigate to `http://localhost:3000` to view the Fibonacci application.

## Additional Notes
- Make sure to adjust firewall settings if necessary to allow traffic on the specified ports.
- For performance comparisons, monitor resource usage in both the virtual machine and Docker environments.

## Troubleshooting
- If you encounter issues, check the logs for both the frontend and Nginx containers using:
  ```
  docker-compose logs
  ```
- Ensure that all dependencies are correctly installed and that there are no version conflicts.