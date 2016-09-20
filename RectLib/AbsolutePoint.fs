namespace RectLib

type AbsolutePoint = {
    HorizontalPosition: float
    VerticalPosition: float
    }
    with
        static member op_Equality (rhs: AbsolutePoint, lhs: AbsolutePoint) = lhs = rhs