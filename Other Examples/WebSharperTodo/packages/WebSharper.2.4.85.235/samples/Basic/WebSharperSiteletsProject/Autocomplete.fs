(*!Autocomplete!*)
(*![
    <p>This example implements an *Autocomplete* panel that completes user
    input based on server-side data.</p>
    <p>This sample demonstrates:</p>
    <ul>
      <li>How to create basic AJAX applications</li>
      <li>How to communicate with the server</li>
    </ul>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open System
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

(*![
    <p>To distinguish server and client-side functions, you may put them
    into separate modules.</p>
]*)
module Server =
    (*![
         <p>A simple list of strings will represent the database of
         possible name matches.  In a real world application the `Name`
         function could use a data source such as a relational database
         or or an XML document to retrieve the values.</p>
     ]*)
    let Names =
        [
            "Adam"; "Anton"; "Bo"; "Carola"; "Diego"; "Danny"; "Eric";
            "Filippa"; "Gary"; "Henri"; "Ina" ; "Joel"; "Kalle"; "Lars";
            "Monica"; "Niclas"; "Oscar"; "Patricia"; "Quantin"; "Richard";
            "Ursula"; "Veronica"; "Zorro"
        ]
    (*![
         <p>A server-side function, `SuggestNames`, searches the `Names`
         list for matching strings. This function is marked as `Rpc` to
         declare that it can be called by the client code.</p>
     ]*)
    [<Rpc>]
    let SuggestNames name =
        match name with
        | "" ->
            []
        | _ ->
            Names
            |> List.filter (fun n ->
            name.ToLower()
            |> n.ToLower().StartsWith)
(*![
    <p>All client-side methods are put in a separate module.</p>
    <p>
    The method `Client.Main` creates the element node containing the different
    panels.
    </p>
    <p>
    To capture the changes of the input text field the function `OnKeyUp` is used.
    It accepts a callback function to be called every time the `KeyUp` event is triggered.
    Here it calls the local function `update` which fetches the suggested values from the
    server and updates the display.
    </p>
]*)
module Client =

    [<JavaScript>]
    let Main () =
        let input = Input [Attr.Type "Text"]
        let hint = Span [Attr.Style "font-style: italic; color: green"]

        let update () =
            hint.Clear()
            input.Value
            |> Server.SuggestNames
            |> List.iter (fun name ->
                hint.Text <- hint.Text + name + " ")
        let label txt =
            Label [Attr.Style "margin-right: 5px; font-weight: bold;"]
                -< [Text txt]

        Div [Attr.Style "padding: 5px;"] -< [
            P [Text "Type a name in the input field below:"]
            Form [
                label "First Name: "
                input
                |>! OnKeyUp (fun _ _ -> update ())
            ]
            P [
                label "Suggestions: "
                hint
            ]
        ]

type AutocompleteViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = Client.Main () :> _