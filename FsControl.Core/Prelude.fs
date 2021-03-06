namespace FsControl.Core

open System.Collections.Generic

module internal Prelude =
    let inline flip f x y = f y x
    let inline const' k _ = k
    let inline choice f g = function Choice2Of2 x -> f x | Choice1Of2 y -> g y
    let inline option n f = function None -> n | Some x -> f x

[<RequireQualifiedAccess>]
module internal List =
    let inline singleton x = [x]
    let inline cons x y = x :: y

[<RequireQualifiedAccess>]
module internal Seq =
    // http://codebetter.com/matthewpodwysocki/2009/05/06/functionally-implementing-intersperse/
    let inline intersperse sep list = seq {
        let notFirst = ref false
        for element in list do 
            if !notFirst then yield sep
            yield element
            notFirst := true}

    let inline splitBy projection (source : _ seq) = seq {
        let rec loop (e : IEnumerator<_>) g (members : List<_>) = seq {        
            members.Add(e.Current)
            if e.MoveNext() then
                let key = projection e.Current
                if key = g then
                    yield! loop e key members
                else
                    yield g, seq members
                    yield! loop e key (List())
            else
                yield g, seq members
        }
        use e = source.GetEnumerator()
        if e.MoveNext() then
            yield! loop e (projection e.Current) (List())
    }

[<RequireQualifiedAccess>]
module internal Error =
    let inline map f = function Choice1Of2 x -> Choice1Of2(f x) | Choice2Of2 x -> Choice2Of2 x
    let inline result x = Choice1Of2 x
    let inline throw  x = Choice2Of2 x
    let inline bind  (f:'t -> Choice<'v,'e>) = function Choice1Of2 v  -> f v | Choice2Of2 e -> Choice2Of2 e
    let inline catch (f:'t -> Choice<'v,'e>) = function Choice1Of2 v  -> Choice1Of2 v | Choice2Of2 e -> f e

namespace FsControl.Core.Types
type Id<'t>(v:'t) =
   let value = v
   member this.getValue = value

[<RequireQualifiedAccess>]
module Id =
    let run   (x:Id<_>) = x.getValue
    let map f (x:Id<_>) = Id (f x.getValue)
    let create x = Id (x)
