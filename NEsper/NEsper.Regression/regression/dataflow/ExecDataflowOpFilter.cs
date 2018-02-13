///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.dataflow;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowOpFilter : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportSourceOp).Namespace + ".*");
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionInvalid(epService);
            RunAssertionAllTypes(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
    
            // invalid: no filter
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 BeaconSource -> instream<SupportBean> {} Filter(instream) -> abc {}",
                    "Failed to instantiate data flow 'DF1': Failed validation for operator 'Filter': Required parameter 'filter' providing the filter expression is not provided");
    
            // invalid: too many output streams
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 BeaconSource -> instream<SupportBean> {} Filter(instream) -> abc,def,efg { filter : true }",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'Filter': Filter operator requires one or two output Stream(s) but produces 3 streams");
    
            // invalid: too few output streams
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 BeaconSource -> instream<SupportBean> {} Filter(instream) { filter : true }",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'Filter': Filter operator requires one or two output Stream(s) but produces 0 streams");
    
            // invalid filter expressions
            TryInvalidInstantiate(epService, "theString = 1",
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Filter': Failed to validate filter dataflow operator expression 'theString=1': Implicit conversion from datatype 'int?' to 'string' is not allowed");
    
            TryInvalidInstantiate(epService, "Prev(theString, 1) = 'abc'",
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Filter': Invalid filter dataflow operator expression 'Prev(theString,1)=\"abc\"': Aggregation, sub-select, previous or prior functions are not supported in this context");
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService) {
            DefaultSupportGraphEventUtil.AddTypeConfiguration(epService);
    
            RunAssertionAllTypes(epService, "MyXMLEvent", DefaultSupportGraphEventUtil.XMLEvents);
            RunAssertionAllTypes(epService, "MyOAEvent", DefaultSupportGraphEventUtil.OAEvents);
            RunAssertionAllTypes(epService, "MyMapEvent", DefaultSupportGraphEventUtil.MapEvents);
            RunAssertionAllTypes(epService, "MyEvent", DefaultSupportGraphEventUtil.POJOEvents);
    
            // test doc sample
            string epl = "create dataflow MyDataFlow\n" +
                    "  create schema SampleSchema(tagId string, locX double),\t// sample type\n" +
                    "  BeaconSource -> samplestream<SampleSchema> {}\n" +
                    "  \n" +
                    "  // Filter all events that have a tag id of '001'\n" +
                    "  Filter(samplestream) -> tags_001 {\n" +
                    "    filter : tagId = '001' \n" +
                    "  }\n" +
                    "  \n" +
                    "  // Filter all events that have a tag id of '001', putting all other tags into the second stream\n" +
                    "  Filter(samplestream) -> tags_001, tags_other {\n" +
                    "    filter : tagId = '001' \n" +
                    "  }";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
    
            // test two streams
            DefaultSupportCaptureOpStatic.Instances.Clear();
            string graph = "create dataflow MyFilter\n" +
                    "Emitter -> sb<SupportBean> {name : 'e1'}\n" +
                    "Filter(sb) -> out.ok, out.fail {filter: theString = 'x'}\n" +
                    "DefaultSupportCaptureOpStatic(out.ok) {}" +
                    "DefaultSupportCaptureOpStatic(out.fail) {}";
            epService.EPAdministrator.CreateEPL(graph);
    
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyFilter");
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
    
            captive.Emitters.Get("e1").Submit(new SupportBean("x", 10));
            captive.Emitters.Get("e1").Submit(new SupportBean("y", 11));
            Assert.AreEqual(10, ((SupportBean) DefaultSupportCaptureOpStatic.Instances[0].Current[0]).IntPrimitive);
            Assert.AreEqual(11, ((SupportBean) DefaultSupportCaptureOpStatic.Instances[1].Current[0]).IntPrimitive);
            DefaultSupportCaptureOpStatic.Instances.Clear();
        }
    
        private void TryInvalidInstantiate(EPServiceProvider epService, string filter, string message) {
            string graph = "create dataflow MySelect\n" +
                    "DefaultSupportSourceOp -> instream<SupportBean>{}\n" +
                    "Filter(instream as ME) -> outstream {filter: " + filter + "}\n" +
                    "DefaultSupportCaptureOp(outstream) {}";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(graph);
    
            try {
                epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect");
                Assert.Fail();
            } catch (EPDataFlowInstantiationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
    
            stmtGraph.Dispose();
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService, string typeName, Object[] events) {
            string graph = "create dataflow MySelect\n" +
                    "DefaultSupportSourceOp -> instream.with.dot<" + typeName + ">{}\n" +
                    "Filter(instream.with.dot) -> outstream.dot {filter: myString = 'two'}\n" +
                    "DefaultSupportCaptureOp(outstream.dot) {}";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(graph);
    
            var source = new DefaultSupportSourceOp(events);
            var capture = new DefaultSupportCaptureOp<>(2);
            var options = new EPDataFlowInstantiationOptions();
            options.DataFlowInstanceUserObject = "myuserobject";
            options.DataFlowInstanceId = "myinstanceid";
            options.OperatorProvider(new DefaultSupportGraphOpProvider(source, capture));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
            Assert.AreEqual("myuserobject", instance.UserObject);
            Assert.AreEqual("myinstanceid", instance.InstanceId);
    
            instance.Run();
    
            Object[] result = capture.GetAndReset()[0].ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreSame(events[1], result[0]);
    
            instance.Cancel();
    
            stmtGraph.Dispose();
        }
    }
} // end of namespace