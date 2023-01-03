// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Conditions which can be used to compare property values.
/// </summary>
public enum ConditionOperator
{
    Eq,
    Ge,
    Gt,
    Le,
    Lt,
    Ne,
    Between
}

/// <summary>
/// Extensions for <c>ConditionOperator</c>.
/// </summary>
public static class ConditionOperatorEx
{
    /// <summary>
    /// Returns the string representation of a ConditionOperator.
    /// </summary>
    public static string ConditionOperatorString(this ConditionOperator op)
    {
        return op switch
        {
            ConditionOperator.Eq => "==",
            ConditionOperator.Ge => ">=",
            ConditionOperator.Gt => ">",
            ConditionOperator.Le => "<=",
            ConditionOperator.Lt => "<",
            ConditionOperator.Ne => "!=",
            ConditionOperator.Between => "~~",
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }

    public static Func<IComparable, IComparable, IComparable?, bool> CompareFunction(this ConditionOperator op)
    {
        return op switch
        {
            ConditionOperator.Eq => (c1, c2, _) => c1.Equals(c2),
            ConditionOperator.Ge => (c1, c2, _) => c1.CompareTo(c2) >= 0,
            ConditionOperator.Gt => (c1, c2, _) => c1.CompareTo(c2) > 0,
            ConditionOperator.Le => (c1, c2, _) => c1.CompareTo(c2) <= 0,
            ConditionOperator.Lt => (c1, c2, _) => c1.CompareTo(c2) < 0,
            ConditionOperator.Ne => (c1, c2, _) => c1.CompareTo(c2) != 0,
            ConditionOperator.Between => (c1, c2, c3) => c1.CompareTo(c2) >= 0 && c1.CompareTo(c3) <= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}
