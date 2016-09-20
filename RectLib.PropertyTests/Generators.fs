module RectLib.PropertyTests.Generators

// Warning 40 is the warning about recursive object definitions
// I'm not doing anything crazy, I just filter edge cases for 
// some of the generators by checking for them and then re-invoking
// the generator if they occur.
#nowarn "40"

open FsCheck

open RectLib

let globalLeft = -8
let globalRight = 8
let globalBottom = -8
let globalTop = 8
let maxWidth = globalRight - globalLeft
let maxHeight = globalTop - globalBottom

let interval minInf maxSup =
    gen {
        let! inf = Gen.choose (minInf, maxSup - 1)
        let! sup = Gen.choose (inf + 1, maxSup)
        return Interval.Create(float inf, float sup)
    }

let rectAnywhereWithin globalLeft globalRight globalBottom globalTop =
    gen {
        let! h = interval globalLeft globalRight
        let! v = interval globalBottom globalTop
        return Rect.FromIntervals(h, v)
    }

let anyRect = rectAnywhereWithin globalLeft globalRight globalBottom globalTop

let rectPairWithNonIntersectingHorizontalRanges =
    gen {
        let! h1 = interval globalLeft (globalRight - 2)
        let! h2 = interval (h1.Sup + 1.0 |> int) globalRight
        let! v1 = interval globalBottom globalTop
        let! v2 = interval globalBottom globalTop
        let r1 = Rect.FromIntervals(h1, v1)
        let r2 = Rect.FromIntervals(h2, v2)
        return (r1, r2)
    }

let rectPairWithNonIntersectingVerticalRanges =
    gen {
        let! h1 = interval globalLeft globalRight
        let! h2 = interval globalLeft globalRight
        let! v1 = interval globalBottom (globalTop - 2)
        let! v2 = interval (v1.Sup + 1.0 |> int) globalTop
        let r1 = Rect.FromIntervals(h1, v1)
        let r2 = Rect.FromIntervals(h2, v2)
        return (r1, r2)
    }

let rectTouchingBottomLeft ofRect =
    gen {
        let left1 = ofRect.HorizontalRange.Inf
        let bottom1 = ofRect.VerticalRange.Inf
        let! left2 = Gen.choose (int left1 - maxWidth, int left1 - 1)
        let! bottom2 = Gen.choose (int bottom1 - maxHeight, int bottom1 - 1)
        let r2 = Rect.FromBoundingBox(float left2, float bottom2, left1, bottom1)
        let pt =
            {
                HorizontalPosition = left1
                VerticalPosition = bottom1
            }
        return (r2, pt)
    }

let rectTouchingBottomRight ofRect =
    gen {
        let right1 = ofRect.HorizontalRange.Sup
        let bottom1 = ofRect.VerticalRange.Inf
        let! right2 = Gen.choose (int right1 + 1, int right1 + maxWidth)
        let! bottom2 = Gen.choose (int bottom1 - maxHeight, int bottom1 - 1)
        let r2 = Rect.FromBoundingBox(right1, float bottom2, float right2, bottom1)
        let pt =
            {
                HorizontalPosition = right1
                VerticalPosition = bottom1
            }
        return (r2, pt)
    }

let rectTouchingTopRight ofRect =
    gen {
        let right1 = ofRect.HorizontalRange.Sup
        let top1 = ofRect.VerticalRange.Sup
        let! right2 = Gen.choose (int right1 + 1, int right1 + maxWidth)
        let! top2 = Gen.choose (int top1 + 1, int top1 + maxWidth)
        let r2 = Rect.FromBoundingBox(right1, top1, float right2, float top2)
        let pt =
            {
                HorizontalPosition = right1
                VerticalPosition = top1
            }
        return (r2, pt)
    }

let rectTouchingTopLeft ofRect =
    gen {
        let left1 = ofRect.HorizontalRange.Inf
        let top1 = ofRect.VerticalRange.Sup
        let! left2 = Gen.choose (int left1 - maxWidth, int left1 - 1)
        let! top2 = Gen.choose (int top1 + 1, int top1 + maxWidth)
        let r2 = Rect.FromBoundingBox(float left2, top1, left1, float top2)
        let pt =
            {
                HorizontalPosition = left1
                VerticalPosition = top1
            }
        return (r2, pt)
    }

let rectPairTouchingOnlyAtCorner =
    gen {
        let! r1 = anyRect
        let! r2, corner = Gen.oneof [ rectTouchingBottomLeft r1
                                      rectTouchingBottomRight r1
                                      rectTouchingTopRight r1
                                      rectTouchingTopLeft r1]
        return (r1, r2, corner)
    }

let rec rectPairExpandedRandomDirs =
    gen {
        let! r1 = anyRect
        let! left2 = Gen.elements [r1.Left; r1.Left - 1.0]
        let! bottom2 = Gen.elements [r1.Bottom; r1.Bottom - 1.0]
        let! right2 = Gen.elements [r1.Right; r1.Right + 1.0]
        let! top2 = Gen.elements [r1.Top; r1.Top + 1.0]
        let r2 = Rect.FromBoundingBox (left2, bottom2, right2, top2)
        if r1 = r2 then return! rectPairExpandedRandomDirs
        else return (r1, r2)
    }

let rectPairExpandedOneDir =
    gen {
        let! r1 = anyRect
        let! r2 = Gen.elements [ Rect.FromBoundingBox(r1.Left - 1.0, r1.Bottom, r1.Right, r1.Top)
                                 Rect.FromBoundingBox(r1.Left, r1.Bottom - 1.0, r1.Right, r1.Top)
                                 Rect.FromBoundingBox(r1.Left, r1.Bottom, r1.Right + 1.0, r1.Top)
                                 Rect.FromBoundingBox(r1.Left, r1.Bottom, r1.Right, r1.Top + 1.0) ]
        return (r1, r2)
    }

let rectPairExpandedTwoOppositeDirs =
    gen {
        let! r1 = anyRect
        let! r2 = Gen.elements [ Rect.FromBoundingBox(r1.Left - 1.0, r1.Bottom, r1.Right + 1.0, r1.Top)
                                 Rect.FromBoundingBox(r1.Left, r1.Bottom - 1.0, r1.Right, r1.Top + 1.0) ]
        return (r1, r2)
    }

let rectPairExpandedTwoOrthoDirs =
    gen {
        let! r1 = anyRect
        let! r2 = Gen.elements [ Rect.FromBoundingBox(r1.Left - 1.0, r1.Bottom - 1.0, r1.Right, r1.Top) 
                                 Rect.FromBoundingBox(r1.Left, r1.Bottom - 1.0, r1.Right + 1.0, r1.Top)
                                 Rect.FromBoundingBox(r1.Left, r1.Bottom, r1.Right + 1.0, r1.Top + 1.0)
                                 Rect.FromBoundingBox(r1.Left - 1.0, r1.Bottom, r1.Right, r1.Top + 1.0) ]
        return (r1, r2)
    }

let rectPairExpandedThreeDirs =
    gen {
        let! r1 = anyRect
        let! r2 = Gen.elements [ Rect.FromBoundingBox(r1.Left - 1.0, r1.Bottom - 1.0, r1.Right + 1.0, r1.Top)
                                 Rect.FromBoundingBox(r1.Left, r1.Bottom - 1.0, r1.Right + 1.0, r1.Top + 1.0)
                                 Rect.FromBoundingBox(r1.Left - 1.0, r1.Bottom, r1.Right + 1.0, r1.Top + 1.0)
                                 Rect.FromBoundingBox(r1.Left - 1.0, r1.Bottom - 1.0, r1.Right, r1.Top + 1.0) ]
        return (r1, r2)
    }

let rectPairExpandedOneDimContractedTheOther =
    gen {
        let! baseRect = anyRect
        let r1 = Rect.FromBoundingBox(baseRect.Left - 1.0, baseRect.Bottom, baseRect.Right + 1.0, baseRect.Top)
        let r2 = Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom - 1.0, baseRect.Right, baseRect.Top + 1.0)
        return (r1, r2)
    }

let rectPairContractedOneDirExpandedOtherDim =
    gen {
        let! baseRect = anyRect
        let expandedUp =
            let r1 = Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom, baseRect.Right, baseRect.Top + 1.0)
            let r2 = Rect.FromBoundingBox(baseRect.Left - 1.0, baseRect.Bottom, baseRect.Right + 1.0, baseRect.Top)
            (r1, r2)
        let expandedDown =
            let r1 = Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom - 1.0, baseRect.Right, baseRect.Top)
            let r2 = Rect.FromBoundingBox(baseRect.Left - 1.0, baseRect.Bottom, baseRect.Right + 1.0, baseRect.Top)
            (r1, r2)
        let expandedLeft =
            let r1 = Rect.FromBoundingBox(baseRect.Left - 1.0, baseRect.Bottom, baseRect.Right, baseRect.Top)
            let r2 = Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom - 1.0, baseRect.Right, baseRect.Top + 1.0)
            (r1, r2)
        let expandedRight =
            let r1 = Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom, baseRect.Right + 1.0, baseRect.Top)
            let r2 = Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom - 1.0, baseRect.Right, baseRect.Top + 1.0)
            (r1, r2)
        return! Gen.elements [expandedUp; expandedDown; expandedLeft; expandedRight]
    }

let rec rectPairTransposedDiagonallySlightly =
    gen {
        let! r1 = anyRect
        if r1.Width <= 2.0 || r1.Height <= 2.0 then return! rectPairTransposedDiagonallySlightly else
        let! deltaX = Gen.elements [1.0; -1.0]
        let! deltaY = Gen.elements [1.0; -1.0]
        let r2 = Rect.TranslationOf(r1, deltaX, deltaY)
        return (r1, r2)
    }

let rectPairAdjacentCopies =
    gen {
        let! r1 = anyRect
        let! deltaX, deltaY = Gen.elements [ -r1.Width, 0.0
                                             0.0, -r1.Height
                                             r1.Width, 0.0
                                             0.0, r1.Height ]
        return (r1, Rect.TranslationOf(r1, deltaX, deltaY))
    }

let rec rectPairTranslated =
    gen {
        let! r1 = anyRect
        let! deltaX = Gen.choose (0, int r1.Width)
        let! deltaY = Gen.choose (0, int r1.Height)
        if deltaX = 0 && deltaY = 0 then return! rectPairTranslated else
        return (r1, Rect.TranslationOf(r1, float deltaX, float deltaY))
    }

let rec rectPairAdjacentCopiesWithOrthogonalOffset =
    let deltas (width: float) (height: float) =
        let rec moveH =
            gen {
                let! h = Gen.elements [-width; width]
                let! v = Gen.choose (int -height + 1, int height - 1)
                if v = 0 then return! moveH else
                return (h, float v, height - abs (float v))
            }
        let rec moveV =
            gen {
                let! v = Gen.elements [-height; height]
                let! h = Gen.choose (int -width + 1, int width - 1)
                if h = 0 then return! moveV else
                return (float h, v, width - abs (float h))
            }
        Gen.oneof [ moveH; moveV ]
    gen {
        let! r1 = anyRect
        if r1.Width = 1.0 || r1.Height = 1.0 then return! rectPairAdjacentCopiesWithOrthogonalOffset else
        let! deltaX, deltaY, commonSideLength = deltas r1.Width r1.Height
        return (r1, Rect.TranslationOf(r1, deltaX, deltaY), commonSideLength)
    }

let rectPairExactlyTwoCornersOfOneInOther =
    gen {
        let! baseRect = anyRect
        let r1 = Rect.FromBoundingBox(baseRect.Left - 1.0, baseRect.Bottom - 1.0, baseRect.Right + 1.0, baseRect.Top + 1.0)
        let! r2 = Gen.elements [ Rect.FromBoundingBox(baseRect.Left - 2.0, baseRect.Bottom, baseRect.Right, baseRect.Top)
                                 Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom - 2.0, baseRect.Right, baseRect.Top)
                                 Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom, baseRect.Right + 2.0, baseRect.Top)
                                 Rect.FromBoundingBox(baseRect.Left, baseRect.Bottom, baseRect.Right, baseRect.Top + 2.0) ]
        return (r1, r2)
    }

let rec rectPairSlidAndExpandedOneOrthogonalDir =
    gen {
        let! r1 = anyRect
        if r1.Width = 1.0 || r1.Height = 1.0 then return! rectPairSlidAndExpandedOneOrthogonalDir else
        let slideX =
            gen {
                let! deltaX = Gen.elements [-1.0; 1.0]
                let! top, bottom = Gen.elements [r1.Top + 1.0, r1.Bottom; r1.Top, r1.Bottom - 1.0]
                return Rect.FromBoundingBox(r1.Left + deltaX, bottom, r1.Right + deltaX, top)
            }
        let slideY =
            gen {
                let! deltaY = Gen.elements [-1.0; 1.0]
                let! left, right = Gen.elements [r1.Left - 1.0, r1.Right; r1.Left, r1.Right + 1.0]
                return Rect.FromBoundingBox(left, r1.Bottom + deltaY, right, r1.Top + deltaY)
            }
        let! r2 = Gen.oneof [slideX; slideY]
        return (r1, r2)
    }