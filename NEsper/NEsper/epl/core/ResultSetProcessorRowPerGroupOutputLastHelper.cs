///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;

namespace com.espertech.esper.epl.core
{
	public interface ResultSetProcessorRowPerGroupOutputLastHelper
    {
	    void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic);
	    void ProcessJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic);
	    UniformPair<EventBean[]> OutputView(bool isSynthesize);
	    UniformPair<EventBean[]> OutputJoin(bool isSynthesize);
	    void Remove(object key);
	    void Destroy();
	}
} // end of namespace
