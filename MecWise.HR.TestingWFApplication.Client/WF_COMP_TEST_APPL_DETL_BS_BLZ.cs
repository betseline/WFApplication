using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MecWise.Blazor.Common;
using MecWise.Blazor.Components;
using MecWise.Blazor.Workflow;
using Newtonsoft.Json.Linq;

namespace MecWise.HR.TestingWFApplication.Client {
    public class WF_COMP_TEST_APPL_DETL_BS_BLZ : Screen {
        const string _defServerSideAssemblyName = "MecWise.HR.TestingWFApplication.Server";
        const string _defServerSideClassName = "WF_COMP_TEST_APPL_DETL_BS_BLZ";

        #region "Screen Events"
        protected override async Task<object> LoadAsync() {
            //Load Function. called when the screen is loaded / displayed
            Console.WriteLine("WF_COMP_TEST_APPL_DETL_BS_BLZ - LoadAsync");
            return await base.LoadAsync();
        }
        protected override async Task<object> ShowRecAsync() {
            Console.WriteLine("WF_COMP_TEST_APPL_DETL_BS_BLZ - ShowRecAsync");
            if (ScrnMode == ScreenMode.Add) {
                await InitFieldsAsync();
            }
            return await Task.FromResult<object>(true);
        }
        private async Task<object> InitFieldsAsync() {
            //Do Initialization here
            Console.WriteLine("WF_COMP_TEST_APPL_DETL_BS_BLZ - InitFieldsAsync");
            SetFieldValue("SRL_NO", "");

            return await Task.FromResult<object>(true);
        }
        protected override async Task<object> SavedRecAsync() {
            Console.WriteLine("WF_COMP_TEST_APPL_DETL_BS_BLZ - SavedRecAsync");
            if (ScrnMode == ScreenMode.Add) {
                //await InsertRoutingList();
            }

            return await Task.FromResult<object>(true);
        }
        protected override async Task<bool> ValidEntryAsync() {
            //If Valid entry returns false then Save will not happen
            Console.WriteLine("WF_COMP_TEST_APPL_DETL_BS_BLZ - ValidEntryAsync");
            try {
                if (ScrnMode == ScreenMode.Add) {
                    string srlNo = GetFieldValue<string>("SRL_NO");
                    if (string.IsNullOrEmpty(srlNo)) {
                        srlNo = await GetNewSrlNoAsync();
                        SetFieldValue("SRL_NO", srlNo);
                    }
                }

                return await Task.FromResult<bool>(true);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToStr());
                return await Task.FromResult<bool>(false);
            }
        }
        protected override async Task<object> AfterRenderAsync() {
            Console.WriteLine("WF_COMP_TEST_APPL_DETL_BS_BLZ - AfterRenderAsync");
            return await base.AfterRenderAsync();
        }
        #endregion

        #region "Custom Functions"
        public async Task<string> GetNewSrlNoAsync() {
            string result = Convert.ToString(await Session.ExecServerFuncAsync(_defServerSideAssemblyName, _defServerSideClassName, "GetNewSrlNo", Session.CompCode, GetFieldValue<string>("DOC_TYPE"), GetFieldValue<string>("DEPT_CODE"), GetFieldValue<string>("RUN_NO")));
            return result;
        }
        #endregion
    }
}
