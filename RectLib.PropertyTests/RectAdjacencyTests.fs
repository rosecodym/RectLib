module RectLib.PropertyTests.RectAdjacencyTest

open FsCheck
open FsCheck.Prop
open NUnit.Framework

open RectLib

[<Test>]
let ``A Rect is adjacent to itself`` () =
    let isAdjacentToSelf (r: Rect) = Rect.HaveCommonSide(r, r)
    let gen = Generators.anyRect
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) isAdjacentToSelf)

[<Test>]
let ``Rect side sharing is symmetric`` () =
    let isSymmetric (r1, r2) = Rect.HaveCommonSide(r1, r2) = Rect.HaveCommonSide(r2, r1);
    let gen = Gen.two Generators.anyRect;
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) isSymmetric)

[<Test>]
let ``Rects that touch only on corners are not adjacent`` () =
    let areNotAdjacent (r1: Rect, r2: Rect, _) = not <| Rect.HaveCommonSide(r1, r2)
    let gen = Generators.rectPairTouchingOnlyAtCorner
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) areNotAdjacent)

[<Test>]
let ``A Rect is not adjacent to a version of itself slightly transposed diagonally`` () =
    let areNotAdjacent (r1: Rect, r2: Rect) = not <| Rect.HaveCommonSide(r1, r2)
    let gen = Generators.rectPairTransposedDiagonallySlightly
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) areNotAdjacent)

[<Test>]
let ``A Rect is not adjacent to a version of itself expanded in one dimension and contracted in the other`` () =
    let areNotAdjacent (r1: Rect, r2: Rect) = not <| Rect.HaveCommonSide(r1, r2)
    let gen = Generators.rectPairExpandedOneDimContractedTheOther
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) areNotAdjacent)

[<Test>]
let ``A Rect is adjacent to a version of itself translated over by one of its dimensions`` () =
    let gen = Generators.rectPairAdjacentCopies
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) Rect.HaveCommonSide)

[<Test>]
let ``A Rect is not adjacent to a version of itself translated over by one of its dimensions but offset orthogonally`` () =
    let areAdjacent (r1, r2, _) = Rect.HaveCommonSide(r1, r2)
    let gen = Generators.rectPairAdjacentCopiesWithOrthogonalOffset
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) (not << areAdjacent))

[<Test>]
let ``A Rect is adjacent to a version of itself expanded in one, two, or three directions`` () =
    let gen = Gen.oneof [ Generators.rectPairExpandedOneDir
                          Generators.rectPairExpandedTwoOppositeDirs
                          Generators.rectPairExpandedTwoOrthoDirs
                          Generators.rectPairExpandedThreeDirs ]
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) Rect.HaveCommonSide)

[<Test>]
let ``A Rect is not adjacent to a slid version of itself`` () =
    let gen = Generators.rectPairSlidAndExpandedOneOrthogonalDir
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) (not << Rect.HaveCommonSide))