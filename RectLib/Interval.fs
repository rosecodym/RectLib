namespace RectLib

open System

type Interval = {
    Inf: float
    Sup: float
    }
    with
        static member Create(inf: float, sup: float) : Interval =
            if inf >= sup then
                raise (ArgumentException("An interval's infimum must be less than its supremum"))
            { Inf = inf; Sup = sup }

        member this.Length = this.Sup - this.Inf

        static member op_Equality (rhs: Interval, lhs: Interval) = lhs = rhs

type internal IntervalRelationship =
    | Same
    | WhollyUnconnected
    | Adjacent of float
    | FirstContainsSecond
    | SecondContainsFirst
    | PartialOverlap of Interval

module internal IntervalInternals =

    let private isValid interval = interval.Inf < interval.Sup

    // Following all assume that a.Inf <= b.Inf
    let private areAdjacent a b = a.Sup = b.Inf
    let private shareInf a b = a.Inf = b.Inf
    let private shareSup a b = a.Sup = b.Sup
    let private shareNothing a b = a.Sup < b.Inf
    let private contains a b = a.Inf <= b.Inf && a.Sup >= b.Sup

    let containsPoint (includeEndpoints: bool) (interval: Interval) (point: float) : bool =
        if includeEndpoints then
            point >= interval.Inf && point <= interval.Sup
        else
            point > interval.Inf && point < interval.Sup

    let rec relationship (a: Interval) (b: Interval) : IntervalRelationship =
        if not <| isValid a then
            let msg = sprintf "Invalid interval (%f is not less than %f)" a.Inf a.Sup
            raise (ArgumentException(msg, "a"))
        elif not <| isValid b then
            let msg = sprintf "Invalid interval (%f is not less than %f)" b.Inf b.Sup
            raise (ArgumentException(msg, "b"))
        elif a = b then Same
        elif a.Inf > b.Inf then
            match relationship b a with
            | FirstContainsSecond -> SecondContainsFirst
            | SecondContainsFirst -> FirstContainsSecond
            | _ as anythingElse -> anythingElse
        elif areAdjacent a b then Adjacent(a.Sup)
        elif shareNothing a b then WhollyUnconnected
        elif contains a b then FirstContainsSecond
        elif contains b a then SecondContainsFirst
        else PartialOverlap(Interval.Create(b.Inf, a.Sup))
