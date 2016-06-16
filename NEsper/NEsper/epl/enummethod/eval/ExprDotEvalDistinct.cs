///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalDistinct : ExprDotEvalEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventAdapterService eventAdapterService)
        {
            return ExprDotNodeUtility.GetSingleLambdaParamEventType(
                enumMethodUsedName, goesToNames, inputEventType, collectionComponentType,
                eventAdapterService);
        }
    
        public override EnumEval GetEnumEval(MethodResolutionService methodResolutionService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, int statementId, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            if (bodiesAndParameters.IsEmpty()) {
                base.TypeInfo = EPTypeHelper.CollectionOfSingleValue(collectionComponentType);
                return new EnumEvalDistinctScalar(numStreamsIncoming);
            }
    
            ExprDotEvalParamLambda first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            if (inputEventType == null) {
                base.TypeInfo = EPTypeHelper.CollectionOfSingleValue(collectionComponentType);
                return new EnumEvalDistinctScalarLambda(first.BodyEvaluator, first.StreamCountIncoming,
                        (ObjectArrayEventType) first.GoesToTypes[0]);
            }
            base.TypeInfo = EPTypeHelper.CollectionOfEvents(inputEventType);
            return new EnumEvalDistinctEvents(first.BodyEvaluator, first.StreamCountIncoming);
        }
    }
}
