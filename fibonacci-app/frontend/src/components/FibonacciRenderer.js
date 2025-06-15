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
        <div className="fibonacci-generator">
            <FibonacciRenderer />
        </div>
    );
};

const FibonacciRenderer = () => {
    const [isGenerating, setIsGenerating] = useState(false);
    const [fibSequence, setFibSequence] = useState([]);
    const [count, setCount] = useState(0);
    const timerRef = useRef(null);
    const gridRef = useRef(null);
    const fibRef = useRef({
        prev1: 10,
        prev2: 5
    });

    useEffect(() => {
        return () => {
            if (timerRef.current) clearInterval(timerRef.current);
        };
    }, []);
    
    useEffect(() => {
        if (gridRef.current && fibSequence.length > 0) {
            gridRef.current.scrollTop = gridRef.current.scrollHeight;
        }
    }, [fibSequence]);

    const calculateNextFib = () => {
        const nextFib = fibRef.current.prev1 + fibRef.current.prev2;
        fibRef.current = {
            prev2: fibRef.current.prev1,
            prev1: nextFib
        };
        return nextFib;
    };

    const startGenerating = () => {
        setFibSequence([5, 10, 15]);
        setCount(3);
        fibRef.current = {
            prev1: 10,
            prev2: 5
        };
        setIsGenerating(true);
        
        let currentCount = 3;
        let initialInterval = 300; // Start slower
        let reducedInterval = 50;  // Then speed up
        
        timerRef.current = setInterval(() => {
            currentCount++;
            setCount(currentCount);
            
            const nextFib = calculateNextFib();
            
            setFibSequence(prevSequence => [...prevSequence, nextFib]);
            
            if (currentCount > 15 && timerRef.current) {
                clearInterval(timerRef.current);
                timerRef.current = setInterval(() => {
                    currentCount++;
                    setCount(currentCount);
                    
                    const nextFib = calculateNextFib();
                    setFibSequence(prevSequence => [...prevSequence, nextFib]);
                    
                    if (currentCount > 50) {
                        for (let i = 0; i < 10000; i++) {
                            Math.sqrt(i * nextFib);
                        }
                    }
                    
                }, reducedInterval);
            }
            
        }, initialInterval);
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
                {count > 0 && (
                    <div className="progress">
                        Generating {count} Fibonacci numbers
                    </div>
                )}
                
                <div 
                    className="sequence-grid"
                    ref={gridRef} // Add ref to the grid container
                    style={{
                        display: 'flex',
                        flexWrap: 'wrap',
                        gap: '10px',
                        marginTop: '20px',
                        maxHeight: '60vh',
                        overflowY: 'auto',
                        padding: '10px',
                        justifyContent: 'center',
                        behavior: 'smooth'
                    }}
                >
                    {fibSequence.map((number, index) => (
                        <div 
                            key={index} 
                            className="fibonacci-number"
                            style={{
                                backgroundColor: `hsl(${(index * 10) % 360}, 70%, 60%)`,
                                fontSize: `${Math.min(20, 16 + index/10)}px`,
                                padding: '10px',
                                borderRadius: '5px',
                                color: 'white',
                                fontWeight: 'bold',
                                minWidth: '80px',
                                textAlign: 'center',
                                boxShadow: '0 2px 4px rgba(0, 0, 0, 0.2)',
                                animation: 'fadeIn 0.3s ease-in'
                            }}
                        >
                            {number}
                        </div>
                    ))}
                </div>
            </div>

            <button 
                className="generate-button"
                onClick={handleButtonClick}
                style={{
                    marginTop: '20px',
                    padding: '10px 20px',
                    backgroundColor: isGenerating ? '#e74c3c' : '#2ecc71',
                    color: 'white',
                    border: 'none',
                    borderRadius: '5px',
                    cursor: 'pointer',
                    fontSize: '16px'
                }}
            >
                {isGenerating ? 'Stop Generation' : 'Start Generating'}
            </button>

            <style jsx>{`
                @keyframes fadeIn {
                    from { opacity: 0; transform: scale(0.8); }
                    to { opacity: 1; transform: scale(1); }
                }
            `}</style>
        </div>
    );
};

export default FibonacciGenerator;