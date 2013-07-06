// Variables, Values, Functions, and Type Inference 
let myNum = 5
let myString = "Hi"
let addOne num = num + 1
printfn "%i" <| addOne myNum
// myNum <- 6 // This is not valid because it's an immutable value
let mutable myNum2 = 5
myNum2 <- 6
printfn "%i" <| addOne myNum2

// Arrays, Lists, and Sequences
[| for i in 0 .. 3 -> (i,i*i) |]
[ for i in 0 .. 3 -> (i,i*i) ]
seq { for i in 0 .. 3 -> (i,i*i) }

// Pattern Matching
let num = 1
match num with
| 1 -> printfn "The number is 1"
| 2 -> printfn "The number is 2"
| i -> printfn "The number is %O" i

// Units of Measure
[<Measure>] type g
[<Measure>] type kg
let convertg2kg (x : float<g>) = x / 1000.0<g/kg>
let grams = 22.2<g>
printfn "%A grams is %A kilograms" grams (convertg2kg grams)

// Mixing Classes and Functions
type MyClass() =
    let add num1 num2 = num1 + num2
    member x.AddOne num = 
        add num 1

let myClass = MyClass()
myClass.AddOne 5 |> printfn "The new number is %i"

// Async Computation Expression and MailboxProcessor
type Agent<'T> = MailboxProcessor<'T>
let agents =
    [ for i in 0 .. 1000 ->
       Agent.Start(fun inbox ->
         async { while true do
                   let! msg = inbox.Receive()
                   printfn "agent %d got message '%s'" i msg } ) ]
 
for agent in agents do
    agent.Post "ping!"
