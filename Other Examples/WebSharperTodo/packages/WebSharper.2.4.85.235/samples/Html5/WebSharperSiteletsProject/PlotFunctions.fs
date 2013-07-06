(*!Plot functions!*)
(*![
    <p>
    This sample implements a function plotter that runs entirely in Javascript
    and can calculate plots of a given function within a given range. The
    implementation provided here is robust and handles undefined function values,
    and takes care of removing outliers that would yield incorrect plots.
    </p><p>
    It demonstrates:
    </p>
    <ul>
    <li>How to embed and evaluate a small <b>expression language</b></li>
    <li>How to use <b>active patterns for parsing</b></li>
    <li>How to use float <b>lists with a skip value</b></li>
    <li>How to perform <b>complex calculations on sequences</b> of numbers.
    </ul>
    <p>To run the example, you will need a compliant browser (such as Firefox)
       that supports the Canvas HTML5 tag.
       Future releases of WebSharper&trade; will provide a drawing API that
       can run on Silverlight, falling back to Canvas if it is not present.</p>
]*)
(*![<h2>Source Code Explained</h2>]*)
namespace Samples

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

(*![
    <p>
    When working on more complex pagelets, it is a good idea to separate the
    different aspects of functionality into modules. Here, you start with the <b>Ast</b>
    module that contains the type definitions needed for constructing
    arithmetic expressions.
    </p>
]*)
module PlotFunctions =

    module Utils =
        open IntelliFactory.WebSharper.Html5

        [<JavaScript>]
        let NewCanvas width height =
            let element = Tags.NewTag "Canvas" []
            let canvas  = As<CanvasElement> element.Dom
            canvas.Width  <- width
            canvas.Height <- height
            canvas, element

    module Ast =
        type var = string

        (*![
            <p>
            You can represent arithmetic expressions as follows:
            <ul>
                <li>Numbers - carrying a float value</li>
                <li>Binary operations - carrying a binary operator function and two operands</li>
                <li>Variables</li>
                <li>Functions - single argument, for simplicity</li>
            </ul>
            </p>
        ]*)
        type Expr =
            | Number   of float
            | BinOp    of (float -> float -> float) * Expr * Expr
            | Var      of var
            | FunApply of var * Expr

            (*![
                <p>
                You can add some utility members to this type to make it
                easier to construct the various forms of arithmetic operations.
                </p>
            ]*)
            [<JavaScript>]
            static member Sum (e1, e2)   = BinOp (( + ), e1, e2)
            [<JavaScript>]
            static member Diff (e1, e2)  = BinOp (( - ), e1, e2)
            [<JavaScript>]
            static member Prod (e1, e2)  = BinOp (( * ), e1, e2)
            [<JavaScript>]
            static member Ratio (e1, e2) = BinOp (( / ), e1, e2)

    (*![
        <p>
        Your next module contains the functionality related to parsing
        arbitrary arithmetic expressions.
        </p>
    ]*)
    module Language =
        open IntelliFactory.WebSharper.EcmaScript
        open System

        (*![
            <p>
            Parsing occurs by breaking the input string into a series
            of tokens. The <b>matchToken</b> function takes a (JavaScript-style)
            regular expression and an input string, and returns the matched
            and the remaining string as a tuple option, or None if matching fails.
            To perform the pattern matching, you use the RegExp type supplied with
            the WebSharper&trade; libraries.
            </p>
        ]*)
        [<JavaScript>]
        let private matchToken pattern s : (string * string) option =
            let regexp = new RegExp("^(" + pattern + ")(.*)")
            if regexp.Test s then
                let results = regexp.Exec s
                if results ==. null then
                    None
                else
                    Some results
                |> Option.map (fun x -> (x.[1], x.[2]))
            else
                None

        (*![
            <p>
            Tokens in the input string are separated by whitespace,
            and these have to be stripped off from the input string
            during tokenization (lexing).
            </p><p>
            You can perform this by defining an active pattern that
            matches whitespace (<b>WHITESPACE</b>) and matching each
            new token with stripping the whitespace off first
            (<b>MatchTokenNoWS</b> and <b>MatchToken</b>).
            </p>
            </p><p>
            And finally, <b>MatchSymbol</b> is a utility function
            that matches a token with the regular expression given,
            and returns with the remainder of the input string.
            This function is useful to parse terminal symbols such
            as keywords, operators, special characters, etc.
            </p>
        ]*)
        [<JavaScript>]
        let (|WHITESPACE|_|) = matchToken @"[ |\t|\n|\n\r]+"

        [<JavaScript>]
        let rec MatchTokenNoWS s pattern =
            match (|WHITESPACE|_|) s with
            | Some (_, rest) ->
                rest |> matchToken pattern
            | None ->
                s |> matchToken pattern

        [<JavaScript>]
        let MatchToken s f pattern =
            pattern |> MatchTokenNoWS s |> Option.bind f

        [<JavaScript>]
        let MatchSymbol s pattern =
            pattern |> MatchToken s (fun (_, rest) -> rest |> Some)

        (*![
            <p>
            For any reasonable grammar, you need the ability to match a
            series of the same token. Here, <b>Star</b> is a parameterized
            active pattern that takes a function (another active pattern)
            and an accumulator as parameters, and parses the input string
            by applying the active pattern given until no further match
            can be made and returns the accumulated tokens as a list.
            </p>
        ]*)
        [<JavaScript>]
        let rec (|Star|_|) f acc s =
            match f s with
            | Some (res, rest) ->
                (|Star|_|) f (res :: acc) rest
            | None ->
                (acc |> List.rev , s) |> Some

        (*![
            Now you can define the literal symbols that the expression language uses.
            First, you write the active pattern to match floating-point numbers, then
            identifiers.
        ]*)
        [<JavaScript>]
        let (|NUMBER|_|) s =
            @"[0-9]+\.?[0-9]*" |> MatchToken s
                (fun (n, rest) -> (n |> Double.Parse, rest) |> Some)

        [<JavaScript>]
        let (|ID|_|) s =
            "[a-zA-Z]+" |> MatchToken s (fun res -> res |> Some)

        (*![
            Using the <b>MatchSymbol</b> utility function you defined earlier, you can
            wrap the basic arithmetic operators (addition, subtraction, multiplication,
            and division) and the left and right parentheses.
        ]*)
        [<JavaScript>]
        let (|PLUS|_|)   s = @"\+" |> MatchSymbol s
        [<JavaScript>]
        let (|MINUS|_|)  s = @"\-"  |> MatchSymbol s
        [<JavaScript>]
        let (|MUL|_|)    s = @"\*" |> MatchSymbol s
        [<JavaScript>]
        let (|DIV|_|)    s = "/"  |> MatchSymbol s
        [<JavaScript>]
        let (|LPAREN|_|) s = @"\(" |> MatchSymbol s
        [<JavaScript>]
        let (|RPAREN|_|) s = @"\)" |> MatchSymbol s

        (*![
            <p>
            At this point you can concetrate on the main part of your grammar. Using
            a recursive-descent approach, you start by defining how to parse "factors"
            - which are primitive literals [numbers in your case], variables, and function
            calls.
            </p>
            <p>
            Next, you define "terms" - multiplying or dividing factors, and similarly, "sums"
            - adding and subtracting terms. Note that each active pattern is applied
            tail-recursively, and thus causing the basic operators to associate to the right.
            </p>
        ]*)
        [<JavaScript>]
        let rec (|Factor|_|) = function
            | NUMBER (n, rest) ->
                (Ast.Expr.Number n, rest) |> Some
            | ID (v, rest) ->
                match rest with
                | LPAREN (Expression (arg, RPAREN rest)) ->
                    (Ast.Expr.FunApply (v, arg), rest) |> Some
                | _ ->
                    (Ast.Expr.Var v, rest) |> Some
            | LPAREN (Expression (e, RPAREN rest)) ->
                    (e, rest) |> Some
            | _ ->
                None

        and [<JavaScript>] (|Term|_|) = function
            | Factor (e1, rest) ->
                match rest with
                | MUL (Term (e2, rest)) ->
                    (Ast.Expr.Prod (e1, e2), rest) |> Some
                | DIV (Term (e2, rest)) ->
                    (Ast.Expr.Ratio (e1, e2), rest) |> Some
                | _ ->
                    (e1, rest) |> Some
            | _ ->
                None

        and [<JavaScript>] (|Sum|_|) = function
            | Term (e1, rest) ->
                match rest with
                | PLUS (Sum (e2, rest)) ->
                    (Ast.Expr.Sum (e1, e2), rest) |> Some
                | MINUS (Sum (e2, rest)) ->
                    (Ast.Expr.Diff (e1, e2), rest) |> Some
                | _ ->
                    (e1, rest) |> Some
            | _ ->
                None

        (*![
            <p>
            Finally, an expression is just a "sum".  You can also define an active pattern
            to recognize the end of the input string - this is handy to make sure that the
            input string does not contain additional characters beyond what you require.
            </p>
        ]*)
        and [<JavaScript>] (|Expression|_|) = (|Sum|_|)

        [<JavaScript>]
        let (|Eof|_|) s =
            if String.IsNullOrEmpty s then
                () |> Some
            else
                match s with
                | WHITESPACE (_, rest) when rest |> String.IsNullOrEmpty ->
                    () |> Some
                | _ ->
                    None

    (*![
        <p>
        The next aspect of functionality for your function plotter is the evaluator.
        This consists of an <b>Eval</b> function that takes an environment of variables
        and their values (represented as a list) and calculates the numeric value of an
        arbitrary expression.
        </p>
        <p>
        This is performed as follows:
        </p>
        <ul>
        <li>Numbers are returned as is.</li>
        <li>Binary operations are evaluated by first evaluating the operands and performing
        the operations on the results.</li>
        <li>Variables are resolved to their values by looking them up in the environment.</li>
        <li>Built-in functions are identified and executed on their argument.</li>
        </ul>
    ]*)
    module Evaluator =
        open System
        open IntelliFactory.WebSharper
        open Ast

        [<JavaScript>]
        let rec Eval (env: (string * float) list) e =
            match e with
            | Expr.Number num        -> num
            | Expr.BinOp (f, e1, e2) -> f (Eval env e1) (Eval env e2)
            | Expr.Var v             ->
                env
                |> List.tryFind (fun (_v, _) -> _v = v)
                |> function
                    | None ->
                        "Unbound variable: " + v |> failwith
                    | Some (_, value) ->
                        value
            | Expr.FunApply (f, e) when f.ToLower() = "sin" ->
                Eval env e |> sin
            | Expr.FunApply (f, e) when f.ToLower() = "cos" ->
                Eval env e |> cos
            | Expr.FunApply (f, _) ->
                "Unknown function: " + f |> failwith

    (*![
        <p>The final aspect of the function plotter is the pagelet definition itself.</p>
    ]*)
    module Client =
        open System
        open IntelliFactory.WebSharper
        open IntelliFactory.WebSharper.Html
        open IntelliFactory.WebSharper.Html5
        open IntelliFactory.WebSharper.Formlet
        open Microsoft.FSharp.Control

        [<JavaScript>]
        let WIDTH = 500
        [<JavaScript>]
        let HEIGHT = 500
        [<JavaScript>]
        let POINTS = 250

        type PlotInfo =
            { Variable : string
              From : float
              To : float
              Formula : string }

        [<JavaScript>]
        let PlotFunctionForm : Formlet<PlotInfo> =
            let compose vVar vFrom vTo vFormula =
                {
                    Variable = vVar
                    From = vFrom |> float
                    To = vTo |> float
                    Formula = vFormula
                }
            Formlet.Yield compose
            <*> (Controls.Input "x"
                 |> Enhance.WithLabelAndInfo "Variable" "The function variable"
                 |> Validator.IsNotEmpty "")
            <*> (Controls.Input "-3.14"
                 |> Enhance.WithLabelAndInfo "From" "The first X value"
                 |> Validator.IsNotEmpty "")
            <*> (Controls.Input "3.14"
                 |> Enhance.WithLabelAndInfo "To" "The last X value"
                 |> Validator.IsNotEmpty "")
            <*> (Controls.Input ""
                 |> Enhance.WithLabelAndInfo "Formula" "The function to plot"
                 |> Validator.IsNotEmpty "")
            |> Enhance.WithSubmitAndResetButtons

        (*![
            <p>
            The main pagelet works by rendering the form defined in <b>PlotFunctionForm</b> to take
            user input, upon submitting those calculating the values to be plotted, and drawing the plot.
            </p>
        ]*)
        [<JavaScript>]
        let Main () =

            // A utility function to evaluate a formula with respect to the given
            // variable assigned with the given value.
            let EvalAt v formula x =
                Evaluator.Eval [v, x] formula

            // The function that plots a list of values with respect to a global
            // minimum and maximum.  It needs the graphics context of the canvas
            // to draw on.
            let draw min max values (ctx: Html5.CanvasRenderingContext2D) =
                // Increase range if min=max to 0..(max*2)
                let min = if min=max then 0. else min
                let max = if min=max then max*2. else max

                // Scale values into the box available (with height HEIGHT).
                let scaleY y = float HEIGHT - (float HEIGHT) / (max - min) * (y - min)

                // Plot each value
                ctx.BeginPath()
                values
                |> List.fold (fun (x, isUndefined) y ->
                    match y with
                    | None ->
                        x+1, true
                    | Some y ->
                        y
                        |> scaleY
                        |> fun sy ->
                            if isUndefined then
                                ctx.MoveTo (float x * (float WIDTH) / (float POINTS), sy)
                            else
                                ctx.LineTo (float x * (float WIDTH) / (float POINTS), sy)
                            (x+1, false)) (0, true)
                |> ignore
                ctx.Stroke()

            let conf =
                {
                    Enhance.FormContainerConfiguration.Default with
                        Header = "Enter a function to plot" |> Enhance.FormPart.Text |> Some
                        Description = "This example runs entirely on the client - \
                                       no server communication is taking place." |> Enhance.FormPart.Text |> Some
                }
            Formlet.Do {
                let! input = PlotFunctionForm |> Enhance.WithCustomFormContainer conf
                let! result =
                    (*![
                        <p>
                        The submission handler takes the input entered by the user (packaged into a <b>PlotInfo</b> record).
                        </p>
                    ]*)
                    (match input.Formula with
                    | Language.Expression (formula, Language.Eof) ->
                        try
                            (*![
                                <p>
                                To calculate the function values that you will be plotting, first you create a list (representing the domain
                                of the function) between the <b>to</b> and <b>from</b> values entered by the user.  Here, the value of
                                <b>POINTS</b> determines how many samples you draw from that range.
                                </p>
                            ]*)
                            input.To - input.From
                            |> fun range ->
                                [ input.From .. (range / (float POINTS)) .. input.To ]
                                (*![
                                    <p>
                                    Next, you fold this list by accumulating the function values at the various X values,
                                    keeping track of the minimum and maximum values encountered so far.  Function values
                                    are represented by the option type, giving None for undefined values (such as 1/0).
                                    </p>
                                    <p>
                                    You calculate function values by correctly handling undefined values and removing
                                    outliers that would otherwise skew the function plot - these are details that could be
                                    omitted giving a smaller implementation, but for robustness they are included here.
                                    </p>
                                    <p>
                                    You can evaluate the formula at a particular point by creating an environment
                                    with the function variable initialized to that point and evaluating the formula.
                                    This is performed by <b>EvalAt</b>.
                                    </p>
                                ]*)
                                // Calculate all Y's, returning None for undefined values.
                                |> List.map (fun x ->
                                    EvalAt input.Variable formula x
                                    |> fun y ->
                                        if y = infinity || y = -infinity then
                                            None
                                        else
                                            Some y)
                                // Calculate the mean and the count of defined values
                                |> fun values ->
                                    values
                                    |> List.fold (fun (sum, count) value ->
                                        match value with
                                        | None ->
                                            sum, count
                                        | Some v ->
                                            sum+v, count+1.) (0., 0.)
                                    |> fun (sum, count) -> sum / count, values, count
                                |> fun (mean, values, count) ->
                                    // Calculate the standard deviation
                                    values
                                    |> List.choose id
                                    |> List.map (fun (value: float) -> (mean - value) ** 2.0)
                                    |> List.fold (+) 0.
                                    |> fun numerator ->
                                        numerator / count |> sqrt
                                    |> fun stddev ->
                                        // Remove (by setting to None) those values that lie
                                        // outside of 3*standard deviations from the mean.
                                        values
                                        |> List.map (fun value ->
                                            match value with
                                            | None ->
                                                None
                                            | Some y ->
                                                if y - mean |> abs > 3.*stddev then
                                                    None
                                                else
                                                    Some y)
                                        // Calculate the global minimum and maximum of the
                                        // remaining values.
                                        |> List.fold (fun (min, max, values) y ->
                                            match y with
                                            | None ->
                                                min, max, y :: values
                                            | Some _y ->
                                                (if _y < min then _y else min),
                                                (if _y > max then _y else max),
                                                y :: values) (infinity, -infinity, [])
                                        (*![
                                            <p>
                                            And finally use the calculated values to draw the function plot and return it in a new DOM node.
                                            </p>
                                         ]*)
                                        |> fun (min, max, values) ->
                                            let canvas, canvasElement = Utils.NewCanvas WIDTH HEIGHT
                                            draw min max (List.rev values) (canvas.GetContext("2d"))
                                            Formlet.OfElement (fun () -> Div [ canvasElement ])

                        with
                            | e ->
                                Formlet.Return ()
                                |> Formlet.MapResult (fun _ -> Result.Failure ["Can not evaluate formula"])
                                |> Enhance.WithErrorSummary "Error"
                    | _ ->
                        Formlet.Return ()
                        |> Formlet.MapResult (fun _ -> Result.Failure ["Can not parse formula"])
                        |> Enhance.WithErrorSummary "Error")
                return ()
            }
            |> Formlet.Run ignore


    [<JavaScript>]
    let Main () =
        Div [Client.Main ()]

type PlotFunctionsViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = PlotFunctions.Main () :> _