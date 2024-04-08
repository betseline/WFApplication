using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MecWise.Blazor.Common;
using MecWise.Blazor.Components;
using MecWise.Blazor.Workflow;
using Newtonsoft.Json.Linq;

namespace MecWise.HR.TestingWFApplication.Client {
    public class WF_COMP_TEST_APPL_BS_BLZ : Screen {

        const string _defServerSideAssemblyName = "MecWise.HR.TestingWFApplication.Server";
        const string _defServerSideClassName = "WF_COMP_TEST_APPL_BS_BLZ";
        FieldWorkflow workflow;

        #region "Screen Events"
        protected override async Task<object> LoadAsync() {
            //Load Function. called when the screen is loaded / displayed
            Console.WriteLine("WF_COMP_TEST_APPL_BS_BLZ - LoadAsync");
            return await base.LoadAsync();
        }
        protected override async Task<object> ShowRecAsync() {
            Console.WriteLine("WF_COMP_TEST_APPL_BS_BLZ - ShowRecAsync");

            string recip_role_id = GetParentFieldValue<String>("RECIP_ROLE_ID");
            SetFieldValue("RECIP_ROLE_ID", recip_role_id);
            SetFieldValue("EMPE_NAME", await GetEmpeDescAsync());
            SetFieldValue("APP_ID", "SDA_TNR");

            if (ScrnId == "TEST_APPL_BS_BLZ") {
                if (ScrnMode == ScreenMode.Add) {
                    await InitFieldsAsync();
                }
            }

            SetCirculateStsDescAsync();
            if(ScrnMode != ScreenMode.Add) {
                string sWfId = await GetWFIDWithAppLinkCompCode();
                SetFieldValue("WF_ID", sWfId);
            }

            if (!string.IsNullOrEmpty(GetFieldValue<string>("WF_ID"))) {
                if (ScrnMode != ScreenMode.Enquiry) {
                    await ChangeModeAsync(ScreenMode.Enquiry);
                }
            }

            if (GetFieldValue<string>("RECIP_ROLE_ID") == "R1") {
                workflow = new FieldWorkflow("WF_CONTAINER", "Workflow Component", ScrnType.applicantScrn, "WF_APPL_ROUT_LIST_B", "WF_APPL_ROUT_STS_B", true);//applicant screen 
                workflow.ParentScreen = this;
                workflow.OnBeforeSubmit += Workflow_onBeforeSubmit;
                workflow.OnAfterSubmit += Workflow_onAfterSubmit;
                workflow.onBeforeWithdraw += Workflow_onBeforeWithdraw;
                workflow.onAfterWithdraw += Workflow_onAfterWithdraw;
                workflow.onBeforeResubmit += Workflow_onBeforeResubmit;
                workflow.onAfterResubmit += Workflow_onAfterResubmit;
                workflow.onCancel += Workflow_onCancel;
                workflow.onBeforeRedirect += Workflow_onBeforeRedirect;
                workflow.onAfterRedirect += Workflow_onAfterRedirect;

                if (string.IsNullOrEmpty(GetFieldValue<string>("RUN_NO"))) {
                    workflow.Routing_List_Grid.BRWS_ID = "WF_MF_APPL_OWN_ROUTE_LIST_B";
                }
                else {
                    workflow.Routing_List_Grid.BRWS_ID = "WF_APPL_ROUT_LIST_B";
                }

                FieldContainer fieldContainer = Fields.Search<FieldContainer>("C_WF_CONTAINER");
                workflow.ParentContainer = fieldContainer;
                fieldContainer.Fields.Add(workflow);

            }
            else {
                SetTitleBarMenuVisible(FieldTitleBarMenuItemType.New, false);
                SetTitleBarMenuVisible(FieldTitleBarMenuItemType.Save, false);
                SetTitleBarMenuVisible(FieldTitleBarMenuItemType.Delete, false);

                workflow = new FieldWorkflow("WF_CONTAINER", "Workflow Component", ScrnType.approvalScrn, "WF_APPL_ROUT_LIST_B", "WF_APPL_ROUT_STS_B", true);//approver screen 
                workflow.ParentScreen = this;
                workflow.onBeforeApprove += Workflow_onBeforeApprove;
                workflow.onAfterApprove += Workflow_onAfterApprove;
                workflow.onBeforeReject += Workflow_onBeforeReject;
                workflow.onAfterReject += Workflow_onAfterReject;
                workflow.onBeforeClarify += Workflow_onBeforeClarify;
                workflow.onAfterClarify += Workflow_onAfterClarify;
                workflow.onBeforeClarifySelect += Workflow_onBeforeClarifySelect;
                workflow.onAfterClarifySelect += Workflow_onAfterClarifySelect;
                workflow.onBeforeClarifyAll += Workflow_onBeforeClarifyAll;
                workflow.onAfterClarifyAll += Workflow_onAfterClarifyAll;
                workflow.onBeforeRedirect += Workflow_onBeforeRedirect;
                workflow.onAfterRedirect += Workflow_onAfterRedirect;

                FieldContainer fieldContainer = Fields.Search<FieldContainer>("C_WF_CONTAINER");
                workflow.ParentContainer = fieldContainer;
                fieldContainer.Fields.Add(workflow);
            }

            return await Task.FromResult<object>(true);
        }
        private async Task<object> InitFieldsAsync() {
            //Do Initialization here
            Console.WriteLine("WF_COMP_TEST_APPL_BS_BLZ - InitFieldsAsync");
            SetFieldValue("COMP_CODE", Session.CompCode);
            SetFieldValue("RUN_NO", "");
            SetFieldValue("EMPE_ID", Session.EmpeID);
            SetFieldValue("TRNS_DATE", DateTime.Today);
            SetFieldValue("CIRCULATE_STS", "0");
            SetFieldValue("CIRCULATE_DATE", DateTime.Today);

            generateAppRouting();

            return await Task.FromResult<object>(true);
        }
        protected override async Task<object> SavedRecAsync() {
            Console.WriteLine("WF_COMP_TEST_APPL_BS_BLZ - SavedRecAsync");
            if (ScrnMode == ScreenMode.Add) {
                await InsertRoutingList(); // to generate Routing list when save is saved
            }

            return await Task.FromResult<object>(true);
        }
        protected override async Task<bool> ValidEntryAsync() {
            //If Valid entry returns false then Save will not happen
            Console.WriteLine("WF_COMP_TEST_APPL_BS_BLZ - ValidEntryAsync");
            try {
                SetFieldValue("CIRCULATE_STS", "1");
                if (ScrnMode == ScreenMode.Add) {
                    string runNo = GetFieldValue<string>("RUN_NO");
                    if (string.IsNullOrEmpty(runNo)) {
                        runNo = await GetNewRunNoAsync();
                        SetFieldValue("RUN_NO", runNo);
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
            Console.WriteLine("WF_COMP_TEST_APPL_BS_BLZ - AfterRenderAsync");
            return await base.AfterRenderAsync();
        }
        #endregion

        #region "Custom Functions"
        public async Task<string> GetNewRunNoAsync() {
            string result = Convert.ToString(await Session.ExecServerFuncAsync(_defServerSideAssemblyName, _defServerSideClassName, "GetNewRunNo", Session.CompCode, GetFieldValue<string>("DOC_TYPE"), GetFieldValue<string>("DEPT_CODE")));
            return result;
        }

        public async Task<string> GetEmpeDescAsync() {
            string result = Convert.ToString(await Session.ExecServerFuncAsync(_defServerSideAssemblyName, _defServerSideClassName, 
                "GetEmpeName", Session.CompCode, GetFieldValue<string>("EMPE_ID")));
            return result;
        }

        public async void SetCirculateStsDescAsync() {
            string result = Convert.ToString(await Session.ExecServerFuncAsync(_defServerSideAssemblyName, _defServerSideClassName, "GetCirculateStsDesc", Session.CompCode, GetFieldValue<string>("CIRCULATE_STS")));
            if (result != null) {
                SetFieldValue("CIRCULATE_STS_DESC", result);
            }
        }

        public async Task<bool> SaveRec(string cmd) {
            return Convert.ToBoolean(await Session.ExecServerFuncAsync(_defServerSideAssemblyName, _defServerSideClassName, "SaveRec", DataSource.ToString(), cmd));
        }
        #endregion

        #region "Workflow Functions"
        public string GetFormText() {
            //var formtext = "This is form text.";
            var formtext = "Employee ID : " + GetFieldValue<string>("EMPE_ID") + "</br>";
            formtext = formtext + " Employee Name : " + GetFieldValue<string>("EMPE_NAME") + "</br>";
            formtext = formtext + " Claim Amount: " + GetFieldValue<string>("CLM_AMT") + "</br>";
            formtext = formtext + " Remark: " + GetFieldValue<string>("REMK") + "</br>";
            return formtext;
        }

        public async Task<string> GetWFIDWithAppLinkCompCode() {
            WorkflowClient wfClient = new WorkflowClient(Session, GetFieldValue<string>("APP_ID"), GetFieldValue<string>("DOC_TYPE"), GetFieldValue<string>("DEPT_CODE"));
            string appLink = GetFieldValue<string>("COMP_CODE") + ":" + GetFieldValue<string>("DOC_TYPE") + ":" + GetFieldValue<string>("DEPT_CODE") + ":" + GetFieldValue<string>("RUN_NO");
            return Convert.ToString(await wfClient.GetWFIDWithAppLinkCompCode(appLink));
        }

        private async Task Workflow_onBeforeSubmit(Object sender, FieldWorkflowActionEventArgs e) {
            try {
                Console.WriteLine("in Workflow_onBeforeSubmit Begin : " + DateTime.Now);
                SetFieldDisable(workflow.btn_SUBMIT.ID, true);
                bool isSubmitOk = true;
                isSubmitOk = Convert.ToBoolean(await ValidEntryAsync());
                if (isSubmitOk) {
                    SetFieldValue("CIRCULATE_STS", "2");
                    if (ScrnMode == ScreenMode.Add) {
                        isSubmitOk = await SaveRec("I");
                    }
                    else if (ScrnMode == ScreenMode.Update) {
                        isSubmitOk = await SaveRec("U");
                    }
                    if (isSubmitOk) {
                        isSubmitOk = await InsertRoutingList();
                        if (isSubmitOk) {
                            var formtext = GetFormText();
                            SetFieldValue("FORM_TEXT", formtext);
                        }
                    }
                }
                else {
                    SetFieldDisable(workflow.btn_SUBMIT.ID, false);
                    Session.ToastMessage(await cmMsg(this, "NOT_SUCC_SUBMIT_EMPL", "0", "You have not submitted successfully."), ToastMessageType.error);
                    e.Cancel = true;
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                e.Cancel = true;
            }
            Console.WriteLine("in Workflow_onBeforeSubmit End : " + DateTime.Now);
        }

        private async Task Workflow_onAfterSubmit(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterSubmit");
            bool isSubmitOk = true;
            if (isSubmitOk) {
                workflow.refreshWFComponent();
                Session.ToastMessage(await cmMsg(this, "SUCC_SUBMIT", "0", "Successfully submitted."), ToastMessageType.success);
            }
            else {
                Session.ToastMessage(await cmMsg(this, "NOT_SUCC_SUBMIT_EMPL", "0", "You have not submitted successfully."), ToastMessageType.error);
            }
        }

        private async Task Workflow_onBeforeApprove(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeApprove");

            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterApprove(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterApprove");
            bool savedRec = true;

            if (GetFieldValue<string>("RECIP_ROLE_ID") == "RA") {
                SetFieldValue("CIRCULATE_STS", "3");
                savedRec = await SaveRec("U");
            }
            if (savedRec) {
                workflow.disableComponent();
                Session.ToastMessage(await cmMsg(this, "TRVL_CLM_APRV_SUCC", "0", "The application has been approved"), ToastMessageType.success);
            }
        }

        private async Task Workflow_onBeforeReject(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeReject");
            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterReject(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterReject");
            bool savedRec = true;

            SetFieldValue("CIRCULATE_STS", "4");
            savedRec = await SaveRec("U");

            if (savedRec) {
                workflow.disableComponent();
                Session.ToastMessage(await cmMsg(this, "TRVL_CLM_NOT_APRV_SUCC", "0", "The application has not been approved"), ToastMessageType.success);
            }
        }

        private async Task Workflow_onBeforeClarify(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeClarify");
            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterClarify(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterClarify");
            workflow.disableComponent();
            Session.ToastMessage(await cmMsg(this, "SENT_CLR", "0", "Sent for Clarification"), ToastMessageType.success);
        }

        private async Task Workflow_onBeforeClarifySelect(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeClarifySelect");
            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterClarifySelect(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterClarifySelect");
            workflow.disableComponent();
            Session.ToastMessage(await cmMsg(this, "SENT_CLR", "0", "Sent for Clarification"), ToastMessageType.success);
        }

        private async Task Workflow_onBeforeClarifyAll(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeClarifyAll");
            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterClarifyAll(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterClarifyAll");
            workflow.disableComponent();
            Session.ToastMessage(await cmMsg(this, "SENT_CLR", "0", "Sent for Clarification"), ToastMessageType.success);
        }

        private async Task Workflow_onBeforeWithdraw(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeWithdraw");
            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterWithdraw(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterWithdraw");
            bool savedRec = true;

            SetFieldValue("CIRCULATE_STS", "6");
            savedRec = await SaveRec("U");
            if (savedRec) {
                workflow.disableComponent();
                Session.ToastMessage(await cmMsg(this, "TRVL_CLM_APRV_SUCC", "0", "The application has been approved"), ToastMessageType.success);
            }
        }

        private async Task Workflow_onBeforeResubmit(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeResubmit");
            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterResubmit(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterResubmit");
            workflow.disableComponent();
            Session.ToastMessage("Resubmitted Successfully", ToastMessageType.success);
        }

        private async Task Workflow_onCancel(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onCancel");
            // implement cancel process in here
        }

        private async Task Workflow_onBeforeRedirect(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onBeforeRedirect");
            //implement for application validataion, if validation fail, set e.Cancel = true;
            /*
                if(!bOK)
                    e.Cancel = true;
            */
        }

        private async Task Workflow_onAfterRedirect(Object sender, FieldWorkflowActionEventArgs e) {
            Console.WriteLine("in Workflow_onAfterRedirect");
            workflow.disableComponent();
            Session.ToastMessage("Redirected Successfully", ToastMessageType.success);
        }

        private async Task<bool> InsertRoutingList() {
            Console.WriteLine("WF Function - InsertRoutingList");
            string RunNo = GetFieldValue<string>("RUN_NO");
            string srlNo = "";
            string LineNo = "";
            string appMainFilter = "";
            string empeID = GetFieldValue<string>("EMPE_ID");
            string appTbl = "";
            string userlog = Session.UserID;
            WorkflowClient wfClient = new WorkflowClient(Session, GetFieldValue<string>("APP_ID"), GetFieldValue<string>("DOC_TYPE"),
                GetFieldValue<string>("DEPT_CODE"));
            return Convert.ToBoolean(await wfClient.InsertRoutingList(RunNo, srlNo, LineNo, appMainFilter, empeID, appTbl, userlog));
        }

        private async void generateAppRouting() {
            Console.WriteLine("WF Function - generateAppRouting");
            string RunNo = GetFieldValue<string>("RUN_NO");
            string srlNo = "";
            string LineNo = "";
            string appMainFilter = "";
            string empeID = GetFieldValue<string>("EMPE_ID");
            string appTbl = "";
            string userlog = Session.UserID;
            WorkflowClient wfClient = new WorkflowClient(Session, GetFieldValue<string>("APP_ID"), GetFieldValue<string>("DOC_TYPE"),
                GetFieldValue<string>("DEPT_CODE"));
            await wfClient.generateAppRouting(RunNo, srlNo, LineNo, appMainFilter, empeID, appTbl, userlog);
        }
        #endregion

        #region "Helper Functions"
        public async Task<string> cmMsg(Screen scrn, string MSG_ID, string langID, string DefaultMsg) {
            return Convert.ToString(await scrn.Session.ExecServerFuncAsync(_defServerSideAssemblyName, _defServerSideClassName, "cmMsg", MSG_ID, langID, DefaultMsg));
        }
        #endregion
    }
}
