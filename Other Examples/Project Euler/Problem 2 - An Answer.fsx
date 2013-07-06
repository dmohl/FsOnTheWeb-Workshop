module Problem2  
  
let FindSum max =  
    let rec getFibonacciSumForEvens lastValue currentValue accumulator =  
        match lastValue + currentValue with  
        | sum when sum >= max -> accumulator  
        | sum when sum % 2 = 0 ->  
            getFibonacciSumForEvens currentValue sum (accumulator + sum)  
        | sum -> getFibonacciSumForEvens currentValue sum accumulator  
    getFibonacciSumForEvens 0 1 0  