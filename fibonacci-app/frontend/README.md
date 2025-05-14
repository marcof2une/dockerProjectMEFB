# Fibonacci App Frontend

This project is a React application that renders Fibonacci sequences. It is designed to compare performance in both a virtual machine and a Dockerized environment.

## Getting Started

To get started with the frontend application, follow these steps:

### Prerequisites

Make sure you have the following installed:

- Node.js (version 14 or higher)
- npm (Node package manager)

### Installation

1. Clone the repository:

   ```
   git clone <repository-url>
   cd fibonacci-app/frontend
   ```

2. Install the dependencies:

   ```
   npm install
   ```

### Running the Application

To run the application in development mode, use the following command:

```
npm start
```

This will start the development server and open the application in your default web browser.

### Building for Production

To build the application for production, run:

```
npm run build
```

This will create an optimized build of the application in the `build` directory.

### Usage

Once the application is running, you can input a number to generate the Fibonacci sequence up to that number. The results will be displayed on the screen.

## Folder Structure

- `src/`: Contains the source code for the application.
  - `App.js`: Main component that manages the state and renders the FibonacciRenderer component.
  - `components/`: Contains reusable components.
    - `FibonacciRenderer.js`: Component that calculates and displays the Fibonacci sequence.
  - `index.js`: Entry point for the React application.
  - `styles/`: Contains CSS styles for the application.
    - `styles.css`: Styles for the components.
- `public/`: Contains static files.
  - `index.html`: HTML template for the React application.

## Contributing

If you would like to contribute to this project, please fork the repository and submit a pull request with your changes.

## License

This project is licensed under the MIT License. See the LICENSE file for details.