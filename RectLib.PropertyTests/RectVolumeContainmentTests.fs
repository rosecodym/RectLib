module RectLib.PropertyTests.RectVolumeContainmentTests

open FsCheck
open FsCheck.Prop
open NUnit.Framework

open RectLib

[<Test>]
let ``A Rect contains its own volume`` () =
    let containsOwnVolume (r: Rect) = r.ContainsVolumeOf(r)
    let gen = Generators.anyRect
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) containsOwnVolume)

[<Test>]
let ``An expanded Rect volume contains itself`` () =
    let volumeContains (smaller: Rect, larger: Rect) = larger.ContainsVolumeOf(smaller)
    let gen = Generators.rectPairExpandedRandomDirs
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) volumeContains)

[<Test>]
let ``Two volumes only contain each other if they are equal`` () =
    let symmetryCorrect (r1: Rect, r2: Rect) =
        match r1.ContainsVolumeOf(r2), r2.ContainsVolumeOf(r1) with
        | true, true when r1 <> r2 -> false
        | _ -> true
    let gen = Gen.two Generators.anyRect
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) symmetryCorrect)

[<Test>]
let ``A Rect does not contain the volume of an expanded version of itself`` () =
    let volumeDoesNotContain (smaller: Rect, larger: Rect) = not <| smaller.ContainsVolumeOf(larger)
    let gen = Generators.rectPairExpandedRandomDirs
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) volumeDoesNotContain)

[<Test>]
let ``A Rect does not contain the volume of itself expanded in one dimension and contracted in the other`` () =
    let volumesDoNotContainEachOther (a: Rect, b: Rect) =
        not <| a.ContainsVolumeOf(b) && not <| b.ContainsVolumeOf(a)
    let gen = Generators.rectPairExpandedOneDimContractedTheOther
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) volumesDoNotContainEachOther)

[<Test>]
let ``A Rect does not contain the volume of itself translated by any non-zero vector`` () =
    let volumesDoNotContainEachOther (a: Rect, b: Rect) =
        not <| a.ContainsVolumeOf(b) && not <| b.ContainsVolumeOf(a)
    let gen = Generators.rectPairTranslated
    Check.QuickThrowOnFailure(forAll (Arb.fromGen gen) volumesDoNotContainEachOther)