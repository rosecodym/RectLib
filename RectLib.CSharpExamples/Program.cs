using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectLib.CSharpExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var r1 = Rect.FromBoundingBox(0, 0, 2, 2);
            var r2 = Rect.FromBoundingBox(1, 0, 3, 2);
            var r3 = Rect.FromBoundingBox(2, 0, 4, 2);
            var r1r2 = Rect.VolumesIntersect(r1, r2); // true
            var r1r3 = Rect.VolumesIntersect(r1, r3); // false, volumes are "open" so adjacencies don't count

            var r4 = Rect.FromBoundingBox(0, 0, 2, 5);
            var r5 = Rect.FromBoundingBox(1, 1, 2, 8);
            AbsolutePoint[] intrPoints;
            OrientedInterval[] intrSegments;
            var intr = Rect.ShellIntersection(r4, r5, false, out intrPoints, out intrSegments); // returns whether any intersection was found
            // intrPoints contains the single point at which the top of r4 intersects with the left side of r5: (1, 5)
            // intrSegments contains the segment common to both Rects: (2,1) to (2,5)

            intr = Rect.ShellIntersection(r4, r5, true, out intrPoints, out intrSegments);
            // intrSegments is same as above
            // intrPoints now additionally contains (2,1) and (2,5)

            var r6 = Rect.AtPosition(0, 0, 2, 2);
            var r7 = Rect.AtPosition(0, 0, 4, 4);
            var r8 = Rect.AtPosition(1, 1, 2, 2);
            var r7r6 = r7.ContainsVolumeOf(r6); // true, r7 is bigger
            var r6r7 = r6.ContainsVolumeOf(r7); // false, r6 is smaller
            var r6r8 = r6.ContainsVolumeOf(r8); // false, intersections don't count

            var r9 = Rect.AtPosition(0, 0, 4, 4);
            var r10 = Rect.AtPosition(4, 0, 4, 4);
            var r9r10 = Rect.HaveCommonSide(r9, r10); // true, r9's right side is r10's left side

            var r11 = Rect.AtPosition(4, 1, 2, 2);
            var r9r11 = Rect.HaveCommonSide(r9, r11); // true, r9's right side wholly covers r11's left side

            var r12 = Rect.AtPosition(4, 2, 4, 4);
            var r9r12 = Rect.HaveCommonSide(r9, r12); // false; the sides overlap, but neither wholly covers the other

            var r13 = Rect.AtPosition(2, 1, 2, 2);
            var r9r13 = Rect.HaveCommonSide(r9, r13); // true, even though r13 is contained in r9

            ;
        }
    }
}
