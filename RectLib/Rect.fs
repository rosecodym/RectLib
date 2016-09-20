namespace RectLib

open System
open System.Runtime.InteropServices

type internal ShellIntersectionFeature =
    | Segment of OrientedInterval
    | Point of AbsolutePoint

type Rect = {
    HorizontalRange: Interval
    VerticalRange: Interval
    }
    with
        static member FromIntervals (horizontal: Interval, vertical: Interval) : Rect =
            {
                HorizontalRange = horizontal
                VerticalRange = vertical
            }
        static member FromBoundingBox (left: float, bottom: float, right: float, top: float) : Rect =
            if left >= right then raise (ArgumentException("A Rect's left bound must be less than its right bound"))
            elif bottom >= top then raise (ArgumentException("A Rect's bottom bound must be less than its top bound"))
            let h = Interval.Create(left, right)
            let v = Interval.Create(bottom, top)
            Rect.FromIntervals (h, v)
        static member AtPosition (left: float, bottom: float, width: float, height: float) : Rect =
            if width <= 0.0 then raise (ArgumentOutOfRangeException("A Rect's width must be positive", "width"))
            elif height <= 0.0 then raise (ArgumentOutOfRangeException("A Rect's height must be positive", "height"))
            Rect.FromBoundingBox (left, bottom, left + width, bottom + height)
        static member TranslationOf (src: Rect, horizontalDelta: float, verticalDelta: float) : Rect =
            Rect.FromBoundingBox(src.Left + horizontalDelta, src.Bottom + verticalDelta, src.Right + horizontalDelta, src.Top + verticalDelta)

        member this.Left : float = this.HorizontalRange.Inf
        member this.Right : float = this.HorizontalRange.Sup
        member this.Bottom : float = this.VerticalRange.Inf
        member this.Top : float = this.VerticalRange.Sup

        member this.Width : float = this.Right - this.Left
        member this.Height : float = this.Top - this.Bottom

        member this.Sides : seq<OrientedInterval> =
            let bottom = this.VerticalRange.Inf
            let top = this.VerticalRange.Sup
            let left = this.HorizontalRange.Inf
            let right = this.HorizontalRange.Sup
            [
                OrientedInterval.Create(Horizontal, bottom, this.HorizontalRange)
                OrientedInterval.Create(Horizontal, top, this.HorizontalRange)
                OrientedInterval.Create(Vertical, left, this.VerticalRange)
                OrientedInterval.Create(Vertical, right, this.VerticalRange)
            ] :> _

        member this.Corners : seq<AbsolutePoint> =
            [
                { HorizontalPosition = this.HorizontalRange.Inf; VerticalPosition = this.VerticalRange.Inf }
                { HorizontalPosition = this.HorizontalRange.Sup; VerticalPosition = this.VerticalRange.Inf }
                { HorizontalPosition = this.HorizontalRange.Sup; VerticalPosition = this.VerticalRange.Sup }
                { HorizontalPosition = this.HorizontalRange.Inf; VerticalPosition = this.VerticalRange.Sup }
            ] :> _

        static member HaveCommonSide (a: Rect, b: Rect) : bool =
            seq {
                for a' in a.Sides do
                    for b' in b.Sides do
                        yield (a', b')
            }
            |> Seq.exists (fun (a', b') ->
                OrientedIntervalInternals.contains a' b' ||
                OrientedIntervalInternals.contains b' a')

        static member ShellIntersection (a: Rect,
                                         b: Rect,
                                         includeCorners: bool,
                                         [<Out>] intersectionPoints: AbsolutePoint [] byref,
                                         [<Out>] intersectionSegments: OrientedInterval [] byref) : bool =
            let points, segments =
                seq {
                    for a' in a.Sides do
                        for b' in b.Sides do
                            match OrientedIntervalInternals.intersection includeCorners a' b', a'.Orientation with
                            | None, _ -> ()
                            | PointOnFirstInterval(p), Horizontal ->
                                yield Point({ HorizontalPosition = p.OnInterval; VerticalPosition = p.IntervalAt })
                            | PointOnFirstInterval(p), Vertical ->
                                yield Point({ HorizontalPosition = p.IntervalAt; VerticalPosition = p.OnInterval })
                            | OrientedInterval(oi), _ -> yield Segment(oi)
                }
                |> Seq.distinct
                |> List.ofSeq
                |> List.partition (function
                    | Point(_) -> true
                    | Segment(segment) -> false)
            intersectionPoints <-
                points
                |> Seq.map (function | Point(p) -> p | _ -> failwith "shouldn't happen")
                |> Array.ofSeq
            intersectionSegments <-
                segments
                |> Seq.map (function | Segment(s) -> s | _ -> failwith "shouldn't happen")
                |> Array.ofSeq
            not (Array.isEmpty intersectionPoints && Array.isEmpty intersectionSegments)

        static member VolumesIntersect (a: Rect, b: Rect) : bool =
            let vertical = IntervalInternals.relationship a.VerticalRange b.VerticalRange
            let horizontal = IntervalInternals.relationship a.HorizontalRange b.HorizontalRange
            let intersects = function
                | Same | FirstContainsSecond | SecondContainsFirst | PartialOverlap(_) -> true
                | _ -> false
            intersects vertical && intersects horizontal

        member this.ContainsVolumeOf (other: Rect) : bool =
            let vertical = IntervalInternals.relationship this.VerticalRange other.VerticalRange
            let horizontal = IntervalInternals.relationship this.HorizontalRange other.HorizontalRange
            let covers = function
                | Same | FirstContainsSecond -> true
                | _ -> false
            covers vertical && covers horizontal

        override this.ToString () =
            sprintf "{Rect from (%f,%f) to (%f,%f)}"
                    this.HorizontalRange.Inf
                    this.VerticalRange.Inf
                    this.HorizontalRange.Sup
                    this.VerticalRange.Sup

        static member op_Equality (lhs: Rect, rhs: Rect) = lhs = rhs