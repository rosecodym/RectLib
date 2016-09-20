namespace RectLib

type Orientation =
    | Horizontal
    | Vertical

type OrientedInterval = {
    Orientation: Orientation
    At: float
    Range: Interval
    }
    with
        static member Create(orientation: Orientation, at: float, range: Interval) : OrientedInterval =
            {
                Orientation = orientation
                At = at
                Range = range
            }
        member this.Start : AbsolutePoint =
            match this.Orientation with
            | Horizontal -> { HorizontalPosition = this.Range.Inf; VerticalPosition = this.At }
            | Vertical -> { HorizontalPosition = this.At; VerticalPosition = this.Range.Inf }
        member this.End : AbsolutePoint =
            match this.Orientation with
            | Horizontal -> { HorizontalPosition = this.Range.Sup; VerticalPosition = this.At }
            | Vertical -> { HorizontalPosition = this.At; VerticalPosition = this.Range.Sup }

        static member op_Equality (lhs: OrientedInterval, rhs: OrientedInterval) = lhs = rhs

type internal PointOnInterval = {
    IntervalAt: float
    OnInterval: float
    }
    with
        static member create (at: float) (along: float) : PointOnInterval =
            { IntervalAt = at; OnInterval = along }

type internal OrientedIntervalIntersection =
    | OrientedInterval of OrientedInterval
    | PointOnFirstInterval of PointOnInterval
    | None

module internal OrientedIntervalInternals =
    let private intersectionSameOrientation includeEndpoints a b =
        assert (a.Orientation = b.Orientation)
        if a.At <> b.At then None else
        match IntervalInternals.relationship a.Range b.Range with
        | Same -> OrientedInterval(a)
        | WhollyUnconnected -> None
        | Adjacent(p) when includeEndpoints -> PointOnFirstInterval(PointOnInterval.create a.At p)
        | Adjacent(_) -> None
        | FirstContainsSecond -> OrientedInterval(b)
        | SecondContainsFirst -> OrientedInterval(a)
        | PartialOverlap(overlap) -> OrientedInterval({ a with Range = overlap })

    let private intersectionOppositeOrientation includeEndpoints a b =
        assert (a.Orientation <> b.Orientation)
        if not <| IntervalInternals.containsPoint includeEndpoints a.Range b.At ||
           not <| IntervalInternals.containsPoint includeEndpoints b.Range a.At
           then None
        else PointOnFirstInterval(PointOnInterval.create a.At b.At)

    let intersection (includeEndpoints: bool) (a: OrientedInterval) (b: OrientedInterval) : OrientedIntervalIntersection =
        if a.Orientation = b.Orientation then intersectionSameOrientation includeEndpoints a b
        else intersectionOppositeOrientation includeEndpoints a b

    let contains (a: OrientedInterval) (b: OrientedInterval) : bool =
        a.Orientation = b.Orientation &&
        a.At = b.At &&
        match IntervalInternals.relationship a.Range b.Range with
        | Same | FirstContainsSecond -> true
        | _ -> false