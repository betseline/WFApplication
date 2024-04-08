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
    public class WF_COMP_TEST_APPL_DETL_BS_BLZ : ScreenService {

        public string GetNewSrlNo(string compCode, string docType, string deptCode, string runNo) {
            string sql = "SELECT ISNULL(MAX(SRL_NO),0) + 10 FROM %<:DBOWNER>SV_TEST_APPL_DETL WHERE COMP_CODE = %s AND DOC_TYPE = %s AND DEPT_CODE = %s AND RUN_NO = %s";
            object result = DB.GetAValue(sql, compCode, docType, deptCode, runNo);
            if(result != null) {
                return result.ToString();
            }
            return "10";
        }
    }
}
