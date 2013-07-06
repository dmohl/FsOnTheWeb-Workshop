module Problem2Tests  
  
open NUnit.Framework     
    
[<TestFixture>]      
type Problem2__When_finding_sum_of_even_valued_terms_in_the_fibonacci_sequence_up_to_89 () =     
    [<Test>]    
    member this.should_return_sum_of_44 () =    
        let result = Problem2.FindSum 89    
        Assert.AreEqual(44, result)          
  
[<TestFixture>]      
type Problem2__When_finding_sum_of_even_valued_terms_in_the_fibonacci_sequence_below_4_million () =     
    [<Test>]    
    member this.should_return_sum_of_4613732 () =    
        let result = Problem2.FindSum 4000000    
        Assert.AreEqual(4613732, result) 