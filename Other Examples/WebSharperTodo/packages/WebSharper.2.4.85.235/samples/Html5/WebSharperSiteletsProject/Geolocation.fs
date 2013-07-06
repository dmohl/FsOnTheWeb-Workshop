(*!Geolocation!*)
(*![
    <p>This example demonstrates how you can use the Html5
    library to get access to the user's location. It shows
    how this can be used together with asynchronous computation
    expressions to make the code clearer.</p>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module Geolocation =

    [<JavaScriptAttribute>]
    let GetPosition() : (Async<Html5.Position>) =
        Async.FromContinuations(fun (onOk, _, _) ->
            Html5.Window.Self.Navigator.Geolocation.GetCurrentPosition(fun pos ->
                onOk pos
            )
        )

    [<JavaScriptAttribute>]
    let Main () =
        Div []
        |>! OnAfterRender (fun elem ->
            async {
                let! position = GetPosition()
                let coords = position.Coords
                let coordsText = (coords.Latitude, coords.Longitude).ToString()
                elem.Html <- elem.Html + "<h2>Your location is: (" + coordsText + ")</h2>"

            }
            |> Async.Start |> ignore
        )

type GeolocationViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body =  Geolocation.Main () :> _