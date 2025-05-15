import React, { useState, useEffect, useRef } from 'react';

const FibonacciGenerator = () => {
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
            <FibonacciRenderer sequence={sequence} />
        </div>
    );
};

const FibonacciRenderer = () => {
    const [isGenerating, setIsGenerating] = useState(false);
    const [currentNumber, setCurrentNumber] = useState(null);
    const [count, setCount] = useState(0);
    const timerRef = useRef(null);
    const fibRef = useRef({
        prev1: 10,
        prev2: 15
    });

    useEffect(() => {
        return () => {
            if (timerRef.current) clearInterval(timerRef.current);
        };
    }, []);

    const startGenerating = () => {
        setCount(1);
        setCurrentNumber(15);
        fibRef.current = {
            prev1: 10,
            prev2: 15
        };
        setIsGenerating(true);
        let generatingTresholdCount = 300;
        let currentCount = 1;
        
        timerRef.current = setInterval(() => {
            currentCount++;
            setCount(currentCount);
            
            const nextFib = fibRef.current.prev1 + fibRef.current.prev2;
            
            fibRef.current = {
                prev2: fibRef.current.prev1,
                prev1: nextFib
            };
            
            setCurrentNumber(nextFib);
            
            if (currentCount > 20 && timerRef.current) {
                clearInterval(timerRef.current);
                timerRef.current = setInterval(() => {
                    currentCount++;
                    setCount(currentCount);
                    
                    const nextFib = fibRef.current.prev1 + fibRef.current.prev2;
                    fibRef.current = {
                        prev2: fibRef.current.prev1,
                        prev1: nextFib
                    };
                    
                    setCurrentNumber(nextFib);
                }, generatingTresholdCount); // Faster interval for larger numbers
            }
            
        }, 800); // Initial interval
    };

    const stopGenerating = () => {
        if (timerRef.current) {
            clearInterval(timerRef.current);
            timerRef.current = null;
        }
        setIsGenerating(false);
    };

    const handleButtonClick = () => {
        if (isGenerating) {
            stopGenerating();
        } else {
            startGenerating();
        }
    };

    return (
        <div className="fibonacci-container">
            <h1>Fibonacci Sequence Generator</h1>
            
            <div className="fibonacci-display">
                {currentNumber !== null && (
                    <div className="current-number">
                        {currentNumber}
                    </div>
                )}
                {count > 0 && (
                    <div className="progress">
                        Number {count} in sequence
                    </div>
                )}
            </div>

            <button 
                className="generate-button"
                onClick={handleButtonClick}
            >
                {isGenerating ? 'Stop' : 'Generate'}
            </button>
        </div>
    );
};

export default FibonacciGenerator;