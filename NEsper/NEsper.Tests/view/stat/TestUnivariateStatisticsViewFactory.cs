///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.stat
{
    [TestFixture]
    public class TestUnivariateStatisticsViewFactory 
    {
        private UnivariateStatisticsViewFactory _factory;

        private readonly ViewFactoryContext _viewFactoryContext = new ViewFactoryContext(null, 1, null, null, false, -1, false);
    
        [SetUp]
        public void SetUp()
        {
            _factory = new UnivariateStatisticsViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {"Price"}, "Price");
    
            TryInvalidParameter(new Object[] {});
        }
    
        [Test]
        public void TestCanReuse()
        {
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] {"Price"}));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            EventType type = UnivariateStatisticsView.CreateEventType(SupportStatementContextFactory.MakeContext(), null, 1);
            UnivariateStatisticsViewFactory factoryOne = new UnivariateStatisticsViewFactory();
            factoryOne.EventType = type;
            factoryOne.FieldExpression = SupportExprNodeFactory.MakeIdentNodeMD("Volume");
            UnivariateStatisticsViewFactory factoryTwo = new UnivariateStatisticsViewFactory();
            factoryTwo.EventType =type;
            factoryTwo.FieldExpression = SupportExprNodeFactory.MakeIdentNodeMD("Price");
            Assert.IsFalse(_factory.CanReuse(new UnivariateStatisticsView(factoryOne, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext())));
            Assert.IsTrue(_factory.CanReuse(new UnivariateStatisticsView(factoryTwo, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext())));
        }
    
        [Test]
        public void TestAttaches()
        {
            // Should attach to anything as long as the fields exists
            EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));
    
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] {"Price"}));
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
            Assert.AreEqual(typeof(double?), _factory.EventType.GetPropertyType(ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE.GetName()));
    
            try
            {
                _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] {"Symbol"}));
                _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected;
            }
        }
    
        private void TryInvalidParameter(Object[] @params)
        {
            try
            {
                _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(@params));
                _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] @params, String fieldName)
        {
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(@params));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, null);
            UnivariateStatisticsView view = (UnivariateStatisticsView) _factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
            Assert.AreEqual(fieldName, view.FieldExpression.ToExpressionStringMinPrecedenceSafe());
        }
    }
}
