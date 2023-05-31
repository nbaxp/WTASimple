using WTA.Infrastructure.Attributes;

namespace WTA.Infrastructure.Data;

public enum OperatorType
{
    [Expression("{0} = @0")]
    Equal,

    [Expression("{0} != @0")]
    NotEqual,

    [Expression("{0} > @0")]
    GreaterThan,

    [Expression("{0} >= @0")]
    GreaterThanOrEqual,

    [Expression("{0} < @0")]
    LessThan,

    [Expression("{0} <= @0")]
    LessThanOrEqual,

    [Expression("{0}.Contains(@0)")]
    Contains,

    [Expression("{0}.StartsWith(@0)")]
    StartsWith,

    [Expression("{0}.EndsWith(@0)")]
    EndsWith,

    //[Expression("{0}")]
    //OrderBy,

    //[Expression("{0} desc")]
    //OrderByDesc

    [Expression("{0}.EndsWith(@0)")]
    Ignore,
}