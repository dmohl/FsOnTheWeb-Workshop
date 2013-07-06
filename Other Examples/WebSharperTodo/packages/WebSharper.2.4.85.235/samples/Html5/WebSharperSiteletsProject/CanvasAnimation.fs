(*!Canvas Animation!*)
(*![
    <p>This is another example of using the Canvas element to provide
    animation.</p>
    <p>
    The example is a direct translation of the code from the
    <a href="http://developer.mozilla.org/en/Canvas_tutorial/Basic_animations">Mozilla Developer Wiki</a>,
    available under the MIT License.
    </p>
    <p>
    To run the example, you will need a compliant browser (such as
    Firefox) that supports the Canvas HTML5 tag.
    </p>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open System
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5

module CanvasAnimation =
    [<JavaScript>]
    let AnimatedCanvas draw width height caption =
        let element = Tags.NewTag "Canvas" []
        let canvas  = As<CanvasElement> element.Dom
        canvas.Width  <- width
        canvas.Height <- height
        let ctx = canvas.GetContext "2d"
        let rec loop =
            async {
                do! Async.Sleep 1000
                do draw ctx
                do! loop
            }
        draw ctx
        Async.Start loop
        Div [ Width (string width); Attr.Style "float:left" ] -< [
            Div [ Attr.Style "float:center" ] -< [
                element
                P [Align "center"] -< [
                    I [Text <| "Example " + caption]
                ]
            ]
        ]

    [<JavaScript>]
    let Main () =
        let example1 (ctx: CanvasRenderingContext2D) =
            let now = new EcmaScript.Date()
            ctx.Save()
            ctx.ClearRect(0., 0., 150., 150.)
            ctx.Translate(75., 75.)
            ctx.Scale(0.4, 0.4)
            ctx.Rotate(- Math.PI / 2.)
            ctx.StrokeStyle <- "black"
            ctx.FillStyle <- "white"
            ctx.LineWidth <- 8.
            ctx.Save()

            // Hour marks
            for i in 1..12 do
                ctx.BeginPath()
                ctx.Rotate(Math.PI / 6.)
                ctx.MoveTo(100., 0.)
                ctx.LineTo(120., 0.)
                ctx.Stroke()
            ctx.Restore()

            // Minute marks
            ctx.Save()
            ctx.LineWidth <- 5.
            for i in 0 .. 59 do
                if (i % 5) <> 0 then
                    ctx.BeginPath()
                    ctx.MoveTo(117., 0.)
                    ctx.LineTo(120., 0.)
                    ctx.Stroke()
                ctx.Rotate(System.Math.PI / 30.)
            ctx.Restore()

            let sec = now.GetSeconds()
            let min = now.GetMinutes()
            let hr  =
                let hr = float (now.GetHours())
                if hr >= 12. then hr - 12. else hr
            ctx.FillStyle <- "black"

            // Write Hours
            ctx.Save()
            Math.PI * (float hr / 6. + float min / 360. + float sec / 21600.)
            |> ctx.Rotate
            ctx.LineWidth <- 14.
            ctx.BeginPath()
            ctx.MoveTo(-20., 0.)
            ctx.LineTo(80., 0.)
            ctx.Stroke()
            ctx.Restore()

            // Write Minutes
            ctx.Save()
            ctx.Rotate(Math.PI * (float min / 30. + float sec / 1800.))
            ctx.LineWidth <- 10.
            ctx.BeginPath()
            ctx.MoveTo(-28., 0.)
            ctx.LineTo(112., 0.)
            ctx.Stroke()
            ctx.Restore()

            // Write Seconds
            ctx.Save()
            ctx.Rotate(float sec * Math.PI / 30.)
            ctx.StrokeStyle <- "#D40000"
            ctx.FillStyle <- "#D40000"
            ctx.LineWidth <- 6.
            ctx.BeginPath()
            ctx.MoveTo (-30., 0.)
            ctx.LineTo (83., 0.)
            ctx.Stroke()
            ctx.BeginPath()
            ctx.Arc(0., 0., 10., 0., Math.PI * 2., true)
            ctx.Fill()
            ctx.BeginPath()
            ctx.Arc(95., 0., 10., 0., Math.PI * 2., true)
            ctx.Stroke()
            ctx.FillStyle <- "#555"
            ctx.Arc(0., 0., 3., 0., Math.PI * 2., true)
            ctx.Fill()
            ctx.Restore()

            ctx.BeginPath()
            ctx.LineWidth <- 14.
            ctx.StrokeStyle <- "#325FA2"
            ctx.Arc(0., 0., 142., 0., Math.PI * 2., true)
            ctx.Stroke()
            ctx.Restore()

        Div [
            AnimatedCanvas example1 150 150 "1"
            Div [Attr.Style "clear:both"]
        ]

type CanvasAnimationViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = CanvasAnimation.Main () :> _