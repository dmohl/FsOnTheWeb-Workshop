(*!Live Chat!*)
(*![
    <p>This sample demonstrates developing a larger application in WebSharper&trade;
       In less than 300 lines of F# code. It implements a working LiveChat component.</p>
    <p>The core communication service is provided by an in-memory queue
       living in the ASP.NET Application State. The clients issue polling requests
       that traverse the queue and select messages that are newer than the last message
       the client has received; when you send a message, it gets enqueued; finally,
       when the number of messages in the queue reaches the queue capacity, they
       are dequeued, freeing memory. There are performance and scalability issues
       with this design (for example, the application will not work in a web farm
       scenario, where Application State is not shared), but the example can be
       easily adapted to work with other communication service providers.
       </p>
    <h2>Source Code Explained</h2>
]*)
namespace Samples

open System
open System.Collections.Generic
open System.Security
open System.Text
open System.Threading
open System.Web
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module Chat =
    (*![
        <p>The Message record contains the sender identity, some text,
        and the virtual time, which is a natural number that is automatically
        incremented to order the messages. Instances of this
        record type can live in both the client and the server,
        and serialize automatically.</p>
    ]*)
    type Message =
        {
            Time:   int
            Sender: string
            Text:   string
        }

    (*![
        <p>The Auth module handles security and authentication.
        In a production application you would want to secure some functionality
        by requiring a login and authenticating the user. In an AJAX context
        this is usually implemented by having the server issue securely signed
        auth tokens that the client can later use for authenticating to the
        different parts of the application. This approach is taken below,
        without going into password checking, a feature that should be
        straightforward to add.</p>
    ]*)
    module Auth =

        type Key = class end

        let Key = typeof<Key>.GUID.ToString()

        type Token =
            {
                Name: string
                Hash: string
            }

        let private sha1 = Cryptography.SHA1.Create()

        let Generate (name: string) : Token =
            let hash = new StringBuilder()
            sprintf "%s:%s" name Key
            |> Encoding.Unicode.GetBytes
            |> sha1.ComputeHash
            |> Array.iter (fun by -> hash.Append(by.ToString("X2")) |> ignore)
            {
                Name = name
                Hash = hash.ToString()
            }

        let Validate (token: Token) : bool =
            Generate token.Name = token

     (*![
        <p>The State module is responsible for storing and querying
        the message queue, the user set, and the time field.
        Destructive operations use a lock.</p>
     ]*)
    module State =

        type State =
            {
                Time:     Ref<int>
                Messages: Queue<Message>
                Users:    Dictionary<string, DateTime>
            }

            member this.Cleanup() =
                while this.Messages.Count > 50 do
                    this.Messages.Dequeue()
                    |> ignore
                let now = DateTime.Now
                List.iter (this.Users.Remove >> ignore) [
                    for pair in this.Users do
                        if (now - pair.Value).TotalSeconds > 20. then
                            yield pair.Key
                ]

        let Lock = []

        let Init () =
            let q =
                {
                    Time = ref 0
                    Messages = new Queue<_>()
                    Users = new Dictionary<_,_>()
                }
            lock Lock (
                fun () ->
                    HttpContext.Current.Application.Set(Auth.Key, q)
            )
            q

        let Get () =
            match HttpContext.Current.Application.Get Auth.Key with
            | null ->
                Init ()
            | :? State as s ->
                s
            | _ ->
                Init ()

    (*![
        <p>The Rpc module contains functions exposed to the client:
        Poll, Login and Talk. The client first logs in, obtains
        an auth token, and then uses it to Talk.
        Attempts to do so with an invalid token are ignored.</p>
     ]*)
    module Rpc =

        [<Rpc>]
        let Poll (auth: Auth.Token) (time: int) =
            let s = State.Get()
            s.Cleanup ()
            lock State.Lock (
                fun () ->
                    s.Users.[auth.Name] <- DateTime.Now
            )
            let m =
                [|
                    for m in s.Messages do
                        if m.Time > time then
                            yield m
                |]
            let u = [| for u in s.Users -> u.Key |]
            async { return (!s.Time, m, u) }

        [<Rpc>]
        let Login (user: string) : Option<Auth.Token> =
            let s = State.Get()
            if s.Users.ContainsKey user then
                None
            else
                lock State.Lock (
                    fun () ->
                        s.Users.Add (user, DateTime.Now)
                        |> ignore
                )
                user |> Auth.Generate |> Some

        [<Rpc>]
        let Talk (auth: Auth.Token) (message: string) : unit =
            if Auth.Validate auth then
                let s = State.Get ()
                let update () =
                    incr s.Time
                    s.Users.[auth.Name] <- DateTime.Now
                    s.Messages.Enqueue
                        ({
                            Time   = !s.Time
                            Sender = auth.Name
                            Text   = message
                        })
                lock State.Lock update

    (*![
        <p>Finally, the Ui module provides the actual HTML controls
        that the user interacts with.</p>
     ]*)
    module Ui =

        (*![
            <p>The following function presents a login form, invoking the callback when
            the form is successfully completed. Remember that
            the forms are not submitted by a page reload; rather,
            the information travels via RPC calls.</p>
         ]*)
        [<JavaScript>]
        let LoginForm onLogin =
            let login = Input []
            let errors = P []
            Div [
                errors
                P [Text "Enter chat as: "]
                P [login]
                Input [Attr.Type "button"; Attr.Value "Enter"]
                |>! OnClick (fun e args ->
                    let name = login.Value
                    match Rpc.Login name with
                    | Some auth ->
                        onLogin auth
                    | None ->
                        errors.Text <- "Error. Username already taken!"
                )
            ]

        (*![
            <p>A SendBox is the chat component that presents an input field with a "Talk"
            button, and asynchronously sends Talk messages to the server by RPC every
            time this button is pressed.</p>
         ]*)
        [<JavaScript>]
        let SendBox auth =
            let sendBox = Input []
            Div [
                sendBox
                Input [Attr.Type "button"; Attr.Value "Talk"]
                |>! OnClick (fun e args ->
                    let msg = sendBox.Value
                    sendBox.Value <- ""
                    if msg <> "" then
                        Rpc.Talk auth msg
                )
            ]

        (*![
            <p>A UserBox displays the active list of users in chat.</p>
        ]*)
        [<JavaScript>]
        let UserBox () =
            let userBox = Div []
            let update users =
                userBox.Clear()
                users
                |> Array.map (fun user -> Div [Text user])
                |> Array.iter userBox.Append
            let xml =
                Div [Attr.Style "float: right;\
                                 height: 250px;\
                                 border: 1px solid silver;\
                                 padding : 10px;\
                                 overflow: auto;"] -<
                [
                    Div [Text "Users in chat:"]
                    userBox
                ]
            (xml, update)

        (*![
            <p>A ReceiveBox is a component responsible for polling the
            server with Poll messages and displaying those messages.
            It has to track message "time", which is achieved by a closing
            over a value of Ref<int> and updating it on every successful
            request.</p>
         ]*)
        [<JavaScript>]
        let ReceiveBox auth =
            let time = ref 0
            let receiveBox =
                Div [Attr.Style "height: 250px;\
                                 border: 1px solid silver;\
                                 border-right : none;\
                                 padding : 10px;\
                                 margin-right : 10px;
                                 overflow: auto;"]
            let (userBox, updateUserBox) = UserBox ()
            let display (messages: Message[]) (users: string[]) =
                messages
                |> Array.iter (fun m ->
                    let dom = Div [B [Text <| m.Sender + ": "]; Span [Text m.Text]]
                    receiveBox.Append(dom)
                    receiveBox.JQuery.ScrollTop (dom.JQuery.Offset().Top)
                    |> ignore)
                updateUserBox users
            let rec receive () =
                async {
                    let! (stime, messages, users) = Rpc.Poll auth !time
                    do  time := stime
                        if receiveBox.JQuery.Parent().Length > 0 then
                            display messages users
                    do! Async.Sleep 1000
                    do! receive ()
                }
            Async.Start <| receive ()
            Div [
                userBox
                receiveBox
            ]

        (*![
            <p>A ChatBox is assembled from the above components.
            It first presents a LoginBox, and then replaces it with
            a combination of SendBox and ReceiveBox upon a successful
            login.</p>
         ]*)
        [<JavaScript>]
        let ChatBox () =
            let mkBox auth =
                Div [
                    SendBox auth
                    ReceiveBox auth
                ]
            let container = Div []
            let onLogin auth =
                container.Clear()
                container.Append(mkBox auth)
            Div [LoginForm onLogin]
            |> container.Append
            container

    (*![ <p>The main entry point simply renders the ChatBox.</p> ]*)
    [<JavaScript>]
    let Main () = Ui.ChatBox()


type ChatViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = Chat.Main () :> _