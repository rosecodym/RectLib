module RectLib.PropertyTests.RectShellIntersectionTests

open FsCheck
open FsCheck.Prop
open NUnit.Framework

open RectLib

[<Test>]
let ``Intersecting (with corners) a Rect with itself yields its four corners and its four sides`` () =
    let selfIntersectionMatchesRect (r: Rect) =
        let corners = r.Corners |> Set.ofSeq
        let sides = r.Sides |> Set.ofSeq
        let _, intersectionPoints, intersectionSegments = Rect.ShellIntersection(r, r, includeCorners=true)
        let cornersMatch = corners = (intersectionPoints |> Set.ofSeq)
        let sidesMatch = sides = (intersectionSegments |> Set.ofSeq)
        cornersMatch && sidesMatch
    let gen = Generators.rectAnywhereWithin Generators.globalLeft Generators.globalRight Generators.globalBottom Generators.globalTop
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) selfIntersectionMatchesRect)

[<Test>]
let ``Intersecting (without corners) a Rect with itself yields its four sides only`` () =
    let selfIntersectionMatchesRect (r: Rect) =
        let sides = r.Sides |> Set.ofSeq
        let _, intersectionPoints, intersectionSegments = Rect.ShellIntersection(r, r, includeCorners=false)
        let sidesMatch = sides = (intersectionSegments |> Set.ofSeq)
        intersectionPoints = [||] && sidesMatch
    let gen = Generators.rectAnywhereWithin Generators.globalLeft Generators.globalRight Generators.globalBottom Generators.globalTop
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) selfIntersectionMatchesRect)

[<TestCase(true)>]
[<TestCase(false)>]
let ``Rects with non-overlapping horizontal ranges have an empty intersection`` (includeCorners) =
    let haveEmptyIntersection (r1, r2) =
        not <| Rect.ShellIntersection(r1, r2, includeCorners, ref [||], ref [||])
    let gen = Generators.rectPairWithNonIntersectingHorizontalRanges
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) haveEmptyIntersection)
    
[<TestCase(true)>]
[<TestCase(false)>]
let ``Rects with non-overlapping vertical ranges have an empty intersection`` (includeCorners) =
    let haveEmptyIntersection (r1, r2) =
        not <| Rect.ShellIntersection(r1, r2, includeCorners, ref [||], ref [||])
    let gen = Generators.rectPairWithNonIntersectingVerticalRanges
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) haveEmptyIntersection)

[<Test>]
let ``An endpoint-containing shell intersection always contains the endpoints of calculated intersection segments as intersection points`` () =
    let containsEndpoints (r1, r2) =
        let _, points, segs = Rect.ShellIntersection(r1, r2, includeCorners=true)
        let pointSet = points |> Set.ofSeq
        segs
        |> Seq.collect (fun seg -> [seg.Start; seg.End])
        |> Seq.exists (not << pointSet.Contains)
        |> not
    let gen = Gen.two Generators.anyRect
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) containsEndpoints)

[<TestCase(true)>]
[<TestCase(false)>]
let ``Rects touching only at one corner have a single point intersection at that corner, iff corner intersections are enabled`` (includeCorners) =
    let haveSinglePointIntersectionAt (r1, r2, corner) =
        let _, intersectionPoints, _ = Rect.ShellIntersection(r1, r2, includeCorners)
        intersectionPoints = if includeCorners then [| corner |] else [||]
    let gen = Generators.rectPairTouchingOnlyAtCorner
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) haveSinglePointIntersectionAt)

[<TestCase(true)>]
[<TestCase(false)>]
let ``Intersecting a Rect with itself transposed slightly diagonally yields exactly two points`` (includeCorners) =
    let hasExactlyTwoPointIntersections (r1, r2) =
        let _, points, segments = Rect.ShellIntersection(r1, r2, includeCorners)
        points.Length = 2 && segments.Length = 0
    let gen = Generators.rectPairTransposedDiagonallySlightly
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) hasExactlyTwoPointIntersections)

[<TestCase(true)>]
[<TestCase(false)>]
let ``Intersecting a Rect with a version of itself expanded in one direction and contracted in the other yields exactly four point intersections`` (includeCorners) =
    let hasExactlyFourPointIntersections (r1, r2) =
        let _, points, segments = Rect.ShellIntersection(r1, r2, includeCorners)
        points.Length = 4 && segments.Length = 0
    let gen = Generators.rectPairExpandedOneDimContractedTheOther
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) hasExactlyFourPointIntersections)

[<Test>]
let ``The endpoint-containing shell intersection of Rect with an adjacent copy of itself is the segment and points they have in common`` () =
    let commonSide (r1: Rect, r2: Rect) =
        r1.Sides
        |> Set.ofSeq
        |> Set.intersect (r2.Sides |> Set.ofSeq)
        |> Seq.exactlyOne
    let commonPoints (r1: Rect, r2: Rect) =
        r1.Corners
        |> Set.ofSeq
        |> Set.intersect (r2.Corners |> Set.ofSeq)
        |> Seq.windowed 2
        |> Seq.exactlyOne
        |> Set.ofArray
    let haveCommonFeaturesAsIntersection (r1: Rect, r2: Rect) =
        let _, intersectionPoints, intersectionSegments = Rect.ShellIntersection(r1, r2, includeCorners=true)
        intersectionPoints |> Set.ofSeq = commonPoints (r1, r2) &&
        intersectionSegments = [| commonSide (r1, r2) |]
    let gen = Generators.rectPairAdjacentCopies
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) haveCommonFeaturesAsIntersection)

[<Test>]
let ``The non-endpoint-containing shell intersection of Rect with an adjacent copy of itself is the segment they have in common`` () =
    let commonSide (r1: Rect, r2: Rect) =
        r1.Sides
        |> Set.ofSeq
        |> Set.intersect (r2.Sides |> Set.ofSeq)
        |> Seq.exactlyOne
    let haveCommonFeaturesAsIntersection (r1: Rect, r2: Rect) =
        let _, intersectionPoints, intersectionSegments = Rect.ShellIntersection(r1, r2, includeCorners=false)
        intersectionPoints = [||] &&
        intersectionSegments = [| commonSide (r1, r2) |]
    let gen = Generators.rectPairAdjacentCopies
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) haveCommonFeaturesAsIntersection)

[<TestCase(true)>]
[<TestCase(false)>]
let ``The endpoint-containing shell intersection of a Rect with an adjacent but orthogonally perturbed copy of itself contains one segment with length equal to their common side length`` (includeCorners) =
    let intersectionIsCorrect (r1, r2, expectedSideLength) =
        let _, _, segments = Rect.ShellIntersection(r1, r2, includeCorners)
        match segments with
        | [| s |] when s.Range.Sup - s.Range.Inf = expectedSideLength -> true
        | _ -> false
    let gen = Generators.rectPairAdjacentCopiesWithOrthogonalOffset
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<Test>]
let ``The non-endpoint-containing shell intersection of a Rect with an adjacent but orthogonally perturbed copy of itself contains no points`` () =
    let intersectionIsCorrect (r1, r2, _) =
        let _, points, _ = Rect.ShellIntersection(r1, r2, includeCorners=false)
        points = [||]
    let gen = Generators.rectPairAdjacentCopiesWithOrthogonalOffset
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<Test>]
let ``The non-endpoint-containing shell intersection of a Rect with a version of itself with any number of sides expanded contains no points`` () =
    let intersectionIsCorrect (smaller, larger) =
        let _, points, _ = Rect.ShellIntersection(smaller, larger, includeCorners=false)
        points = [||]
    let gen = Generators.rectPairExpandedRandomDirs
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of a Rect with a version of itself with one side expanded contains three segments that are its edges`` (includeCorners) =
    let intersectionIsCorrect (smaller, larger) =
        let _, _, segs = Rect.ShellIntersection(smaller, larger, includeCorners)
        let edges = smaller.Sides |> Set.ofSeq
        segs.Length = 3 && segs |> Seq.exists (not << edges.Contains) |> not
    let gen = Generators.rectPairExpandedOneDir
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of a Rect with a version of itself expanded both ways along one axis contains two segments that are its edges`` (includeCorners) =
    let intersectionIsCorrect (smaller, larger) =
        let _, _, segs = Rect.ShellIntersection(smaller, larger, includeCorners)
        let edges = smaller.Sides |> Set.ofSeq
        segs.Length = 2 && segs |> Seq.exists (not << edges.Contains) |> not
    let gen = Generators.rectPairExpandedTwoOppositeDirs
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of a Rect with a version of itself expanded in two orthogonal directions contains two segments that are its edges`` (includeCorners) =
    let intersectionIsCorrect (smaller, larger) =
        let _, _, segs = Rect.ShellIntersection(smaller, larger, includeCorners)
        let edges = smaller.Sides |> Set.ofSeq
        segs.Length = 2 && segs |> Seq.exists (not << edges.Contains) |> not
    let gen = Generators.rectPairExpandedTwoOrthoDirs
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of a Rect with a version of itself expanded in three directions contains one segment that is one of its edges`` (includeCorners) =
    let intersectionIsCorrect (smaller, larger) =
        let _, _, segs = Rect.ShellIntersection(smaller, larger, includeCorners)
        let edges = smaller.Sides |> Set.ofSeq
        segs.Length = 1 && segs |> Seq.exists (not << edges.Contains) |> not
    let gen = Generators.rectPairExpandedThreeDirs
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of a Rect with a version of itself contracted in one direction and expanded in the orthogonal dimension contains a single segment that is one of its edges`` (includeCorners) =
    let intersectionIsCorrect (orig, modified) =
        let _, _, segs = Rect.ShellIntersection(orig, modified, includeCorners)
        let edges = orig.Sides |> Set.ofSeq
        segs.Length = 1 && segs |> Seq.exists (not << edges.Contains) |> not
    let gen = Generators.rectPairContractedOneDirExpandedOtherDim
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)

[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of a Rect with a version of itself contracted in one direction and expanded in the orthogonal dimension contains two points that are not intersection segment endpoints`` (includeCorners) =
    let intersectionIsCorrect (orig, modified) =
        let _, points, segs = Rect.ShellIntersection(orig, modified, includeCorners)
        let endpoints = segs |> Seq.collect (fun s -> [s.Start; s.End])
        let notEndpoints = points |> Seq.except endpoints |> List.ofSeq
        notEndpoints.Length = 2
    let gen = Generators.rectPairContractedOneDirExpandedOtherDim
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)
    
[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of two Rects such that exactly two corners of one are strictly within the other consists of zero segments and two points`` (includeCorners) =
    let intersectionIsCorrect (r1, r2) =
        let _, points, segs = Rect.ShellIntersection(r1, r2, includeCorners)
        segs.Length = 0 && points.Length = 2
    let gen = Generators.rectPairExactlyTwoCornersOfOneInOther
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)
    
[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of two Rects such that exactly one corner of one is strictly within the other contains exactly one segment that is not an edge of either Rect`` (includeCorners) =
    let intersectionIsCorrect (r1, r2) =
        let _, _, segs = Rect.ShellIntersection(r1, r2, includeCorners)
        let edges = Seq.append r1.Sides r2.Sides |> Set.ofSeq
        not <| Seq.contains (Seq.exactlyOne segs) edges
    let gen = Generators.rectPairSlidAndExpandedOneOrthogonalDir
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)
    
[<TestCase(true)>]
[<TestCase(false)>]
let ``The shell intersection of two Rects such that exactly one corner of one is strictly within the other contains exactly one point that is not a corner of either Rect`` (includeCorners) =
    let intersectionIsCorrect (r1, r2) =
        let _, points, _ = Rect.ShellIntersection(r1, r2, includeCorners)
        let corners = Seq.append r1.Corners r2.Corners 
        points |> Seq.except corners |> Seq.length = 1
    let gen = Generators.rectPairSlidAndExpandedOneOrthogonalDir
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectionIsCorrect)