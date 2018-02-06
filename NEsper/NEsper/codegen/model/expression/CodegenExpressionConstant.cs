///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionConstant : ICodegenExpression
    {
        private readonly Object constant;

        public CodegenExpressionConstant(Object constant)
        {
            this.constant = constant;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            CodegenExpressionUtil.RenderConstant(builder, constant);
        }

        public void MergeClasses(ICollection<Type> classes)
        {
        }
    }
} // end of namespace