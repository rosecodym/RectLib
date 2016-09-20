module RectLib.PropertyTests.RectTests

open FsCheck
open FsCheck.Prop
open NUnit.Framework

open RectLib

[<Test>]
let ``A Rect translated by the null vector equals itself`` () =
    let equalsNullTranslation (r: Rect) = r = Rect.TranslationOf(r, 0.0, 0.0)
    Check.QuickThrowOnFailure(forAll (Arb.fromGen Generators.anyRect) equalsNullTranslation)

[<Test>]
let ``A Rect built from an existing Rect's left, bottom, width, and height properties equals that Rect`` () =
    let matches (r: Rect) = r = Rect.AtPosition(r.Left, r.Bottom, r.Width, r.Height)
    Check.QuickThrowOnFailure(forAll (Arb.fromGen Generators.anyRect) matches)

[<Test>]
let ``A Rect built from an existing Rect's bounding box equals that Rect`` () =
    let matches (r: Rect) = r = Rect.FromBoundingBox(r.Left, r.Bottom, r.Right, r.Top)
    Check.QuickThrowOnFailure(forAll (Arb.fromGen Generators.anyRect) matches)

[<Test>]
let ``Each of a Rect's corners is an endpoint of one of its sides`` () =
    let cornersBoundSides (r: Rect) =
        let sideEndpoints = r.Sides |> Seq.collect (fun s -> [s.Start; s.End]) |> Set.ofSeq
        r.Corners |> Seq.exists (not << sideEndpoints.Contains) |> not
    Check.QuickThrowOnFailure(forAll (Arb.fromGen Generators.anyRect) cornersBoundSides)

[<Test>]
let ``Each of a Rect's side endpoints is one of its corners`` () =
    let sideEndpointsAreCorners (r: Rect) =
        let corners = Set.ofSeq r.Corners
        r.Sides
        |> Seq.collect (fun s -> [s.Start; s.End])
        |> Seq.exists (not << corners.Contains)
        |> not
    Check.QuickThrowOnFailure(forAll (Arb.fromGen Generators.anyRect) sideEndpointsAreCorners)