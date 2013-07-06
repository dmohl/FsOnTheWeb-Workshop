(*!Canvas!*)
(*![
    <p>WebSharper&trade; comes with low-level bindings to the <em>HTML5 Canvas element</em>.
    The example is a direct translation of code from the Mozilla
    Developer <a href="https://developer.mozilla.org/en/Canvas_tutorial/Basic_animations">Wiki</a>, available under the MIT License.
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

module Canvas =
    [<JavaScript>]
    let Main () =

        let example1 (ctx: CanvasRenderingContext2D) =
            ctx.FillStyle <- "rgb(200, 0, 0)"
            ctx.FillRect(10., 10., 55., 50.)
            ctx.FillStyle <- "rgba(0, 0, 200, 0.5)"
            ctx.FillRect(30., 30., 55., 50.)

        let example2 (ctx: CanvasRenderingContext2D) =
            ctx.FillRect(25., 25., 100., 100.)
            ctx.ClearRect(45., 45., 60., 60.)
            ctx.StrokeRect(50., 50., 50., 50.)

        let example3 (ctx: CanvasRenderingContext2D) =
            ctx.BeginPath()
            ctx.Arc(75., 75., 50., 0., Math.PI * 2., true)
            ctx.MoveTo(110., 75.)
            ctx.Arc(75., 75., 35., 0., Math.PI, false)
            ctx.MoveTo(65., 65.)
            ctx.Arc(60., 65., 5., 0., Math.PI * 2., true)
            ctx.MoveTo(95., 65.)
            ctx.Arc(90., 65., 5., 0., Math.PI * 2., true)
            ctx.Stroke()

        let example4 (ctx: CanvasRenderingContext2D) =
            // Filled triangle
            ctx.BeginPath()
            ctx.MoveTo(25., 25.)
            ctx.LineTo(105., 25.)
            ctx.LineTo(25., 105.)
            ctx.Fill()

            // Stroked triangle
            ctx.BeginPath()
            ctx.MoveTo(125., 125.)
            ctx.LineTo(125., 45.)
            ctx.LineTo(45., 125.)
            ctx.ClosePath()
            ctx.Stroke()

        let example5 (ctx: CanvasRenderingContext2D) =
            ctx.BeginPath()
            ctx.MoveTo(75., 25.)
            ctx.QuadraticCurveTo(25., 25., 25., 62.5)
            ctx.QuadraticCurveTo(25., 100., 50., 100.)
            ctx.QuadraticCurveTo(50., 120., 30., 125.)
            ctx.QuadraticCurveTo(60., 120., 65., 100.)
            ctx.QuadraticCurveTo(125., 100., 125., 62.5)
            ctx.QuadraticCurveTo(125., 25., 75., 25.)
            ctx.Stroke()

        let example6 (ctx: CanvasRenderingContext2D) =
            let roundedRect(x: float, y: float, width: float,
                            height: float, radius: float)  =
                ctx.BeginPath()
                ctx.MoveTo(x, y + radius)
                ctx.LineTo(x, y + height - radius)
                ctx.QuadraticCurveTo(x, y + height, x + radius, y + height)
                ctx.LineTo(x + width - radius, y + height)
                ctx.QuadraticCurveTo(x + width, y + height,
                                     x + width, y + height - radius)
                ctx.LineTo(x + width, y + radius)
                ctx.QuadraticCurveTo(x + width, y, x + width - radius, y)
                ctx.LineTo(x + radius, y)
                ctx.QuadraticCurveTo(x, y, x, y + radius)
                ctx.Stroke()
            roundedRect(12., 12., 150., 150., 15.)
            roundedRect(19., 19., 150., 150., 9.)
            roundedRect(53., 53., 49., 33., 10.)
            roundedRect(53., 119., 49., 16., 6.)
            roundedRect(135., 53., 49., 33., 10.)
            roundedRect(135., 119., 25., 49., 10.)
            ctx.BeginPath()
            ctx.Arc(37., 37., 13., Math.PI / 7., - Math.PI / 7., true)
            ctx.LineTo(31., 37.)
            ctx.Fill()
            for i in 0. .. 7. do
                ctx.FillRect(51. + i * 16., 35., 4., 4.)
            for i in 0. .. 5. do
                ctx.FillRect(115., 51. + i * 16., 4., 4.)
            for i in 0. .. 7. do
                ctx.FillRect(51. + i * 16., 99., 4., 4.)
            ctx.BeginPath()
            ctx.MoveTo(83., 116.)
            ctx.LineTo(83., 102.)
            ctx.BezierCurveTo(83., 94., 89., 88., 97., 88.)
            ctx.BezierCurveTo(105., 88., 111., 94., 111., 102.)
            ctx.LineTo(111., 116.)
            ctx.LineTo(106.333, 111.333)
            ctx.LineTo(101.666, 116.)
            ctx.LineTo(97., 111.333)
            ctx.LineTo(92.333, 116.)
            ctx.LineTo(87.666, 111.333)
            ctx.LineTo(83., 116.)
            ctx.Fill()
            ctx.FillStyle <- "white"
            ctx.BeginPath()
            ctx.MoveTo(91., 96.)
            ctx.BezierCurveTo(88., 96., 87., 99., 87., 101.)
            ctx.BezierCurveTo(87., 103., 88., 106., 91., 106.)
            ctx.BezierCurveTo(94., 106., 95., 103., 95., 101.)
            ctx.BezierCurveTo(95., 99., 94., 96., 91., 96.)
            ctx.MoveTo(103., 96.)
            ctx.BezierCurveTo(100., 96., 99., 99., 99., 101.)
            ctx.BezierCurveTo(99., 103., 100., 106., 103., 106.)
            ctx.BezierCurveTo(106., 106., 107., 103., 107., 101.)
            ctx.BezierCurveTo(107., 99., 106., 96., 103., 96.)
            ctx.Fill()
            ctx.FillStyle <- "black"
            ctx.BeginPath()
            ctx.Arc(101., 102., 2., 0., Math.PI * 2., true)
            ctx.Fill()
            ctx.BeginPath()
            ctx.Arc(89., 102., 2., 0., Math.PI * 2., true)
            ctx.Fill()

        let Example (draw: CanvasRenderingContext2D -> unit) width height caption =
            let element = Tags.NewTag "canvas" []
            let canvas  = As<CanvasElement> element.Dom
            canvas.Height <- height
            canvas.Width  <- width
            draw (canvas.GetContext "2d")
            Div [Attr.Style "float: left"] -< [
                element
                P [Align "center"] -< [
                    I [Text ("Example " + caption)]

                ]
            ]

        Div [
            Div [
                Example example1 100 200 "1"
                Example example2 150 200 "2"
                Example example3 150 200 "3"
                Example example4 150 200 "4"
            ]
            Div [Attr.Style "clear: left"] -< [
                Example example5 150 200 "5"
                Example example6 200 200 "6"
            ]
            Div [Attr.Style "clear:both"]
        ]

type CanvasViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = Canvas.Main () :> _