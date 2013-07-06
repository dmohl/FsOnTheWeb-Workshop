namespace Samples

[<AutoOpen>]
module JQueryExtensions =
    open IntelliFactory.WebSharper
    open IntelliFactory.WebSharper.JQuery

    type IntelliFactory.WebSharper.Html.Element with
        [<JavaScript>]
        member self.JQuery = JQuery.Of self.Dom

#nowarn "191"

/// Used to refer to this assembly.
type Marker = class end
