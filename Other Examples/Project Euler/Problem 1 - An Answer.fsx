module Problem1  
  
let GetSumOfMultiplesOf3And5 max =  
    seq{3..max-1} |> Seq.fold(fun acc number ->  
                        (if (number % 3 = 0 || number % 5 = 0) then   
                            acc + number else acc)) 0 