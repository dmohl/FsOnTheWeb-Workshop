(*!Toggle Panel!*)
(*![
    <p>
    In this sample a custom control is created for wrapping content
    in a Toggle Panel. It demonstrates:
    </p>
    <ul>
        <li>How to write reusable functions for building page components.</li>
        <li>How to use JQuery for DOM manipulation.</li>
    </ul>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.JQuery

module TogglePanel =

    [<JavaScript>]
    let TogglePanelClass = "TogglePanel"

    [<JavaScript>]
    let MenuClass = "MenuClass"

    [<JavaScript>]
    let ToggleLinkClass = "ToggleLink"

    (*![
        <p><em>TogglePanel</em> takes two string arguments representing the captions shown when
        the panel is expanded and collapsed. The third argument is the body to be wrapped in the panel.
        This function defines a reusable component that can be embedded into any HTML builder expression,
        as will be shown below.</p>
        <p>
        A Div tag with CSS class <em>TogglePanel</em> is created, containing a <em>Menu</em>
        div with the toggle panel link, and the body of the content that is to be wrapped
        inside the toggle panel.
        </p>
        <p>
        When the toggle link is clicked, the visibility property of the content is checked.
        If the content is visible the panel is collapsed and the link text is changed,
        otherwise the panel is expanded.
        </p>
    ]*)
    [<JavaScript>]
    let TogglePanel showCaption hideCaption (body: Element) =

        let content = Div [body]

        Div [Attr.Class TogglePanelClass] -< [
            Div [Attr.Class MenuClass] -< [
                Div [Attr.Class ToggleLinkClass] -< [Text hideCaption]
                |>! OnClick (fun alink event ->
                    if JQuery.Of(content.Dom).Is ":visible" then
                        JQuery.Of(content.Dom).SlideUp("fast", ignore).Ignore
                        JQuery.Of(alink.Dom).Text(showCaption).Ignore
                    else
                        JQuery.Of(content.Dom).SlideDown("fast", ignore).Ignore
                        JQuery.Of(alink.Dom).Text(hideCaption).Ignore
                )
            ]
            content
        ]
    (*![
        <p>Since your toggle panel control uses certain CSS classes, you may configure
        the visual appearance of the component by supplying your own style definitions.</p>
    ]*)
    [<JavaScript>]
    let Main () =
        let tp x = TogglePanel "Show" "Hide" x
        Div [
            tp (Div [Text "First Panel"])
            tp (Div [Text "Second Panel"])
            tp (Div [Text "Third Panel"])
        ]

type TogglePanelViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = TogglePanel.Main () :> _