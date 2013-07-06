(*! Calculations !*)
(*![
    <p>
    When it comes to WebSharper&trade; applications, the full expressive power of
    the F# language is at your service.  This sample implements the
    mandatory "Hello world" of functional programming: the factorial
    function.  It also demonstrates:
    </p>
    <ul>
        <li>How to use pattern matching</li>
        <li>How to use higher-order functions</li>
        <li>How to use WebSharper&trade; to enhance existing markup</li>
    </ul>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module Calculations =

    (*![
        <p>You can define the factorial function using pattern matching:</p>
     ]*)
    [<JavaScript>]
    let rec Fact1 n =
        match n with
        | n when n < 2 -> 1
        | n -> n * Fact1 (n - 1)

    (*![
        <p>Alternatively, you can use a higher-order function (<code>fold</code>) with a
        function argument (the multiplication operator), and initial value,
        and an array comprehension to create the array of values that are
        to be multiplied.</p>
     ]*)
    [<JavaScript>]
    let Fact2 n = Array.fold ( * ) 1  [| 1 .. n |]
    (*![
        <p>You can compute the factorial using both functions to check that
        they indeed produce the same result.  These are then inserted into
        a message and displayed in the label we called Output in the ASPX
        markup.</p>
     ]*)
    [<JavaScript>]
    let Main () =
        let input = Input [Attr.Type "text"]
        let output = Pre []
        let button =
            Input [Attr.Type "button"; Attr.Value "Factorial"]
            |>! OnClick (fun e args ->
                let v = int input.Value
                let msg =
                    "Fact1 = "
                    + string (Fact1 v)
                    + ". Fact2 = "
                    + string (Fact2 v)
                output.Text <- msg)
        Div [
            input
            button
            output
        ]

type CalculationsViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = Calculations.Main () :> _