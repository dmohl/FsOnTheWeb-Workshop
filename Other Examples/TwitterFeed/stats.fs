
namespace global

module MultiMap = 
    let empty<'T,'U when 'T : comparison> : Map<'T,'U list> = Map.empty
    let add key x multiMap = 
        let prev = match Map.tryFind key multiMap with None -> [] | Some v -> v 
        Map.add key (x::prev) multiMap

type Histogram<'T when 'T : comparison> = Map<'T,int>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Histogram = 
    let empty<'T when 'T : comparison> : Histogram<'T> = Map.empty
    let add histogram key  = 
        let prev = match Map.tryFind key histogram with None -> 0 | Some v -> v 
        Map.add key (prev+1) histogram

    let addMany histogram  keys =  Seq.fold add histogram keys 

    let top n (histogram: Histogram<'T>) = 
        histogram |> Seq.sortBy (fun (KeyValue(_,d)) -> -d) |> Seq.truncate n |> Seq.toArray


module Event = 
    let histogram ev = 
        ev |> Event.scan Histogram.addMany Histogram.empty 

    let histogramBy f ev = 
        ev |> Event.map f |> histogram

    let indexBy f (ev:IEvent<_>) = Event.scan (fun z x -> MultiMap.add (f x) x z) MultiMap.empty ev
