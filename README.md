#RectLib
A little library for simple operations on axis-aligned rectangles

###Requirements and building
The library itself only depends on FSharp.Core. It targets F# 4.0 and .NET 4.5.2. (It's written in F# but the API intention was to play nicely with any .NET language.) The property test suite requires NUnit and FsCheck, although everything you need should be set up correctly as a NuGet dependency, so just mashing your build button once you've got the solution open should work ok. (Of course, if you want to actually run the tests, you'll need to bring your own NUnit 3 runner.)

All of the example code in this document is found in the RectLib.CSharpExamples project. It doesn't "do" anything, but you can attach a debugger and follow along.

###Rectangles, segments, and points
The fundamental type in RectLib is the Rect, which represents an axis-aligned rectangle. Rects can be created several ways:

    var r1 = Rect.FromBoundingBox(left: 0, bottom: 0, right: 10, top: 7);
    var r2 = Rect.AtPosition(left: 4, bottom: -3, width: 5, height: 6);
    var r3 = Rect.TranslationOf(r1, horizontalDelta: 10, verticalDelta: 2);

Rects, like all RectLib types, are immutable and use structural equality.

Conceptually, Rects consist of two parts: the _shell_ and the _volume_. A Rect's shell is its sides and corners, and its volume is the area it defines. Volumes are **open**: the "boundary" of a volume does not count as a part of it. A Rect's volume only exists implicitly, but its shell can be directly inspected with the Rect.Corners and Rect.Sides properties. Rect.Corners is straightforward. Rect.Sides returns a sequence of OrientedInterval objects; each such object defines a line segment by defining whether it's vertical or horizontal, the line on which the segment lies, and the range on that line that the segment spans. It can be easily unpacked into absolute coordinates via the Start and End properties. Line segments are _always_ defined to start at the "lower" coordinate and end at the "upper" coordinate.

###Rectangle intersection
RectLib provides two separate intersection operations. The simpler is a simple boolean predicate that determines whether two Rect volumes intersect at all:

    var r1 = Rect.FromBoundingBox(0, 0, 2, 2);
    var r2 = Rect.FromBoundingBox(1, 0, 3, 2);
    var r3 = Rect.FromBoundingBox(2, 0, 4, 2);
    var r1r2 = Rect.VolumesIntersect(r1, r2); // true
    var r1r3 = Rect.VolumesIntersect(r1, r3); // false, volumes are "open" so adjacencies don't count

More powerfully, RectLib can calculate a shell intersection of two Rects. This intersection can operate in two modes: excluding the input corners (i.e. searching only for edge intersections) or including them. In both modes, the result set will include points at which edges cross, and segments common to two edges.

    var r4 = Rect.FromBoundingBox(0, 0, 2, 5);
    var r5 = Rect.FromBoundingBox(1, 1, 2, 8);
    AbsolutePoint[] intrPoints;
    OrientedInterval[] intrSegments;
    var intr = Rect.ShellIntersection(r4, r5, false, out intrPoints, out intrSegments); // returns whether any intersection was found
    // intrPoints contains the single point at which the top of r4 intersects with the left side of r5: (1, 5)
    // intrSegments contains the segment common to both Rects: (2,1) to (2,5)

In the latter mode, the result set will additionally include corners of one Rect that lie on edges or corners of the other.

    intr = Rect.ShellIntersection(r4, r5, true, out intrPoints, out intrSegments);
    // intrSegments is same as above
    // intrPoints now additionally contains (2,0) and (2,5)

###Volume containment
RectLib can determine whether one Rect volume fully contains another Rect volume. (**A** contains **B** if **B** - **A** is empty. This means that two identical Rects contain each other.)

    var r6 = Rect.AtPosition(0, 0, 2, 2);
    var r7 = Rect.AtPosition(0, 0, 4, 4);
    var r8 = Rect.AtPosition(1, 1, 2, 2);
    var r7r6 = r7.ContainsVolumeOf(r6); // true, r7 is bigger
    var r6r7 = r6.ContainsVolumeOf(r7); // false, r6 is smaller
    var r6r8 = r6.ContainsVolumeOf(r8); // false, intersections don't count

### Common side detection
RectLib can determine whether, given two Rects, any edge of either of them exists on an edge of the other.

    var r9 = Rect.AtPosition(0, 0, 4, 4);
    var r10 = Rect.AtPosition(4, 0, 4, 4);
    var r9r10 = Rect.HaveCommonSide(r9, r10); // true, r9's right side is r10's left side

The two sides don't need to be identical; one just needs to exist wholly on the other.

    var r11 = Rect.AtPosition(4, 1, 2, 2);
    var r9r11 = Rect.HaveCommonSide(r9, r11); // true, r9's right side wholly covers r11's left side

Partial intersections don't count. (Neither do corners.) If you need them, use Rect.ShellIntersection, which detects and calculates them.

    var r12 = Rect.AtPosition(4, 2, 4, 4);
    var r9r12 = Rect.HaveCommonSide(r9, r12); // false; the sides overlap, but neither wholly covers the other

Note that this check is done without respect to volume intersections. If you want to filter those, do a volume intersection check as well.

    var r13 = Rect.AtPosition(2, 1, 2, 2);
    var r9r13 = Rect.HaveCommonSide(r9, r13); // true, even though r13 is contained in r9

###Caveats

While the asymptotic performance of all of RectLib's operations should be reasonable, it hasn't been optimized for large data sets in any way. There's also no epsilon or tolerance in any of the internal predicates, so calling code is responsible for dealing with numerical imprecision issues. However, the fact that Rects are axis-aligned means that RectLib is entirely a predicate library, so it won't introduce any new instability to your data.