(*!Hello World!*)
(*![
    <p>
    This sample implements "Hello, world!" as a WebSharper&trade; application.
    It demonstrates:
    </p>
    <ul>
        <li>How to <b>compose HTML/XML</b> with WebSharper&trade; XML combinators
        <li>How to <b>create a button</b> dynamically</li>
        <li>How to <b>bind an event handler</b> to that button</li>
        <li>How to <b>update the contents</b> of an existing DOM element</li>
    </ul>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module HelloWorld =

    (*![ <p>The entry point of the sample will be the `Main` function.
         Notice the `JavaScript` attribute. This attribute marks code
         that will be translated to JavaScript and run on the client.</p>
     ]*)
    [<JavaScript>]
    let Main () =
        (*![
            <p>Next come the XML combinators. To define the layout,
            we use functions that look and feel a lot like XML tags.</p>
        ]*)
        (*![
            <p>Note that you can use these WebSharper&trade; HTML combinators
            to compose any HTML (or XML) structure within F#. These combinators
            take a list of HTML/XML nodes that will be composed as child
            nodes underneath. You can also list attributes using the <code>-@</code> operator.</p>
        ]*)
        let welcome = P [Text "Welcome"]
        Div [
            welcome
            Input [Attr.Type "Button"; Attr.Value "Click me!"]
            (*![
                <p>To bind events to any node within the tree, you call the appropriate event
                combinator, a function that transforms a node into one that has an event handler
                attached. Let's bind a click handler to the above button.</p>
            ]*)
            |>! OnClick (fun e args ->
                (*![
                    <p>Within the handler, you display the welcome message.</p>
                ]*)
                welcome.Text <- "Hello, world!")
        ]

(*![
    <p>To serve pagelets via server controls, you need to override the Body member
    in your pagelet class derived from IntelliFactory.WebSharper.Web.Control.
    Here, `HelloWorld.Main` points to the function that implements your pagelet logic.</p>
]*)
type HelloWorldViewer()=
    inherit Web.Control()

    [<JavaScript>]
    override this.Body = HelloWorld.Main () :> Html.IPagelet