module Problem1Tests  
  
open NUnit.Framework   
  
[<TestFixture>]    
type FindPrimes2__When_Getting_Sum_Of_Multiples_Of_3_And_5_to_a_max_number_of_10 () =   
    [<Test>]  
    member this.should_return_sum_of_23 () =  
        let result = Problem1.GetSumOfMultiplesOf3And5(10)  
        Assert.AreEqual(23, result)  