(*! Calculator !*)
(*![
    <p>This sample implements a basic calculator. It demonstrates how to:</p>
    <ul>
        <li>Develop small-scale applications</li>
        <li>Splitting the logic and presentation into separate functions within a single module.</li>
    </ul>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module Calculator =

    [<JavaScript>]
    let Main ()  =

        (*![
             <p>There are three registers that represent the state of the calculator:
             the old number, the current number, and the arithmetic
             operation. The latter register can either contain a value or
             be empty - this is modeled by giving it the F# option type.</p>
         ]*)
        let (onum, num, op) =
            (ref 0, ref 0, ref None)

        (*![
             <p>The most crucial component of the calculator is the display screen.
             Let us model it by a simple HTML input box with a unique id. For
             later convenience we also provide a function that updates the
             display by showing the current state of the `num` register.</p>
        ]*)
        let display = Input [Attr.Type "Text"; Attr.Value "0"]

        let updateDisplay () = display.Value <- string !num

        (*![
             <p>You can model the behavior of the calculator with a few functions. Let D (digit entry) update the
             `num` register, `C` and `AC` clear the registers,
             `N` negate the current number, `E` perform the
             operation currently in the `op` register, and
             `O` push an arbitrary operation onto the `op`
             register.</p>
        ]*)
        let D n =
            num := 10 * !num + n
            updateDisplay ()

        let C () =
            num := 0
            updateDisplay()

        let AC () =
            num  := 0
            onum := 0
            op   := None
            updateDisplay ()

        let N () =
            num := - !num
            updateDisplay ()

        let E () =
            match !op with
            | None ->
                ()
            | Some f ->
                num := f !onum !num
                op  := None
                updateDisplay ()

        let O o () =
            match !op with
            | None ->
                ()
            | Some f ->
                num := f !onum !num
                updateDisplay ()
            onum := !num
            num  := 0
            op   := Some o

         (*[
             <p>What remains is providing the user interface.
             To make it easier, define some helpers for creating buttons
             (as DOM nodes) and attaching click actions.</p>
         ]*)
        let btn caption action =
            Input [Attr.Type "button"; Attr.Value caption; Attr.Style "width: 30px"]
            |>! OnClick (fun e _ -> action ())

        let digit n =
            btn (string n) (fun () -> D n)

        (*![
             <p>You can now easily compose the calculator from
             the components defined above:</p>
         ]*)
        Div [
            display
            Br []
            Div [
                digit 7; digit 8; digit 9; btn "/" (O ( / ))
                Br []
                digit 4; digit 5; digit 6; btn "*" (O ( * ))
                Br []
                digit 1; digit 2; digit 3; btn "-" (O ( - ))
                Br []
                digit 0; btn "C" C; btn "AC" AC; btn "+" (O ( + ));
                Br []
                btn "+/-" N; btn "=" E
            ]
        ]

type CalculatorViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = Calculator.Main () :> _