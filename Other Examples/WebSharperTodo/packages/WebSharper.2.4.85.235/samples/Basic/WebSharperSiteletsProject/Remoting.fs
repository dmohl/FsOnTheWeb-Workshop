(*!Remoting!*)
(*![
    <p>A WebSharper&trade; application is two-tiered -- it executes on both the client and the server.
       The conceptual model is simple: the client always has the control, and calls the server
       when necessary. The critical component in the implementation of this model is the remote
       procedure call (RPC) mechanism, which is demonstrated in this sample.</p>
    <p>The RPC mechanism is designed to be both flexible and simple to use.
       It is flexible by supporting three different ways of doing a remote call:
       message-passing, synchronous and asynchronous. It is simple by
       making basic (synchronous or message-passing) calls look just like regular
       function calls. </p>
    <p>The mechanism employs JSON serialization for data transport. Behind the scenes,
       an automatic JSON serializer is making sure that
       the RPC functions can accept arguments and return values of any F# data type
       (scalars, arrays, tuples, records, union types including lists and options),
       without requiring any annotation on the type. Passing and returning first-class functions,
       however, is not currently supported.
    </p>
    <p>This sample demonstrates:</p>
    <ul>
        <li>How to expose server-side code to be callable from the client</li>
        <li>How to distinguish message-passing, synchronous and asynchronous calls</li>
        <li>How to send messages to the server</li>
        <li>How to call the server functions synchronously</li>
        <li>How to declare and execute asynchronous call workflows</li>
    </ul>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open System
open System.Security.Cryptography
open System.Text
open System.Threading
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module Remoting =

    (*![
        <p>In WebSharper&trade; you can mark server-side functions for use on the client.
        One useful algorithm that is present in the .NET standard library but not
        in the browser is the MD5 message digest algorithm. Let us expose the
        algorithm as a string transformer function. The
        Rpc attribute indicates that this function may
        be safely called from the client-side code.</p>
     ]*)
    [<Rpc>]
    let Md5 (data: string) : string =
        use md5 = MD5.Create()
        let sb = new StringBuilder()
        data.ToCharArray()
        |> Encoding.Unicode.GetBytes
        |> md5.ComputeHash
        |> Array.iter (fun b -> sb.Append(b.ToString("X2")) |> ignore)
        sb.ToString()

    (*![
        <p>From now on, an attempt to call `MD5` from client code will succeed
           as if MD5 was defined in JavaScript. In reality, the call will
           trigger the RPC mechanism, which will serialize the `data` argument,
           send it to the server, invoke the `MD5` function in the .NET server runtime,
           serialize the result, and return in to the client. During this process,
           the browser will block the JavaScript execution.</p>
        <p>Calls to MD5 are therefore synchronous. Using synchronous calls trades off
           the ease of development for the risk of having the user notice an UI
           unresponsiveness in case of significant network latency.</p>
        <p>The following function generates a little UI to test the MD5 function.</p>
     ]*)
    [<JavaScript>]
    let Md5Ui () =
        let output = P []
        let input = Input [Attr.Type "text"]
        input
        |> OnKeyPress (fun _ _ ->
            output.Text <- Md5 input.Value)
        Div [Attr.Class "md5-calculator"] -< [
            H4 [Text "MD5 Calculator"]
            P [Text "Text:"]
            P [input]
            P [Text "MD5:"]
            P [output]
        ]

     (*![
        <p>The simplest alternative to synchronous calls that does not block the
           browser is a message-passing call. Calls to all `Rpc` methods that do
           not return a value (are of `unit` return type) use message-passing
           semantics.
        </p>
        <p>The next example demonstrates calls to a long-running
           (artificially delayed) server method with message-passing. For you to observe
           the effects of the call, the server method will update a counter, which
           another client-side function will query. Notice that the browser does not
           block when calling the slow `IncreaseCounter` method.</p>
     ]*)
    let Counter = ref 0

    [<Rpc>]
    let IncreaseCounter () =
        Thread.Sleep 3000
        lock Counter (fun () -> Counter := (!Counter + 1) % 1000)

    [<Rpc>]
    let GetCounter () = !Counter

    [<JavaScript>]
    let CounterUi () =
        let out = P []
        Div [Attr.Class "counter"] -< [
            H4 [Text "Counter"]
            P [
                Input [Attr.Type "button"; Attr.Value "Increase"]
                |>! OnClick (fun _ _ -> IncreaseCounter ())
                Input [Attr.Type "button"; Attr.Value "Go"]
                |>! OnClick (fun e args -> out.Text <- string (GetCounter ()))
            ]
            P [Text "Counter state:"]
            out
        ]

     (*![
        <p>Asynchronous calls present the final alternative for using RPC.
           All `Rpc` methods that return an `Async` value from the
           WebSharper&trade; Control library, do not block the browser, and
           do not return immediately. Rather, their `Async` type presents
           a convenient abstraction to control multi-stage computations.
           It is beyond the scope of this sample to explain `Async` type.
           For now, just note that `Async` forms an F# workflow.
           An example follows.
           </p>
     ]*)
    [<Rpc>]
    let IncreaseCounterAsync () =
        IncreaseCounter ()
        async { return GetCounter () }

    [<JavaScript>]
    let CounterWorkflow () =
        async {
            let! x = IncreaseCounterAsync ()
            let! y = IncreaseCounterAsync ()
            return (x, y)
        }

    [<JavaScript>]
    let WorkflowUi () =
        let out = P []
        let work =
            async {
                let! (x, y) = CounterWorkflow ()
                return out.Text <-
                        "the counter was " + string x +
                        " and then became " + string y
            }
        Div [Attr.Class "counter"] -< [
            H4 [Text "Workflow"]
            Input [Attr.Type "button"; Attr.Value "Start"]
            |>! OnClick (fun e args -> Async.Start work)
            out
        ]

    (*![
        <p>The final function unites the UIs in a DOM-builder expression.</p>
     ]*)
    [<JavaScript>]
    let Main () =
        Div [
            Md5Ui ()
            CounterUi ()
            WorkflowUi ()
        ]

type RemotingViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = Remoting.Main () :> _