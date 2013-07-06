(*!Geolocation!*)
(*![
    <p>This example demonstrates how can you use the Html5
    library to get persitent storage.</p>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module DataStorage =

    [<JavaScriptAttribute>]
    let Main () =
        Div [
            H1 [Text "LocalStorage Test"]
        ]
        |>! OnAfterRender (fun elem ->
            let storage = Window.Self.LocalStorage
            let key = "intReference"
            let intReference = storage.GetItem(key)
            if intReference = null || intReference = JavaScript.Undefined then
                storage.SetItem(key, "0")
            else
                let oldValue = int (intReference)
                storage.SetItem(key, (oldValue + 1).ToString())
            elem.Html <- elem.Html
                         + "<h2>localStorage is now: ("
                         + storage.GetItem(key) + ")</h2>"
        )

type DataStorageViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body =  DataStorage.Main () :> _