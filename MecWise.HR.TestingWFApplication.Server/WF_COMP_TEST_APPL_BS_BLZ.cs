using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MecWise.Blazor.Api.DataAccess;
using MecWise.Blazor.Api.Services;
using MecWise.Blazor.Common;
using Newtonsoft.Json.Linq;

namespace MecWise.HR.TestingWFApplication.Server {
    public class WF_COMP_TEST_APPL_BS_BLZ : ScreenService {
        public string GetEmpeName(string compCode, string empeID) {
            string sql = "SELECT FAM_NAME FROM %<:DBOWNER>SV_EMPE_PROFILE WHERE COMP_CODE = %s and EMPE_ID = %s";
            object result = DB.GetAValue(sql, compCode, empeID);
            if(result != null) {
                return result.ToString();
            }
            return "";
        }

        public string GetCirculateStsDesc(string compCode, string stsCode) {
            object result = DB.GetAValue("SELECT TYPE_DESC FROM %<:DBOWNER>SV_MF_DEFA WHERE COMP_CODE = %s AND FIELD_NAME = %s AND TYPE_CODE = %s", compCode, "MF-CIRCULATE-STS", stsCode);
            if(result != null) {
                return result.ToString();
            }
            return "";
        }

        public string GetNewRunNo(string CompCode, string docType, string deptCode) {
            DBParameterCollection Params = new DBParameterCollection();
            Params.Add("RET_VAL", 0);
            Params.Add("CMD", "GN");
            Params.Add("COMP_CODE", CompCode);
            Params.Add("DOC_TYPE", docType);
            Params.Add("DEPT_CODE", deptCode);
            Params.Add("RUN_NO", "", System.Data.ParameterDirection.Output);
            DB.ExecProcedure("CP_RUN_NO", Params);

            string runNo = Params["RUN_NO"].Value.ToString();
            return runNo;
        }

        public string cmMsg(string MSG_ID, string LangID = "0", string MSG_DEF = "") {
            string sql = "SELECT MSG_0 FROM DBO.SV_CM_MSG_TBL WHERE MSG_ID=%s ";
            object result = DB.GetAValue(sql, MSG_ID);
            if (result != null) {
                return result.ToString();
            }
            else {
                return MSG_DEF;
            }
        }

        public bool SaveRec(string objDataSource, string cmd) {
            bool retval = true;
            JObject objData = JObject.Parse(objDataSource);

            DBParameterCollection Params = new DBParameterCollection();
            Params.Add("RET_VAL", 0);
            Params.Add("CMD", cmd);
            Params.Add("COMP_CODE", objData["COMP_CODE"]);
            Params.Add("DOC_TYPE", objData["DOC_TYPE"]);
            Params.Add("DEPT_CODE", objData["DEPT_CODE"]);
            Params.Add("RUN_NO", objData["RUN_NO"]);
            Params.Add("TRNS_DATE", objData["TRNS_DATE"]);
            Params.Add("EMPE_ID", objData["EMPE_ID"]);
            Params.Add("CLM_AMT", objData["CLM_AMT"]);
            Params.Add("CIRCULATE_STS", objData["CIRCULATE_STS"]);
            Params.Add("CIRCULATE_DATE", objData["CIRCULATE_DATE"]);
            Params.Add("REMK", objData["REMK"]);
            Params.Add("SUBMIT_ID", objData["SUBMIT_ID"]);
            Params.Add("DATA_ACES_ID", objData["DATA_ACES_ID"]);
            Params.Add("REF_DOC_TYPE", objData["REF_DOC_TYPE"]);
            Params.Add("REF_DEPT_CODE", objData["REF_DEPT_CODE"]);
            Params.Add("REF_RUN_NO", objData["REF_RUN_NO"]);
            Params.Add("RSV_CHAR_FIELD1", objData["RSRV_CHAR_FIELD1"]);
            Params.Add("RSV_CHAR_FIELD2", objData["RSRV_CHAR_FIELD2"]);
            Params.Add("RSV_CHAR_FIELD3", objData["RSRV_CHAR_FIELD3"]);
            Params.Add("RSV_CHAR_FIELD4", objData["RSRV_CHAR_FIELD4"]);
            Params.Add("RSV_CHAR_FIELD5", objData["RSRV_CHAR_FIELD5"]);
            Params.Add("RSRV_NUM_FIELD1", objData["RSRV_NUM_FIELD1"]);
            Params.Add("RSRV_NUM_FIELD2", objData["RSRV_NUM_FIELD2"]);
            Params.Add("RSRV_NUM_FIELD3", objData["RSRV_NUM_FIELD3"]);
            Params.Add("RSRV_NUM_FIELD4", objData["RSRV_NUM_FIELD4"]);
            Params.Add("RSRV_NUM_FIELD5", objData["RSRV_NUM_FIELD5"]);
            Params.Add("RSRV_DATE_FIELD1", objData["RSRV_DATE_FIELD1"]);
            Params.Add("RSRV_DATE_FIELD2", objData["RSRV_DATE_FIELD2"]);
            Params.Add("RSRV_DATE_FIELD3", objData["RSRV_DATE_FIELD3"]);
            Params.Add("RSRV_DATE_FIELD4", objData["RSRV_DATE_FIELD4"]);
            Params.Add("RSRV_DATE_FIELD5", objData["RSRV_DATE_FIELD5"]);

            DB.ExecProcedure("UP_TEST_APPL", Params);

            if (Params["RET_VAL"].Value.ToInt() != 0) {
                retval = false;
            }
            return retval;
        }
    }
}
