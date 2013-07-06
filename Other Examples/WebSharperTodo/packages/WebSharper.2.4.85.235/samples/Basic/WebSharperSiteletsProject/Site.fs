namespace WebSharperSiteletsProject

open System
open System.IO
open System.Web

open IntelliFactory.Html
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper

type Samples =
    | Index
    | Sample of string

module Controls =
    let list =
        typeof<Samples>.Assembly.GetTypes()
        |> Seq.filter (fun t -> typeof<Web.Control>.IsAssignableFrom t)
        |> Seq.map (fun t ->
            let name = t.Name.Substring(0, t.Name.Length - "Viewer".Length)
            (name, t))
        |> List.ofSeq

module Skin =

    type Page =
        {
            Back : list<Content.HtmlElement>
            Caption : list<Content.HtmlElement>
            Content : list<Content.HtmlElement>
            Title : string
        }

    let MainTemplate =
        let path = HttpContext.Current.Server.MapPath("~/Template.html")
        Content.Template<Page>(path)
            .With("Back", fun x -> x.Back)
            .With("Caption", fun x -> x.Caption)
            .With("Content", fun x -> x.Content)
            .With("Title", fun x -> x.Title)

module Site =
    let pages =
        Controls.list
        |> List.map (fun (name, t) ->
            let el = Activator.CreateInstance(t) :?> Web.Control
            let page =
                Content.WithTemplate Skin.MainTemplate <| fun ctx ->
                {
                    Back = [A [HRef (ctx.Link Index)] -< [Text "Back"]]
                    Caption = [Div [Text name]]
                    Content = [Div [el]]
                    Title = "WebSharper Samples"
                }
            (name, page))
        |> Map.ofSeq

    let index =
        Content.WithTemplate Skin.MainTemplate <| fun ctx ->
        {
            Title = "WebSharper Samples"
            Back = []
            Caption = [Div [Text "Samples"]]
            Content =
                [
                    Table [
                        for name, _ in Controls.list do
                            yield TR [ TD [ A [HRef (ctx.Link (Sample name))] -< [Text name] ]]
                    ]
                ]
        }

    let controller =
        let handler = function
            | Index -> index
            | Sample s -> pages.[s]

        { Handle = handler }

type Website() =
    interface IWebsite<Samples> with
        member this.Actions = []
        member this.Sitelet =
            {
                Controller = Site.controller
                Router = Router.Table [Index, "/"] <|> Router.Infer()
            }


[<assembly: WebsiteAttribute(typeof<Website>)>]
do ()