module RectLib.PropertyTests.RectVolumeIntersectionTests

open FsCheck
open FsCheck.Prop
open NUnit.Framework

open RectLib

[<Test>]
let ``A Rect intersects its own volume`` () =
    let intersectsOwnVolume (r: Rect) = Rect.VolumesIntersect(r, r)
    let gen = Generators.anyRect
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) intersectsOwnVolume)

[<Test>]
let ``Rect volume intersection is symmetric`` () =
    let isSymmetric (r1, r2) = Rect.VolumesIntersect(r1, r2) = Rect.VolumesIntersect(r2, r1)
    let gen = Gen.two Generators.anyRect
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) isSymmetric)

[<Test>]
let ``The volumes of a Rect and an expanded version of itself intersect`` () =
    let gen = Generators.rectPairExpandedRandomDirs
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) Rect.VolumesIntersect)

[<Test>]
let ``Transposing a Rect slightly diagonally yields two Rects whose volumes intersect`` () =
    let gen = Generators.rectPairTransposedDiagonallySlightly
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) Rect.VolumesIntersect)

[<Test>]
let ``The volume of a Rect does not intersect with an adjacent copy of itself`` () =
    let gen = Generators.rectPairAdjacentCopies
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) (not << Rect.VolumesIntersect))

[<Test>]
let ``A Rect intersects the volume of another Rect that strictly contains exactly two of its corners`` () =
    let gen = Generators.rectPairExactlyTwoCornersOfOneInOther
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) Rect.VolumesIntersect)

[<Test>]
let ``Transposing a Rect slightly along an axis yields two Rects whose volumes intersect`` () =
    let gen = Generators.rectPairSlidAndExpandedOneOrthogonalDir
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) Rect.VolumesIntersect)