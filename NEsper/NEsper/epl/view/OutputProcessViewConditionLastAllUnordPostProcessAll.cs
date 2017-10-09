///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.view
{
	public class OutputProcessViewConditionLastAllUnordPostProcessAll : OutputProcessViewConditionLastAllUnord
	{
	    private readonly OutputStrategyPostProcess _postProcessor;

	    public OutputProcessViewConditionLastAllUnordPostProcessAll(
	        ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
	        ResultSetProcessor resultSetProcessor,
	        long? afterConditionTime,
	        int? afterConditionNumberOfEvents,
	        bool afterConditionSatisfied,
	        OutputProcessViewConditionFactory parent,
	        AgentInstanceContext agentInstanceContext,
	        OutputStrategyPostProcess postProcessor)
            : base(resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied, parent, agentInstanceContext)
        {
	        _postProcessor = postProcessor;
	    }

	    protected override void Output(bool forceUpdate, UniformPair<EventBean[]> results)
	    {
	        // Child view can be null in replay from named window
	        if (ChildView != null)
	        {
	            _postProcessor.Output(forceUpdate, results, ChildView);
	        }
	    }
	}
} // end of namespace
