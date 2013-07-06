
namespace global

module Event = 
    /// An event which triggers on every 'n' triggers of the input event
    let every n (ev:IEvent<_>) = 
        let out = new Event<_>()
        let count = ref 0 
        ev.Add (fun arg -> incr count; if !count % n = 0 then out.Trigger arg)
        out.Publish

    /// An event which triggers on every 'n' triggers of the input event
    let window n (ev:IEvent<_>) = 
        let out = new Event<_>()
        let queue = System.Collections.Generic.Queue<_>()
        ev.Add (fun arg -> queue.Enqueue arg; 
                           if queue.Count >= n then 
                                out.Trigger (queue.ToArray()); 
                                queue.Dequeue() |> ignore)
        out.Publish

    let pairwise  (ev:IEvent<_>) = 
        let out = new Event<_>()
        let queue = System.Collections.Generic.Queue<_>()
        ev.Add (fun arg -> queue.Enqueue arg; 
                           if queue.Count >= 2 then 
                                let elems = queue.ToArray()
                                out.Trigger (elems.[0], elems.[1])
                                queue.Dequeue() |> ignore)
        out.Publish

