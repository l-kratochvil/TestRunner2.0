using CookComputing.XmlRpc;

using TestRunner.App.TestLinkApi.Types;

namespace TestRunner.App.TestLinkApi
{
    using System;

    public static class XmlRpcStructConvertors
    {
        internal static TestLinkErrorMessage ToTestLinkErrorMessage(XmlRpcStruct data)
        {
            var item = new TestLinkErrorMessage();
            item.code = ToInt(data, "code");
            item.message = (string)data["message"];
            return item;
        }

        internal static GeneralResult ToGeneralResult(XmlRpcStruct data)
        {
            var item = new GeneralResult();
            item.operation = (string)data["operation"];
            item.status = (bool)data["status"];
            item.id = ToInt(data, "id");
            item.message = (string)data["message"];
            if (data.ContainsKey("additionalInfo") &&
                data["additionalInfo"] is XmlRpcStruct)
                item.additionalInfo = ToAdditionalInfo(data["additionalInfo"] as XmlRpcStruct);
            else
                item.additionalInfo = null;

            return item;
        }


        /// <summary>
        ///  constructor used by XMLRPC interface on decoding the function return
        /// </summary>
        /// <param name="data">data returned by Testlink</param>
        internal static AttachmentRequestResponse ToAttachmentRequestResponse(XmlRpcStruct data)
        {
            var item = new AttachmentRequestResponse();
            item.foreignKeyId = ToInt(data, "fk_id");
            item.linkedTableName = (string)data["fk_table"];
            item.title = (string)data["title"];
            item.description = (string)data["description"];
            item.file_name = (string)data["file_name"];
            item.file_type = (string)data["file_type"];
            item.size = ToInt(data, "file_size");

            return item;
        }

        /// <summary>
        ///  constructor used by XMLRPC interface on decoding the function return
        /// </summary>
        /// <param name="data">data returned by Testlink</param>
        internal static AdditionalInfo ToAdditionalInfo(XmlRpcStruct data)
        {
            var item = new AdditionalInfo();
            item.new_name = (string)data["new_name"];
            item.status_ok = ToInt(data, "status_ok") == 1;
            item.msg = (string)data["msg"];
            item.id = ToInt(data, "id");
            item.external_id = ToInt(data, "external_id");
            item.version_number = ToInt(data, "version_number");
            item.has_duplicate = ToBool(data, "has_duplicate");

            return item;
        }

        internal static Build ToBuild(XmlRpcStruct data)
        {
            var item = new Build();
            item.id = ToInt(data, "id");
            item.active = ToInt(data, "active") == 1;
            item.name = (string)data["name"];
            item.notes = (string)data["notes"];
            item.testplan_id = ToInt(data, "testplan_id");
            item.is_open = ToInt(data, "is_open") == 1;

            return item;
        }


        internal static TestCaseFromTestSuite ToTestCaseFromTestSuite(XmlRpcStruct data)
        {
            var item = new TestCaseFromTestSuite();
            item.active = int.Parse((string)data["active"]) == 1;
            item.id = ToInt(data, "id");
            item.name = (string)data["name"];
            item.version = ToInt(data, "version");
            item.tcversion_id = ToInt(data, "tcversion_id");
            //steps = (string)data["steps"];
            //expected_results = (string)data["expected_results"];
            item.external_id = (string)data["tc_external_id"];
            item.testSuite_id = ToInt(data, "parent_id");
            item.is_open = int.Parse((string)data["is_open"]) == 1;
            item.modification_ts = ToDate(data, "modification_ts");
            item.updater_id = ToInt(data, "updater_id");
            item.execution_type = ToInt(data, "execution_type");
            item.summary = (string)data["summary"];
            if (data.ContainsKey("details"))
                item.details = (string)data["details"];
            else
                item.details = string.Empty;
            item.author_id = ToInt(data, "author_id");
            item.creation_ts = ToDate(data, "creation_ts");
            item.importance = ToInt(data, "importance");
            item.parent_id = ToInt(data, "parent_id");
            item.node_type_id = ToInt(data, "node_type_id");
            item.node_order = ToInt(data, "node_order");
            item.node_table = (string)data["node_table"];
            item.layout = (string)data["layout"];
            item.status = ToInt(data, "status");
            item.preconditions = (string)data["preconditions"];

            return item;
        }


        /// <summary>
        ///  constructor used by the XML Rpc return
        /// </summary>
        /// <param name="data"></param>
        internal static TestStep ToTestStep(XmlRpcStruct data)
        {
            var item = new TestStep();
            item.id = ToInt(data, "id");
            item.step_number = ToInt(data, "step_number");
            item.actions = (string)data["actions"];
            item.expected_results = (string)data["expected_results"];
            item.active = ToInt(data, "active") == 1;
            item.execution_type = ToInt(data, "execution_type");

            return item;
        }

        /// <summary>
        ///  constructor used by XMLRPC interface on decoding the function return
        /// </summary>
        /// <param name="data">data returned by Testlink</param>
        internal static TestSuite ToTestSuite(XmlRpcStruct data)
        {
            var item = new TestSuite();
            item._name = (string)data["name"];
            item._id = ToInt(data, "id");
            item._details = (string)data["details"];
            item._parentId = ToInt(data, "parent_id");
            item._nodeTypeId = ToInt(data, "node_type_id");
            item._nodeOrder = ToInt(data, "node_order");

            return item;
        }


        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        internal static TestPlatform ToTestPlatform(XmlRpcStruct data)
        {
            var item = new TestPlatform();
            item.id = ToInt(data, "id");
            item.name = (string)data["name"];
            item.notes = (string)data["notes"];

            return item;
        }


        static int ToInt(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name))
            {
                var val = data[name];
                switch (val)
                {
                    case string s:
                        if (int.TryParse(s, out var n)) return n;
                        break;
                    case int _:
                        return (int)val;
                }
            }

            return 0;
        }

        static bool? ToBool(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name))
            {
                var val = data[name];
                if (val is string)
                {
                    bool.TryParse(val as string, out var result);
                    return result;
                }

                return data[name] as bool?;
            }

            return null;
        }


        static DateTime ToDate(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name) && DateTime.TryParse((string)data[name], out var n)) return n;
            return DateTime.MinValue;
        }

        static char ToChar(XmlRpcStruct data, string name)
        {
            if (data.ContainsKey(name) && data[name] is string)
            {
                var s = (string)data[name];
                return s[0];
            }

            return '\x00';
        }
    }
}