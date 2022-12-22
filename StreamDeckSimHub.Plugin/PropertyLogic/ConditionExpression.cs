// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.PropertyLogic;

public class ConditionExpression
{
    public ConditionExpression(string property, ConditionOperator op, string compareValue)
    {
        Property = property;
        Operator = op;
        CompareValue = compareValue;
    }

    public string Property { get; }
    public ConditionOperator Operator { get; }
    public string CompareValue { get; }
}
