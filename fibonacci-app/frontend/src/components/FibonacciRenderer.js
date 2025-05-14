import React, { useState } from 'react';

const FibonacciRenderer = () => {
    const [input, setInput] = useState('');
    const [sequence, setSequence] = useState([]);

    const generateFibonacci = (n) => {
        let fib = [0, 1];
        for (let i = 2; i < n; i++) {
            fib[i] = fib[i - 1] + fib[i - 2];
        }
        return fib.slice(0, n);
    };

    const handleInputChange = (event) => {
        setInput(event.target.value);
    };

    const handleSubmit = (event) => {
        event.preventDefault();
        const num = parseInt(input);
        if (!isNaN(num) && num > 0) {
            setSequence(generateFibonacci(num));
        }
    };

    return (
        <div>
            <h1>Fibonacci Sequence Generator</h1>
            <form onSubmit={handleSubmit}>
                <input
                    type="number"
                    value={input}
                    onChange={handleInputChange}
                    placeholder="Enter a number"
                />
                <button type="submit">Generate</button>
            </form>
            <div>
                <h2>Fibonacci Sequence:</h2>
                <p>{sequence.join(', ')}</p>
            </div>
        </div>
    );
};

export default FibonacciRenderer;