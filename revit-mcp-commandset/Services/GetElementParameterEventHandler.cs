using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using RevitMCPSDK.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RevitMCPCommandSet.Services
{
    public class GetElementParametersEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
    {
        // 输入参数
        private JObject _parameters;

        // 输出结果
        public object Result { get; private set; }

        // 同步状态
        public bool TaskCompleted { get; private set; }
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public void SetParameters(JObject parameters)
        {
            _parameters = parameters;
        }

        public bool WaitForCompletion(int timeoutMilliseconds = 10000)
        {
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.Document;
                var results = new List<Dictionary<string, object>>();

                List<ElementId> ids = new List<ElementId>();

                if (_parameters != null && _parameters.TryGetValue("elementIds", out JToken idToken) && idToken is JArray idArray)
                {
                    foreach (var id in idArray)
                    {
                        if (int.TryParse(id.ToString(), out int intId))
                        {
                            ids.Add(new ElementId(intId));
                        }
                    }
                }

                if (ids.Count > 0)
                {
                    foreach (var id in ids)
                    {
                        Element element = doc.GetElement(id);
                        if (element != null)
                        {
                            var elementData = ExtractElementParameters(element);
                            results.Add(elementData);
                        }
                    }
                }
                else
                {
                    // Fallback: collect all elements (customize as needed)
                    var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                    foreach (var element in collector)
                    {
                        var elementData = ExtractElementParameters(element);
                        results.Add(elementData);
                    }
                }

                Result = results;
            }
            catch (Exception)
            {
                Result = "参数提取失败";
            }
            finally
            {
                TaskCompleted = true;
                _resetEvent.Set();
            }
        }

        public string GetName()
        {
            return "获取构件参数信息";
        }

        private Dictionary<string, object> ExtractElementParameters(Element element)
        {
            var data = new Dictionary<string, object>
            {
                { "ElementId", element.Id.IntegerValue },
                { "Name", element.Name },
                { "Category", element.Category?.Name }
            };

            foreach (Parameter param in element.Parameters)
            {
                try
                {
                    if (param.Definition != null && param.HasValue)
                    {
                        string paramName = param.Definition.Name;
                        string value = param.AsValueString() ?? param.AsString();
                        data[paramName] = value;
                    }
                }
                catch
                {
                    // Skip any bad parameters
                }
            }

            return data;
        }
    }
}
