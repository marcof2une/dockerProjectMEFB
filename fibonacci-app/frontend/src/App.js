import React, { useState } from 'react';
import FibonacciRenderer from './components/FibonacciRenderer';
import './styles/styles.css';

const App = () => {
    const [sequence, setSequence] = useState([]);

    const handleGenerate = (n) => {
        const fib = generateFibonacci(n);
        setSequence(fib);
    };

    const generateFibonacci = (n) => {
        let fib = [0, 1];
        for (let i = 2; i < n; i++) {
            fib[i] = fib[i - 1] + fib[i - 2];
        }
        return fib.slice(0, n);
    };

    return (
        <div className="App">
            <h1>Fibonacci Sequence Generator</h1>
            <input
                type="number"
                min="1"
                placeholder="Enter a number"
                onChange={(e) => handleGenerate(e.target.value)}
            />
            <FibonacciRenderer sequence={sequence} />
        </div>
    );
};

export default App;