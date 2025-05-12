using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Xml;
using System.IO;
using System.Linq;

using Covi.Framework.Data;
using CF = Covi.Framework;
using CAS = Covi.BizService.ApprovalService;
using Covi.WebService;
using CFMEM = Covi.Framework.Mail.EWSMail;

using Covi.BizService;

using System.Transactions;
using CoviEAccount;

using System.Net;//200602추가


/// <summary>
/// ApprovalWebService의 요약 설명입니다.
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// ASP.NET AJAX를 사용하여 스크립트에서 이 웹 서비스를 호출하려면 다음 줄의 주석 처리를 제거합니다. 
[System.Web.Script.Services.ScriptService]
//public class ApprovalWebService : System.Web.Services.WebService //Covi.WebService.WebServiceBase
public class ApprovalWebService : Covi.WebService.WebServiceBase
{

    //public System.String fmpf; //양식 prefixpra8

    public String fmid; //양식 id
    public String scid; //스키마 id
    public String fmrv; //양식 revision
    public String fmnm; //양식명
    public XmlDocument oApvList = new XmlDocument();
    public String fiid; //양식 instance id
    public String piid; //process instance id

    private bool ApprToRjct = false;

    public String strLangIndex = "0"; //다국어 처리용


    public const string Processing = "002"; // 진행중 => 기안 시 사용
    public const string Complete = "003";   // 완료   => 최종결재 완료 시
    public const string Reject = "004";     // 반송   => 반송 시
    public const string Rollback = "006";   // 회수   => 회수 시

    private const string strInfoMailSender = "kr-dmpark@no1.com";

    protected enum SSLFlag { Secure, InSecure, Ignore };
    protected SSLFlag _sSSLFlag = SSLFlag.InSecure; // 부분 SSL 적용

    public ApprovalWebService()
    {

        //디자인된 구성 요소를 사용하는 경우 다음 줄의 주석 처리를 제거합니다. 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld()
    {
        return "Hello World";
    }

    //결재문서 상신처리
    //parameter
    //key : 상태 update를 위한 erp key값
    //subject : 제목 (varchar - 200)
    //bodycontext : 본문
    //apvline : 결재선 A@사번;C@사번    결재구분@사번;결재구분@사번  결재구분 ( SA: 발신결재, SC: 발신병렬합의, RA:수신결재, RC:수신병렬합의)
    //empno : 사번
    //fmpf : 양식 key
    //etc : 기타 필드
    [WebMethod]
    public string WSDraftForm(string key, string subject, string bodycontext, string apvline, string entcode, string empno, string fmpf, string etc)
    {
        System.Boolean breturn = false;
        System.Data.DataSet oDS = new System.Data.DataSet();
        //System.Xml.XmlDocument oApvList = new System.Xml.XmlDocument();
        StringBuilder sb = new StringBuilder();

        DataPack INPUT = new DataPack();

        try
        {
            #region 파라미터 확인
            if (key == string.Empty)
            {
                throw new System.Exception("key 값을 입력하십시오.");
            }
            if (subject == string.Empty)
            {
                throw new System.Exception("제목을 입력하십시오.");
            }

            if (apvline == string.Empty)
            {
                throw new System.Exception("결재선을 입력하십시오.");
            }

            //회사코드가 없는 경우가 많음
            //if (entcode == string.Empty)
            //{
            //    throw new System.Exception("회사코드를 입력하십시오.");
            //}

            if (empno == string.Empty)
            {
                throw new System.Exception("기안자 사번을 입력하십시오.");
            }

            if (fmpf == string.Empty)
            {
                throw new System.Exception("양식 key 값을 입력하십시오.");

            }

            sb.Append("<BODY_CONTEXT><DOCLINKS></DOCLINKS>");
            sb.Append(makeNode("mSubject", subject, true));
            sb.Append(makeNode("mKEY", key));
            //본문을 그려오는 경우 
            if (bodycontext.ToLower().IndexOf("<table") > -1)
            {
                sb.Append(makeNode("tbContentElement", bodycontext, true));
            }
            else //XML데이터가 오는경우
            {
                if (bodycontext.ToUpper().IndexOf("<BODY_CONTEXT>") > -1)
                {
                    XmlDocument xmltmp = new XmlDocument();
                    xmltmp.LoadXml(bodycontext);
                    sb.Append(xmltmp.SelectSingleNode("BODY_CONTEXT").InnerXml);
                }
                else
                {
                    sb.Append(bodycontext);
                }
            }
            sb.Append("</BODY_CONTEXT>");
            #endregion

            System.String INITIATOR_ID = ""; //기안자 person code
            System.String INITIATOR_NAME = ""; //기안자 name
            System.String INITIATOR_OU_ID = ""; //기안자 unit code
            System.String INITIATOR_OU_NAME = ""; //기안자 unit name

            #region 양식값 변환
            fmpf = fmpf.ToUpper();

            //양식정보 획득
            //2015.09.15 양식 form prefix를 바로 받지 않고 FORM_LEGACY TABLE에 정의된 KEY값으로 입력받는다.
            string RtnFormID = "";
            DataSet ds = new DataSet();

            INPUT = new DataPack();
            INPUT.add("@LEGACY_FORMID", fmpf);

            using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wfform_getFormInfo_legacy", INPUT);
            }

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                fmid = ds.Tables[0].Rows[0]["FORM_ID"].ToString();
                scid = ds.Tables[0].Rows[0]["SCHEMA_ID"].ToString();
                fmrv = ds.Tables[0].Rows[0]["REVISION"].ToString();
                fmnm = ds.Tables[0].Rows[0]["FORM_NAME"].ToString();
            }
            INPUT.Clear();
            #endregion

            #region 사용자 정보 변환

            oApvList.LoadXml("<steps></steps>");

            System.Xml.XmlElement oSteps = oApvList.DocumentElement;
            System.Xml.XmlElement oDiv = oApvList.CreateElement("division");
            System.Xml.XmlElement oDivTaskinfo = oApvList.CreateElement("taskinfo");

            System.Xml.XmlElement oStep = oApvList.CreateElement("step");
            System.Xml.XmlElement oOU = oApvList.CreateElement("ou");
            System.Xml.XmlElement oPerson = oApvList.CreateElement("person");
            System.Xml.XmlElement oPersonTaskinfo = oApvList.CreateElement("taskinfo");

            //기안자 결재선 정보
            oSteps.AppendChild(oDiv).AppendChild(oDivTaskinfo);
            oDiv.AppendChild(oStep).AppendChild(oOU).AppendChild(oPerson).AppendChild(oPersonTaskinfo);

            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
            {
                INPUT.add("@PERSON_CODE", empno);
                oDS = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PersonInfo_R", INPUT);
                if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow oDR in oDS.Tables[0].Rows)
                    {
                        INITIATOR_ID = oDR["PERSON_CODE"].ToString();
                        INITIATOR_NAME = oDR["DISPLAY_NAME"].ToString();
                        INITIATOR_OU_ID = oDR["UNIT_CODE"].ToString();
                        INITIATOR_OU_NAME = oDR["UNIT_NAME"].ToString();

                        oSteps.SetAttribute("initiatorcode", INITIATOR_ID);
                        oSteps.SetAttribute("initiatoroucode", INITIATOR_OU_ID);
                        oSteps.SetAttribute("status", "inactive");

                        oDiv.SetAttribute("divisiontype", "send");
                        oDiv.SetAttribute("name", "발신");
                        oDiv.SetAttribute("oucode", INITIATOR_OU_ID);
                        oDiv.SetAttribute("ouname", INITIATOR_OU_NAME);

                        oDivTaskinfo.SetAttribute("status", "inactive");
                        oDivTaskinfo.SetAttribute("result", "inactive");
                        oDivTaskinfo.SetAttribute("kind", "send");
                        oDivTaskinfo.SetAttribute("datereceived", DateTime.Now.ToString());

                        oStep.SetAttribute("unittype", "person");
                        oStep.SetAttribute("routetype", "approve");
                        oStep.SetAttribute("name", "기안자");

                        oOU.SetAttribute("code", INITIATOR_OU_ID);
                        oOU.SetAttribute("name", INITIATOR_OU_NAME);

                        oPerson.SetAttribute("code", oDR["PERSON_CODE"].ToString());
                        oPerson.SetAttribute("name", oDR["DISPLAY_NAME"].ToString());
                        oPerson.SetAttribute("oucode", oDR["UNIT_CODE"].ToString());
                        oPerson.SetAttribute("ouname", oDR["UNIT_NAME"].ToString());
                        oPerson.SetAttribute("position", oDR["JOBPOSITION_Z"].ToString());//oDR["jobposition"].ToString().Split('&')[1] + ";" + oDR["jobposition"].ToString().Split('&')[0]);
                        oPerson.SetAttribute("title", oDR["JOBTITLE_Z"].ToString());//oDR["jobtitle"].ToString().Split('&')[1] + ";" + oDR["jobtitle"].ToString().Split('&')[0]);
                        oPerson.SetAttribute("level", oDR["JOBLEVEL_Z"].ToString());//oDR["joblevel"].ToString().Split('&')[1] + ";" + oDR["joblevel"].ToString().Split('&')[0]);

                        oPersonTaskinfo.SetAttribute("status", "inactive");
                        oPersonTaskinfo.SetAttribute("result", "inactive");
                        oPersonTaskinfo.SetAttribute("kind", "charge");
                        oPersonTaskinfo.SetAttribute("datereceived", DateTime.Now.ToString());

                    }
                }
                else
                {
                    throw new System.Exception("통합그룹웨어에 해당 사용자가 존재하지 않습니다.");
                }
            }

            #endregion

            #region 결재선변환

            System.Xml.XmlElement oRecDiv = oApvList.CreateElement("division");
            System.Xml.XmlElement oRecDivTaskInfo = oApvList.CreateElement("taskinfo");
            System.Xml.XmlElement oAssistStep = oApvList.CreateElement("step");
            System.Xml.XmlElement oAssistStepTaskInfo = oApvList.CreateElement("taskinfo");
            System.Xml.XmlElement oRecAssistStep = oApvList.CreateElement("step");
            System.Xml.XmlElement oRecAssistStepTaskInfo = oApvList.CreateElement("taskinfo");

            System.String[] aApprovers = apvline.Split(';');
            foreach (System.String ApproverInfo in aApprovers)
            {
                System.String[] Approver = ApproverInfo.Split('@');
                switch (Approver[0])
                {
                    case "SA"://발신결재
                        oDiv.AppendChild(makeApvLine(Approver[1], entcode, "normal"));
                        break;
                    case "SC"://발신병렬합의
                        if (Approver[0] == "SC" && oDiv.SelectSingleNode("step[@routetype='assist']") == null)
                        {
                            oDiv.AppendChild(oAssistStep).AppendChild(oAssistStepTaskInfo);

                            oAssistStep.SetAttribute("unittype", "person");
                            oAssistStep.SetAttribute("routetype", "approve");
                            oAssistStep.SetAttribute("allottype", "parallel");
                            oAssistStep.SetAttribute("name", "합의");

                            oAssistStepTaskInfo.SetAttribute("status", "inactive");
                            oAssistStepTaskInfo.SetAttribute("result", "inactive");
                            oAssistStepTaskInfo.SetAttribute("kind", "send");

                        }
                        oAssistStep.AppendChild(makeApvLine(Approver[1], entcode, "assist"));
                        break;
                    case "RA"://수신결재
                        //수신division여부 확인부터 시작
                        if (oSteps.SelectSingleNode("division[@divisiontype='receive']") != null)
                        {
                            oRecDiv.AppendChild(makeApvLine(Approver[1], entcode, "normal"));
                        }
                        else
                        {
                            oSteps.AppendChild(oRecDiv).AppendChild(oRecDivTaskInfo);
                            oRecDiv.SetAttribute("divisiontype", "receive");
                            oRecDiv.SetAttribute("name", "담당부서");
                            oRecDiv.SetAttribute("oucode", "");
                            oRecDiv.SetAttribute("ouname", "");

                            oRecDivTaskInfo.SetAttribute("status", "inactive");
                            oRecDivTaskInfo.SetAttribute("result", "inactive");
                            oRecDivTaskInfo.SetAttribute("kind", "receive");
                            oRecDivTaskInfo.SetAttribute("datereceived", DateTime.Now.ToString());
                            oRecDiv.AppendChild(makeApvLine(Approver[1], entcode, "charge"));
                        }
                        break;
                    case "RC"://수신병렬합의
                        if (Approver[0] == "SC" && oRecDiv.SelectSingleNode("step[@routetype='assist']") == null)
                        {
                            oRecDiv.AppendChild(oRecAssistStep).AppendChild(oRecAssistStepTaskInfo);

                            oRecAssistStep.SetAttribute("unittype", "person");
                            oRecAssistStep.SetAttribute("routetype", "approve");
                            oRecAssistStep.SetAttribute("allottype", "parallel");
                            oRecAssistStep.SetAttribute("name", "합의");

                            oRecAssistStepTaskInfo.SetAttribute("status", "inactive");
                            oRecAssistStepTaskInfo.SetAttribute("result", "inactive");
                            oRecAssistStepTaskInfo.SetAttribute("kind", "send");

                        }
                        oRecAssistStep.AppendChild(makeApvLine(Approver[1], entcode, "assist"));
                        break;
                }

            }

            #endregion

            #region form_info_ext 정보 입력

            #endregion
            #region 상신요청

            CfnFormManager.WfFormManager oFormDBMgr = new CfnFormManager.WfFormManager();
            CfnCoreEngine.WfProcessManager oPMgr = null;

            fiid = CfnEntityClasses.WfEntity.NewGUID();
            piid = CfnEntityClasses.WfEntity.NewGUID();
            System.String pdef = string.Empty;

            System.Xml.XmlElement oSchema;
            CfnFormManager.WfFormSchema oFS = (CfnFormManager.WfFormSchema)oFormDBMgr.GetDefinitionEntity(scid, CfnFormManager.CfFormEntityKind.fekdFormSchema);

            //schema 값 설정
            System.Xml.XmlDocument oXML = new System.Xml.XmlDocument();
            oXML.LoadXml(oFS.Context);
            oSchema = oXML.DocumentElement;
            pdef = oSchema.GetAttribute("pdef");
            System.Collections.Specialized.NameValueCollection oDic = new System.Collections.Specialized.NameValueCollection();
            foreach (System.Xml.XmlNode oSchemaNode in oSchema.ChildNodes)
            {
                System.String sSchemaName = oSchemaNode.Name;
                oDic.Add(sSchemaName, oSchema.GetAttribute(sSchemaName));
                oDic.Add(sSchemaName + "V", oSchemaNode.InnerText);
            }

            //PIDC 생성
            System.Xml.XmlDocument m_oFormInfos = new System.Xml.XmlDocument();
            m_oFormInfos.LoadXml("<?xml version='1.0' encoding='utf-8'?><ClientAppInfo><App name='FormInfo'><forminfos><forminfo/></forminfos></App></ClientAppInfo>");
            System.Xml.XmlElement root = m_oFormInfos.DocumentElement;
            System.Xml.XmlElement currNode = (System.Xml.XmlElement)root.ChildNodes.Item(0).ChildNodes.Item(0).ChildNodes.Item(0);
            currNode.SetAttribute("prefix", fmpf);
            currNode.SetAttribute("revision", fmrv);
            currNode.SetAttribute("instanceid", fiid);
            currNode.SetAttribute("id", fmid);
            currNode.SetAttribute("name", fmnm);
            currNode.SetAttribute("schemaid", scid);
            currNode.SetAttribute("index", "0");
            currNode.SetAttribute("filename", "");
            currNode.SetAttribute("subject", subject);
            currNode.SetAttribute("secure_doc", "0");
            currNode.SetAttribute("req_response", "");


            TransactionScope scope = null;


            TransactionOptions options = new TransactionOptions();
            options.Timeout = TimeSpan.FromSeconds(60);
            options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            try
            {
                using (scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {

                    #region 양식정보생성
                    CfnFormManager.WfFormInstance oFormInfo = new CfnFormManager.WfFormInstance(
                        fiid,
                        fmnm,
                        fmpf,
                       Convert.ToInt32(fmrv)
                    );
                    //추가 필드 넣기 ( 입력 대상 필드, 수정시 수정된 필드만 넘김)
                    System.Collections.Hashtable oFields = new System.Collections.Hashtable();

                    oFields.Add("FORM_INST_ID", fiid);
                    oFields.Add("FORM_ID", fmid);
                    oFields.Add("SCHEMA_ID", scid);
                    oFields.Add("REVISION", fmrv);
                    oFields.Add("FORM_NAME", fmnm);
                    oFields.Add("FORM_PREFIX", fmpf);
                    oFields.Add("BODY_TYPE", "");

                    oFields.Add("INITIATOR_NAME", INITIATOR_NAME);
                    oFields.Add("INITIATOR_ID", INITIATOR_ID);
                    oFields.Add("INITIATOR_OU_ID", INITIATOR_OU_ID);
                    oFields.Add("INITIATOR_OU_NAME", INITIATOR_OU_NAME);
                    oFields.Add("ENT_CODE", entcode);

                    oFields.Add("SUBJECT", subject);
                    oFields.Add("BODY_CONTEXT", sb.ToString());

                    oFormInfo.Fields = oFields;

                    oFormInfo.ProcessId = piid;
                    oFormInfo.InitiatedDate = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                    oFormDBMgr.CreateEntity((CfnFormManager.WfFormInstance)oFormInfo);
                    #endregion

                    #region process정보생성
                    //양식 정보 설정
                    StringBuilder sbDC = new StringBuilder();
                    //BizData 설정
                    StringBuilder sbBizData1 = new StringBuilder("");
                    StringBuilder sbBizData2 = new StringBuilder("");
                    StringBuilder sbBizData3 = new StringBuilder("");   // BizData3, 4 추가 (2013-05-22 leesh)
                    StringBuilder sbBizData4 = new StringBuilder("");

                    int iInstancePriority = (int)CfnEntityClasses.CfInstancePriority.inprNormal;

                    sbDC.Append(m_oFormInfos.SelectSingleNode("ClientAppInfo").OuterXml);

                    oPMgr = new CfnCoreEngine.WfProcessManager();
                    string sPDID = pdef;

                    oPMgr.GetDefinition(sPDID);

                    CfnEntityClasses.WfProcess oPD = oPMgr.GetDefinition(sPDID);
                    CfnEntityClasses.WfProcessInstance oPI = new CfnEntityClasses.WfProcessInstance(
                        piid,
                        (int)CfnEntityClasses.CfProcessInstanceKind.pikdNormal,
                        null,
                        null,
                        oPD.id,
                        oPD.name,
                        sbDC.ToString(),
                        subject,
                        iInstancePriority,
                        false,
                        oPD.transactionMode,
                        INITIATOR_ID,
                        INITIATOR_NAME,
                        INITIATOR_OU_ID,
                        INITIATOR_OU_NAME,
                        fiid,
                        (int)CfnEntityClasses.CfInstanceState.instOpen_NotRunning_NotStarted,
                        "03_01_01",
                        Convert.ToDateTime(null),
                        Convert.ToDateTime(null),
                        Convert.ToDateTime(null),
                        Convert.ToDateTime(null),
                        sbBizData1.ToString(),
                        sbBizData2.ToString(),
                        sbBizData3.ToString(),
                        sbBizData4.ToString()
                    );

                    //'파라미터 넘기기
                    //'" <forminfo>"
                    //'"  <outerpub>False</outerpub>"
                    //'"  <innerpub>False</innerpub>"
                    //'"  <innerpost>False</innerpost>"
                    //'"  <sealauthority/>"
                    //'"  <scEdms>False</scEdms>"
                    //'" </forminfo>"
                    System.Collections.Specialized.NameValueCollection oPDIs = new System.Collections.Specialized.NameValueCollection();
                    oPDIs.Add("FORM_INFO_EXT", this.getFormInfoExtXML(oDic));
                    System.Collections.Specialized.NameValueCollection oDDIs = new System.Collections.Specialized.NameValueCollection();
                    oDDIs.Add("APPROVERCONTEXT", oSteps.OuterXml);

                    oPMgr.Create(oPI, oPDIs, oDDIs);
                    oPMgr.RequestStart(oPI);
                    sbDC = null;
                    sbBizData1 = null;
                    sbBizData2 = null;
                    sbBizData3 = null;  // BizData3, 4 추가 (2013-05-22 leesh)
                    sbBizData4 = null;
                    #endregion
                    scope.Complete();
                }
            }
            catch (System.Exception ex)
            {
                if (scope != null)
                {
                    scope.Dispose();
                    scope = null;
                }

                throw ex;
            }
            finally
            {
                //oFormDBMgr.Dispose();
                if (oPMgr != null)
                {
                    oPMgr.Dispose();
                }
            }

            #endregion

            breturn = true;
        }
        catch (System.Exception ex)
        {
            //throw ex;
            return ParseStackTrace(ex);

        }
        finally
        {
            oDS.Dispose();
            sb = null;
            INPUT.Dispose();
        }
        return breturn.ToString();
    }

    //결재문서 상신처리 모바일 근태신청서용
    //parameter
    //subject : 제목 (varchar - 200)
    //bodycontext : 본문
    //apvline : 결재선 SA@사번;SA@사번    결재구분@사번;결재구분@사번  결재구분 ( SA: 발신결재, SC: 발신병렬합의, RA:수신결재, RC:수신병렬합의)
    //empno : 사번
    //fmpf : 양식 key
    //etc : 기타 필드
    [WebMethod(true)]
    public string WSDraftForm_Mail(string RECEIVER, string ACC, string SECRET_RECEIVER, string BODY, string APVLINE, string ENTCODE, string EMPNO, string FMPF, string ETC, string SUBJECT)
    {
        System.Boolean breturn = false;
        System.Data.DataSet oDS = new System.Data.DataSet();
        StringBuilder sb = new StringBuilder();
        string ENTNAME = string.Empty;
        DataPack INPUT = new DataPack();

        try
        {
            #region 파라미터 확인
            //if (APVLINE == string.Empty)
            //{
            //    throw new System.Exception("결재선을 입력하십시오.");
            //}

            if (EMPNO == string.Empty)
            {
                throw new System.Exception("기안자 사번을 입력하십시오.");
            }

            if (FMPF == string.Empty)
            {
                throw new System.Exception("양식 key 값을 입력하십시오.");

            }

            if (ENTCODE == string.Empty)
            {
                throw new System.Exception("회사코드 값을 입력하십시오.");

            }

            #endregion

            System.String INITIATOR_ID = ""; //기안자 person code
            System.String INITIATOR_NAME = ""; //기안자 name
            System.String INITIATOR_OU_ID = ""; //기안자 unit code
            System.String INITIATOR_OU_NAME = ""; //기안자 unit name

            //#region 양식값 변환
            DataSet ds = new DataSet();

            INPUT = new DataPack();
            INPUT.add("@FORM_PREFIX", FMPF);

            using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wfform_getFormInfo_fmpt", INPUT);
            }

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                fmid = ds.Tables[0].Rows[0]["FORM_ID"].ToString();
                scid = ds.Tables[0].Rows[0]["SCHEMA_ID"].ToString();
                fmrv = ds.Tables[0].Rows[0]["REVISION"].ToString();
                fmnm = ds.Tables[0].Rows[0]["FORM_NAME"].ToString();
            }
            INPUT.Clear();

            //#endregion

            #region 사용자 정보 변환

            oApvList.LoadXml("<steps></steps>");

            System.Xml.XmlElement oSteps = oApvList.DocumentElement;
            System.Xml.XmlElement oDiv = oApvList.CreateElement("division");
            System.Xml.XmlElement oDivTaskinfo = oApvList.CreateElement("taskinfo");

            System.Xml.XmlElement oStep = oApvList.CreateElement("step");
            System.Xml.XmlElement oOU = oApvList.CreateElement("ou");
            System.Xml.XmlElement oPerson = oApvList.CreateElement("person");
            System.Xml.XmlElement oPersonTaskinfo = oApvList.CreateElement("taskinfo");

            //기안자 결재선 정보
            oSteps.AppendChild(oDiv).AppendChild(oDivTaskinfo);
            oDiv.AppendChild(oStep).AppendChild(oOU).AppendChild(oPerson).AppendChild(oPersonTaskinfo);

            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
            {
                INPUT.add("@PERSON_CODE", EMPNO);
                oDS = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PersonInfo_R", INPUT);
                if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow oDR in oDS.Tables[0].Rows)
                    {
                        INITIATOR_ID = oDR["PERSON_CODE"].ToString();
                        INITIATOR_NAME = oDR["EX_DISPLAY_NAME"].ToString();
                        INITIATOR_OU_ID = oDR["UNIT_CODE"].ToString();
                        INITIATOR_OU_NAME = oDR["EX_UNIT_NAME"].ToString();
                        ENTNAME = oDR["ENT_NAME"].ToString();

                        oSteps.SetAttribute("initiatorcode", INITIATOR_ID);
                        oSteps.SetAttribute("initiatoroucode", INITIATOR_OU_ID);
                        oSteps.SetAttribute("status", "inactive");

                        oDiv.SetAttribute("divisiontype", "send");
                        oDiv.SetAttribute("name", "발신");
                        oDiv.SetAttribute("oucode", INITIATOR_OU_ID);
                        oDiv.SetAttribute("ouname", INITIATOR_OU_NAME);

                        oDivTaskinfo.SetAttribute("status", "inactive");
                        oDivTaskinfo.SetAttribute("result", "inactive");
                        oDivTaskinfo.SetAttribute("kind", "send");
                        oDivTaskinfo.SetAttribute("datereceived", DateTime.Now.ToString());

                        oStep.SetAttribute("unittype", "person");
                        oStep.SetAttribute("routetype", "approve");
                        oStep.SetAttribute("name", "기안자");

                        oOU.SetAttribute("code", INITIATOR_OU_ID);
                        oOU.SetAttribute("name", INITIATOR_OU_NAME);

                        oPerson.SetAttribute("code", INITIATOR_ID);
                        oPerson.SetAttribute("name", INITIATOR_NAME);
                        oPerson.SetAttribute("oucode", INITIATOR_OU_ID);
                        oPerson.SetAttribute("ouname", INITIATOR_OU_NAME);
                        oPerson.SetAttribute("position", oDR["JOBPOSITION_Z"].ToString().Replace("&", ";"));//oDR["jobposition"].ToString().Split('&')[1] + ";" + oDR["jobposition"].ToString().Split('&')[0]);
                        oPerson.SetAttribute("title", oDR["JOBTITLE_Z"].ToString().Replace("&", ";"));//oDR["jobtitle"].ToString().Split('&')[1] + ";" + oDR["jobtitle"].ToString().Split('&')[0]);
                        oPerson.SetAttribute("level", oDR["JOBLEVEL_Z"].ToString().Replace("&", ";"));//oDR["joblevel"].ToString().Split('&')[1] + ";" + oDR["joblevel"].ToString().Split('&')[0]);

                        oPersonTaskinfo.SetAttribute("status", "inactive");
                        oPersonTaskinfo.SetAttribute("result", "inactive");
                        oPersonTaskinfo.SetAttribute("kind", "charge");
                        oPersonTaskinfo.SetAttribute("datereceived", DateTime.Now.ToString());

                    }
                }
                else
                {
                    throw new System.Exception("통합그룹웨어에 해당 사용자가 존재하지 않습니다.");
                }
            }

            #endregion

            #region 결재선변환

            System.Xml.XmlElement oRecDiv = oApvList.CreateElement("division");
            System.Xml.XmlElement oRecDivTaskInfo = oApvList.CreateElement("taskinfo");
            System.Xml.XmlElement oAssistStep = oApvList.CreateElement("step");
            System.Xml.XmlElement oAssistStepTaskInfo = oApvList.CreateElement("taskinfo");
            System.Xml.XmlElement oRecAssistStep = oApvList.CreateElement("step");
            System.Xml.XmlElement oRecAssistStepTaskInfo = oApvList.CreateElement("taskinfo");

            System.String[] aApprovers = APVLINE.Split(';');
            foreach (System.String ApproverInfo in aApprovers)
            {
                System.String[] Approver = ApproverInfo.Split('@');
                switch (Approver[0])
                {
                    case "SA"://발신결재
                        oDiv.AppendChild(makeApvLine(Approver[1], ENTCODE, "normal"));
                        break;
                    case "SC"://발신병렬합의
                        if (Approver[0] == "SC" && oDiv.SelectSingleNode("step[@routetype='assist']") == null)
                        {
                            oDiv.AppendChild(oAssistStep).AppendChild(oAssistStepTaskInfo);

                            oAssistStep.SetAttribute("unittype", "person");
                            oAssistStep.SetAttribute("routetype", "approve");
                            oAssistStep.SetAttribute("allottype", "parallel");
                            oAssistStep.SetAttribute("name", "합의");

                            oAssistStepTaskInfo.SetAttribute("status", "inactive");
                            oAssistStepTaskInfo.SetAttribute("result", "inactive");
                            oAssistStepTaskInfo.SetAttribute("kind", "send");

                        }
                        oAssistStep.AppendChild(makeApvLine(Approver[1], ENTCODE, "assist"));
                        break;
                    case "RA"://수신결재
                        //수신division여부 확인부터 시작
                        if (oSteps.SelectSingleNode("division[@divisiontype='receive']") != null)
                        {
                            oRecDiv.AppendChild(makeApvLine(Approver[1], ENTCODE, "normal"));
                        }
                        else
                        {
                            oSteps.AppendChild(oRecDiv).AppendChild(oRecDivTaskInfo);
                            oRecDiv.SetAttribute("divisiontype", "receive");
                            oRecDiv.SetAttribute("name", "담당부서");
                            oRecDiv.SetAttribute("oucode", "");
                            oRecDiv.SetAttribute("ouname", "");

                            oRecDivTaskInfo.SetAttribute("status", "inactive");
                            oRecDivTaskInfo.SetAttribute("result", "inactive");
                            oRecDivTaskInfo.SetAttribute("kind", "receive");
                            oRecDivTaskInfo.SetAttribute("datereceived", DateTime.Now.ToString());
                            oRecDiv.AppendChild(makeApvLine(Approver[1], ENTCODE, "charge"));
                        }
                        break;
                    case "RC"://수신병렬합의
                        if (Approver[0] == "SC" && oRecDiv.SelectSingleNode("step[@routetype='assist']") == null)
                        {
                            oRecDiv.AppendChild(oRecAssistStep).AppendChild(oRecAssistStepTaskInfo);

                            oRecAssistStep.SetAttribute("unittype", "person");
                            oRecAssistStep.SetAttribute("routetype", "approve");
                            oRecAssistStep.SetAttribute("allottype", "parallel");
                            oRecAssistStep.SetAttribute("name", "합의");

                            oRecAssistStepTaskInfo.SetAttribute("status", "inactive");
                            oRecAssistStepTaskInfo.SetAttribute("result", "inactive");
                            oRecAssistStepTaskInfo.SetAttribute("kind", "send");

                        }
                        oRecAssistStep.AppendChild(makeApvLine(Approver[1], ENTCODE, "assist"));
                        break;
                    case "CC"://수신처
                        System.Xml.XmlElement oCcinfo = oApvList.CreateElement("ccinfo");
                        oSteps.AppendChild(oCcinfo);
                        oCcinfo.SetAttribute("datereceived", "");
                        oCcinfo.SetAttribute("belongto", "sender");
                        oCcinfo.SetAttribute("senderid", INITIATOR_ID);
                        oCcinfo.SetAttribute("sendername", INITIATOR_NAME);

                        oCcinfo.AppendChild(makeApvLine(Approver[1], ENTCODE, "ccinfo"));
                        break;
                }

            }

            #endregion

            #region form_info_ext 정보 입력

            #endregion
            #region 상신요청
            // string SUBJECT = Covi.Framework.Dictionary.GetDicInfo(fmnm, CF.LanguageType.ko);

            sb.Append("<BODY_CONTEXT>");
            sb.Append("<tbContentElement><![CDATA[" + BODY + "]]></tbContentElement>");
            sb.Append("<DOC_LEVEL_NAME></DOC_LEVEL_NAME>");
            sb.Append("<INITIATOR_CODE_DP></INITIATOR_CODE_DP>");
            sb.Append("<INITIATOR_DATE_INFO></INITIATOR_DATE_INFO>");
            sb.Append("<INITIATED_DATE></INITIATED_DATE>");
            sb.Append("<USER_ID></USER_ID>");
            sb.Append("<mKey>" + ETC + "</mKey>");
            sb.Append("<mSubject><![CDATA[" + SUBJECT + "]]></mSubject>");
            sb.Append("<mLegacy_form></mLegacy_form>");
            sb.Append("<mAttachfile></mAttachfile>");
            sb.Append("<PMLINKS></PMLINKS>");
            sb.Append("<REJECTDOCLINKS></REJECTDOCLINKS>");
            sb.Append("<SEL_ENTPART></SEL_ENTPART>");
            sb.Append("<INITIATOR_DP><![CDATA[" + Covi.Framework.Dictionary.GetDicInfo(INITIATOR_NAME, CF.LanguageType.ko) + "]]></INITIATOR_DP>");
            sb.Append("<INITIATOR_OU_DP><![CDATA[" + Covi.Framework.Dictionary.GetDicInfo(INITIATOR_OU_NAME, CF.LanguageType.ko) + "]]></INITIATOR_OU_DP>");
            sb.Append("<RECEIVER><![CDATA[" + Server.HtmlEncode(RECEIVER) + "]]></RECEIVER>");
            sb.Append("<ACC><![CDATA[" + Server.HtmlEncode(ACC) + "]]></ACC>");
            sb.Append("<SECRET_RECEIVER><![CDATA[" + Server.HtmlEncode(SECRET_RECEIVER) + "]]></SECRET_RECEIVER>");
            string date = DateTime.Now.ToString("yyyy-MM-dd").Split('-')[0] + "년" + DateTime.Now.ToString("yyyy-MM-dd").Split('-')[1] + "월" + DateTime.Now.ToString("yyyy-MM-dd").Split('-')[2] + "일";
            sb.Append("<DOC_DATE>" + date + "</DOC_DATE>");

            sb.Append("</BODY_CONTEXT>");

            CfnFormManager.WfFormManager oFormDBMgr = new CfnFormManager.WfFormManager();
            CfnCoreEngine.WfProcessManager oPMgr = null;

            fiid = CfnEntityClasses.WfEntity.NewGUID();
            piid = CfnEntityClasses.WfEntity.NewGUID();
            System.String pdef = string.Empty;

            System.Xml.XmlElement oSchema;
            CfnFormManager.WfFormSchema oFS = (CfnFormManager.WfFormSchema)oFormDBMgr.GetDefinitionEntity(scid, CfnFormManager.CfFormEntityKind.fekdFormSchema);

            //schema 값 설정
            System.Xml.XmlDocument oXML = new System.Xml.XmlDocument();
            oXML.LoadXml(oFS.Context);
            oSchema = oXML.DocumentElement;
            pdef = oSchema.GetAttribute("pdef");
            System.Collections.Specialized.NameValueCollection oDic = new System.Collections.Specialized.NameValueCollection();
            string sSC_CHK = "0";
            string sSC_VAL = "";
            foreach (System.Xml.XmlNode oSchemaNode in oSchema.ChildNodes)
            {
                System.String sSchemaName = oSchemaNode.Name;
                if (sSchemaName.Equals("scChgr"))
                {
                    if (sSC_CHK.Equals("0"))
                    {
                        sSC_CHK = oSchema.GetAttribute(sSchemaName);
                        sSC_VAL = oSchemaNode.InnerText;
                    }
                }
                else if (sSchemaName.Equals("scChgrEnt"))
                {
                    if (sSC_CHK.Equals("0"))
                    {
                        sSC_CHK = oSchema.GetAttribute(sSchemaName);
                        sSC_VAL = oSchemaNode.InnerText;
                        if (sSC_CHK.Equals("1"))
                        {
                            if (sSC_VAL.Equals(""))
                            {
                                sSC_CHK = "0";
                                sSC_VAL = "";
                            }
                            else
                            {
                                System.Xml.XmlDocument m_oSC = new System.Xml.XmlDocument();
                                m_oSC.LoadXml("<?xml version='1.0' encoding='utf-8'?>" + sSC_VAL);
                                System.Xml.XmlElement rootSC = m_oSC.DocumentElement;
                                if (rootSC.SelectSingleNode("ENT_" + ENTCODE) != null)
                                {
                                    sSC_VAL = rootSC.SelectSingleNode("ENT_" + ENTCODE).InnerText;
                                }
                            }
                        }
                        else
                        {
                            sSC_CHK = "0";
                            sSC_VAL = "";
                        }
                    }
                }
                else
                {
                    oDic.Add(sSchemaName, oSchema.GetAttribute(sSchemaName));
                    oDic.Add(sSchemaName + "V", oSchemaNode.InnerText);
                }
            }
            oDic.Add("scChgr", sSC_CHK);
            oDic.Add("scChgrV", sSC_VAL);
            oDic.Add("entcode", ENTCODE);
            oDic.Add("entname", ENTNAME);

            //PIDC 생성
            System.Xml.XmlDocument m_oFormInfos = new System.Xml.XmlDocument();
            m_oFormInfos.LoadXml("<?xml version='1.0' encoding='utf-8'?><ClientAppInfo><App name='FormInfo'><forminfos><forminfo/></forminfos></App></ClientAppInfo>");
            System.Xml.XmlElement root = m_oFormInfos.DocumentElement;
            System.Xml.XmlElement currNode = (System.Xml.XmlElement)root.ChildNodes.Item(0).ChildNodes.Item(0).ChildNodes.Item(0);
            currNode.SetAttribute("prefix", FMPF);
            currNode.SetAttribute("revision", fmrv);
            currNode.SetAttribute("instanceid", fiid);
            currNode.SetAttribute("id", fmid);
            currNode.SetAttribute("name", fmnm.Split(';')[0]);
            currNode.SetAttribute("schemaid", scid);
            currNode.SetAttribute("index", "0");
            currNode.SetAttribute("filename", "");
            currNode.SetAttribute("subject", SUBJECT);
            currNode.SetAttribute("secure_doc", "0");
            currNode.SetAttribute("req_response", "");


            TransactionScope scope = null;


            TransactionOptions options = new TransactionOptions();
            options.Timeout = TimeSpan.FromSeconds(60);
            options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            try
            {
                using (scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {

                    #region 양식정보생성
                    CfnFormManager.WfFormInstance oFormInfo = new CfnFormManager.WfFormInstance(
                        fiid,
                        fmnm,
                        FMPF,
                       Convert.ToInt32(fmrv)
                    );
                    //추가 필드 넣기 ( 입력 대상 필드, 수정시 수정된 필드만 넘김)
                    System.Collections.Hashtable oFields = new System.Collections.Hashtable();

                    oFields.Add("FORM_INST_ID", fiid);
                    oFields.Add("FORM_ID", fmid);
                    oFields.Add("SCHEMA_ID", scid);
                    oFields.Add("REVISION", fmrv);
                    oFields.Add("FORM_NAME", fmnm);
                    oFields.Add("FORM_PREFIX", FMPF);
                    oFields.Add("BODY_TYPE", "");

                    oFields.Add("INITIATOR_NAME", INITIATOR_NAME);
                    oFields.Add("INITIATOR_ID", INITIATOR_ID);
                    oFields.Add("INITIATOR_OU_ID", INITIATOR_OU_ID);
                    oFields.Add("INITIATOR_OU_NAME", INITIATOR_OU_NAME);
                    oFields.Add("ENT_CODE", ENTCODE);
                    oFields.Add("ENT_NAME", ENTNAME);

                    oFields.Add("SUBJECT", SUBJECT);
                    oFields.Add("BODY_CONTEXT", sb.ToString());

                    oFormInfo.Fields = oFields;

                    oFormInfo.ProcessId = piid;
                    oFormInfo.InitiatedDate = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                    oFormDBMgr.CreateEntity((CfnFormManager.WfFormInstance)oFormInfo);
                    #endregion

                    #region process정보생성
                    //양식 정보 설정
                    StringBuilder sbDC = new StringBuilder();
                    //BizData 설정
                    StringBuilder sbBizData1 = new StringBuilder("");
                    StringBuilder sbBizData2 = new StringBuilder("");
                    StringBuilder sbBizData3 = new StringBuilder("");   // BizData3, 4 추가 (2013-05-22 leesh)
                    StringBuilder sbBizData4 = new StringBuilder("");

                    int iInstancePriority = (int)CfnEntityClasses.CfInstancePriority.inprNormal;

                    sbDC.Append(m_oFormInfos.SelectSingleNode("ClientAppInfo").OuterXml);

                    oPMgr = new CfnCoreEngine.WfProcessManager();
                    string sPDID = pdef;

                    oPMgr.GetDefinition(sPDID);

                    CfnEntityClasses.WfProcess oPD = oPMgr.GetDefinition(sPDID);
                    CfnEntityClasses.WfProcessInstance oPI = new CfnEntityClasses.WfProcessInstance(
                        piid,
                        (int)CfnEntityClasses.CfProcessInstanceKind.pikdNormal,
                        null,
                        null,
                        oPD.id,
                        oPD.name,
                        sbDC.ToString(),
                        SUBJECT,
                        iInstancePriority,
                        false,
                        oPD.transactionMode,
                        INITIATOR_ID,
                        INITIATOR_NAME,
                        INITIATOR_OU_ID,
                        INITIATOR_OU_NAME,
                        fiid,
                        (int)CfnEntityClasses.CfInstanceState.instOpen_NotRunning_NotStarted,
                        "03_01_01",
                        Convert.ToDateTime(null),
                        Convert.ToDateTime(null),
                        Convert.ToDateTime(null),
                        Convert.ToDateTime(null),
                        sbBizData1.ToString(),
                        sbBizData2.ToString(),
                        sbBizData3.ToString(),
                        sbBizData4.ToString()
                    );

                    //'파라미터 넘기기
                    //'" <forminfo>"
                    //'"  <outerpub>False</outerpub>"
                    //'"  <innerpub>False</innerpub>"
                    //'"  <innerpost>False</innerpost>"
                    //'"  <sealauthority/>"
                    //'"  <scEdms>False</scEdms>"
                    //'" </forminfo>"
                    System.Collections.Specialized.NameValueCollection oPDIs = new System.Collections.Specialized.NameValueCollection();
                    oPDIs.Add("FORM_INFO_EXT", this.getFormInfoExtXML(oDic));
                    System.Collections.Specialized.NameValueCollection oDDIs = new System.Collections.Specialized.NameValueCollection();
                    oDDIs.Add("APPROVERCONTEXT", oSteps.OuterXml);

                    oPMgr.Create(oPI, oPDIs, oDDIs);
                    oPMgr.RequestStart(oPI);
                    sbDC = null;
                    sbBizData1 = null;
                    sbBizData2 = null;
                    sbBizData3 = null;  // BizData3, 4 추가 (2013-05-22 leesh)
                    sbBizData4 = null;
                    #endregion


                    DataSet dsu = new DataSet();

                    INPUT = new DataPack();
                    INPUT.add("@PIID", piid);
                    INPUT.add("@KEEPYEAR", "5");
                    INPUT.add("@DOCID", "95");
                    INPUT.add("@DOCNAME", "공문서");

                    using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_INST_ConnectionString"))
                    {
                        dsu = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wfform_update_ex_mail", INPUT);
                    }


                    scope.Complete();


                }
            }
            catch (System.Exception ex)
            {
                if (scope != null)
                {
                    scope.Dispose();
                    scope = null;
                }

                throw ex;
            }
            finally
            {
                //oFormDBMgr.Dispose();
                if (oPMgr != null)
                {
                    oPMgr.Dispose();
                }
            }

            #endregion

            breturn = true;
        }
        catch (System.Exception ex)
        {
            //throw ex;
            return ParseStackTrace(ex);

        }
        finally
        {
            oDS.Dispose();
            sb = null;
            INPUT.Dispose();
        }
        return breturn.ToString();
    }




    protected System.Xml.XmlElement makeApvLine(string empno, string entcode, string apvkind)
    {
        System.Xml.XmlElement oReturn = null;
        System.Data.DataSet oDS = null;
        System.Xml.XmlElement oStep = oApvList.CreateElement("step");
        System.Xml.XmlElement oOU = oApvList.CreateElement("ou");
        System.Xml.XmlElement oPerson = oApvList.CreateElement("person");
        System.Xml.XmlElement oPersonTaskinfo = oApvList.CreateElement("taskinfo");
        System.Xml.XmlElement oStepTaskInfo = oApvList.CreateElement("taskinfo");
        DataPack INPUT = new DataPack();
        switch (apvkind)
        {
            case "charge":
            case "normal":
                oStep.AppendChild(oOU).AppendChild(oPerson).AppendChild(oPersonTaskinfo);
                break;
            case "assist":
            case "consult":
                oOU.AppendChild(oPerson).AppendChild(oPersonTaskinfo);
                break;
        }

        try
        {
            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
            {
                INPUT.add("@PERSON_CODE", empno);
                oDS = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PersonInfo_R", INPUT);
                if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow oDR in oDS.Tables[0].Rows)
                    {
                        if (apvkind == "normal" || apvkind == "charge")
                        {
                            oStep.SetAttribute("unittype", "person");
                            oStep.SetAttribute("routetype", "approve");
                            oStep.SetAttribute("name", "결재단계");
                        }
                        oOU.SetAttribute("code", oDR["UNIT_CODE"].ToString());
                        oOU.SetAttribute("name", oDR["UNIT_NAME"].ToString());

                        oPerson.SetAttribute("code", oDR["PERSON_CODE"].ToString());
                        oPerson.SetAttribute("name", oDR["DISPLAY_NAME"].ToString());
                        oPerson.SetAttribute("oucode", oDR["UNIT_CODE"].ToString());
                        oPerson.SetAttribute("ouname", oDR["UNIT_NAME"].ToString());
                        oPerson.SetAttribute("position", oDR["JOBPOSITION_Z"].ToString());
                        oPerson.SetAttribute("title", oDR["JOBTITLE_Z"].ToString());
                        oPerson.SetAttribute("level", oDR["JOBLEVEL_Z"].ToString());

                        oPersonTaskinfo.SetAttribute("status", "inactive");
                        oPersonTaskinfo.SetAttribute("result", "inactive");

                        if (apvkind == "assist")
                        {
                            oPersonTaskinfo.SetAttribute("kind", "normal");
                        }
                        else if (apvkind == "consult")
                        {
                            oPersonTaskinfo.SetAttribute("kind", "consent");
                        }
                        else
                        {
                            oPersonTaskinfo.SetAttribute("kind", apvkind);
                        }

                    }
                }
                else
                {
                    throw new System.Exception("통합그룹웨어에 해당 사용자가 존재하지 않습니다.");
                }
            }
        }
        catch (System.Exception e)
        {
            throw e;
        }
        finally
        {
            if (oDS != null)
            {
                oDS.Dispose();
            }
            INPUT.Dispose();
        }
        switch (apvkind)
        {
            case "charge":
            case "normal":
                oReturn = (System.Xml.XmlElement)oStep.Clone();
                break;
            case "assist":
                oReturn = (System.Xml.XmlElement)oOU.Clone();
                break;
        }
        return oReturn;
    }
    protected System.String makeNode(string sName, string vVal)
    {
        return "<" + sName + "><![CDATA[" + vVal + "]]></" + sName + ">";
    }
    protected System.String makeNode(string sName, string vVal, bool bcData)
    {
        return "<" + sName + "><![CDATA[" + vVal + "]]></" + sName + ">";
    }
    protected System.String getFormInfoExtXML(System.Collections.Specialized.NameValueCollection getInfo)
    {
        StringBuilder forminfoext = new StringBuilder("");
        string szReturn = "";
        try
        {
            forminfoext.Append("<forminfo>");
            forminfoext.Append(makeNode("entcode", getInfo["entcode"]));
            forminfoext.Append(makeNode("entname", getInfo["entname"]));
            forminfoext.Append(makeNode("outerpub", iif(getInfo["scOPub"], "1", "True", "False")));
            forminfoext.Append(makeNode("innerpub", iif(getInfo["scIPub"], "1", "True", "False")));
            forminfoext.Append(makeNode("innerpost", "False"));
            //각 부서함들 저장여부 설정
            forminfoext.Append(makeNode("scABox", getInfo["scABox"])).Append(makeNode("scSBox", getInfo["scSBox"])).Append(makeNode("scRBox", getInfo["scRBox"])).Append(makeNode("scRPBox", getInfo["scRPBox"])).Append(makeNode("scCBox", getInfo["scCBox"])).Append(makeNode("scCPBox", getInfo["scCPBox"])).Append(makeNode("scGARBox", getInfo["scGARBox"])).Append(makeNode("scGAPBox", getInfo["scGAPBox"])).Append(makeNode("scSAPBox", getInfo["scSAPBox"])).Append(makeNode("scSARBox", getInfo["scSARBox"])).Append(makeNode("scFAPBox", getInfo["scFAPBox"]));

            forminfoext.Append(makeNode("receiptlist", ""));

            forminfoext.Append(makeNode("sealauthority", ""));
            forminfoext.Append(makeNode("scEdms", iif(getInfo["scEdms"], "1", "True", "False"))).Append(makeNode("scEdmsLegacy", iif(getInfo["scEdmsLegacy"], "1", "True", "False"))); //2006.08추가 by sunny
            forminfoext.Append(makeNode("docclass", getInfo["scDocClassV"]));
            forminfoext.Append(makeNode("issuedocno", iif(getInfo["scDNum"], "1", "True", "False")));
            forminfoext.Append("<docinfo>");
            forminfoext.Append("<outerpubdocno />"); //외부공문번호 넣어줄것
            forminfoext.Append(makeNode("docno", ""));
            forminfoext.Append(makeNode("recno", ""));

            forminfoext.Append(makeNode("dpdsn", ""));
            forminfoext.Append(makeNode("catcode", ""));//문서분류코드
            forminfoext.Append(makeNode("catname", ""));//문서분류명
            forminfoext.Append(makeNode("keepyear", ""));//보존년한
            forminfoext.Append(makeNode("fiscalyear", DateTime.Now.Year.ToString()));
            forminfoext.Append(makeNode("enforcedate", DateTime.Now.Year.ToString()));
            forminfoext.Append(makeNode("docsec", ""));//문서보안등급
            forminfoext.Append(makeNode("ispublic", ""));
            forminfoext.Append(makeNode("deptcode", "")).Append(makeNode("deptpath", ""));
            forminfoext.Append("<attach></attach>");	//첨부파일path
            forminfoext.Append(makeNode("circulation", ""));
            forminfoext.Append(makeNode("sentunitname", ""));
            forminfoext.Append(makeNode("regcomment", ""));
            forminfoext.Append("</docinfo>");
            forminfoext.Append(makeNode("JFID", iif(getInfo["scChgr"], "1", getInfo["scChgrV"], "")));//담당자처리
            forminfoext.Append(makeNode("IsChargeConfirm", iif(getInfo["scChgrConf"], "1", "true", "false")));//담당자 확인
            forminfoext.Append(makeNode("ChargeOU", iif(getInfo["scChgrOU"], "1", getInfo["scChgrOUV"], "")));
            forminfoext.Append(makeNode("rejectedto", iif(getInfo["scRJTO"], "1", "true", "false")));
            forminfoext.Append(makeNode("MakeReport", iif(getInfo["scMRPT"], "1", getInfo["scMRPTV"], "")));
            forminfoext.Append(makeNode("WORKREQUEST_ID", ""));
            forminfoext.Append(makeNode("parentfiid", ""));
            forminfoext.Append(makeNode("IsLegacy", iif(getInfo["scLegacy"], "1", getInfo["scLegacyV"], "")));
            forminfoext.Append(makeNode("entcode", getInfo["ENT_CODE"]));
            forminfoext.Append(makeNode("entname", getInfo["ENT_NAME"]));
            forminfoext.Append(makeNode("docnotype", getInfo["scDNumV"]));
            forminfoext.Append(makeNode("ConsultOK", iif(getInfo["scConsultOK"], "1", "true", "false")));
            forminfoext.Append(makeNode("IsMobile", iif(getInfo["scMobile"], "1", "true", "false")));
            forminfoext.Append(makeNode("IsSubReturn", iif(getInfo["scDCooReturn"], "1", "true", "false")));
            forminfoext.Append(makeNode("IsDeputy", "false"));
            forminfoext.Append(makeNode("scAutoCmm", getInfo["scAutoCmm"]));
            forminfoext.Append(makeNode("scAutoCmmV", getInfo["scAutoCmmV"]));
            forminfoext.Append(makeNode("IsReUse", getInfo["REUSE"]));
            forminfoext.Append(makeNode("scDocBoxRE", getInfo["scDocBoxRE"]));
            forminfoext.Append(makeNode("nCommitteeCount", "2"));
            forminfoext.Append(makeNode("scASSBox", getInfo["scASSBox"])); //개인합의 부서함저장
            forminfoext.Append(makeNode("scPreDocNum", getInfo["scPreDocNum"]));  //문서번호선발번

            forminfoext.Append("</forminfo>");

            szReturn = forminfoext.ToString();
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            forminfoext = null;
        }

        return szReturn;
    }
    protected string iif(string vVal, string vCmp, string btrue, string bfalse)
    {
        if (vVal == vCmp)
        {
            return btrue;
        }
        else
        {
            return bfalse;
        }
    }
    public static string ParseStackTrace(Exception _Exception)
    {
        try
        {

            Exception InnEx = _Exception;

            StringBuilder sb = new StringBuilder(" #").Append(InnEx.Message).Append("#").Append(InnEx.StackTrace);

            while (InnEx.InnerException != null)
            {

                InnEx = InnEx.InnerException;
                sb.Insert(0, " #").Append(InnEx.Message).Append("#").Append(InnEx.StackTrace);
            }
            sb.Insert(0, "[" + InnEx.GetType().ToString() + "]");
            return sb.ToString();

        }
        catch (Exception ex)
        {
            throw new System.Exception(null, ex);
        }
    }
    /// <summary>
    /// 이관대상 문서 조회.
    /// </summary>
    [WebMethod]
    public System.Data.DataSet getEDMSConvertList()
    {
        System.Data.DataSet oDS = null;
        DataPack INPUT = new DataPack();
        try
        {
            using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                oDS = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wfform_edmsconvertlist", INPUT);
            }
        }
        catch { }
        finally
        {
            INPUT.Dispose();
        }
        return oDS;

        //if (oDS != null)
        //{
        //    oDS.Dispose();
        //}
    }

    /// <summary>
    /// 이관성공 저장
    /// </summary>
    [WebMethod]
    public bool setEDMSCommit(string docid)
    {
        bool oReturn = false;
        DataPack INPUT = new DataPack();
        int iReturn;
        try
        {
            using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                INPUT.add("@docid", docid);
                INPUT.add("@endflag", "2");
                iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wfform_setedmscommit", INPUT);
                oReturn = true;
            }
            return oReturn;
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            INPUT.Dispose();
        }
    }
    /// <summary>
    /// 이관실패 저장
    /// </summary>
    [WebMethod]
    public bool setEDMSFail(string docid, System.String errmsg)
    {
        bool oReturn = false;
        DataPack INPUT = new DataPack();
        int iReturn;

        try
        {
            using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                INPUT.add("@docid", docid);
                INPUT.add("@errmsg", errmsg);
                iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wfform_setedmsfail", INPUT);
                oReturn = true;
            }
            return oReturn;
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            INPUT.Dispose();
        }
    }

    #region MOBILE 웹서비스
    /// <summary>
    /// - 최초작성자 : 코비전 황선희<br />
    /// - 최초작성일 : 2008년  10월 09일<br />
    /// MOBILE 2010.03.02 SYB
    /// [B010]결재문서 목록 조회
    /// </summary>
    /// <param name="LoginID">사용자사번(key)</param>
    /// <param name="UnitCode">부서코드</param>
    /// <param name="StartDate">시작일자</param>
    /// <param name="EndDate">종료일자</param>
    /// <param name="MaxCount">Max개수</param>
    /// <param name="CntPerPage">페이지당 조회 건수</param>
    /// <param name="CurPageNo">조회페이지</param>
    /// <param name="Mode">결재함구분(APPROVAL:미결, PROCESS:진행, COMPLETE:완료, TCINFO 회람/참조)</param>
    /// <returns>성공시:요청한 데이터 형태의 xml 문자열, 실패시:ERROR 메시지</returns>

    #region //--> MobileGetWorkList
    [WebMethod]
    public string MobileGetWorkList(string LoginID, string UnitCode, string StartDate, string EndDate, string MaxCount, string CntPerPage, string CurPageNo, string Mode)
    {
        return MobileGetWorkListSearch(LoginID, UnitCode, StartDate, EndDate, MaxCount, CntPerPage, CurPageNo, Mode, "", "");
        //기존코드 주석처리--> 함수재구현
    }


    /// <summary>
    /// 2015-08-29 모바일 목록 검색 추가 MobileGetWorkList 메소드에서 파라미터만 추가
    /// </summary>
    /// <param name="LoginID"></param>
    /// <param name="UnitCode"></param>
    /// <param name="StartDate"></param>
    /// <param name="EndDate"></param>
    /// <param name="MaxCount"></param>
    /// <param name="CntPerPage"></param>
    /// <param name="CurPageNo"></param>
    /// <param name="Mode"></param>
    /// <param name="SearchCondition">검색조건</param>
    /// <param name="SearchValue">검색조건값</param>
    /// <returns></returns>
    [WebMethod(Description = "모바일 전자결재 목록 API 완료함 관련 함에 검색기능 포함")]
    public string MobileGetWorkListSearch(string LoginID, string UnitCode, string StartDate, string EndDate, string MaxCount, string CntPerPage, string CurPageNo, string Mode, string SearchCondition, string SearchValue)
    {

        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        // 1753년 1월 1일 오전 12:00:00
        DateTime dtStart = new DateTime(1753, 1, 1, 0, 0, 0, 0);
        DateTime dtEnd = DateTime.MaxValue;
        string sDateTimeEmpty = "00000000";
        string sMaxCntEmpty = "0000";
        string sPageCntEmpty = "000";
        string sPNumCntEmpty = "0000";
        int MAXROWCNT = 4096; // SELECT 최대 개수

        try
        {
            int imaxcnt = 0;
            int ipagenum = 0;
            int ipagesize = 0;

            if (MaxCount != string.Empty && !MaxCount.Trim().Equals(sMaxCntEmpty))
                imaxcnt = System.Convert.ToInt32(MaxCount);

            if (CntPerPage != string.Empty && !CntPerPage.Trim().Equals(sPageCntEmpty))
                ipagesize = System.Convert.ToInt32(CntPerPage);

            if (CurPageNo != string.Empty && !CurPageNo.Trim().Equals(sPNumCntEmpty))
                ipagenum = System.Convert.ToInt32(CurPageNo);

            if (imaxcnt == 0)
            {
                if (ipagenum == 0 || ipagesize == 0)
                {
                    imaxcnt = MAXROWCNT;
                    ipagenum = 1;
                    ipagesize = imaxcnt;
                }
            }
            else
            {
                //ipagenum = 1;
                ipagesize = imaxcnt;
            }

            string mode = "start";
            if (StartDate != null && StartDate != string.Empty
                                  && !StartDate.Trim().Equals(sDateTimeEmpty))
            {
                dtStart = StringToDateTime(mode, StartDate, dtStart);
            }

            mode = "end";
            if (EndDate != null && EndDate != string.Empty
                                && !EndDate.Trim().Equals(sDateTimeEmpty))
            {
                dtEnd = StringToDateTime(mode, EndDate, dtEnd);
            }

            if (LoginID != string.Empty)
            {
                string sSpName = "dbo.usp_wf_worklistquery01mobile";
                if (Mode == null || Mode == string.Empty) Mode = "APPROVAL";

                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@USER_ID", LoginID);
                    INPUT.add("@MODE", Mode);
                    INPUT.add("@UNIT_CODE", UnitCode);
                    INPUT.add("@StartDate", dtStart);
                    INPUT.add("@EndDate", dtEnd);
                    INPUT.add("@maxcnt", imaxcnt);
                    INPUT.add("@pagenum", ipagenum);
                    INPUT.add("@page_size", ipagesize); //string StartDate, string EndDate,
                    INPUT.add("@gubun", SearchCondition);
                    INPUT.add("@title", SearchValue);
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    oDS.DataSetName = "RESPONSE";
                }
            }

            if (oDS != null && oDS.Tables.Count > 1 && oDS.Tables[0].Rows.Count > 0)
            {
                oDS.Tables[1].TableName = "DOC";

                oReturnXML.LoadXml(oDS.GetXml());

                if (Mode == "TEMPSAVE")
                {
                }
                else
                {
                    //변환하기
                    XmlNode oRoot = oReturnXML.DocumentElement;
                    XmlNodeList oList = oRoot.SelectNodes("DOC");
                    foreach (System.Xml.XmlNode oNode in oList)
                    {

                        // 문서종류코드, 문서종류명, 첨부파일 유무	

                        XmlNode oFNode = null;
                        if (oNode.SelectSingleNode("PI_DSCR").InnerText != null &&
                                oNode.SelectSingleNode("PI_DSCR").InnerText.Trim().Length > 0)
                        {
                            XmlDocument oXML = new System.Xml.XmlDocument();
                            oXML.LoadXml(oNode.SelectSingleNode("PI_DSCR").InnerText);
                            oFNode = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                        }
                        oNode.RemoveChild(oNode.SelectSingleNode("PI_DSCR"));

                        // 문서종류코드
                        XmlElement prefix = oReturnXML.CreateElement("ApprDocTypeCode");
                        if (oFNode != null)
                        {
                            prefix.InnerText = oFNode.Attributes["prefix"].Value;

                            oNode.AppendChild(prefix);

                            // 문서종류명
                            XmlElement fName = oReturnXML.CreateElement("ApprDocTypeName");
                            fName.InnerText = oFNode.Attributes["name"].Value;
                            oNode.AppendChild(fName);

                            // 첨부파일 유무
                            XmlElement isFile = oReturnXML.CreateElement("AttachFileYN");
                            if (oFNode.Attributes["isfile"] != null && oFNode.Attributes["isfile"].Value.Equals("1"))
                            {
                                isFile.InnerText = "Y";
                            }
                            else
                            {
                                isFile.InnerText = "N";
                            }
                            oNode.AppendChild(isFile);
                        }
                        // 결재종류 코드
                        // (string sPiSate, string sWiSate, string sSubKind, string sPiBzStat, string sMode)

                        //string sTCode = GetApprTypeCode(oNode.SelectSingleNode("PI_STATE").InnerText,
                        //									oNode.SelectSingleNode("WI_STATE").InnerText,
                        //									oNode.SelectSingleNode("ApprTypeCode").InnerText,
                        //									oNode.SelectSingleNode("PI_BUSINESS_STATE").InnerText,
                        //									oNode.SelectSingleNode("MODE").InnerText);
                        string sTCode = GetApprTypeCode(oNode.SelectSingleNode("PI_STATE").InnerText,
                                                            oNode.SelectSingleNode("WI_STATE").InnerText,
                                                            oNode.SelectSingleNode("ApprTypeCode").InnerText,
                                                            (oNode.SelectSingleNode("PI_BUSINESS_STATE") == null) ? "" : oNode.SelectSingleNode("PI_BUSINESS_STATE").InnerText,
                                                            oNode.SelectSingleNode("MODE").InnerText);

                        // 결재종류명
                        XmlElement appTypeName = oReturnXML.CreateElement("ApprTypeName");
                        appTypeName.InnerText = GetApprTypeNameByCode(sTCode);
                        oNode.AppendChild(appTypeName);

                        //WIID 재설정 - 회람인 경우 WORKITEMID 강제설정
                        if (oNode.SelectSingleNode("ApprDocWID") == null && oNode.SelectSingleNode("P1") != null)
                        {
                            XmlElement ApprDocWID = oReturnXML.CreateElement("ApprDocWID");
                            oNode.AppendChild(ApprDocWID);
                            ApprDocWID.InnerText = oNode.SelectSingleNode("P1").InnerText;
                            XmlElement WI_ID = oReturnXML.CreateElement("WI_ID");
                            oNode.AppendChild(WI_ID);
                            WI_ID.InnerText = oNode.SelectSingleNode("P1").InnerText;
                            oNode.RemoveChild(oNode.SelectSingleNode("P1"));
                        }
                        //oNode.SelectSingleNode("ApprTypeCode").InnerText = sTCode;
                        oNode.RemoveChild(oNode.SelectSingleNode("PI_STATE"));
                        oNode.RemoveChild(oNode.SelectSingleNode("WI_STATE"));

                        //oNode.RemoveChild(oNode.SelectSingleNode("PI_BUSINESS_STATE"));
                        if (oNode.SelectSingleNode("PI_BUSINESS_STATE") != null) oNode.RemoveChild(oNode.SelectSingleNode("PI_BUSINESS_STATE"));

                        oNode.RemoveChild(oNode.SelectSingleNode("MODE"));
                    }
                }
            }
            return oReturnXML.OuterXml;
        }
        catch (Exception ex)
        {
            // 에러 로그 기록
            ExceptionWebServiceHandling(ex, CF.ExceptionType.Error, _logEnt.RepererURL);

            return "<WebService><LIST><Error><![CDATA[" + ex.Message + "]]></Error></LIST></WebService>";
        }
        finally
        {
            oDS.Dispose();
            // 성능 로그 기록(주 메써드의 마지막에 호출되어야 함.)
            PerformanceWebServiceLogWrite(true);
        }
    }


    #endregion


    #region //--> New MobileGetWorkList
    [WebMethod]
    public ReturnType_MobileGetWorkList MobileGetWorkList_New(string LoginID, string UnitCode, string StartDate, string EndDate, string MaxCount, string CntPerPage, string CurPageNo, string Mode)
    {
        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        // 1753년 1월 1일 오전 12:00:00
        DateTime dtStart = new DateTime(1753, 1, 1, 0, 0, 0, 0);
        DateTime dtEnd = DateTime.MaxValue;
        string sDateTimeEmpty = "00000000";
        string sMaxCntEmpty = "0000";
        string sPageCntEmpty = "000";
        string sPNumCntEmpty = "0000";
        int MAXROWCNT = 4096; // SELECT 최대 개수

        // New Type 
        ReturnType_MobileGetWorkList retVal = new ReturnType_MobileGetWorkList();

        try
        {
            int imaxcnt = 0;
            int ipagenum = 0;
            int ipagesize = 0;

            if (MaxCount != string.Empty && !MaxCount.Trim().Equals(sMaxCntEmpty))
                imaxcnt = System.Convert.ToInt32(MaxCount);

            if (CntPerPage != string.Empty && !CntPerPage.Trim().Equals(sPageCntEmpty))
                ipagesize = System.Convert.ToInt32(CntPerPage);

            if (CurPageNo != string.Empty && !CurPageNo.Trim().Equals(sPNumCntEmpty))
                ipagenum = System.Convert.ToInt32(CurPageNo);

            if (imaxcnt == 0)
            {
                if (ipagenum == 0 || ipagesize == 0)
                {
                    imaxcnt = MAXROWCNT;
                    ipagenum = 1;
                    ipagesize = imaxcnt;
                }
            }
            else
            {
                //ipagenum = 1;
                ipagesize = imaxcnt;
            }

            string mode = "start";
            if (StartDate != null && StartDate != string.Empty
                                  && !StartDate.Trim().Equals(sDateTimeEmpty))
            {
                dtStart = StringToDateTime(mode, StartDate, dtStart);
            }

            mode = "end";
            if (EndDate != null && EndDate != string.Empty
                                && !EndDate.Trim().Equals(sDateTimeEmpty))
            {
                dtEnd = StringToDateTime(mode, EndDate, dtEnd);
            }

            if (LoginID != string.Empty)
            {
                string sSpName = "dbo.usp_wf_worklistquery01mobile";
                if (Mode == null || Mode == string.Empty) Mode = "APPROVAL";

                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@USER_ID", LoginID);
                    INPUT.add("@MODE", Mode);
                    INPUT.add("@UNIT_CODE", UnitCode);
                    INPUT.add("@StartDate", dtStart);
                    INPUT.add("@EndDate", dtEnd);
                    INPUT.add("@maxcnt", imaxcnt);
                    INPUT.add("@pagenum", ipagenum);
                    INPUT.add("@page_size", ipagesize); //string StartDate, string EndDate,
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    oDS.DataSetName = "RESPONSE";
                }
            }

            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                // New Type
                retVal.Table = new ReturnType_MobileGetWorkList_Table();
                retVal.Table.pagenum = int.Parse(oDS.Tables[0].Rows[0]["pagenum"].ToString());
                retVal.Table.totalcount = int.Parse(oDS.Tables[0].Rows[0]["totalcount"].ToString());

                retVal.Doc = new ReturnType_MobileGetWorkList_Doc[oDS.Tables[1].Rows.Count];

                oDS.Tables[1].TableName = "DOC";
                oReturnXML.LoadXml(oDS.GetXml());

                //변환하기
                XmlNode oRoot = oReturnXML.DocumentElement;
                XmlNodeList oList = oRoot.SelectNodes("DOC");

                // New Typw
                for (int i = 0; i < oDS.Tables[1].Rows.Count; i++)
                {
                    retVal.Doc[i] = new ReturnType_MobileGetWorkList_Doc();
                    retVal.Doc[i].rownum = int.Parse(oDS.Tables[1].Rows[i]["rownum"].ToString());
                    retVal.Doc[i].ApprDocPID = oDS.Tables[1].Rows[i]["ApprDocPID"].ToString();
                    retVal.Doc[i].ApprDocWID = oDS.Tables[1].Rows[i]["ApprDocWID"].ToString();
                    retVal.Doc[i].DrafterLoginID = oDS.Tables[1].Rows[i]["DrafterLoginID"].ToString();
                    retVal.Doc[i].DrafterName = oDS.Tables[1].Rows[i]["DrafterName"].ToString();
                    //retVal.Doc[i].PositionCode = oDS.Tables[1].Rows[i]["PositionCode"].ToString();
                    //retVal.Doc[i].PositionName = oDS.Tables[1].Rows[i]["PositionName"].ToString();
                    retVal.Doc[i].DraftDprtCode = oDS.Tables[1].Rows[i]["DraftDprtCode"].ToString();
                    retVal.Doc[i].DraftDprtName = oDS.Tables[1].Rows[i]["DraftDprtName"].ToString();
                    retVal.Doc[i].ApprTypeCode = oDS.Tables[1].Rows[i]["ApprTypeCode"].ToString();
                    retVal.Doc[i].ApprDocTitle = oDS.Tables[1].Rows[i]["ApprDocTitle"].ToString();
                    retVal.Doc[i].DraftDateTime = oDS.Tables[1].Rows[i]["DraftDateTime"].ToString();
                    //retVal.Doc[i].BellShowYN = oDS.Tables[1].Rows[i]["BellShowYN"].ToString();
                    retVal.Doc[i].WORKDT = oDS.Tables[1].Rows[i]["WORKDT"].ToString();
                    // <WI_ID>a48437c7-5140-49b0-884e-b98f3b8c2261</WI_ID>
                    //retVal.Doc[i].WI_CREATED = oDS.Tables[1].Rows[i]["WI_CREATED"].ToString();
                    //retVal.Doc[i].PI_INITIATOR_SIPADDRESS = oDS.Tables[1].Rows[i]["PI_INITIATOR_SIPADDRESS"].ToString();
                    //retVal.Doc[i].H1 = oDS.Tables[1].Rows[i]["H1"].ToString();
                    //retVal.Doc[i].M1 = oDS.Tables[1].Rows[i]["M1"].ToString();

                }

                int rCnt = 0;
                foreach (System.Xml.XmlNode oNode in oList)
                {
                    // 문서종류코드, 문서종류명, 첨부파일 유무
                    XmlNode oFNode = null;
                    if (oNode.SelectSingleNode("PI_DSCR").InnerText != null &&
                            oNode.SelectSingleNode("PI_DSCR").InnerText.Trim().Length > 0)
                    {
                        XmlDocument oXML = new System.Xml.XmlDocument();
                        oXML.LoadXml(oNode.SelectSingleNode("PI_DSCR").InnerText);
                        oFNode = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                    }
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_DSCR"));

                    // 문서종류코드
                    XmlElement prefix = oReturnXML.CreateElement("ApprDocTypeCode");
                    if (oFNode != null)
                    {
                        prefix.InnerText = oFNode.Attributes["prefix"].Value;
                        oNode.AppendChild(prefix);

                        retVal.Doc[rCnt].ApprDocTypeCode = oFNode.Attributes["prefix"].Value;

                        // 문서종류명
                        XmlElement fName = oReturnXML.CreateElement("ApprDocTypeName");
                        fName.InnerText = oFNode.Attributes["name"].Value;
                        oNode.AppendChild(fName);

                        retVal.Doc[rCnt].ApprDocTypeName = oFNode.Attributes["name"].Value;

                        // 첨부파일 유무
                        XmlElement isFile = oReturnXML.CreateElement("AttachFileYN");
                        if (oFNode.Attributes["isfile"] != null && oFNode.Attributes["isfile"].Value.Equals("1"))
                        {
                            isFile.InnerText = "Y";
                            retVal.Doc[rCnt].AttachFileYN = "Y";
                        }
                        else
                        {
                            isFile.InnerText = "N";
                            retVal.Doc[rCnt].AttachFileYN = "N";
                        }
                        oNode.AppendChild(isFile);
                    }
                    // 결재종류 코드
                    // (string sPiSate, string sWiSate, string sSubKind, string sPiBzStat, string sMode)
                    string sTCode = GetApprTypeCode(oNode.SelectSingleNode("PI_STATE").InnerText,
                                                        oNode.SelectSingleNode("WI_STATE").InnerText,
                                                        oNode.SelectSingleNode("ApprTypeCode").InnerText,
                                                        oNode.SelectSingleNode("PI_BUSINESS_STATE").InnerText,
                                                        oNode.SelectSingleNode("MODE").InnerText);

                    // 결재종류명
                    XmlElement appTypeName = oReturnXML.CreateElement("ApprTypeName");
                    appTypeName.InnerText = GetApprTypeNameByCode(sTCode);
                    oNode.AppendChild(appTypeName);

                    retVal.Doc[rCnt].ApprTypeName = GetApprTypeNameByCode(sTCode);

                    //WIID 재설정 - 회람인 경우 WORKITEMID 강제설정
                    if (oNode.SelectSingleNode("ApprDocWID") == null && oNode.SelectSingleNode("P1") != null)
                    {
                        XmlElement ApprDocWID = oReturnXML.CreateElement("ApprDocWID");
                        oNode.AppendChild(ApprDocWID);
                        ApprDocWID.InnerText = oNode.SelectSingleNode("P1").InnerText;

                        retVal.Doc[rCnt].ApprDocWID = oNode.SelectSingleNode("P1").InnerText;

                        XmlElement WI_ID = oReturnXML.CreateElement("WI_ID");
                        oNode.AppendChild(WI_ID);
                        WI_ID.InnerText = oNode.SelectSingleNode("P1").InnerText;
                        oNode.RemoveChild(oNode.SelectSingleNode("P1"));
                    }
                    //oNode.SelectSingleNode("ApprTypeCode").InnerText = sTCode;
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("WI_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_BUSINESS_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("MODE"));

                    rCnt++;
                }
            }
            //return oReturnXML.OuterXml;
            retVal.ReturnDesc = "Success";
        }
        catch (Exception ex)
        {
            //return "<WebService><LIST><Error><![CDATA[" + ex.Message + "]]></Error></LIST></WebService>";
            retVal.ReturnDesc = "Error : " + ex.Message.ToString();
        }
        finally
        {
            oDS.Dispose();
        }

        return retVal;
    }
    #endregion

    /// <summary>
    /// [B011] 결재문서 상세 조회
    /// </summary>
    /// <param name="ApprDocPID"></param>
    /// <param name="ApprDocWID"></param>
    /// <returns></returns>
    #region //--> MobileGetApprDocDetail
    [WebMethod]
    public string MobileGetApprDocDetail(string ApprDocPID, string ApprDocWID, string ApprDocTypeCode)
    {
        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        DateTime dtStart = DateTime.MinValue;
        DateTime dtEnd = DateTime.MaxValue;
        String szURL = String.Empty;
        StringBuilder sb = new StringBuilder();
        int iCommentCount = 0;
        try
        {

            if (ApprDocPID != string.Empty)//&& ApprDocWID != string.Empty
            {
                ApprDocPID = ApprDocPID.Trim();
                ApprDocWID = ApprDocWID.Trim();

                string sSpName = "dbo.usp_wf_worklistquery01apprdocdetailmobile";
                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@ApprDocPID", ApprDocPID);
                    INPUT.add("@ApprDocWID", ApprDocWID);
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    oDS.DataSetName = "RESPONSE";
                }
                if (oDS != null && oDS.Tables[0].Rows.Count == 0)
                {
                    using (SqlDacManager oDac = new SqlDacManager("INST_ARCHIVE_ConnectionString"))
                    {
                        oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDS.DataSetName = "RESPONSE";
                    }
                }
            }

            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                oDS.Tables[0].TableName = "DOC";
                oReturnXML.LoadXml(oDS.GetXml());

                //변환하기
                XmlNode oRoot = oReturnXML.DocumentElement;
                XmlNodeList oList = oRoot.SelectNodes("DOC");
                foreach (System.Xml.XmlNode oNode in oList)
                {
                    #region 기안정보

                    // 문서종류코드, 문서종류명, 첨부파일 유무	

                    XmlDocument oXML = new System.Xml.XmlDocument();
                    XmlNode oFNode = null;
                    if (oNode.SelectSingleNode("PI_DSCR").InnerText != null &&
                            oNode.SelectSingleNode("PI_DSCR").InnerText.Trim().Length > 0)
                    {
                        oXML.LoadXml(oNode.SelectSingleNode("PI_DSCR").InnerText);
                        oFNode = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                    }
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_DSCR"));

                    // 문서종류코드
                    XmlElement prefix = oReturnXML.CreateElement("ApprDocTypeCode");
                    if (oFNode != null)
                    {
                        prefix.InnerText = oFNode.Attributes["prefix"].Value;
                    }
                    oNode.AppendChild(prefix);

                    // 문서종류명
                    XmlElement fName = oReturnXML.CreateElement("ApprDocTypeName");
                    if (oFNode != null)
                    {
                        fName.InnerText = oFNode.Attributes["name"].Value;
                    }
                    oNode.AppendChild(fName);


                    // form_inst_id
                    XmlElement ffiid = oReturnXML.CreateElement("ApprDocFID");
                    if (ffiid != null)
                    {
                        ffiid.InnerText = oFNode.Attributes["instanceid"].Value;
                    }
                    oNode.AppendChild(ffiid);

                    // 현재 Workitem State
                    XmlElement wState = oReturnXML.CreateElement("wState");
                    wState.InnerText = oNode.SelectSingleNode("WI_STATE").InnerText;
                    oNode.AppendChild(wState);

                    //// 결재종류 코드
                    //oNode.SelectSingleNode("ApprTypeCode").InnerText
                    //    = ApprTypeToCode(oNode.SelectSingleNode("ApprTypeCode").InnerText.Trim());

                    //// 결재종류명
                    //XmlElement appTypeName = oReturnXML.CreateElement("ApprTypeName");
                    //appTypeName.InnerText = ApprTypeToName(oNode.SelectSingleNode("ApprTypeCode").InnerText.Trim());
                    //oNode.AppendChild(appTypeName);


                    // 결재종류 코드
                    // (string sPiSate, string sWiSate, string sSubKind, string sPiBzStat, string sMode)
                    string sTCode = GetApprTypeCode(oNode.SelectSingleNode("PI_STATE").InnerText,
                                                        oNode.SelectSingleNode("WI_STATE").InnerText,
                                                        oNode.SelectSingleNode("ApprTypeCode").InnerText,
                                                        oNode.SelectSingleNode("PI_BUSINESS_STATE").InnerText,
                                                        oNode.SelectSingleNode("MODE").InnerText);

                    oNode.SelectSingleNode("ApprTypeCode").InnerText = sTCode;
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("WI_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_BUSINESS_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("MODE"));

                    // 결재종류명
                    XmlElement appTypeName = oReturnXML.CreateElement("ApprTypeName");
                    appTypeName.InnerText = GetApprTypeNameByCode(sTCode);
                    oNode.AppendChild(appTypeName);

                    // 첨부파일 유무
                    XmlNodeList oAttFiles = null;
                    XmlElement attachFileCount = oReturnXML.CreateElement("AttachFileCount");

                    // 연결문서 확인
                    string strDocLinks = null;
                    XmlElement bodyHtml = oReturnXML.CreateElement("BodyHtml");

                    //@fmpf as varchar(64), -- 양식 영문명
                    //@fmrv as varchar(4), --양식 revision 
                    //@fiid as varchar(50) --양식 instance id 

                    DataSet oFInfo = null;
                    if (oFNode != null)
                    {

                        oFInfo = GetFormInfo(oFNode.Attributes["prefix"].Value,
                                                        oFNode.Attributes["revision"].Value,
                                                        oFNode.Attributes["instanceid"].Value);
                        #region URL
                        //sb.Append("http://").Append(Covi.Framework.ConfigurationManager.GetBaseConfig("WebServerDomain","0"));
                        sb.Append("/WebSite/Approval/Forms/Form.aspx?mobileyn=Y");
                        sb.Append("&mode=").Append("mobile");
                        sb.Append("&piid=").Append(ApprDocPID);
                        sb.Append("&wiid=").Append(ApprDocWID);
                        sb.Append("&pfid=").Append("");
                        sb.Append("&ptid=").Append("");
                        #endregion
                    }

                    if (oFInfo != null)
                    {
                        XmlDocument oFInfoXML = new XmlDocument();
                        oFInfoXML.LoadXml(oFInfo.GetXml());
                        XmlDocument oAttFileXML = new System.Xml.XmlDocument();

                        if (oFInfoXML.SelectSingleNode("RESPONSE/Table/ATTACH_FILE_INFO") != null)
                        {
                            string atchFileList = oFInfoXML.SelectSingleNode("RESPONSE/Table/ATTACH_FILE_INFO").InnerText.Trim();
                            if (atchFileList.Length > 0)
                            {
                                oAttFileXML.LoadXml(atchFileList);
                                oAttFiles = oAttFileXML.SelectNodes("fileinfos/fileinfo/file");
                                attachFileCount.InnerText = oAttFiles.Count.ToString();
                            }
                        }
                        else
                        {
                            attachFileCount.InnerText = "0";
                        }
                        XmlDocument oDocLinkXML = new System.Xml.XmlDocument();
                        if (oFInfoXML.SelectSingleNode("RESPONSE/Table/DOCLINKS") != null)
                        {
                            strDocLinks = oFInfoXML.SelectSingleNode("RESPONSE/Table/DOCLINKS").InnerText.Trim();
                        }

                        #region HTML
                        if (oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT") != null)
                        {
                            //XmlCDataSection CDataHTML;
                            //CDataHTML = oReturnXML.CreateCDataSection(oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT").InnerText);
                            //bodyHtml.AppendChild(CDataHTML);

                            string bodyContent = oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT").InnerText.Trim();
                            bodyHtml.InnerText = bodyContent;
                        }
                        else
                        {
                            bodyHtml.InnerText = "";
                        }
                        #endregion

                    }
                    else
                    {
                        attachFileCount.InnerText = "0";

                    }

                    oNode.AppendChild(attachFileCount);

                    // HTML
                    oNode.AppendChild(bodyHtml);
                    #endregion
                    #region 결재라인
                    string sSpName = "dbo.usp_wf_worklistquery01apprline";
                    DataSet oDSR = null;
                    INPUT.Clear();
                    using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                    {
                        INPUT.add("@ApprDocPID", ApprDocPID);
                        INPUT.add("@ApprDocWID", ApprDocWID);
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDSR.DataSetName = "DSApprLine";
                    }

                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        oDSR.Tables[0].TableName = "ApprovalLine";
                        if (oDSR.Tables.Count > 1)
                        {
                            oDSR.Tables[1].TableName = "CCLine";
                        }
                        XmlDocument oApprLineXML = new XmlDocument();
                        oApprLineXML.LoadXml(oDSR.GetXml());

                        XmlNode oApprLineRoot = oApprLineXML.DocumentElement;
                        XmlNodeList approvalList = oApprLineRoot.SelectNodes("ApprovalLine");
                        XmlElement approvalLine = oReturnXML.CreateElement("ApprovalLine");

                        foreach (System.Xml.XmlNode oApproval in approvalList)
                        {

                            XmlElement approval = oReturnXML.CreateElement("Approval");

                            //결재순서		C	4	0	ApprOrder	
                            XmlElement apprOrder = oReturnXML.CreateElement("ApprOrder");
                            apprOrder.InnerText = oApproval.SelectSingleNode("ApprOrder").InnerText;
                            approval.AppendChild(apprOrder);

                            //결재종류코드		C	3	4	ApprTypeCode
                            XmlElement apprTypeCode = oReturnXML.CreateElement("ApprTypeCode");
                            //apprTypeCode.InnerText = GetApprTypeCodeByRouteType(
                            //                            oApproval.SelectSingleNode("RouteType").InnerText,
                            //                            oApproval.SelectSingleNode("ApprTypeName").InnerText,
                            //                            oApproval.SelectSingleNode("Name").InnerText);
                            apprTypeCode.InnerText = oApproval.SelectSingleNode("ApprTypeName").InnerText;
                            approval.AppendChild(apprTypeCode);

                            //결재종류명		C	20	7	ApprTypeName
                            XmlElement apprTypeName = oReturnXML.CreateElement("ApprTypeName");
                            //apprTypeName.InnerText = GetApprTypeNameByCode(apprTypeCode.InnerText);
                            apprTypeName.InnerText = this.convertKindToSignTypeByRTnUT(apprTypeCode.InnerText, string.Empty, oApproval.SelectSingleNode("RouteType").InnerText, oApproval.SelectSingleNode("UnitType").InnerText, string.Empty);
                            approval.AppendChild(apprTypeName);


                            //결재상태코드		C	3	27	ApprYNCode
                            XmlElement apprYNCode = oReturnXML.CreateElement("ApprYNCode");
                            apprYNCode.InnerText = oApproval.SelectSingleNode("ApprYNCode").InnerText;
                            approval.AppendChild(apprYNCode);

                            //결재상태명		C	20	30	ApprYNName
                            XmlElement apprYNName = oReturnXML.CreateElement("ApprYNName");
                            apprYNName.InnerText = convertSignResult(apprYNCode.InnerText, apprTypeCode.InnerText, string.Empty);
                            approval.AppendChild(apprYNName);

                            //처리자 ID		C	30	50	ApproverLoginID	
                            XmlElement approverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                            approverLoginID.InnerText = oApproval.SelectSingleNode("ApproverLoginID").InnerText;
                            approval.AppendChild(approverLoginID);

                            //처리자명		C	40	80	ApproverName
                            XmlElement approverName = oReturnXML.CreateElement("ApproverName");
                            approverName.InnerText = oApproval.SelectSingleNode("ApproverName").InnerText;
                            approval.AppendChild(approverName);

                            //직위코드		C	50	120	PositionCode
                            XmlElement positionCode = oReturnXML.CreateElement("PositionCode");
                            if (oApproval.SelectSingleNode("PositionCode") != null)
                            {
                                positionCode.InnerText = oApproval.SelectSingleNode("PositionCode").InnerText;
                            }
                            approval.AppendChild(positionCode);

                            //직위명		C	100	170	PositionName
                            XmlElement positionName = oReturnXML.CreateElement("PositionName");
                            if (oApproval.SelectSingleNode("PositionName") != null)
                            {
                                positionName.InnerText = oApproval.SelectSingleNode("PositionName").InnerText;
                            }
                            approval.AppendChild(positionName);

                            //부서코드		C	50	270	ApprDprtCode
                            XmlElement apprDprtCode = oReturnXML.CreateElement("ApprDprtCode");
                            apprDprtCode.InnerText = oApproval.SelectSingleNode("ApprDprtCode").InnerText;
                            approval.AppendChild(apprDprtCode);

                            //부서명		C	100	320	ApprDprtName
                            XmlElement apprDprtName = oReturnXML.CreateElement("ApprDprtName");
                            apprDprtName.InnerText = oApproval.SelectSingleNode("ApprDprtName").InnerText;
                            approval.AppendChild(apprDprtName);

                            //승인일시		C	20	420	ApprDateTime
                            XmlElement apprDateTime = oReturnXML.CreateElement("ApprDateTime");
                            if (oApproval.SelectSingleNode("ApprDateTime") != null)
                            {
                                apprDateTime.InnerText = oApproval.SelectSingleNode("ApprDateTime").InnerText;
                            }
                            approval.AppendChild(apprDateTime);

                            //의견		C	200	440	Comment	
                            XmlElement comment = oReturnXML.CreateElement("Comment");
                            if (oApproval.SelectSingleNode("Comment") != null)
                            {
                                iCommentCount++;
                                comment.InnerText = oApproval.SelectSingleNode("Comment").InnerText;
                            }
                            approval.AppendChild(comment);

                            approvalLine.AppendChild(approval);


                            //모바일 승인 유무
                            XmlElement apprMobileYN = oReturnXML.CreateElement("MobileYN");
                            if (oApproval.SelectSingleNode("mobilegubun") != null)
                            {
                                apprMobileYN.InnerText = oApproval.SelectSingleNode("mobilegubun").InnerText;
                            }
                            approval.AppendChild(apprMobileYN);
                        }
                        oNode.AppendChild(approvalLine);

                        XmlNodeList CCList = oApprLineRoot.SelectNodes("CCLine");
                        XmlElement CCLine = oReturnXML.CreateElement("CCLine");

                        foreach (System.Xml.XmlNode occinfo in CCList)
                        {
                            XmlElement ccinfo = oReturnXML.CreateElement("ccinfo");

                            //처리자 ID		C	30	50	ApproverLoginID	
                            XmlElement approverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                            approverLoginID.InnerText = occinfo.SelectSingleNode("ApproverLoginID").InnerText;
                            ccinfo.AppendChild(approverLoginID);

                            //처리자명		C	40	80	ApproverName
                            XmlElement approverName = oReturnXML.CreateElement("ApproverName");
                            approverName.InnerText = occinfo.SelectSingleNode("ApproverName").InnerText;
                            ccinfo.AppendChild(approverName);

                            //직급코드		C	50	120	PositionCode
                            XmlElement positionCode = oReturnXML.CreateElement("PositionCode");
                            if (occinfo.SelectSingleNode("PositionCode") != null)
                            {
                                positionCode.InnerText = occinfo.SelectSingleNode("PositionCode").InnerText;
                            }
                            ccinfo.AppendChild(positionCode);

                            //직급명		C	100	170	PositionName
                            XmlElement positionName = oReturnXML.CreateElement("PositionName");
                            if (occinfo.SelectSingleNode("PositionName") != null)
                            {
                                positionName.InnerText = occinfo.SelectSingleNode("PositionName").InnerText;
                            }
                            ccinfo.AppendChild(positionName);

                            //부서코드		C	50	270	ApprDprtCode
                            XmlElement apprDprtCode = oReturnXML.CreateElement("ApprDprtCode");
                            apprDprtCode.InnerText = occinfo.SelectSingleNode("ApprDprtCode").InnerText;
                            ccinfo.AppendChild(apprDprtCode);

                            //부서명		C	100	320	ApprDprtName
                            XmlElement apprDprtName = oReturnXML.CreateElement("ApprDprtName");
                            apprDprtName.InnerText = occinfo.SelectSingleNode("ApprDprtName").InnerText;
                            ccinfo.AppendChild(apprDprtName);

                            CCLine.AppendChild(ccinfo);
                        }
                        oNode.AppendChild(CCLine);
                        oDSR.Dispose();

                    }
                    #endregion
                    #region 수신자참조라인

                    sSpName = "dbo.usp_wf_worklistquery01grpreceiptlistmobile";
                    oDSR = null;
                    INPUT.Clear();
                    using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                    {
                        INPUT.add("@ApprDocPID", ApprDocPID);
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDSR.DataSetName = "ReceiptList";
                    }

                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        XmlDocument oRListXML = new XmlDocument();
                        oRListXML.LoadXml(oDSR.GetXml());

                        XmlElement receiptList = oReturnXML.CreateElement("ReceiptList");
                        string sContent = string.Empty;

                        for (int i = 0; i < oDSR.Tables.Count; i++)
                        {
                            XmlElement receipt = oReturnXML.CreateElement("Receipt");

                            //결재순서		C	4	0	ApprOrder	
                            XmlElement rapprOrder = oReturnXML.CreateElement("ApprOrder");
                            rapprOrder.InnerText = oDSR.Tables[i].Rows[0]["ApprOrder"].ToString();
                            receipt.AppendChild(rapprOrder);

                            //결재종류코드		C	3	4	ApprTypeCode
                            XmlElement rapprTypeCode = oReturnXML.CreateElement("ApprTypeCode");
                            sContent = oDSR.Tables[i].Rows[0]["ApprTypeCode"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapprTypeCode.InnerText = sContent;
                            }
                            receipt.AppendChild(rapprTypeCode);

                            //결재종류명		C	20	7	ApprTypeName	
                            XmlElement rapprTypeName = oReturnXML.CreateElement("ApprTypeName");
                            sContent = oDSR.Tables[i].Rows[0]["ApprTypeName"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapprTypeName.InnerText = sContent;
                            }
                            receipt.AppendChild(rapprTypeName);

                            //처리자 ID		C	30	27	ApproverLoginID	
                            XmlElement rapproverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                            sContent = oDSR.Tables[i].Rows[0]["ApproverLoginID"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapproverLoginID.InnerText = sContent;
                            }
                            receipt.AppendChild(rapproverLoginID);

                            //처리자명		C	40	57	ApproverName
                            XmlElement rapproverName = oReturnXML.CreateElement("ApproverName");
                            sContent = oDSR.Tables[i].Rows[0]["ApproverName"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapproverName.InnerText = sContent;
                            }
                            receipt.AppendChild(rapproverName);

                            //직급코드		C	50	97	PositionCode
                            XmlElement rpositionCode = oReturnXML.CreateElement("PositionCode");
                            sContent = oDSR.Tables[i].Rows[0]["PositionCode"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rpositionCode.InnerText = sContent;
                            }
                            receipt.AppendChild(rpositionCode);

                            //직급명		C	100	147	PositionName
                            XmlElement rpositionName = oReturnXML.CreateElement("PositionName");
                            sContent = oDSR.Tables[i].Rows[0]["PositionName"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rpositionName.InnerText = sContent;
                            }
                            receipt.AppendChild(rpositionName);

                            //부서코드		C	50	247	RecpDprtCode
                            XmlElement rrecpDprtCode = oReturnXML.CreateElement("RecpDprtCode");
                            rrecpDprtCode.InnerText = oDSR.Tables[i].Rows[0]["RecpDprtCode"].ToString();
                            receipt.AppendChild(rrecpDprtCode);

                            //부서명		C	100	297	RecpDprtName	
                            XmlElement rrecpDprtName = oReturnXML.CreateElement("RecpDprtName");
                            rrecpDprtName.InnerText = oDSR.Tables[i].Rows[0]["RecpDprtName"].ToString();
                            receipt.AppendChild(rrecpDprtName);

                            receiptList.AppendChild(receipt);
                        }
                        oNode.AppendChild(receiptList);
                        oDSR.Dispose();
                    }

                    #endregion
                    #region 회람라인
                    DataSet oCirculationInfo = null;
                    if (oFNode != null)
                    {
                        oCirculationInfo = GetCirculationInfo(oFNode.Attributes["instanceid"].Value, ApprDocPID, "P");
                        if (oCirculationInfo != null)
                        {
                            XmlDocument oCirculationInfoXML = new XmlDocument();
                            oCirculationInfoXML.LoadXml(oCirculationInfo.GetXml());
                            XmlDocument oCirculationXML = new System.Xml.XmlDocument();
                            XmlNodeList oCLList = oCirculationInfoXML.SelectNodes("RESPONSE/Table");

                            XmlElement circulationList = oReturnXML.CreateElement("circulationLine");
                            foreach (XmlNode oCLNode in oCLList)
                            {
                                XmlElement circulation = oReturnXML.CreateElement("circulation");
                                //처리자 ID		C	30	50	ApproverLoginID	
                                XmlElement approverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                                approverLoginID.InnerText = oCLNode.SelectSingleNode("RECEIPT_ID").InnerText;
                                circulation.AppendChild(approverLoginID);

                                //처리자명		C	40	80	ApproverName
                                XmlElement approverName = oReturnXML.CreateElement("ApproverName");
                                approverName.InnerText = oCLNode.SelectSingleNode("RECEIPT_NAME").InnerText;
                                circulation.AppendChild(approverName);

                                //부서코드		C	50	270	ApprDprtCode
                                XmlElement apprDprtCode = oReturnXML.CreateElement("ApprDprtCode");
                                apprDprtCode.InnerText = oCLNode.SelectSingleNode("RECEIPT_OU_ID").InnerText;
                                circulation.AppendChild(apprDprtCode);

                                //부서명		C	100	320	ApprDprtName
                                XmlElement apprDprtName = oReturnXML.CreateElement("ApprDprtName");
                                apprDprtName.InnerText = oCLNode.SelectSingleNode("RECEIPT_OU_NAME").InnerText;
                                circulation.AppendChild(apprDprtName);

                                circulationList.AppendChild(circulation);
                            }
                            oNode.AppendChild(circulationList);
                        }
                    }
                    #endregion
                    #region 첨부파일 목록
                    XmlElement attachFileList = oReturnXML.CreateElement("AttachFileList");
                    if (oAttFiles != null)
                    {
                        foreach (System.Xml.XmlNode oAFileXML in oAttFiles)
                        {
                            XmlElement attachFile = oReturnXML.CreateElement("AttachFile");

                            if (oAFileXML != null)
                            {
                                // 첨부파일 ID                             
                                XmlElement attachFileID = oReturnXML.CreateElement("AttachFileID");
                                attachFileID.InnerText = oAFileXML.Attributes["location"].Value.Substring(oAFileXML.Attributes["location"].Value.LastIndexOf("/") + 1);
                                attachFile.AppendChild(attachFileID);

                                // 첨부파일 제목                           
                                XmlElement attachFileTitle = oReturnXML.CreateElement("AttachFileTitle");
                                attachFileTitle.InnerText = oAFileXML.Attributes["name"].Value;
                                attachFile.AppendChild(attachFileTitle);

                                // 첨부파일 사이즈                          
                                XmlElement attachFileSize = oReturnXML.CreateElement("AttachFileSize");
                                attachFileSize.InnerText = oAFileXML.Attributes["size"] != null ? oAFileXML.Attributes["size"].Value : "";
                                attachFile.AppendChild(attachFileSize);

                                // 첨부파일 종류                         
                                XmlElement attachFileType = oReturnXML.CreateElement("AttachFileType");
                                attachFileType.InnerText = oAFileXML.Attributes["name"].Value.Substring(oAFileXML.Attributes["name"].Value.LastIndexOf(".") + 1);
                                attachFile.AppendChild(attachFileType);

                                // 첨부파일 다운로드 경로                            
                                XmlElement attachFileURL = oReturnXML.CreateElement("AttachFileURL");
                                //attachFileURL.InnerText = "http://" + ConfigurationManager.AppSettings["LinKURL"].ToString() + oAFileXML.Attributes["location"].Value;
                                attachFileURL.InnerText = oAFileXML.Attributes["location"].Value;
                                attachFile.AppendChild(attachFileURL);

                                attachFileList.AppendChild(attachFile);
                            }
                        }
                    }
                    oNode.AppendChild(attachFileList);
                    #endregion

                    #region 연결문서 추가
                    XmlElement docLinkList = oReturnXML.CreateElement("DocLinks");
                    docLinkList.InnerText = strDocLinks;
                    oNode.AppendChild(docLinkList);
                    #endregion
                    #region 결재문서URL
                    XmlElement ReturnURL = oReturnXML.CreateElement("ReturnURL");
                    XmlElement ApprDocURL = oReturnXML.CreateElement("ApprDocURL");
                    szURL = sb.ToString();
                    sb = null;
                    XmlCDataSection CData;
                    CData = oReturnXML.CreateCDataSection(szURL);
                    ApprDocURL.AppendChild(CData);
                    //ApprDocURL.InnerText = szURL;
                    ReturnURL.AppendChild(ApprDocURL);
                    oNode.AppendChild(ReturnURL);
                    #endregion                }


                    XmlElement commentCount = oReturnXML.CreateElement("CommentCount");
                    commentCount.InnerText = iCommentCount.ToString();
                    oNode.AppendChild(commentCount);


                    #region 모바일 추가 - 결재선 변경건
                    // 결재선 정보 Xml
                    XmlElement appLineXml = oReturnXML.CreateElement("ApprovalLineXml");
                    appLineXml.InnerText = getProcessDomainData(ApprDocPID);
                    oNode.AppendChild(appLineXml);

                    sSpName = "dbo.usp_wf_get_workitem_list";
                    oDSR = null;
                    INPUT.Clear();
                    if (ApprDocWID == "") ApprDocWID = oNode.SelectSingleNode("ApprDocWID").InnerText;

                    using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                    {
                        INPUT.add("@WORKITME_ID", ApprDocWID);

                        //[2014-12-05 modi]
                        //INPUT.add("@SELECTNODE", "PI_STATE,WI_STATE,WI_DEPUTY_ID,PF_PERFORMER_ID,PI_BUSINESS_STATE");
                        INPUT.add("@SELECTNODE", " '528' as PI_STATE, '528' as WI_STATE, w.DEPUTY_ID, w.PERFORMER_ID, w.PI_BUSINESS_STATE ");
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    }
                    string strPiState = string.Empty;
                    string strWiState = string.Empty;
                    string strWiDeputyId = string.Empty;
                    string strPfPerformerId = string.Empty;
                    string strMode = string.Empty;
                    string strBusinessState = "";

                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        strPiState = oDSR.Tables[0].Rows[0]["PI_STATE"].ToString();
                        strWiState = oDSR.Tables[0].Rows[0]["WI_STATE"].ToString();
                        strWiDeputyId = oDSR.Tables[0].Rows[0]["WI_DEPUTY_ID"].ToString();
                        strPfPerformerId = oDSR.Tables[0].Rows[0]["PF_PERFORMER_ID"].ToString();
                        strBusinessState = oDSR.Tables[0].Rows[0]["PI_BUSINESS_STATE"].ToString();
                    }

                    // Mode 정보 가져오기
                    if (strPiState == "288")
                    {
                        if (strWiState == "288") strMode = "APPROVAL";
                        else strMode = "PROCESS";
                    }
                    else if (strPiState == "528") strMode = "COMPLETE";

                    strMode = Covi.BizService.ApprovalService.pGetReadMode(strBusinessState, strMode);


                    // 현 미결함 원결재자 정보
                    XmlElement appDocPtID = oReturnXML.CreateElement("ApprDocPtID");
                    appDocPtID.InnerText = strPfPerformerId;
                    oNode.AppendChild(appDocPtID);

                    // 현 미결함 대결재자 정보
                    XmlElement appDocDeputyID = oReturnXML.CreateElement("ApprDocDeputyID");
                    appDocDeputyID.InnerText = strWiDeputyId;
                    oNode.AppendChild(appDocDeputyID);

                    // 스키마



                    XmlElement schemaId = oReturnXML.CreateElement("SchemaId");
                    XmlElement scPAgr = oReturnXML.CreateElement("scPAgr");
                    XmlElement scCC = oReturnXML.CreateElement("scCC");

                    schemaId.InnerText = oFNode.Attributes["schemaid"].Value;

                    sSpName = "dbo.usp_wfform_getSchemaKeyValue";
                    oDSR = null;
                    INPUT.Clear();
                    using (SqlDacManager oDac = new SqlDacManager("FORM_DEF_ConnectionString"))
                    {
                        INPUT.add("@SCID", oFNode.Attributes["schemaid"].Value);
                        INPUT.add("@KEY", "scPAgr");
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    }
                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        scPAgr.InnerText = oDSR.Tables[0].Rows[0]["CHECKED"].ToString();
                    }

                    sSpName = "dbo.usp_wfform_getSchemaKeyValue";
                    oDSR = null;
                    INPUT.Clear();
                    using (SqlDacManager oDac = new SqlDacManager("FORM_DEF_ConnectionString"))
                    {
                        INPUT.add("@SCID", oFNode.Attributes["schemaid"].Value);
                        INPUT.add("@KEY", "scCC");
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    }
                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        scCC.InnerText = oDSR.Tables[0].Rows[0]["CHECKED"].ToString();
                    }

                    XmlElement schema = oReturnXML.CreateElement("Schema");
                    schema.AppendChild(schemaId);
                    schema.AppendChild(scPAgr);
                    schema.AppendChild(scCC);
                    oNode.AppendChild(schema);

                    // Mode
                    XmlElement appMode = oReturnXML.CreateElement("Mode");
                    appMode.InnerText = strMode;
                    oNode.AppendChild(appMode);
                    #endregion


                }
            }
            return oReturnXML.OuterXml;
        }
        catch (Exception ex)
        {
            //ExceptionHandler(ex, WOORI.IGW.Framework.SystemOption.APV);
            return "<WebService><LIST><Error><![CDATA[" + ex.Message + "]]></Error></LIST></WebService>";
        }
        finally
        {
            oDS.Dispose();
        }
    }

    #endregion

    #region //--> New MobileGetApprDocDetail
    [WebMethod]
    public ReturnType_MobileGetApprDocDetail MobileGetApprDocDetail_New(string ApprDocPID, string ApprDocWID, string ApprDocTypeCode)
    {
        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        DateTime dtStart = DateTime.MinValue;
        DateTime dtEnd = DateTime.MaxValue;
        String szURL = String.Empty;
        StringBuilder sb = new StringBuilder();

        // NewType
        ReturnType_MobileGetApprDocDetail retVal = new ReturnType_MobileGetApprDocDetail();
        try
        {

            if (ApprDocPID != string.Empty)//&& ApprDocWID != string.Empty
            {
                ApprDocPID = ApprDocPID.Trim();
                ApprDocWID = ApprDocWID.Trim();

                string sSpName = "dbo.usp_wf_worklistquery01apprdocdetailmobile";
                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@ApprDocPID", ApprDocPID);
                    INPUT.add("@ApprDocWID", ApprDocWID);
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    oDS.DataSetName = "RESPONSE";
                }
                if (oDS != null && oDS.Tables[0].Rows.Count == 0)
                {
                    using (SqlDacManager oDac = new SqlDacManager("INST_ARCHIVE_ConnectionString"))
                    {
                        oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDS.DataSetName = "RESPONSE";
                    }
                }
            }

            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                // NewType 
                retVal.Doc = new ReturnType_MobileGetApprDocDetail_Doc();

                for (int i = 0; i < oDS.Tables[0].Rows.Count; i++)
                {
                    retVal.Doc.RowNum = oDS.Tables[0].Rows[i]["rownum"].ToString();
                    retVal.Doc.ApprDocPID = oDS.Tables[0].Rows[i]["ApprDocPID"].ToString();
                    retVal.Doc.ApprDocWID = oDS.Tables[0].Rows[i]["ApprDocWID"].ToString();
                    retVal.Doc.DrafterLoginID = oDS.Tables[0].Rows[i]["DrafterLoginID"].ToString();
                    retVal.Doc.DrafterName = oDS.Tables[0].Rows[i]["DrafterName"].ToString();
                    retVal.Doc.ApprLoginID = oDS.Tables[0].Rows[i]["ApprLoginID"].ToString();
                    retVal.Doc.ApprName = oDS.Tables[0].Rows[i]["ApprName"].ToString();
                    retVal.Doc.PositionCode = oDS.Tables[0].Rows[i]["PositionCode"].ToString();
                    retVal.Doc.PositionName = oDS.Tables[0].Rows[i]["PositionName"].ToString();
                    retVal.Doc.Mobile = oDS.Tables[0].Rows[i]["Mobile"].ToString();
                    retVal.Doc.DraftDprtCode = oDS.Tables[0].Rows[i]["DraftDprtCode"].ToString();
                    retVal.Doc.DraftDprtName = oDS.Tables[0].Rows[i]["DraftDprtName"].ToString();
                    retVal.Doc.ApprTypeCode = oDS.Tables[0].Rows[i]["ApprTypeCode"].ToString();
                    retVal.Doc.ApprDocTitle = oDS.Tables[0].Rows[i]["ApprDocTitle"].ToString();
                    retVal.Doc.DraftDateTime = oDS.Tables[0].Rows[i]["DraftDateTime"].ToString();
                }

                oDS.Tables[0].TableName = "DOC";
                oReturnXML.LoadXml(oDS.GetXml());

                //변환하기
                XmlNode oRoot = oReturnXML.DocumentElement;
                XmlNodeList oList = oRoot.SelectNodes("DOC");
                foreach (System.Xml.XmlNode oNode in oList)
                {
                    #region 기안정보

                    // 문서종류코드, 문서종류명, 첨부파일 유무	
                    XmlDocument oXML = new System.Xml.XmlDocument();
                    XmlNode oFNode = null;
                    if (oNode.SelectSingleNode("PI_DSCR").InnerText != null &&
                            oNode.SelectSingleNode("PI_DSCR").InnerText.Trim().Length > 0)
                    {
                        oXML.LoadXml(oNode.SelectSingleNode("PI_DSCR").InnerText);
                        oFNode = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                    }
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_DSCR"));

                    // 문서종류코드
                    XmlElement prefix = oReturnXML.CreateElement("ApprDocTypeCode");
                    if (oFNode != null)
                    {
                        prefix.InnerText = oFNode.Attributes["prefix"].Value;
                        retVal.Doc.ApprDocTypeCode = oFNode.Attributes["prefix"].Value;
                    }
                    oNode.AppendChild(prefix);

                    // 문서종류명
                    XmlElement fName = oReturnXML.CreateElement("ApprDocTypeName");
                    if (oFNode != null)
                    {
                        fName.InnerText = oFNode.Attributes["name"].Value;
                        retVal.Doc.ApprDocTypeName = oFNode.Attributes["name"].Value;
                    }
                    oNode.AppendChild(fName);

                    //// 결재종류 코드
                    //oNode.SelectSingleNode("ApprTypeCode").InnerText
                    //    = ApprTypeToCode(oNode.SelectSingleNode("ApprTypeCode").InnerText.Trim());

                    //// 결재종류명
                    //XmlElement appTypeName = oReturnXML.CreateElement("ApprTypeName");
                    //appTypeName.InnerText = ApprTypeToName(oNode.SelectSingleNode("ApprTypeCode").InnerText.Trim());
                    //oNode.AppendChild(appTypeName);


                    // 결재종류 코드
                    // (string sPiSate, string sWiSate, string sSubKind, string sPiBzStat, string sMode)
                    string sTCode = GetApprTypeCode(oNode.SelectSingleNode("PI_STATE").InnerText,
                                                        oNode.SelectSingleNode("WI_STATE").InnerText,
                                                        oNode.SelectSingleNode("ApprTypeCode").InnerText,
                                                        oNode.SelectSingleNode("PI_BUSINESS_STATE").InnerText,
                                                        oNode.SelectSingleNode("MODE").InnerText);

                    oNode.SelectSingleNode("ApprTypeCode").InnerText = sTCode;
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("WI_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_BUSINESS_STATE"));
                    oNode.RemoveChild(oNode.SelectSingleNode("MODE"));

                    // 결재종류명
                    XmlElement appTypeName = oReturnXML.CreateElement("ApprTypeName");
                    appTypeName.InnerText = GetApprTypeNameByCode(sTCode);
                    oNode.AppendChild(appTypeName);

                    retVal.Doc.ApprTypeName = GetApprTypeNameByCode(sTCode);

                    // 첨부파일 유무
                    XmlNodeList oAttFiles = null;
                    XmlElement attachFileCount = oReturnXML.CreateElement("AttachFileCount");
                    XmlElement bodyHtml = oReturnXML.CreateElement("BodyHtml");

                    //@fmpf as varchar(64), -- 양식 영문명
                    //@fmrv as varchar(4), --양식 revision 
                    //@fiid as varchar(50) --양식 instance id 

                    DataSet oFInfo = null;
                    if (oFNode != null)
                    {

                        oFInfo = GetFormInfo(oFNode.Attributes["prefix"].Value,
                                                        oFNode.Attributes["revision"].Value,
                                                        oFNode.Attributes["instanceid"].Value);
                        #region URL
                        //sb.Append("http://").Append(ConfigurationManager.AppSettings["LinKURL"].ToString());
                        sb.Append("/WebSite/Approval/Forms/Form.aspx?mobileyn=Y");
                        sb.Append("&piid=").Append(ApprDocPID);
                        sb.Append("&wiid=").Append(ApprDocWID);
                        sb.Append("&fmpf=").Append(oFNode.Attributes["prefix"].Value);
                        sb.Append("&fmrv=").Append(oFNode.Attributes["revision"].Value);
                        sb.Append("&fiid=").Append(oFNode.Attributes["instanceid"].Value);
                        sb.Append("&fmid=").Append(oFNode.Attributes["id"].Value);
                        sb.Append("&scid=").Append(oFNode.Attributes["schemaid"].Value);
                        #endregion
                    }

                    #region //--> File Count
                    if (oFInfo != null)
                    {
                        XmlDocument oFInfoXML = new XmlDocument();
                        oFInfoXML.LoadXml(oFInfo.GetXml());
                        XmlDocument oAttFileXML = new System.Xml.XmlDocument();

                        if (oFInfoXML.SelectSingleNode("RESPONSE/Table/ATTACH_FILE_INFO") != null)
                        {
                            string atchFileList = oFInfoXML.SelectSingleNode("RESPONSE/Table/ATTACH_FILE_INFO").InnerText.Trim();
                            if (atchFileList.Length > 0)
                            {
                                oAttFileXML.LoadXml(atchFileList);
                                oAttFiles = oAttFileXML.SelectNodes("fileinfos/fileinfo/file");
                                attachFileCount.InnerText = oAttFiles.Count.ToString();

                                retVal.Doc.AttachFileCount = oAttFiles.Count.ToString();
                            }
                        }
                        else
                        {
                            attachFileCount.InnerText = "0";
                            retVal.Doc.AttachFileCount = "0";
                        }

                        #region HTML
                        if (oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT") != null)
                        {
                            //XmlCDataSection CDataHTML;
                            //CDataHTML = oReturnXML.CreateCDataSection(oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT").InnerText);
                            //bodyHtml.AppendChild(CDataHTML);

                            string bodyContent = oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT").InnerText.Trim();
                            bodyHtml.InnerText = bodyContent;

                            retVal.Doc.BodyHtml = bodyContent;
                        }
                        else
                        {
                            bodyHtml.InnerText = "";
                            retVal.Doc.BodyHtml = "";
                        }
                        #endregion

                    }
                    else
                    {
                        attachFileCount.InnerText = "0";

                    }
                    #endregion

                    oNode.AppendChild(attachFileCount);

                    // Reply 개수, 수정 필요. 유대리님
                    XmlElement replyCount = oReturnXML.CreateElement("ReplyCount");
                    replyCount.InnerText = "0";
                    oNode.AppendChild(replyCount);

                    // PrevApprDocPID, 수정 필요, 윤대리님
                    XmlElement prevApprDocPID = oReturnXML.CreateElement("PrevApprDocPID");
                    prevApprDocPID.InnerText = "00000000-0000-0000-0000-000000000000";
                    oNode.AppendChild(prevApprDocPID);

                    // PrevApprDocWID, 수정 필요, 윤대리님
                    XmlElement prevApprDocWID = oReturnXML.CreateElement("PrevApprDocWID");
                    prevApprDocWID.InnerText = "00000000-0000-0000-0000-000000000000";
                    oNode.AppendChild(prevApprDocWID);

                    // NextApprDocPID, 수정 필요, 윤대리님
                    XmlElement NextApprDocPID = oReturnXML.CreateElement("NextApprDocPID");
                    NextApprDocPID.InnerText = "00000000-0000-0000-0000-000000000000";
                    oNode.AppendChild(NextApprDocPID);

                    // NextApprDocWID, 수정 필요, 윤대리님
                    XmlElement NextApprDocWID = oReturnXML.CreateElement("NextApprDocWID");
                    NextApprDocWID.InnerText = "00000000-0000-0000-0000-000000000000";
                    oNode.AppendChild(NextApprDocWID);

                    // HTML
                    oNode.AppendChild(bodyHtml);
                    #endregion

                    #region 결재라인
                    string sSpName = "dbo.usp_wf_worklistquery01apprline";
                    DataSet oDSR = null;
                    INPUT.Clear();
                    using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                    {
                        INPUT.add("@ApprDocPID", ApprDocPID);
                        INPUT.add("@ApprDocWID", ApprDocWID);
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDSR.DataSetName = "DSApprLine";
                    }

                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        // NewType
                        retVal.Doc.ApprovalLine = new ReturnType_MobileGetApprDocDetail_Approval[oDSR.Tables[0].Rows.Count];

                        oDSR.Tables[0].TableName = "ApprovalLine";
                        if (oDSR.Tables.Count > 1)
                        {
                            oDSR.Tables[1].TableName = "CCLine";
                            retVal.Doc.CCLine = new ReturnType_MobileGetApprDocDetail_CcInfo[oDSR.Tables[1].Rows.Count];
                        }
                        XmlDocument oApprLineXML = new XmlDocument();
                        oApprLineXML.LoadXml(oDSR.GetXml());

                        XmlNode oApprLineRoot = oApprLineXML.DocumentElement;
                        XmlNodeList approvalList = oApprLineRoot.SelectNodes("ApprovalLine");
                        XmlElement approvalLine = oReturnXML.CreateElement("ApprovalLine");

                        int rCnt = 0;
                        foreach (System.Xml.XmlNode oApproval in approvalList)
                        {
                            // NewType
                            retVal.Doc.ApprovalLine[rCnt] = new ReturnType_MobileGetApprDocDetail_Approval();

                            XmlElement approval = oReturnXML.CreateElement("Approval");

                            //결재순서		C	4	0	ApprOrder	
                            XmlElement apprOrder = oReturnXML.CreateElement("ApprOrder");
                            apprOrder.InnerText = oApproval.SelectSingleNode("ApprOrder").InnerText;
                            approval.AppendChild(apprOrder);

                            //결재종류코드		C	3	4	ApprTypeCode
                            XmlElement apprTypeCode = oReturnXML.CreateElement("ApprTypeCode");
                            //apprTypeCode.InnerText = GetApprTypeCodeByRouteType(
                            //                            oApproval.SelectSingleNode("RouteType").InnerText,
                            //                            oApproval.SelectSingleNode("ApprTypeName").InnerText,
                            //                            oApproval.SelectSingleNode("Name").InnerText);
                            apprTypeCode.InnerText = oApproval.SelectSingleNode("ApprTypeName").InnerText;
                            approval.AppendChild(apprTypeCode);

                            //결재종류명		C	20	7	ApprTypeName
                            XmlElement apprTypeName = oReturnXML.CreateElement("ApprTypeName");
                            //apprTypeName.InnerText = GetApprTypeNameByCode(apprTypeCode.InnerText);
                            apprTypeName.InnerText = this.convertKindToSignTypeByRTnUT(apprTypeCode.InnerText, string.Empty, oApproval.SelectSingleNode("RouteType").InnerText, oApproval.SelectSingleNode("UnitType").InnerText, string.Empty);
                            approval.AppendChild(apprTypeName);


                            //결재상태코드		C	3	27	ApprYNCode
                            XmlElement apprYNCode = oReturnXML.CreateElement("ApprYNCode");
                            apprYNCode.InnerText = oApproval.SelectSingleNode("ApprYNCode").InnerText;
                            approval.AppendChild(apprYNCode);

                            //결재상태명		C	20	30	ApprYNName
                            XmlElement apprYNName = oReturnXML.CreateElement("ApprYNName");
                            apprYNName.InnerText = convertSignResult(apprYNCode.InnerText, apprTypeCode.InnerText, string.Empty);
                            approval.AppendChild(apprYNName);

                            //처리자 ID		C	30	50	ApproverLoginID	
                            XmlElement approverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                            approverLoginID.InnerText = oApproval.SelectSingleNode("ApproverLoginID").InnerText;
                            approval.AppendChild(approverLoginID);

                            //처리자명		C	40	80	ApproverName
                            XmlElement approverName = oReturnXML.CreateElement("ApproverName");
                            approverName.InnerText = oApproval.SelectSingleNode("ApproverName").InnerText;
                            approval.AppendChild(approverName);

                            //직급코드		C	50	120	PositionCode
                            XmlElement positionCode = oReturnXML.CreateElement("PositionCode");
                            if (oApproval.SelectSingleNode("PositionCode") != null)
                            {
                                positionCode.InnerText = oApproval.SelectSingleNode("PositionCode").InnerText;
                            }
                            approval.AppendChild(positionCode);

                            //직급명		C	100	170	PositionName
                            XmlElement positionName = oReturnXML.CreateElement("PositionName");
                            if (oApproval.SelectSingleNode("PositionName") != null)
                            {
                                positionName.InnerText = oApproval.SelectSingleNode("PositionName").InnerText;
                            }
                            approval.AppendChild(positionName);

                            //부서코드		C	50	270	ApprDprtCode
                            XmlElement apprDprtCode = oReturnXML.CreateElement("ApprDprtCode");
                            apprDprtCode.InnerText = oApproval.SelectSingleNode("ApprDprtCode").InnerText;
                            approval.AppendChild(apprDprtCode);

                            //부서명		C	100	320	ApprDprtName
                            XmlElement apprDprtName = oReturnXML.CreateElement("ApprDprtName");
                            apprDprtName.InnerText = oApproval.SelectSingleNode("ApprDprtName").InnerText;
                            approval.AppendChild(apprDprtName);

                            //승인일시		C	20	420	ApprDateTime
                            XmlElement apprDateTime = oReturnXML.CreateElement("ApprDateTime");
                            if (oApproval.SelectSingleNode("ApprDateTime") != null)
                            {
                                apprDateTime.InnerText = oApproval.SelectSingleNode("ApprDateTime").InnerText;
                            }
                            approval.AppendChild(apprDateTime);

                            //의견		C	200	440	Comment	
                            XmlElement comment = oReturnXML.CreateElement("Comment");
                            if (oApproval.SelectSingleNode("Comment") != null)
                            {
                                comment.InnerText = oApproval.SelectSingleNode("Comment").InnerText;
                            }
                            approval.AppendChild(comment);

                            approvalLine.AppendChild(approval);

                            // NewType
                            retVal.Doc.ApprovalLine[rCnt].ApprOrder = oApproval.SelectSingleNode("ApprOrder").InnerText;
                            retVal.Doc.ApprovalLine[rCnt].ApprTypeCode = apprTypeCode.InnerText;
                            retVal.Doc.ApprovalLine[rCnt].ApprTypeName = apprTypeName.InnerText;
                            retVal.Doc.ApprovalLine[rCnt].ApprYNCode = oApproval.SelectSingleNode("ApprYNCode").InnerText; ;
                            retVal.Doc.ApprovalLine[rCnt].ApprYNName = apprYNName.InnerText;
                            retVal.Doc.ApprovalLine[rCnt].ApproverLoginID = oApproval.SelectSingleNode("ApproverLoginID").InnerText;
                            retVal.Doc.ApprovalLine[rCnt].ApproverName = oApproval.SelectSingleNode("ApproverName").InnerText;
                            retVal.Doc.ApprovalLine[rCnt].PositionCode = oApproval.SelectSingleNode("PositionCode").InnerText;
                            retVal.Doc.ApprovalLine[rCnt].PositionName = oApproval.SelectSingleNode("PositionName").InnerText;
                            retVal.Doc.ApprovalLine[rCnt].ApprDprtCode = oApproval.SelectSingleNode("ApprDprtCode").InnerText;
                            retVal.Doc.ApprovalLine[rCnt].ApprDprtName = oApproval.SelectSingleNode("ApprDprtName").InnerText;
                            if (oApproval.SelectSingleNode("ApprDateTime") != null)
                            {
                                retVal.Doc.ApprovalLine[rCnt].ApprDateTime = oApproval.SelectSingleNode("ApprDateTime").InnerText;
                            }
                            if (oApproval.SelectSingleNode("Comment") != null)
                            {
                                retVal.Doc.ApprovalLine[rCnt].Comment = oApproval.SelectSingleNode("Comment").InnerText;
                            }

                            rCnt++;
                        }
                        oNode.AppendChild(approvalLine);

                        XmlNodeList CCList = oApprLineRoot.SelectNodes("CCLine");
                        XmlElement CCLine = oReturnXML.CreateElement("CCLine");

                        rCnt = 0;
                        foreach (System.Xml.XmlNode occinfo in CCList)
                        {
                            // NewType 
                            retVal.Doc.CCLine[rCnt] = new ReturnType_MobileGetApprDocDetail_CcInfo();

                            XmlElement ccinfo = oReturnXML.CreateElement("ccinfo");

                            //처리자 ID		C	30	50	ApproverLoginID	
                            XmlElement approverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                            approverLoginID.InnerText = occinfo.SelectSingleNode("ApproverLoginID").InnerText;
                            ccinfo.AppendChild(approverLoginID);

                            //처리자명		C	40	80	ApproverName
                            XmlElement approverName = oReturnXML.CreateElement("ApproverName");
                            approverName.InnerText = occinfo.SelectSingleNode("ApproverName").InnerText;
                            ccinfo.AppendChild(approverName);

                            //직급코드		C	50	120	PositionCode
                            XmlElement positionCode = oReturnXML.CreateElement("PositionCode");
                            if (occinfo.SelectSingleNode("PositionCode") != null)
                            {
                                positionCode.InnerText = occinfo.SelectSingleNode("PositionCode").InnerText;
                            }
                            ccinfo.AppendChild(positionCode);

                            //직급명		C	100	170	PositionName
                            XmlElement positionName = oReturnXML.CreateElement("PositionName");
                            if (occinfo.SelectSingleNode("PositionName") != null)
                            {
                                positionName.InnerText = occinfo.SelectSingleNode("PositionName").InnerText;
                            }
                            ccinfo.AppendChild(positionName);

                            //부서코드		C	50	270	ApprDprtCode
                            XmlElement apprDprtCode = oReturnXML.CreateElement("ApprDprtCode");
                            apprDprtCode.InnerText = occinfo.SelectSingleNode("ApprDprtCode").InnerText;
                            ccinfo.AppendChild(apprDprtCode);

                            //부서명		C	100	320	ApprDprtName
                            XmlElement apprDprtName = oReturnXML.CreateElement("ApprDprtName");
                            apprDprtName.InnerText = occinfo.SelectSingleNode("ApprDprtName").InnerText;
                            ccinfo.AppendChild(apprDprtName);

                            CCLine.AppendChild(ccinfo);

                            // NewType
                            retVal.Doc.CCLine[rCnt].ApproverLoginID = occinfo.SelectSingleNode("ApproverLoginID").InnerText;
                            retVal.Doc.CCLine[rCnt].ApproverName = occinfo.SelectSingleNode("ApproverName").InnerText;
                            retVal.Doc.CCLine[rCnt].PositionCode = occinfo.SelectSingleNode("PositionCode").InnerText;
                            retVal.Doc.CCLine[rCnt].PositionName = occinfo.SelectSingleNode("PositionName").InnerText;
                            retVal.Doc.CCLine[rCnt].ApprDprtCode = occinfo.SelectSingleNode("ApprDprtCode").InnerText;
                            retVal.Doc.CCLine[rCnt].ApprDprtName = occinfo.SelectSingleNode("ApprDprtName").InnerText;

                            rCnt++;
                        }
                        oNode.AppendChild(CCLine);
                        oDSR.Dispose();

                    }
                    #endregion

                    // 문서에는 없는 부분이라서 보류(2011.03.07)
                    #region 수신자참조라인

                    sSpName = "dbo.usp_wf_worklistquery01grpreceiptlistmobile";
                    oDSR = null;
                    INPUT.Clear();
                    using (SqlDacManager oDac = new SqlDacManager())
                    {
                        INPUT.add("@ApprDocPID", ApprDocPID);
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDSR.DataSetName = "ReceiptList";
                    }

                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        XmlDocument oRListXML = new XmlDocument();
                        oRListXML.LoadXml(oDSR.GetXml());

                        XmlElement receiptList = oReturnXML.CreateElement("ReceiptList");
                        string sContent = string.Empty;

                        // NewType 
                        retVal.Doc.ReceiptList = new ReturnType_MobileGetApprDocDetail_Receipt[oDSR.Tables.Count];
                        for (int i = 0; i < oDSR.Tables.Count; i++)
                        {
                            XmlElement receipt = oReturnXML.CreateElement("Receipt");

                            //결재순서		C	4	0	ApprOrder	
                            XmlElement rapprOrder = oReturnXML.CreateElement("ApprOrder");
                            rapprOrder.InnerText = oDSR.Tables[i].Rows[0]["ApprOrder"].ToString();
                            receipt.AppendChild(rapprOrder);

                            //결재종류코드		C	3	4	ApprTypeCode
                            XmlElement rapprTypeCode = oReturnXML.CreateElement("ApprTypeCode");
                            sContent = oDSR.Tables[i].Rows[0]["ApprTypeCode"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapprTypeCode.InnerText = sContent;
                            }
                            receipt.AppendChild(rapprTypeCode);

                            //결재종류명		C	20	7	ApprTypeName	
                            XmlElement rapprTypeName = oReturnXML.CreateElement("ApprTypeName");
                            sContent = oDSR.Tables[i].Rows[0]["ApprTypeName"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapprTypeName.InnerText = sContent;
                            }
                            receipt.AppendChild(rapprTypeName);

                            //처리자 ID		C	30	27	ApproverLoginID	
                            XmlElement rapproverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                            sContent = oDSR.Tables[i].Rows[0]["ApproverLoginID"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapproverLoginID.InnerText = sContent;
                            }
                            receipt.AppendChild(rapproverLoginID);

                            //처리자명		C	40	57	ApproverName
                            XmlElement rapproverName = oReturnXML.CreateElement("ApproverName");
                            sContent = oDSR.Tables[i].Rows[0]["ApproverName"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rapproverName.InnerText = sContent;
                            }
                            receipt.AppendChild(rapproverName);

                            //직급코드		C	50	97	PositionCode
                            XmlElement rpositionCode = oReturnXML.CreateElement("PositionCode");
                            sContent = oDSR.Tables[i].Rows[0]["PositionCode"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rpositionCode.InnerText = sContent;
                            }
                            receipt.AppendChild(rpositionCode);

                            //직급명		C	100	147	PositionName
                            XmlElement rpositionName = oReturnXML.CreateElement("PositionName");
                            sContent = oDSR.Tables[i].Rows[0]["PositionName"].ToString().Trim();
                            if (sContent.Length > 0)
                            {
                                rpositionName.InnerText = sContent;
                            }
                            receipt.AppendChild(rpositionName);

                            //부서코드		C	50	247	RecpDprtCode
                            XmlElement rrecpDprtCode = oReturnXML.CreateElement("RecpDprtCode");
                            rrecpDprtCode.InnerText = oDSR.Tables[i].Rows[0]["RecpDprtCode"].ToString();
                            receipt.AppendChild(rrecpDprtCode);

                            //부서명		C	100	297	RecpDprtName	
                            XmlElement rrecpDprtName = oReturnXML.CreateElement("RecpDprtName");
                            rrecpDprtName.InnerText = oDSR.Tables[i].Rows[0]["RecpDprtName"].ToString();
                            receipt.AppendChild(rrecpDprtName);

                            receiptList.AppendChild(receipt);

                            // NewType
                            retVal.Doc.ReceiptList[i] = new ReturnType_MobileGetApprDocDetail_Receipt();
                            retVal.Doc.ReceiptList[i].ApprOrder = oDSR.Tables[i].Rows[0]["ApprOrder"].ToString();
                            retVal.Doc.ReceiptList[i].ApprTypeCode = oDSR.Tables[i].Rows[0]["ApprTypeCode"].ToString().Trim();
                            retVal.Doc.ReceiptList[i].ApprTypeName = oDSR.Tables[i].Rows[0]["ApprTypeName"].ToString().Trim();
                            retVal.Doc.ReceiptList[i].ApproverLoginID = oDSR.Tables[i].Rows[0]["ApproverLoginID"].ToString().Trim();
                            retVal.Doc.ReceiptList[i].ApproverName = oDSR.Tables[i].Rows[0]["ApproverName"].ToString().Trim();
                            retVal.Doc.ReceiptList[i].PositionCode = oDSR.Tables[i].Rows[0]["PositionCode"].ToString().Trim();
                            retVal.Doc.ReceiptList[i].PositionName = oDSR.Tables[i].Rows[0]["PositionName"].ToString().Trim();
                            retVal.Doc.ReceiptList[i].RecpDprtCode = oDSR.Tables[i].Rows[0]["RecpDprtCode"].ToString();
                            retVal.Doc.ReceiptList[i].RecpDprtName = oDSR.Tables[i].Rows[0]["RecpDprtName"].ToString();
                        }
                        oNode.AppendChild(receiptList);
                        oDSR.Dispose();
                    }

                    #endregion

                    #region 회람라인
                    DataSet oCirculationInfo = null;
                    if (oFNode != null)
                    {
                        oCirculationInfo = GetCirculationInfo(oFNode.Attributes["instanceid"].Value, ApprDocPID, "P");
                        if (oCirculationInfo != null)
                        {
                            XmlDocument oCirculationInfoXML = new XmlDocument();
                            oCirculationInfoXML.LoadXml(oCirculationInfo.GetXml());
                            XmlDocument oCirculationXML = new System.Xml.XmlDocument();
                            XmlNodeList oCLList = oCirculationInfoXML.SelectNodes("RESPONSE/Table");

                            XmlElement circulationList = oReturnXML.CreateElement("circulationLine");

                            // NewType
                            retVal.Doc.CirculationLine = new ReturnType_MobileGetApprDocDetail_Circulation[oCLList.Count];
                            int rCnt = 0;
                            foreach (XmlNode oCLNode in oCLList)
                            {
                                XmlElement circulation = oReturnXML.CreateElement("circulation");
                                //처리자 ID		C	30	50	ApproverLoginID	
                                XmlElement approverLoginID = oReturnXML.CreateElement("ApproverLoginID");
                                approverLoginID.InnerText = oCLNode.SelectSingleNode("RECEIPT_ID").InnerText;
                                circulation.AppendChild(approverLoginID);

                                //처리자명		C	40	80	ApproverName
                                XmlElement approverName = oReturnXML.CreateElement("ApproverName");
                                approverName.InnerText = oCLNode.SelectSingleNode("RECEIPT_NAME").InnerText;
                                circulation.AppendChild(approverName);

                                //부서코드		C	50	270	ApprDprtCode
                                XmlElement apprDprtCode = oReturnXML.CreateElement("ApprDprtCode");
                                apprDprtCode.InnerText = oCLNode.SelectSingleNode("RECEIPT_OU_ID").InnerText;
                                circulation.AppendChild(apprDprtCode);

                                //부서명		C	100	320	ApprDprtName
                                XmlElement apprDprtName = oReturnXML.CreateElement("ApprDprtName");
                                apprDprtName.InnerText = oCLNode.SelectSingleNode("RECEIPT_OU_NAME").InnerText;
                                circulation.AppendChild(apprDprtName);

                                circulationList.AppendChild(circulation);

                                // NewType
                                retVal.Doc.CirculationLine[rCnt].ApproverLoginID = oCLNode.SelectSingleNode("RECEIPT_ID").InnerText;
                                retVal.Doc.CirculationLine[rCnt].ApproverName = oCLNode.SelectSingleNode("RECEIPT_NAME").InnerText;
                                retVal.Doc.CirculationLine[rCnt].ApprDprtCode = oCLNode.SelectSingleNode("RECEIPT_OU_ID").InnerText;
                                retVal.Doc.CirculationLine[rCnt].ApprDprtName = oCLNode.SelectSingleNode("RECEIPT_OU_NAME").InnerText;

                                rCnt++;
                            }
                            oNode.AppendChild(circulationList);
                        }
                    }
                    #endregion

                    #region 첨부파일 목록
                    XmlElement attachFileList = oReturnXML.CreateElement("AttachFileList");
                    if (oAttFiles != null)
                    {
                        // NewType
                        retVal.Doc.AttachFileList = new ReturnType_MobileGetApprDocDetail_AttachFile[oAttFiles.Count];
                        int rCnt = 0;

                        foreach (System.Xml.XmlNode oAFileXML in oAttFiles)
                        {
                            XmlElement attachFile = oReturnXML.CreateElement("AttachFile");

                            if (oAFileXML != null)
                            {
                                // 첨부파일 ID                             
                                XmlElement attachFileID = oReturnXML.CreateElement("AttachFileID");
                                attachFileID.InnerText = oAFileXML.Attributes["location"].Value.Substring(oAFileXML.Attributes["location"].Value.LastIndexOf("/") + 1);
                                attachFile.AppendChild(attachFileID);

                                // 첨부파일 제목                           
                                XmlElement attachFileTitle = oReturnXML.CreateElement("AttachFileTitle");
                                attachFileTitle.InnerText = oAFileXML.Attributes["name"].Value;
                                attachFile.AppendChild(attachFileTitle);

                                // 첨부파일 사이즈                          
                                XmlElement attachFileSize = oReturnXML.CreateElement("AttachFileSize");
                                attachFileSize.InnerText = oAFileXML.Attributes["size"] != null ? oAFileXML.Attributes["size"].Value : "";
                                attachFile.AppendChild(attachFileSize);

                                // 첨부파일 종류                         
                                XmlElement attachFileType = oReturnXML.CreateElement("AttachFileType");
                                attachFileType.InnerText = oAFileXML.Attributes["name"].Value.Substring(oAFileXML.Attributes["name"].Value.LastIndexOf(".") + 1);
                                attachFile.AppendChild(attachFileType);

                                // 첨부파일 다운로드 경로                            
                                XmlElement attachFileURL = oReturnXML.CreateElement("AttachFileURL");
                                //attachFileURL.InnerText = "http://" + ConfigurationManager.AppSettings["LinKURL"].ToString() + oAFileXML.Attributes["location"].Value;
                                attachFileURL.InnerText = oAFileXML.Attributes["location"].Value;
                                attachFile.AppendChild(attachFileURL);

                                attachFileList.AppendChild(attachFile);

                                // NewType
                                retVal.Doc.AttachFileList[rCnt] = new ReturnType_MobileGetApprDocDetail_AttachFile();

                                retVal.Doc.AttachFileList[rCnt].AttachFileID = attachFileID.InnerText;
                                retVal.Doc.AttachFileList[rCnt].AttachFileName = attachFileTitle.InnerText;
                                retVal.Doc.AttachFileList[rCnt].AttachFileSize = oAFileXML.Attributes["size"].Value;
                                retVal.Doc.AttachFileList[rCnt].AttachFileType = attachFileType.InnerText;
                                retVal.Doc.AttachFileList[rCnt].AttachFileURL = attachFileURL.InnerText;

                                rCnt++;
                            }
                        }
                    }
                    oNode.AppendChild(attachFileList);
                    #endregion

                    #region 결재문서URL
                    XmlElement ReturnURL = oReturnXML.CreateElement("ReturnURL");
                    XmlElement ApprDocURL = oReturnXML.CreateElement("ApprDocURL");
                    szURL = sb.ToString();
                    sb = null;
                    XmlCDataSection CData;
                    CData = oReturnXML.CreateCDataSection(szURL);
                    ApprDocURL.AppendChild(CData);
                    //ApprDocURL.InnerText = szURL;
                    ReturnURL.AppendChild(ApprDocURL);
                    oNode.AppendChild(ReturnURL);

                    // NewType
                    retVal.Doc.ReturnURL = new ReturnType_MobileGetApprDocDetail_ApprDocURL();
                    retVal.Doc.ReturnURL.ApprDocURL = ApprDocURL.InnerText;
                    #endregion
                }
            }
            //return oReturnXML.OuterXml;            
        }
        catch (Exception ex)
        {
            //ExceptionHandler(ex, WOORI.IGW.Framework.SystemOption.APV);
            //return "<WebService><LIST><Error><![CDATA[" + ex.Message + "]]></Error></LIST></WebService>";
        }
        finally
        {
            oDS.Dispose();
        }

        // NewType Return
        return retVal;
    }
    #endregion

    /// <summary>
    /// <b>[B012]모바일 개인함 결재하기</b><br />
    /// - 최초작성자 : 코비전 황선희<br />
    /// - 최초작성일 : 2008년  10월 23일<br />
    /// - 최종수정자 : SYB<br />
    /// - 최종수정일 : 2010.03<br />
    /// </summary>
    /// <param name="LoginID">사번 - 현재 결재자</param>
    /// <param name="ApprDocPID">결재문서 PID</param>
    /// <param name="ApprDocWID">결재문서 WID</param>
    /// <param name="ApproverID">결재자 ID - 원결재자performer_id</param>
    /// <param name="ApprGbnCode">결재구분코드 - 1: 승인,2: 반려, 5:보류</param>
    /// <param name="Comment">결재의견</param>
    /// <param name="RtnLoginID">반려대상자 ID</param>
    /// <returns>성공시:OK, 실패시:ERROR 메시지</returns>
    #region //--> MobileApproved
    [WebMethod]
    public string MobileApproved(string LoginID, string ApprDocPID, string ApprDocWID, string ApproverID,
                            string ApprGbnCode, string Comment, string RtnLoginID, string ApprovalLineXml = "", string signImagePath = "")
    {

        string isPC = "N";
        if (ApprGbnCode.IndexOf("_pc") > -1)
        {
            ApprGbnCode = ApprGbnCode.Replace("_pc", "");
            isPC = "Y";
        }
        if (isPC == "N" || ApprGbnCode != "3")
        {
            ApprovalLineXml = "";
        }
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        string sApvLine = string.Empty;
        string fmpf = string.Empty;
        string fmrv = string.Empty;
        string fmid = string.Empty;
        string fiid = string.Empty;
        string scid = string.Empty;

        LoginID = LoginID.Trim();
        ApprDocPID = ApprDocPID.Trim();
        ApprDocWID = ApprDocWID.Trim();
        ApproverID = ApproverID.Trim();
        ApprGbnCode = ApprGbnCode.Trim();
        Comment = Comment.Trim();
        RtnLoginID = RtnLoginID.Trim();

        if (ApprGbnCode == "5")
        {
            //보류
            XmlDocument oFormXMLDOM = new XmlDocument();
            try
            {
                oFormXMLDOM.LoadXml("<request></request>");
                XmlElement request = oFormXMLDOM.DocumentElement;
                XmlElement xpiid = oFormXMLDOM.CreateElement("piid");
                XmlElement xfiid = oFormXMLDOM.CreateElement("fiid");
                XmlElement xusid = oFormXMLDOM.CreateElement("usid");
                XmlElement xdpid = oFormXMLDOM.CreateElement("dpid");
                XmlElement xactidx = oFormXMLDOM.CreateElement("actidx");
                XmlElement xactcmt = oFormXMLDOM.CreateElement("actcmt");
                XmlElement xpfsk = oFormXMLDOM.CreateElement("pfsk");
                XmlElement xptid = oFormXMLDOM.CreateElement("ptid");
                XmlElement xfmnm = oFormXMLDOM.CreateElement("fmnm");
                XmlElement xClientAppInfo = oFormXMLDOM.CreateElement("ClientAppInfo");
                XmlElement xforminfoext = oFormXMLDOM.CreateElement("forminfoext");
                //사인이미지추가
                XmlElement xsignimagetype = oFormXMLDOM.CreateElement("signimagetype");
                xsignimagetype.InnerText = signImagePath;

                string sSpName = "dbo.usp_wf_getMobileReserveinfo";
                DataSet oDS = new DataSet();


                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    DataPack INPUT = new DataPack();
                    INPUT.add("@WIID", ApprDocWID);
                    INPUT.add("@FIID", ApprDocPID);
                    INPUT.add("@LoginId", ApproverID);

                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                }

                if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                {

                    xpiid.InnerText = ApprDocPID;
                    xfiid.InnerText = fiid;
                    xusid.InnerText = LoginID;
                    xdpid.InnerText = oDS.Tables[0].Rows[0]["UNIT_CODE"].ToString();
                    xactidx.InnerText = "reserve";
                    xactcmt.InnerText = Comment;
                    xpfsk.InnerText = oDS.Tables[0].Rows[0]["SUB_KIND"].ToString();
                    xptid.InnerText = ApproverID;
                    xfmnm.InnerText = oDS.Tables[0].Rows[0]["FMNM"].ToString();
                }
                request.AppendChild(xpiid);
                request.AppendChild(xfiid);
                request.AppendChild(xusid);
                request.AppendChild(xdpid);
                request.AppendChild(xactidx);
                request.AppendChild(xactcmt);
                request.AppendChild(xpfsk);
                request.AppendChild(xptid);
                request.AppendChild(xfmnm);
                request.AppendChild(xsignimagetype);

                Covi.BizService.ApprovalService.ProcessDomainData(oFormXMLDOM);
                //보류시 목록에 아이콘 표시 - WfProcessInstance.description 에 update
                CfnEntityClasses.WfProcessInstance PInstance = null;
                CfnDatabaseManager.WfDBManager oDBMgr = null;
                CfnCoreEngine.WfProcessManager oPMgr = null;
                try
                {
                    oPMgr = new CfnCoreEngine.WfProcessManager();
                    PInstance = oPMgr.GetEntity(ApprDocPID);
                    XmlDocument oXML = new XmlDocument();
                    oXML.LoadXml(PInstance.description);

                    XmlAttribute oAttr = null;
                    oAttr = oXML.CreateAttribute("reserve");
                    oAttr.Value = "1";
                    oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes.SetNamedItem(oAttr);
                    oDBMgr = new CfnDatabaseManager.WfDBManager();
                    oDBMgr.ChangeProperty("WfProcessInstance", ApprDocPID, "description", oXML.OuterXml);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (PInstance != null)
                    {
                        PInstance = null;
                    }
                    if (oDBMgr != null)
                    {
                        oDBMgr.Dispose();
                        oDBMgr = null;
                    }
                    if (oPMgr != null)
                    {
                        oPMgr.Dispose();
                        oPMgr = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }

        }
        try
        {
            if (ApprGbnCode != "5")
            {
                {//보류 아이코 없애기
                    //보류시 목록에 아이콘 표시 - WfProcessInstance.description 에 update
                    CfnEntityClasses.WfProcessInstance PInstance = null;
                    CfnDatabaseManager.WfDBManager oDBMgr = null;
                    CfnCoreEngine.WfProcessManager oPMgr = null;
                    try
                    {
                        oPMgr = new CfnCoreEngine.WfProcessManager();
                        PInstance = oPMgr.GetEntity(ApprDocPID);
                        XmlDocument oXML = new XmlDocument();
                        oXML.LoadXml(PInstance.description);

                        XmlNode oForminfo = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                        if (oForminfo.Attributes.GetNamedItem("reserve") != null) oForminfo.Attributes.GetNamedItem("reserve").Value = "0";
                        oDBMgr = new CfnDatabaseManager.WfDBManager();
                        oDBMgr.ChangeProperty("WfProcessInstance", ApprDocPID, "description", oXML.OuterXml);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (PInstance != null)
                        {
                            PInstance = null;
                        }
                        if (oDBMgr != null)
                        {
                            oDBMgr.Dispose();
                            oDBMgr = null;
                        }
                        if (oPMgr != null)
                        {
                            oPMgr.Dispose();
                            oPMgr = null;
                        }
                    }
                }

                string sSpName = "dbo.usp_wf_approvedmobile";
                int iReturn = 0;

                // 지정반려인 경우 처리
                //if (ApprGbnCode.Equals(REJECTEDTO) && RtnLoginID != string.Empty)
                //{
                //    ApprovalLineXml = UpdateApprLine(LoginID, RtnLoginID, ApprDocPID, ApprDocWID);

                //    //if (ApprToRjct == true) // 기안자와 지정된 반려자가 동일한 경우 일반 반려로 처리
                //    //{
                //    //    ApprGbnCode = REJECT;
                //    //}
                //}

                if (ApprovalLineXml != "" && isPC != "Y")
                {
                    ApprovalLineXml = SetMobileAtt(ApprovalLineXml);
                }
                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {

                    DataPack INPUT = new DataPack();
                    INPUT.add("@wiid", ApprDocWID);
                    INPUT.add("@pfid", ApprDocPID);
                    INPUT.add("@ptid", ApproverID);
                    INPUT.add("@usid", LoginID);
                    INPUT.add("@actionindex", ApprGbnCode);
                    INPUT.add("@actioncomment", Comment);
                    INPUT.add("@apvlist", ApprovalLineXml);
                    //INPUT.add("@signimagetype", this.GetSigninform(LoginID, "sign"));//201108
                    INPUT.add("@signimagetype", signImagePath);//20150917 수정 -> 지정된 사인이미지로 저장
                    INPUT.add("@isPC", isPC);

                    iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
                }
            }
            sbXml.Append("OK");
        }
        catch (System.Exception ex)
        {
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }
        return sReturn;
    }

    #endregion

    #region //--> New MobileApproved
    [WebMethod]
    public ReturnType_MobileApproved MobileApproved_New(string LoginID, string ApprDocPID, string ApprDocWID, string ApproverID,
                            string ApprGbnCode, string Comment, string RtnLoginID)
    {
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        string APPROVE = "1";
        string REJECT = "2";
        string REJECTEDTO = "3";
        string sApvLine = string.Empty;
        string fmpf = string.Empty;
        string fmrv = string.Empty;
        string fmid = string.Empty;
        string fiid = string.Empty;
        string scid = string.Empty;
        string ACTIONINDEX = "approve";

        LoginID = LoginID.Trim();
        ApprDocPID = ApprDocPID.Trim();
        ApprDocWID = ApprDocWID.Trim();
        ApproverID = ApproverID.Trim();
        ApprGbnCode = ApprGbnCode.Trim();
        Comment = Comment.Trim();
        RtnLoginID = RtnLoginID.Trim();
        if (ApprGbnCode == "2")
        {
            ACTIONINDEX = "reject";
        }
        else if (ApprGbnCode == "3")
        {
            ACTIONINDEX = "rejectedto";
        }

        // NewType
        ReturnType_MobileApproved retVal = new ReturnType_MobileApproved();

        try
        {
            string sSpName = "dbo.usp_wf_approvedmobile";
            int iReturn = 0;

            // 지정반려인 경우 처리
            if (ApprGbnCode.Equals(REJECTEDTO) && RtnLoginID != string.Empty)
            {
                sReturn = UpdateApprLine(LoginID, RtnLoginID, ApprDocPID, ApprDocWID);

                //if (ApprToRjct == true) // 기안자와 지정된 반려자가 동일한 경우 일반 반려로 처리
                //{
                //    ApprGbnCode = REJECT;
                //}
            }

            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                DataPack INPUT = new DataPack();
                INPUT.add("@wiid", ApprDocWID);
                INPUT.add("@pfid", ApprDocPID);
                INPUT.add("@ptid", ApproverID);
                INPUT.add("@usid", LoginID);
                INPUT.add("@actionindex", ApprGbnCode);
                INPUT.add("@actioncomment", Comment);
                INPUT.add("@apvlist", sReturn);

                iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
            }
            //sbXml.Append("OK");
            retVal.ReturnCode = "OK";
            retVal.ReturnDesc = "Success";
        }
        catch (System.Exception ex)
        {
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");

            // NewType
            retVal.ReturnCode = "Error";
            retVal.ReturnDesc = ex.Message.ToString();
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }
        //return sReturn;
        return retVal;
    }
    #endregion

    /// <summary>
    /// <b>[B012]모바일 개인함 결재하기(암호확인포함)</b><br />
    /// - 최초작성자 : 코비전 황선희<br />
    /// - 최초작성일 : 2011년  1월 23일<br />
    /// - 최종수정자 : <br />
    /// - 최종수정일 : <br />
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <param name="ApprDocPID">결재문서 PID</param>
    /// <param name="ApprDocWID">결재문서 WID</param>
    /// <param name="ApproverID">결재자 ID</param>
    /// <param name="ApprGbnCode">결재구분코드</param>
    /// <param name="Comment">결재의견</param>
    /// <param name="RtnLoginID">반려대상자 ID</param>
    /// <param name="ApprPWD">결재암호</param>
    /// <returns>성공시:OK, 실패시:ERROR 메시지</returns>
    #region //--> MobileApprovedPWD
    [WebMethod]
    public string MobileApprovedPWD(string LoginID, string ApprDocPID, string ApprDocWID, string ApproverID,
                            string ApprGbnCode, string Comment, string RtnLoginID, string ApprPWD)
    {
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        try
        {
            DataSet ds = null;
            DataPack INPUT = null;
            try
            {
                ds = new DataSet();
                INPUT = new DataPack();

                using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@vc_PERSON_CODE", LoginID);
                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_WF_CK_APPROVAL_PWD", INPUT);
                }

                string sDBPWD = ds.Tables[0].Rows[0][0].ToString();
                string sInputPWD = ApprPWD;
                if (sDBPWD == "" && sInputPWD == "")
                {
                    sReturn = "OK";
                }
                else
                {
                    sInputPWD = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(ApprPWD, "MD5");
                    if (sDBPWD == sInputPWD)
                    {
                        sReturn = "OK";
                    }
                    else
                    {
                        //throw new System.Exception(Resources.Approval.msg_102);
                        throw new System.Exception();
                    }
                }
            }
            catch (System.Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (ds != null)
                {
                    ds.Dispose();
                    ds = null;
                }
                if (INPUT != null)
                {
                    INPUT.Dispose();
                    INPUT = null;
                }
            }

            if (sReturn == "OK")
            {
                sbXml.Append(MobileApproved(LoginID, ApprDocPID, ApprDocWID, ApproverID, ApprGbnCode, Comment, RtnLoginID));
            }
        }
        catch (System.Exception ex)
        {
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }
        return sReturn;
    }
    #endregion

    #region //--> New MobileApprovedPWD
    [WebMethod]
    public ReturnType_MobileApprovedPWD MobileApprovedPWD_New(string LoginID, string ApprDocPID, string ApprDocWID, string ApproverID,
                            string ApprGbnCode, string Comment, string RtnLoginID, string ApprPWD)
    {
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        // New Type
        ReturnType_MobileApprovedPWD retVal = new ReturnType_MobileApprovedPWD();
        try
        {
            DataSet ds = null;
            DataPack INPUT = null;
            try
            {
                ds = new DataSet();
                INPUT = new DataPack();

                using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@vc_PERSON_CODE", LoginID);
                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_WF_CK_APPROVAL_PWD", INPUT);
                }

                string sDBPWD = ds.Tables[0].Rows[0][0].ToString();
                string sInputPWD = ApprPWD;
                if (sDBPWD == "" && sInputPWD == "")
                {
                    sReturn = "OK";

                    // NewType
                    retVal.ReturnCode = "OK";
                    retVal.ReturnDesc = "Success";
                }
                else
                {
                    sInputPWD = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(ApprPWD, "MD5");
                    if (sDBPWD == sInputPWD)
                    {
                        sReturn = "OK";

                        // NewType
                        retVal.ReturnCode = "OK";
                        retVal.ReturnDesc = "Success";
                    }
                    else
                    {
                        //throw new System.Exception(Resources.Approval.msg_102);
                        throw new System.Exception();
                    }
                }
            }
            catch (System.Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (ds != null)
                {
                    ds.Dispose();
                    ds = null;
                }
                if (INPUT != null)
                {
                    INPUT.Dispose();
                    INPUT = null;
                }
            }

            if (sReturn == "OK")
            {
                sbXml.Append(MobileApproved(LoginID, ApprDocPID, ApprDocWID, ApproverID, ApprGbnCode, Comment, RtnLoginID));
            }
        }
        catch (System.Exception ex)
        {
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");

            // NewType
            retVal.ReturnCode = "Error";
            retVal.ReturnDesc = ex.Message.ToString();
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }
        //return sReturn;
        return retVal;
    }
    #endregion

    /// <summary>
    /// MOBILE 2010.03. SYB
    /// [B013] Request Review 등록
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <param name="ApprDocPID">결재문서 PID</param>
    /// <param name="ApprDocWID">결재문서 WID</param>
    /// <param name="RequestComment">Request 의견</param>
    /// <param name="ReplierLoginID">Review 대상자 ID</param>
    /// <returns>성공시:OK, 실패시:ERROR 메시지</returns>
    //[WebMethod]
    public string MobileRequestReview(string LoginID, string ApprDocPID,
                                        string ApprDocWID, string RequestComment, string ReplierLoginID)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        try
        {
            string sSpName = "dbo.*";
            int iReturn = 0;

            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                //For test
                ApprDocPID = "9f0073e0-5399-468f-8294-016f7c557eac";

                //INPUT.add("@wiid", ApprDocWID.Trim());
                //INPUT.add("@pfid", ApprDocPID.Trim());
                //INPUT.add("@ptid", ApproverID.Trim());
                //INPUT.add("@usid", LoginID.Trim());
                //INPUT.add("@actionindex", ApprGbnCode.Trim());
                //INPUT.add("@actioncomment", Comment.Trim());
                //INPUT.add("@rtnlonginid", RtnLoginID.Trim());

                // iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
            }
            sbXml.Append("OK");
        }
        catch (System.Exception ex)
        {
            //ExceptionHandler(ex, WOORI.IGW.Framework.SystemOption.APV);
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }

        return sReturn;
    }


    /// <summary>
    /// MOBILE 2010.03. SYB
    /// [B015] Message 조회
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <param name="ApprDocPID">결재문서 PID</param>
    /// <param name="ApprDocWID">결재문서 WID</param>
    /// <returns></returns>
    //[WebMethod]
    public string MobileGetMessage(string LoginID, string ApprDocPID, string ApprDocWID)
    {


        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        try
        {
            string sSpName = "dbo.*";
            int iReturn = 0;

            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                //For test
                ApprDocPID = "9f0073e0-5399-468f-8294-016f7c557eac";

                //INPUT.add("@wiid", ApprDocWID.Trim());
                //INPUT.add("@pfid", ApprDocPID.Trim());
                //INPUT.add("@ptid", ApproverID.Trim());
                //INPUT.add("@usid", LoginID.Trim());
                //INPUT.add("@actionindex", ApprGbnCode.Trim());
                //INPUT.add("@actioncomment", Comment.Trim());
                //INPUT.add("@rtnlonginid", RtnLoginID.Trim());

                // iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);

                //Request ID		C	10	0	RequestID	
                //요청자 ID		C	30	10	ReqLoginID	
                //요청자명		C	40	40	ReqName	
                //직급코드		C	10	80	ReqPosCode	
                //직급명		C	40	90	ReqPosName	
                //부서코드		C	50	130	ReqDprtCode	
                //부서명		C	100	180	ReqDprtName	
                //요청일시		C	20	280	ReqDateTime	
                //Request 의견		C	200	300	ReqComment	
                //Reply ID		C	10	500	ReplyID	
                //작성자 ID		C	30	510	ReplyLoginID	
                //요청자명		C	40	540	PeplyName	
                //직급코드		C	10	580	ReplyPosCode	
                //직급명		C	40	590	ReplyPosName	
                //부서코드		C	50	630	ReplyDprtCode	
                //부서명		C	100	680	ReplyDprtName	
                //Reply일시		C	20	780	ReplyDateTime	
                //Reply 의견		C	200	510	ReplyComment	




            }
            sbXml.Append("OK");
        }
        catch (System.Exception ex)
        {
            //ExceptionHandler(ex, WOORI.IGW.Framework.SystemOption.APV);
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }

        return sReturn;
    }

    /// <summary>
    /// MOBILE 2010.03. SYB
    /// [B016] Message List 조회
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <param name="ApprDocPID">결재문서 PID</param>
    /// <param name="ApprDocWID">결재문서 WID</param>
    /// <returns></returns>
    //[WebMethod]
    public string MobileGetMessageList(string LoginID, string ApprDocPID, string ApprDocWID)
    {


        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        try
        {
            string sSpName = "dbo.*";
            int iReturn = 0;

            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                //For test
                ApprDocPID = "9f0073e0-5399-468f-8294-016f7c557eac";

                //INPUT.add("@wiid", ApprDocWID.Trim());
                //INPUT.add("@pfid", ApprDocPID.Trim());
                //INPUT.add("@ptid", ApproverID.Trim());
                //INPUT.add("@usid", LoginID.Trim());
                //INPUT.add("@actionindex", ApprGbnCode.Trim());
                //INPUT.add("@actioncomment", Comment.Trim());
                //INPUT.add("@rtnlonginid", RtnLoginID.Trim());

                // iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);

                //요청자 ID		C	30	0	RequestLoginID	
                //요청자명		C	40	30	RequestName	
                //직급코드		C	10	70	PositionCode	
                //직급명		C	40	80	PositionName	
                //부서코드		C	50	120	ReqDprtCode	
                //부서명		C	100	170	ReqDprtName	
                //Request ID		C	10	270	RequestID	
                //Request 의견		C	200	280	RequestComment	

            }
            sbXml.Append("OK");
        }
        catch (System.Exception ex)
        {
            //ExceptionHandler(ex, WOORI.IGW.Framework.SystemOption.APV);
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }

        return sReturn;
    }


    /// <summary>
    /// MOBILE 2010.03. SYB
    /// [B017] Reply Review 등록
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <param name="ApprDocPID">결재문서 PID</param>
    /// <param name="ApprDocWID">결재문서 WID</param>
    /// <param name="RequestLoginID">요청자 ID</param>
    /// <param name="RequestID">Request ID</param>
    /// <param name="ReplyComment">Reply 의견</param>
    /// <returns>성공시:OK, 실패시:ERROR 메시지</returns>
    //[WebMethod]
    public string MobileReplyReview(string LoginID, string ApprDocPID,
                                        string ApprDocWID, string RequestLoginID, string RequestID, string ReplyComment)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        try
        {
            string sSpName = "dbo.*";
            int iReturn = 0;

            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                //For test
                ApprDocPID = "9f0073e0-5399-468f-8294-016f7c557eac";

                //INPUT.add("@wiid", ApprDocWID.Trim());
                //INPUT.add("@pfid", ApprDocPID.Trim());
                //INPUT.add("@ptid", ApproverID.Trim());
                //INPUT.add("@usid", LoginID.Trim());
                //INPUT.add("@actionindex", ApprGbnCode.Trim());
                //INPUT.add("@actioncomment", Comment.Trim());
                //INPUT.add("@rtnlonginid", RtnLoginID.Trim());

                // iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
            }
            sbXml.Append("OK");
        }
        catch (System.Exception ex)
        {
            //ExceptionHandler(ex, WOORI.IGW.Framework.SystemOption.APV);
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }

        return sReturn;
    }


    /// <summary>
    /// MOBILE 2010.03. SYB
    /// [B021] Assing(Inform) 결재문서 확인하기
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <param name="ApprDocPID">결재문서 PID</param>
    /// <param name="ApprDocWID">결재문서 WID</param>
    /// <returns>성공시:OK, 실패시:ERROR 메시지</returns>
    [WebMethod]
    public string MobileAssignApproved(string LoginID, string ApprDocPID, string ApprDocWID)
    {
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        try
        {
            string sSpName = "dbo.usp_wf_approvedmobile";
            int iReturn = 0;

            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                //For test
                //ApprDocPID = "9f0073e0-5399-468f-8294-016f7c557eac";
                DataPack INPUT = new DataPack();

                INPUT.add("@wiid", ApprDocWID.Trim());
                INPUT.add("@pfid", ApprDocPID.Trim());
                INPUT.add("@ptid", LoginID.Trim());
                INPUT.add("@usid", LoginID.Trim());
                INPUT.add("@actionindex", "1"); // 무조건 승인. Confirm
                INPUT.add("@actioncomment", "");
                INPUT.add("@rtnlonginid", "");

                iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
            }
            sbXml.Append("OK");
        }
        catch (System.Exception ex)
        {
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }
        return sReturn;
    }


    /// <summary>
    /// 모바일 웹서비스용 결재 카운트 (2011-08-16 Leesh)
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <returns>미결|진행|완료</returns>
    [WebMethod]
    public string MobileApprovalCount(string LoginID, string DeptCode = null)
    {

        string sReturn = string.Empty;

        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        StringBuilder sbXml = new StringBuilder();
        try
        {
            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                if (LoginID != string.Empty)
                {
                    string sSpName = "dbo.usp_wf_approvalcount";

                    INPUT.add("@USER_ID", LoginID);
                    if (DeptCode != null && DeptCode != "")
                    {
                        INPUT.add("@UNIT_CODE", DeptCode);
                    }
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                    {
                        sbXml.Append(oDS.Tables[0].Rows[0]["PREAPPROVAL"].ToString());			// 개인 예고함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["APPROVAL"].ToString());				// 개인 미결함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["PROCESS"].ToString());				// 개인 진행함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["COMPLETE"].ToString());				// 개인 완료함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["REJECT"].ToString());				// 개인 반려함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["TEMPSAVE"].ToString());				// 개인 임시함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["PCIRCULATION"].ToString());			// 개인 참조/회람함
                        sbXml.Append("|");

                        sbXml.Append(oDS.Tables[0].Rows[0]["UCOMPLETE"].ToString());			// 부서 완료함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["RECEIVE"].ToString());				// 부서 수신함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["URECOMPLETE"].ToString());			// 부서 수신처리함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["UCIRCULATION"].ToString());			// 부서 참조/회람함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["UPROCESS"].ToString());				// 부서 진행함
                        sbXml.Append("|");
                        sbXml.Append(oDS.Tables[0].Rows[0]["GPCIRCULATION"].ToString()); //** 일반 전표 개인 참조/회람함 240822
                        sbXml.Append("|"); 
                        sbXml.Append(oDS.Tables[0].Rows[0]["GDCIRCULATION"].ToString()); //** 일반 전표 부서 참조/회람함 240822

                    }
                }
            }

            sReturn = sbXml.ToString();
        }
        catch (Exception ex)
        {
            sReturn = "Error : " + ex.Message;
        }
        finally
        {
            oDS.Dispose();
        }

        return sReturn;
    }

    /// <summary>
    /// 모바일 웹서비스용 결재 카운트 (2011-08-16 Leesh)
    /// </summary>
    /// <param name="LoginID">사번</param>
    /// <returns>ReturnType_MobileCount</returns>
    [WebMethod]
    public ReturnType_MobileCount MobileApprovalCount_New(string LoginID)
    {


        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        // New Type 
        ReturnType_MobileCount retVal = new ReturnType_MobileCount();
        try
        {
            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                if (LoginID != string.Empty)
                {
                    string sSpName = "dbo.usp_wf_worklistquery01mobile_count";

                    INPUT.add("@USER_ID", LoginID);
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    oDS.DataSetName = "RESPONSE";

                    if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                    {
                        //approval_count	process_count	complete_count
                        retVal.approval_count = int.Parse(oDS.Tables[0].Rows[0]["approval_count"].ToString());
                        retVal.process_count = int.Parse(oDS.Tables[0].Rows[0]["process_count"].ToString());
                        retVal.complete_count = int.Parse(oDS.Tables[0].Rows[0]["complete_count"].ToString());

                    }
                }
            }
            retVal.ReturnDesc = "Success";
        }
        catch (Exception ex)
        {
            //return "<WebService><LIST><Error><![CDATA[" + ex.Message + "]]></Error></LIST></WebService>";
            retVal.ReturnDesc = "Error : " + ex.Message.ToString();
        }
        finally
        {
            oDS.Dispose();
        }

        return retVal;
    }
    /// <summary>
    /// 모바일용 양식 리스트 반환
    /// </summary>
    /// <param name="EntCode"></param>
    /// <returns></returns>
    #region //--> MobileGetFormList
    [WebMethod]
    public string MobileGetFormList(string EntCode)
    {
        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        try
        {
            string sSpName = "dbo.usp_wfform_mobileformlistquery01 ";
            using (SqlDacManager oDac = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                INPUT.add("@ent_code", EntCode);
                oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                oDS.DataSetName = "RESPONSE";
            }
            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                oDS.Tables[0].TableName = "FORM";
                oReturnXML.LoadXml(oDS.GetXml());
            }
            return oReturnXML.OuterXml;
        }
        catch (Exception ex)
        {
            return "<WebService><LIST><Error><![CDATA[" + ex.Message + "]]></Error></LIST></WebService>";
        }
        finally
        {
            oDS.Dispose();
        }
        //이전소스 주석처리 --> 함수 재구현
    }
    #endregion

    /// <summary>
    /// 모바일 기안하기 부산은행 기준으로 엎었습니다. (2014-07-04 by 박은선)
    /// </summary>
    /// <param name="Subject">제목</param>
    /// <param name="InitiatorID">기안부서</param>
    /// <param name="InitiatorUnitID">기안부서</param>
    /// <param name="ApprovalLineXml">결재선정보xml</param>
    /// <param name="AttachFileInfo">첨부파일정보</param>
    /// <param name="BodyHtml">본문정보</param>
    /// <returns>성공시:OK, 실패시:ERROR 메시지</returns>
    [WebMethod(Description = "모바일 기안")]
    public string MobileDraft(string Subject, string InitiatorID, string InitiatorUnitID, string ApprovalLineXml, string AttachFileInfo, string Body_Context, string FormId, string ShchmeID)
    {
        XmlDocument oFormXMLDOM = new XmlDocument();
        CfnFormManager.WfFormManager objMTS = null;
        DataSet oDS = null;
        DataPack INPUT = null;

        try
        {
            string strFormInstanceID = System.String.Empty;
            string strProcessID = System.String.Empty;

            strFormInstanceID = CfnEntityClasses.WfEntity.NewGUID();
            strProcessID = CfnEntityClasses.WfEntity.NewGUID();

            objMTS = new CfnFormManager.WfFormManager();

            oFormXMLDOM.LoadXml("<request></request>");

            XmlElement request = oFormXMLDOM.DocumentElement;
            XmlElement piid = oFormXMLDOM.CreateElement("piid");
            XmlElement pdef = oFormXMLDOM.CreateElement("pdef");
            XmlElement mode = oFormXMLDOM.CreateElement("mode");
            XmlElement fmid = oFormXMLDOM.CreateElement("fmid");
            XmlElement scid = oFormXMLDOM.CreateElement("scid");
            XmlElement fmnm = oFormXMLDOM.CreateElement("fmnm");
            XmlElement fmpf = oFormXMLDOM.CreateElement("fmpf");
            XmlElement fmbt = oFormXMLDOM.CreateElement("fmbt");
            XmlElement fmrv = oFormXMLDOM.CreateElement("fmrv");
            XmlElement fiid = oFormXMLDOM.CreateElement("fiid");
            XmlElement usid = oFormXMLDOM.CreateElement("usid");
            XmlElement usdn = oFormXMLDOM.CreateElement("usdn");

            XmlElement ClientAppInfo = oFormXMLDOM.CreateElement("ClientAppInfo");
            XmlElement apvlist = oFormXMLDOM.CreateElement("apvlist");
            XmlElement forminfoext = oFormXMLDOM.CreateElement("forminfoext");
            XmlElement formdata = oFormXMLDOM.CreateElement("formdata");



            oDS = new DataSet();
            INPUT = new DataPack();

            string sSpName = "dbo.usp_userinfo";
            using (SqlDacManager oDac = new SqlDacManager("ORG_ConnectionString"))
            {
                INPUT.add("@PERSON_CODE", InitiatorID);
                INPUT.add("@UNIT_CODE", InitiatorUnitID);
                oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
            }

            string strformdata_INITIATOR_NAME = string.Empty;
            string strformdata_INITIATOR_OU_NAME = string.Empty;
            string strformdata_INITIATOR_OU_ID = string.Empty;
            string strformdata_ENT_CODE = string.Empty;
            string strformdata_ENT_NAME = string.Empty;
            string strformdata_INTER_TEL = string.Empty;
            string[] arrformdata_JOB_POSITION;
            string[] strformdata_JOB_POSITION;
            if (oDS != null && oDS.Tables[1].Rows.Count > 0)
            {
                strformdata_INITIATOR_NAME = oDS.Tables[0].Rows[0]["DISPLAY_NAME"].ToString();
                strformdata_INITIATOR_OU_NAME = oDS.Tables[1].Rows[0]["NAME"].ToString();
                strformdata_INITIATOR_OU_ID = oDS.Tables[1].Rows[0]["UNIT_CODE"].ToString();
                strformdata_ENT_CODE = oDS.Tables[1].Rows[0]["ENT_CODE"].ToString();
                strformdata_ENT_NAME = oDS.Tables[1].Rows[0]["ENT_NAME"].ToString();
                strformdata_INTER_TEL = oDS.Tables[0].Rows[0]["INTER_TEL"].ToString();
                arrformdata_JOB_POSITION = oDS.Tables[0].Rows[0]["JOBPOSITION_Z"].ToString().Split('&');
                strformdata_JOB_POSITION = arrformdata_JOB_POSITION[1].Split(';');
            }
            else
            {
                throw new Exception("기안자 정보가 틀립니다.");
            }


            //첨부파일 처리 시작            
            StringBuilder AttachFileInfoXml = new StringBuilder();
            string[] splitStr = { "^B" };
            string[] attachfiles = AttachFileInfo.Split(splitStr, StringSplitOptions.None);
            //파일 이동을 위한 xml작성
            for (int i = 0; i < attachfiles.Length; i++)
            {
                string attachfilesInfos = attachfiles[i];
                if (attachfilesInfos == "")
                    continue;

                string[] attatchFileInfo = attachfilesInfos.Split(('|'));
                AttachFileInfoXml.AppendFormat("<fileinfo index=\"{1}\"><file name=\"{0}\" storageid=\"{1}\" id=\"{0}\" location=\"{2}\" user_name=\"{3}\" dept_name=\"{4}\" size=\"{5}\" state=\"NEW\" /></fileinfo>", System.IO.Path.GetFileName(attatchFileInfo[0]), i.ToString(), attatchFileInfo[0], strformdata_INITIATOR_NAME, strformdata_INITIATOR_OU_NAME, attatchFileInfo[1]);
            }

            string sFiles = "<request>";
            sFiles += "<fileinfos>" + AttachFileInfoXml + "</fileinfos>";
            sFiles += "<fiid>" + strFormInstanceID + "</fiid>";
            sFiles += "</request>";

            System.Xml.XmlDocument oXMLData = new XmlDocument();
            oXMLData.LoadXml(sFiles);

            //frontStorage 에서 gwStorage로 이동 
            Covi.BizService.ApprovalService oApprovalService = new Covi.BizService.ApprovalService();
            string gStorageFileInfo = oApprovalService.pMoveFile(oXMLData.DocumentElement);

            //첨부파일 처리 끝


            //piid
            piid.InnerText = strProcessID;
            request.AppendChild(piid);


            //pdef
            XmlElement oSchema;
            oSchema = Covi.BizService.ApprovalService.pGetSchema(ShchmeID);
            pdef.InnerText = oSchema.GetAttribute("pdef");
            request.AppendChild(pdef);


            //mode
            mode.InnerText = "DRAFT";
            request.AppendChild(mode);


            //fmid
            fmid.InnerText = FormId;
            request.AppendChild(fmid);


            //scid
            scid.InnerText = ShchmeID;
            request.AppendChild(scid);



            //fmnm
            CfnFormManager.WfFormDefinition oFormDef = (CfnFormManager.WfFormDefinition)objMTS.GetDefinitionEntity(FormId, CfnFormManager.CfFormEntityKind.fekdFormDefinition);
            fmnm.InnerText = oFormDef.Name;
            request.AppendChild(fmnm);


            //fmpf
            fmpf.InnerText = oFormDef.FormPrefix;
            request.AppendChild(fmpf);


            //fmbt
            fmbt.InnerText = oFormDef.BodyType;
            request.AppendChild(fmbt);


            //fmrv
            fmrv.InnerText = Convert.ToString(oFormDef.Revision);
            request.AppendChild(fmrv);


            //fiid
            fiid.InnerText = strFormInstanceID;
            request.AppendChild(fiid);

            //usid
            usid.InnerText = InitiatorID;
            request.AppendChild(usid);

            //ClientAppInfo
            XmlElement ClientAppInfo_App = oFormXMLDOM.CreateElement("App");
            XmlElement ClientAppInfo_forminfos = oFormXMLDOM.CreateElement("forminfos");
            XmlElement ClientAppInfo_forminfo = oFormXMLDOM.CreateElement("forminfo");

            ClientAppInfo_App.SetAttribute("name", "FormInfo");
            ClientAppInfo_forminfo.SetAttribute("name", oFormDef.Name);
            ClientAppInfo_forminfo.SetAttribute("filename", oFormDef.FileName);
            ClientAppInfo_forminfo.SetAttribute("feedback", "0");
            ClientAppInfo_forminfo.SetAttribute("selfeedback", "1");
            ClientAppInfo_forminfo.SetAttribute("prefix", oFormDef.FormPrefix);
            ClientAppInfo_forminfo.SetAttribute("revision", Convert.ToString(oFormDef.Revision));
            ClientAppInfo_forminfo.SetAttribute("instanceid", strFormInstanceID);
            ClientAppInfo_forminfo.SetAttribute("id", FormId);
            ClientAppInfo_forminfo.SetAttribute("schemaid", ShchmeID);
            ClientAppInfo_forminfo.SetAttribute("index", "0");
            ClientAppInfo_forminfo.SetAttribute("subject", Subject);
            ClientAppInfo_forminfo.SetAttribute("secure_doc", "0");
            ClientAppInfo_forminfo.SetAttribute("req_response", "");
            ClientAppInfo_forminfo.SetAttribute("isfile", "0");
            ClientAppInfo_forminfo.SetAttribute("fileext", "");
            ClientAppInfo_forminfo.SetAttribute("comment", "0");
            ClientAppInfo_forminfo.SetAttribute("mobiledraft", "1");    //모바일기안 여부 추가

            ClientAppInfo.AppendChild(ClientAppInfo_App).AppendChild(ClientAppInfo_forminfos).AppendChild(ClientAppInfo_forminfo);
            request.AppendChild(ClientAppInfo);

            //apvlist
            XmlDocument oApprovalLineDOM = new XmlDocument();
            oApprovalLineDOM.LoadXml(ApprovalLineXml);
            apvlist.InnerXml = oApprovalLineDOM.SelectSingleNode("steps").OuterXml;
            request.AppendChild(apvlist);

            //forminfoext
            XmlDocument oForminfoextDOM = new XmlDocument();
            System.Collections.Specialized.NameValueCollection oDic = new System.Collections.Specialized.NameValueCollection();
            foreach (System.Xml.XmlNode oSchemaNode in oSchema.ChildNodes)
            {
                System.String sSchemaName = oSchemaNode.Name;
                oDic.Add(sSchemaName, oSchema.GetAttribute(sSchemaName));
                oDic.Add(sSchemaName + "V", oSchemaNode.InnerText);
            }
            oDic.Add("entcode", strformdata_ENT_CODE);
            oDic.Add("entname", strformdata_ENT_NAME);

            //form_info 가져오기 (부산은행에서 커스터마이징된 담당자, 담당부서 가져오기)
            //DataSet forminfoDS = oApprovalService.GetFormInfo(FormId, "");
            //if (forminfoDS.Tables.Count > 0 && forminfoDS.Tables[0].Rows.Count > 0)
            //{
            //    oDic.Add("FORM_CHARGEOUS", forminfoDS.Tables[0].Rows[0]["RESERVED1"].ToString());
            //    oDic.Add("FORM_CHARGEPEOPLE", forminfoDS.Tables[0].Rows[0]["RESERVED2"].ToString());
            //}
            //forminfoDS.Dispose();
            //첨부파일 
            oDic.Add("attach", gStorageFileInfo);
            oForminfoextDOM.LoadXml(getFormInfoExtXML(oDic));
            forminfoext.InnerXml = oForminfoextDOM.SelectSingleNode("forminfo").OuterXml;
            request.AppendChild(forminfoext);

            //formdata
            XmlElement formdata_BODY_CONTEXT = oFormXMLDOM.CreateElement("BODY_CONTEXT");
            XmlElement formdata_INITIATOR_NAME = oFormXMLDOM.CreateElement("INITIATOR_NAME");
            XmlElement formdata_INITIATOR_ID = oFormXMLDOM.CreateElement("INITIATOR_ID");
            XmlElement formdata_INITIATOR_OU_NAME = oFormXMLDOM.CreateElement("INITIATOR_OU_NAME");
            XmlElement formdata_INITIATOR_OU_ID = oFormXMLDOM.CreateElement("INITIATOR_OU_ID");
            XmlElement formdata_ATTACH_FILE_INFO = oFormXMLDOM.CreateElement("ATTACH_FILE_INFO");
            XmlElement formdata_INITIATOR_INFO = oFormXMLDOM.CreateElement("INITIATOR_INFO");
            XmlElement formdata_APPLIED = oFormXMLDOM.CreateElement("APPLIED");
            XmlElement formdata_ISPUBLIC = oFormXMLDOM.CreateElement("ISPUBLIC");
            XmlElement formdata_APPLIED_TERM = oFormXMLDOM.CreateElement("APPLIED_TERM");
            XmlElement formdata_RECEIVE_NO = oFormXMLDOM.CreateElement("RECEIVE_NO");
            XmlElement formdata_RECEIVE_NAMES = oFormXMLDOM.CreateElement("RECEIVE_NAMES");
            XmlElement formdata_RECEIPT_LIST = oFormXMLDOM.CreateElement("RECEIPT_LIST");
            XmlElement formdata_DOC_CLASS_ID = oFormXMLDOM.CreateElement("DOC_CLASS_ID");
            XmlElement formdata_DOC_OU_NAME = oFormXMLDOM.CreateElement("DOC_OU_NAME");
            XmlElement formdata_ENT_CODE = oFormXMLDOM.CreateElement("ENT_CODE");
            XmlElement formdata_ENT_NAME = oFormXMLDOM.CreateElement("ENT_NAME");
            XmlElement formdata_DOC_SUMMARY = oFormXMLDOM.CreateElement("DOC_SUMMARY");
            XmlElement formdata_DOCLINKS = oFormXMLDOM.CreateElement("DOCLINKS");
            XmlElement formdata_DOC_NO = oFormXMLDOM.CreateElement("DOC_NO");
            XmlElement formdata_DOC_CLASS_NAME = oFormXMLDOM.CreateElement("DOC_CLASS_NAME");
            XmlElement formdata_DOC_LEVEL = oFormXMLDOM.CreateElement("DOC_LEVEL");
            XmlElement formdata_SAVE_TERM = oFormXMLDOM.CreateElement("SAVE_TERM");
            XmlElement formdata_SUBJECT = oFormXMLDOM.CreateElement("SUBJECT");
            XmlElement formdata_TELEPHONE = oFormXMLDOM.CreateElement("TELEPHONE");

            //본문
            XmlDocument oBODY_CONTEXT = new XmlDocument();
            oBODY_CONTEXT.LoadXml("<BODY_CONTEXT></BODY_CONTEXT>");
            XmlElement BODY_CONTEXT_tbContentElement = oBODY_CONTEXT.CreateElement("tbContentElement");
            XmlElement BODY_CONTEXT_DOC_LEVEL_NAME = oBODY_CONTEXT.CreateElement("DOC_LEVEL_NAME");
            XmlElement BODY_CONTEXT_PMLINKS = oBODY_CONTEXT.CreateElement("PMLINKS");
            XmlElement BODY_CONTEXT_INITIATOR_DP = oBODY_CONTEXT.CreateElement("INITIATOR_DP");
            XmlElement BODY_CONTEXT_INITIATOR_OU_DP = oBODY_CONTEXT.CreateElement("INITIATOR_OU_DP");
            XmlElement BODY_CONTEXT_SEL_ISAUDIT = oBODY_CONTEXT.CreateElement("SEL_ISAUDIT");
            XmlElement BODY_CONTEXT_SEL_ISAUDIT_TEXT = oBODY_CONTEXT.CreateElement("SEL_ISAUDIT_TEXT");

            //BODY_CONTEXT_tbContentElement.InnerXml = "<![CDATA[" + Body_Context + "]]>";
            BODY_CONTEXT_DOC_LEVEL_NAME.InnerText = "";
            BODY_CONTEXT_PMLINKS.InnerText = "";
            BODY_CONTEXT_INITIATOR_DP.InnerText = Covi.Framework.Dictionary.GetDicInfo(strformdata_INITIATOR_NAME, CF.LanguageType.ko);
            BODY_CONTEXT_INITIATOR_OU_DP.InnerText = Covi.Framework.Dictionary.GetDicInfo(strformdata_INITIATOR_OU_NAME, CF.LanguageType.ko);
            BODY_CONTEXT_SEL_ISAUDIT.InnerText = "무";
            BODY_CONTEXT_SEL_ISAUDIT_TEXT.InnerText = "무";

            oBODY_CONTEXT.DocumentElement.AppendChild(BODY_CONTEXT_tbContentElement);
            oBODY_CONTEXT.DocumentElement.AppendChild(BODY_CONTEXT_DOC_LEVEL_NAME);
            oBODY_CONTEXT.DocumentElement.AppendChild(BODY_CONTEXT_PMLINKS);
            oBODY_CONTEXT.DocumentElement.AppendChild(BODY_CONTEXT_INITIATOR_DP);
            oBODY_CONTEXT.DocumentElement.AppendChild(BODY_CONTEXT_INITIATOR_OU_DP);
            oBODY_CONTEXT.DocumentElement.AppendChild(BODY_CONTEXT_SEL_ISAUDIT);
            oBODY_CONTEXT.DocumentElement.AppendChild(BODY_CONTEXT_SEL_ISAUDIT_TEXT);

            formdata_BODY_CONTEXT.InnerText = oBODY_CONTEXT.InnerXml;
            //기안자 정보
            formdata_INITIATOR_NAME.InnerText = strformdata_INITIATOR_NAME;
            formdata_INITIATOR_ID.InnerText = InitiatorID;
            formdata_INITIATOR_OU_NAME.InnerText = strformdata_INITIATOR_OU_NAME;
            formdata_INITIATOR_OU_ID.InnerText = strformdata_INITIATOR_OU_ID;
            //formdata_INITIATOR_INFO.InnerText = strformdata_INITIATOR_NAME;
            formdata_INITIATOR_INFO.InnerText = Covi.Framework.Dictionary.GetDicInfo(strformdata_INITIATOR_NAME, CF.LanguageType.ko) + "/" + Covi.Framework.Dictionary.GetDicInfo(strformdata_INITIATOR_OU_NAME, CF.LanguageType.ko) + "/" + strformdata_JOB_POSITION[0];
            formdata_ENT_CODE.InnerText = strformdata_ENT_CODE;
            formdata_ENT_NAME.InnerText = strformdata_ENT_NAME;
            //첨부파일
            formdata_ATTACH_FILE_INFO.InnerText = gStorageFileInfo;
            //기안일
            formdata_APPLIED.InnerText = DateTime.Now.ToString("yyyy-MM-dd");
            //양식정보
            formdata_ISPUBLIC.InnerText = "";
            formdata_APPLIED_TERM.InnerText = "";
            formdata_RECEIVE_NO.InnerText = "";
            formdata_RECEIVE_NAMES.InnerText = "@@";
            formdata_RECEIPT_LIST.InnerText = "@@";
            formdata_DOC_CLASS_ID.InnerText = "123";
            formdata_DOC_OU_NAME.InnerText = Covi.Framework.Dictionary.GetDicInfo(strformdata_INITIATOR_OU_NAME, CF.LanguageType.ko);
            formdata_DOC_SUMMARY.InnerText = "";
            formdata_DOCLINKS.InnerText = "";
            formdata_DOC_NO.InnerText = "";
            formdata_DOC_CLASS_NAME.InnerText = "TEST";
            formdata_DOC_LEVEL.InnerText = "100";
            formdata_SAVE_TERM.InnerText = "1";
            formdata_SUBJECT.InnerText = Subject;
            formdata_TELEPHONE.InnerText = strformdata_INTER_TEL;

            formdata.AppendChild(formdata_BODY_CONTEXT);
            formdata.AppendChild(formdata_INITIATOR_NAME);
            formdata.AppendChild(formdata_INITIATOR_ID);
            formdata.AppendChild(formdata_INITIATOR_OU_NAME);
            formdata.AppendChild(formdata_INITIATOR_OU_ID);
            formdata.AppendChild(formdata_ATTACH_FILE_INFO);
            formdata.AppendChild(formdata_INITIATOR_INFO);
            formdata.AppendChild(formdata_APPLIED);
            formdata.AppendChild(formdata_ISPUBLIC);
            formdata.AppendChild(formdata_APPLIED_TERM);
            formdata.AppendChild(formdata_RECEIVE_NO);
            formdata.AppendChild(formdata_RECEIVE_NAMES);
            formdata.AppendChild(formdata_RECEIPT_LIST);
            formdata.AppendChild(formdata_DOC_CLASS_ID);
            formdata.AppendChild(formdata_DOC_OU_NAME);
            formdata.AppendChild(formdata_ENT_CODE);
            formdata.AppendChild(formdata_ENT_NAME);
            formdata.AppendChild(formdata_DOC_SUMMARY);
            formdata.AppendChild(formdata_DOCLINKS);
            formdata.AppendChild(formdata_DOC_NO);
            formdata.AppendChild(formdata_DOC_CLASS_NAME);
            formdata.AppendChild(formdata_DOC_LEVEL);
            formdata.AppendChild(formdata_SAVE_TERM);
            formdata.AppendChild(formdata_SUBJECT);
            formdata.AppendChild(formdata_TELEPHONE);

            XmlDocument bodyDoc = new XmlDocument();
            bodyDoc.LoadXml(Body_Context);
            XmlNode newBodyConxtextNode = formdata.OwnerDocument.ImportNode(bodyDoc.DocumentElement.SelectSingleNode("bodyinfo"), true);
            formdata.AppendChild(newBodyConxtextNode);
            request.AppendChild(formdata);

            usdn.InnerText = strformdata_INITIATOR_NAME;
            request.AppendChild(usdn);
            Covi.BizService.ApprovalService.processFormDataWorkFlow(oFormXMLDOM);
        }
        catch (Exception ex)
        {
            return string.Format("<WebService><LIST><Error><![CDATA[{0}]]></Error></LIST></WebService>", ex.Message.ToString());
        }
        finally
        {
            if (objMTS != null)
            {
                objMTS = null;
            }
            oDS.Dispose();
        }

        return "OK";

        //string _return = "OK";
        //return _return;
    }

    /// <summary>
    /// 모바일 결재선 유효성 검사
    /// </summary>
    /// <param name="SchemaId">SchemaId</param>
    /// <param name="ApprovalLineXml">결재선xml</param>
    /// <returns>성공시:OK, 실패시:ERROR 메시지</returns>
    [WebMethod]
    public string MobileEvaluateApvList(string SchemaId, string ApprovalLineXml)
    {
        string _return = "OK";
        XmlDocument oApprovalLine = new XmlDocument();
        DataPack INPUT = null;
        DataSet oDS = null;
        try
        {
            oApprovalLine.LoadXml(ApprovalLineXml);
            XmlElement oApproval = oApprovalLine.DocumentElement;
            XmlElement oSchema = Covi.BizService.ApprovalService.pGetSchema(SchemaId);
            XmlDocument oForminfoextDOM = new XmlDocument();
            System.Collections.Specialized.NameValueCollection getInfo = new System.Collections.Specialized.NameValueCollection();
            foreach (System.Xml.XmlNode oSchemaNode in oSchema.ChildNodes)
            {
                System.String sSchemaName = oSchemaNode.Name;
                getInfo.Add(sSchemaName, oSchema.GetAttribute(sSchemaName));
                getInfo.Add(sSchemaName + "V", oSchemaNode.InnerText);
            }


            #region 합의/협조 최종위치 체크
            if (getInfo["scPAgr"] == "1"
                || getInfo["scPAgrSEQ"] == "1"
                || getInfo["scDAgr"] == "1"
                || getInfo["scDAgrSEQ"] == "1"
                || getInfo["scPCooPL"] == "1"
                || getInfo["scPCoo"] == "1"
                || getInfo["scDCooPL"] == "1"
                || getInfo["scDCoo"] == "1")
            {
                string strRouteType = "";
                for (int i = 0; i < oApproval.SelectNodes("division").Count; i++)
                {
                    XmlNodeList elmConsult = oApproval.SelectNodes("division[" + (i + 1) + "]/step");
                    if (elmConsult != null)
                    {
                        foreach (XmlNode elm in elmConsult)
                        {
                            //마지막 결재 routetype 가져오기
                            if (elm.SelectSingleNode("ou/person/taskinfo").Attributes["kind"].Value == "skip"
                                || elm.SelectSingleNode("ou/person/taskinfo").Attributes["kind"].Value == "review"
                                || elm.SelectSingleNode("ou/person/taskinfo").Attributes["kind"].Value == "bypass")
                            {
                                //결재안함,후결,후열은 제외
                            }
                            else
                            {
                                strRouteType = elm.Attributes["routetype"].Value;
                            }
                        }
                        if (strRouteType == "consult" || strRouteType == "assist")
                        {
                            _return = CF.Dictionary.GetDic("msg_apv_350", CF.LanguageType.ko);
                            //return _return;
                        }
                    }
                }

            }
            #endregion


            #region 중복 체크
            if (getInfo["scChkDuplicateApv"] == "1")
            {
                XmlNodeList elmPerson = oApproval.SelectNodes("division/step[@unittype='person']/ou/person");
                XmlNodeList elmPersonCc = oApproval.SelectNodes("ccinfo/ou/person");
                string sPerson = "";
                foreach (XmlNode elm in elmPerson)
                {
                    if (sPerson.IndexOf(elm.Attributes["code"].Value + ";") > -1)
                    {
                        _return = CF.Dictionary.GetDic("msg_apv_351", CF.LanguageType.ko);//결재선 또는 참조에 중복된 사용자가 있습니다.
                    }
                    sPerson += elm.Attributes["code"].Value + ";";
                }
                foreach (XmlNode elm in elmPersonCc)
                {
                    if (sPerson.IndexOf(elm.Attributes["code"].Value + ";") > -1)
                    {
                        _return = CF.Dictionary.GetDic("msg_apv_351", CF.LanguageType.ko);//결재선 또는 참조에 중복된 사용자가 있습니다.
                    }
                    sPerson += elm.Attributes["code"].Value + ";";
                }

                XmlNodeList elmOu = oApproval.SelectNodes("division/step[@unittype='ou']/ou");
                XmlNodeList elmOuCc = oApproval.SelectNodes("ccinfo/ou[not(person)]");
                string sOu = "";
                foreach (XmlNode elm in elmOu)
                {
                    if (sPerson.IndexOf(elm.Attributes["code"].Value + ";") > -1)
                    {
                        _return = CF.Dictionary.GetDic("msg_apv_352", CF.LanguageType.ko);//결재선 또는 참조에 중복된 부서가 있습니다.
                    }
                    sOu += elm.Attributes["code"].Value + ";";
                }
                foreach (XmlNode elm in elmOuCc)
                {
                    if (sPerson.IndexOf(elm.Attributes["code"].Value + ";") > -1)
                    {
                        _return = CF.Dictionary.GetDic("msg_apv_352", CF.LanguageType.ko);//결재선 또는 참조에 중복된 부서가 있습니다.
                    }
                    sOu += elm.Attributes["code"].Value + ";";
                }
            }
            #endregion

            #region 인사정보 확인
            XmlNodeList elmApproval = oApproval.SelectNodes("division/step/ou/person");
            string sUsers = "";
            foreach (XmlNode elm in elmApproval)
            {
                if (sUsers.Length > 0)
                {
                    var szcmpUsers = ";" + sUsers + ";";
                    if (szcmpUsers.IndexOf(";" + elm.Attributes["code"].Value + ";") == -1) { sUsers += ";" + elm.Attributes["code"].Value; }
                }
                else
                {
                    sUsers += elm.Attributes["code"].Value;
                }
            }
            INPUT = new DataPack();
            oDS = new DataSet();
            using (SqlDacManager oDac = new SqlDacManager("ORG_ConnectionString"))
            {
                INPUT.add("@USER_ID", sUsers);
                oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_CheckAbsentMember_R", INPUT);
            }


            string sAbsentResult = "";
            string sResult = "";
            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                foreach (XmlNode elm in elmApproval)
                {
                    sAbsentResult += elm.Attributes["code"].Value + ",";
                    sResult += elm.Attributes["code"].Value + ",";
                    foreach (DataRow oDR in oDS.Tables[0].Rows)
                    {
                        if (oDR["PERSON_CODE"].ToString() == elm.Attributes["code"].Value)
                        {
                            if (oDR["UNIT_CODE"].ToString().Replace("&", ";") == elm.Attributes["oucode"].Value
                                && oDR["JOBPOSITION_Z"].ToString().Replace("&", ";") == elm.Attributes["position"].Value
                                && oDR["JOBTITLE_Z"].ToString().Replace("&", ";") == elm.Attributes["title"].Value
                                && oDR["JOBLEVEL_Z"].ToString().Replace("&", ";") == elm.Attributes["level"].Value)
                            {
                                sResult = sResult.Replace(elm.Attributes["code"].Value + ",", "");
                            }
                            sAbsentResult = sAbsentResult.Replace(elm.Attributes["code"].Value + ",", "");
                        }
                        else
                        {//퇴직자
                        }
                    }
                }
            }
            if (sAbsentResult != "")
            {
                _return = CF.Dictionary.GetDic("msg_apv_057", CF.LanguageType.ko) + sAbsentResult; //"선택한 개인 결재선에 퇴직자가 포함되어 적용이 되지 않습니다.\n\n---퇴직자--- \n\n"
            }
            else if (sResult != "")
            {
                _return = CF.Dictionary.GetDic("msg_apv_173", CF.LanguageType.ko) + sResult; //"선택한 개인 결재선의 부서/인사정보가 최신정보와 일치하지 않아 적용이 되지 않습니다.\n\n---변경자--- \n\n"
            }
            #endregion
        }
        catch (Exception ex)
        {
            _return = ex.Message;
            // 에러 로그 기록
            ExceptionWebServiceHandling(ex, CF.ExceptionType.Error, _logEnt.RepererURL);
        }
        finally
        {
            // 성능 로그 기록(주 메써드의 마지막에 호출되어야 함.)
            PerformanceWebServiceLogWrite(true);
        }
        return _return;

    }

    #region MOBILE 모바일 재기안
    /// <summary>
    /// 모바일 재기안(KCC요청). (2017-01-18 by 박은선)
    /// </summary>
    /// <param name="LoginID"></param>
    /// <param name="ApprDocPID"></param>
    /// <param name="ApprDocWID"></param>
    /// <param name="ApproverID"></param>
    /// <param name="Comment"></param>
    /// <param name="signImagePath"></param>
    /// <param name="Approvers"></param>
    /// <returns></returns>
    [WebMethod(Description = "모바일 재기안")]
    public string MobileReDraft(string LoginID, string ApprDocPID, string ApprDocWID, string ApproverID,
                            string Comment, string signImagePath = "", params string[] Approvers)
    {

        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        string sApvLine = string.Empty;
        string fmpf = string.Empty;
        string fmrv = string.Empty;
        string fmid = string.Empty;
        string fiid = string.Empty;
        string scid = string.Empty;

        LoginID = LoginID.Trim();
        ApprDocPID = ApprDocPID.Trim();
        ApprDocWID = ApprDocWID.Trim();
        ApproverID = ApproverID.Trim();

        Comment = Comment.Trim();

        string strApprovers = "";
        if (Approvers != null)
        {
            foreach (string approver in Approvers)
            {
                strApprovers += approver + "|";
            }
        }

        try
        {

            //보류 아이코 없애기
            //보류시 목록에 아이콘 표시 - WfProcessInstance.description 에 update
            CfnEntityClasses.WfProcessInstance PInstance = null;
            CfnDatabaseManager.WfDBManager oDBMgr = null;
            CfnCoreEngine.WfProcessManager oPMgr = null;
            try
            {
                oPMgr = new CfnCoreEngine.WfProcessManager();
                PInstance = oPMgr.GetEntity(ApprDocPID);
                XmlDocument oXML = new XmlDocument();
                oXML.LoadXml(PInstance.description);

                XmlNode oForminfo = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                if (oForminfo.Attributes.GetNamedItem("reserve") != null) oForminfo.Attributes.GetNamedItem("reserve").Value = "0";
                oDBMgr = new CfnDatabaseManager.WfDBManager();
                oDBMgr.ChangeProperty("WfProcessInstance", ApprDocPID, "description", oXML.OuterXml);
                //보류 아이콘 해제 끝


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (PInstance != null)
                {
                    PInstance = null;
                }
                if (oDBMgr != null)
                {
                    oDBMgr.Dispose();
                    oDBMgr = null;
                }
                if (oPMgr != null)
                {
                    oPMgr.Dispose();
                    oPMgr = null;
                }
            }

            string sSpName = "dbo.usp_wf_approvedmobile";
            int iReturn = 0;


            //if (ApprovalLineXml != "")
            //{
            //    ApprovalLineXml = SetMobileAtt(ApprovalLineXml);
            //}
            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {

                DataPack INPUT = new DataPack();
                INPUT.add("@wiid", ApprDocWID);
                INPUT.add("@pfid", ApprDocPID);
                INPUT.add("@ptid", ApproverID);
                INPUT.add("@usid", LoginID);
                INPUT.add("@actionindex", "1");
                INPUT.add("@actioncomment", Comment);
                INPUT.add("@apvlist", "");
                INPUT.add("@signimagetype", signImagePath);//20150917 수정 -> 지정된 사인이미지로 저장
                INPUT.add("@approvers", strApprovers);


                iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
            }
            sbXml.Append("OK");
        }
        catch (System.Exception ex)
        {
            sbXml.Append("<WebService><LIST><Error>");
            sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
            sbXml.Append("</Error></LIST></WebService>");
        }
        finally
        {
            sReturn = sbXml.ToString();
            sbXml = null;
        }
        return sReturn;


    }
    #endregion

    #region MOBILE 모바일 일괄결재
    /// <summary>
    /// 모바일 일괄결재(KCC요청). (2017-03-10 by 박은선)
    /// </summary>
    /// <param name="LoginID"></param>
    /// <param name="ApprDocPID"></param>
    /// <param name="ApprDocWID"></param>
    /// <param name="ApproverID"></param>
    /// <param name="Comment"></param>
    /// <param name="signImagePath"></param>
    /// <param name="Approvers"></param>
    /// <returns></returns>
    [WebMethod(Description = "모바일 일괄결재")]
    public string MobileApprovedBatch(string LoginID, string[] ApprDocWIDList, string ApproverID, string signImagePath = "")
    {
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        string sApvLine = string.Empty;
        string fmpf = string.Empty;
        string fmrv = string.Empty;
        string fmid = string.Empty;
        string fiid = string.Empty;
        string scid = string.Empty;

        LoginID = LoginID.Trim();
        ApproverID = ApproverID.Trim();
        //string[] ApprDocWIDList = ApprDocWIDList1.Split(new char[] { ','});
        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
        {
            try
            {
                string sSpName = "dbo.usp_wf_approvedmobile";
                int iReturn = 0;

                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    for (int i = 0; i < ApprDocWIDList.Length; i++)
                    {
                        string ApprDocWID = ApprDocWIDList[i];

                        DataPack INPUT = new DataPack();
                        INPUT.add("@wiid", ApprDocWID);
                        INPUT.add("@pfid", "");
                        INPUT.add("@ptid", ApproverID);
                        INPUT.add("@usid", LoginID);
                        INPUT.add("@actionindex", "1");
                        INPUT.add("@actioncomment", "");
                        INPUT.add("@apvlist", "");
                        INPUT.add("@signimagetype", signImagePath);//20150917 수정 -> 지정된 사인이미지로 저장
                        INPUT.add("@approvers", "");
                        INPUT.add("@approvaloucode", "");

                        iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
                    }
                }
                sbXml.Append("OK");
                scope.Complete();
            }
            catch (System.Exception ex)
            {
                sbXml.Append("<WebService><LIST><Error>");
                sbXml.Append("<![CDATA[").Append(ex.Message).Append("\r\n").Append(ex.StackTrace).Append("]]>");
                sbXml.Append("</Error></LIST></WebService>");
            }
            finally
            {
                sReturn = sbXml.ToString();
                sbXml = null;
            }
        }
        return sReturn;
    }
    #endregion

    #region MOBILE 세부 함수

    // 지정반려 처리. CoviWeb\Approval\ApvProcess\ApvActBasic.aspx 참고
    private string UpdateApprLine(string LoginID, string RtnLoginID, string ApprDocPID, string ApprDocWID)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();

        bool bAdd = false; //지정반송 반송대상 추가
        string sApvList = "";
        try
        {
            string sSpName = "dbo.usp_wf_getApprLine";
            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                INPUT.add("@ApprDocPID", ApprDocPID);
                //INPUT.add("@ApprDocWID", ApprDocWID);
                oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                oDS.DataSetName = "ApvList";
            }

            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                oDS.Tables[0].TableName = "DATA";
                XmlDocument oApprLineXML = new XmlDocument();
                oApprLineXML.LoadXml(oDS.GetXml());
                XmlNode oRoot = oApprLineXML.DocumentElement;
                XmlDocument oApvListXML = new System.Xml.XmlDocument();
                oApvListXML.LoadXml(oRoot.SelectSingleNode("DATA/DOMAIN_DATA_CONTEXT").InnerText);
                string sBZState = GetReadMode(oRoot.SelectSingleNode("DATA/BUSINESS_STATE").InnerText);
                XmlNode oLastStep = null;
                XmlNode oOU = null;
                XmlNodeList oSteps = null;

                if (sBZState == "RECAPPROVAL")
                {
                    oLastStep = oApvListXML.SelectSingleNode("steps/division[taskinfo/@status='pending']/step[ .//taskinfo/@kind!='review' and .//taskinfo/@kind!='skip' and ( .//taskinfo/@status='inactive' )]"); //@routetype='approve' andand .//taskinfo/@kind!='bypass' 
                    oSteps = oApvListXML.SelectNodes("steps/division[taskinfo/@status='pending']/step[@routetype='approve'  and  .//taskinfo/@kind!='review' and .//taskinfo/@kind!='bypass'  and .//taskinfo/@kind!='skip' and (.//taskinfo/@status='completed' or .//taskinfo/@status='pending'  )]");
                }
                else if (sBZState == "SUBAPPROVAL")
                {
                    oOU = oApvListXML.SelectSingleNode("steps/division[taskinfo/@status='pending']/step[taskinfo/@status='pending']/ou[taskinfo/@status='pending' and taskinfo/@piid='" + ApprDocPID.ToUpper() + "']");
                    oLastStep = oApvListXML.SelectSingleNode("steps/division[taskinfo/@status='pending']/step[taskinfo/@status='pending' ]/ou[taskinfo/@status='pending' and taskinfo/@piid='" + ApprDocPID.ToUpper() + "']/(person|role)[taskinfo/@status='inactive']"); //taskinfo/@kind='normal' and (친전이 올 수 있음)
                    oSteps = oApvListXML.SelectNodes("steps/division[taskinfo/@status='pending']/step[taskinfo/@status='pending' ]/ou[taskinfo/@status='pending' and taskinfo/@piid='" + ApprDocPID.ToUpper() + "']/(person|role)[(taskinfo/@kind!='review' and taskinfo/@kind!='bypass'  and taskinfo/@kind!='skip' and taskinfo/@kind!='conveyance') and ( taskinfo/@status='completed' or taskinfo/@status='pending'  )]");
                }
                else
                {
                    oLastStep = oApvListXML.SelectSingleNode("steps/division[taskinfo/@status='pending']/step[ .//taskinfo/@kind!='review' and .//taskinfo/@kind!='skip' and ( .//taskinfo/@status='inactive' )]"); //@routetype='approve' andand .//taskinfo/@kind!='bypass' 
                    oSteps = oApvListXML.SelectNodes("steps/division[taskinfo/@status='pending']/step[@routetype='approve' and .//taskinfo/@kind!='review' and .//taskinfo/@kind!='bypass'  and .//taskinfo/@kind!='skip' and (.//taskinfo/@status='completed' or .//taskinfo/@status='pending'  )]");
                }

                XmlNode oRtnPerson = oApvListXML.SelectSingleNode("steps/division[taskinfo/@status='pending']/step/ou/person[@code='" + RtnLoginID + "' and  .//taskinfo/@status='completed']");
                string sRtnWiid = oRtnPerson.SelectSingleNode("taskinfo").Attributes.GetNamedItem("wiid").Value;

                XmlNode oStep = null;
                XmlNode oPerson = null;
                XmlNode oTaskInfo = null;

                for (int i = 0; i < oSteps.Count; i++)
                {
                    if (sBZState == "SUBAPPROVAL")
                    {
                        oStep = oSteps[i];
                        oPerson = oStep;
                    }
                    else
                    {
                        oStep = oSteps[i];
                        oPerson = oStep.SelectSingleNode("ou/person[.//taskinfo/@kind!='conveyance']");
                    }
                    oTaskInfo = oPerson.SelectSingleNode("taskinfo");


                    if (oTaskInfo.Attributes.GetNamedItem("wiid") != null &&
                        oTaskInfo.Attributes.GetNamedItem("wiid").Value.Equals(sRtnWiid) || bAdd == true)
                    {
                        XmlNode oCStep = oStep.CloneNode(true);
                        XmlNode oCTaskInfo;
                        if (sBZState == "SUBAPPROVAL")
                        {
                            oCTaskInfo = oCStep.SelectSingleNode("taskinfo");
                        }
                        else
                        {
                            XmlNode oCOU = oCStep.SelectSingleNode("ou");
                            oCTaskInfo = oCStep.SelectSingleNode("ou/person/taskinfo[@kind!='conveyance']");
                            //전달자들은 삭제 
                            XmlNodeList oRmvPerson = oCOU.SelectNodes("person[taskinfo/@kind='conveyance']");
                            for (int k = 0; k < oRmvPerson.Count; k++)
                            {
                                oCOU.RemoveChild(oRmvPerson[i]);
                            }
                        }

                        // 반려자의 속성중 daterejectedto, customattribute1, datecompleted는 엔진에 의해 생성되고 값이 할당된다.
                        string statusNresult = "inactive";
                        oCTaskInfo.Attributes.GetNamedItem("status").Value = statusNresult;
                        oCTaskInfo.Attributes.GetNamedItem("result").Value = statusNresult;
                        oCTaskInfo.Attributes.RemoveNamedItem("datecompleted");
                        oCTaskInfo.Attributes.RemoveNamedItem("customattribute1");
                        oCTaskInfo.Attributes.RemoveNamedItem("wiid");
                        oCTaskInfo.Attributes.RemoveNamedItem("visible");
                        oCTaskInfo.Attributes.RemoveNamedItem("rejectee");
                        oCTaskInfo.Attributes.RemoveNamedItem("datereceived");

                        XmlNode oComment = oCTaskInfo.SelectSingleNode("comment");
                        if (oComment != null)
                        {
                            oCTaskInfo.RemoveChild(oComment);
                        }
                        bAdd = true;
                        if (!oTaskInfo.Attributes.GetNamedItem("kind").Value.Equals("charge"))
                        {
                            if (oTaskInfo.Attributes.GetNamedItem("visible") == null)
                            {
                                XmlAttribute oAttr = oApvListXML.CreateAttribute("visible");
                                oTaskInfo.Attributes.SetNamedItem(oAttr);
                            }
                            oTaskInfo.Attributes.GetNamedItem("visible").Value = "n";
                        }
                        else
                        {// 기안자가 지정된 반려자인 경우
                            ApprToRjct = true;
                            break;
                        }

                        if (oTaskInfo.Attributes.GetNamedItem("rejectee") == null)
                        {
                            XmlAttribute oAttr = oApvListXML.CreateAttribute("rejectee");
                            oTaskInfo.Attributes.SetNamedItem(oAttr);
                        }
                        oTaskInfo.Attributes.GetNamedItem("rejectee").Value = "y";

                        if (oTaskInfo.Attributes.GetNamedItem("status").Value.Equals("pending"))
                        { //지정반송자
                            if (oTaskInfo.Attributes.GetNamedItem("daterejectedto") != null)
                                oTaskInfo.Attributes.GetNamedItem("daterejectedto").Value = DateTime.Now.ToString();

                        }

                        if (sBZState == "SUBAPPROVAL")
                        {
                            if (oLastStep == null)
                            {
                                oOU.AppendChild(oCStep);
                            }
                            else
                            {
                                oOU.InsertBefore(oCStep, oLastStep);
                            }
                        }
                        else
                        {
                            if (oLastStep != null)
                            {
                                oApvListXML.SelectSingleNode("steps/division[taskinfo/@status='pending']").InsertBefore(oCStep, oLastStep);
                            }
                            else
                            {
                                oApvListXML.SelectSingleNode("steps/division").AppendChild(oCStep);
                            }
                        }
                    }
                } // end for
                sApvList = oApvListXML.OuterXml;
            }
        }
        catch (System.Exception ex)
        {
            throw new System.Exception("UpdateApprLine", ex);
        }
        finally
        {

            if (oDS != null)
                oDS.Dispose();

            if (INPUT != null)
            {
                INPUT.Dispose();
            }
        }
        return sApvList;
    }


    private DataSet GetFormInfo(string prefix, string revision, string instanceID)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();

        try
        {
            string sSpName = "dbo.usp_wfform_getforminfo";
            using (SqlDacManager oDac = new SqlDacManager("FORM_INST_ConnectionString"))
            {
                INPUT.add("@fmpf", prefix);
                INPUT.add("@fmrv", revision);
                INPUT.add("@fiid", instanceID);
                oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                oDS.DataSetName = "RESPONSE";
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        return oDS;
    }

    private DateTime StringToDateTime(string mode, string strDateTime, DateTime orgDateTime)
    {
        int hour = 0;
        int minute = 0;
        int second = 0;

        if (mode.Equals("end"))
        {
            hour = 23;
            minute = 59;
            second = 59;
        }

        try
        {
            DateTime oDateTime = new DateTime(Convert.ToInt32(strDateTime.Substring(0, 4)),
                                            Convert.ToInt32(strDateTime.Substring(4, 2)),
                                            Convert.ToInt32(strDateTime.Substring(6, 2))
                                            , hour, minute, second);
            return oDateTime;
        }
        catch (System.Exception ex)
        {
        }
        return orgDateTime;
    }

    private string GetApprTypeCodeByRouteType(string routeType, string kind, string name)
    {
        string apprTypeCode = "1";
        routeType = routeType.ToLower();
        kind = kind.ToLower();
        name = name.ToLower();

        if (routeType.ToLower().Equals("approve"))
        {
            if (name.Equals("exttype"))
            {
                // 특이결재 - 7
                apprTypeCode = "7";
            }
            else
            {
                switch (kind)
                {
                    case "normal": // 결재 - 1
                        apprTypeCode = "1";
                        break;
                    case "authorize": // 전결 - 3
                        apprTypeCode = "3";
                        break;
                    case "review": // 후결 - 8
                        apprTypeCode = "8";
                        break;
                    case "skip":  // 결재안함 - 9
                        apprTypeCode = "9";
                        break;
                    default: //charge  // 기안 - 10
                        apprTypeCode = "10";
                        break;
                }
            }
        }
        else if (routeType.Equals("audit"))
        {
            switch (name)
            {
                case "exttype": // 특이결재 - 7
                    apprTypeCode = "7";
                    break;
                case "Deliberate": // 심의 - 5
                    apprTypeCode = "5";
                    break;
                case "request": // 신청 - 6
                    apprTypeCode = "6";
                    break;
                default: //confirm  // 확인 - 4
                    apprTypeCode = "4";
                    break;
            }
        }
        else //consult
        {
            // 합의 - 2
            apprTypeCode = "2";
        }
        return apprTypeCode;
    }

    private string GetApprYNCodeByName(string apprYNName)
    {
        string apprYNCode = "1";
        apprYNName = apprYNName.ToLower();

        switch (apprYNName)
        {
            case "pending":
                apprYNCode = "1";
                break;
            case "approved":
                apprYNCode = "2";
                break;
            case "agreed":
                apprYNCode = "3";
                break;
            case "reject":
                apprYNCode = "4";
                break;
            default: //inactive
                apprYNCode = "5";
                break;
        }
        return apprYNCode;
    }

    private string GetApprTypeCode(string sPiSate, string sWiSate, string sSubKind, string sPiBzStat, string sMode)
    {
        //'8'-Withdrawn
        string sApprTypeCode = "1";
        if (sMode != null && sMode.Equals("TEMPSAVE"))
        {
            sApprTypeCode = "8"; //'8'-Withdrawn
        }
        else
        {
            string PENDING = "288";
            string FINISHED = "528";

            sApprTypeCode = sSubKind;
        }
        return sApprTypeCode;
    }


    private string GetApprTypeNameByCode(string sKind)
    {

        //''1'-결재(전결, 확인 등)
        //'2'-합의,
        //'3'-수신(참조)
        //'4'-Review,
        //'5'-Assign(Inform),
        //'6'-Pending(진행),
        //'7'-sKind
        //'8'-Withdrawn

        string apprTypeName = "결재";
        switch (sKind)
        {
            case "1":
                apprTypeName = "Approve";
                break;
            case "2":
                apprTypeName = "Agree";
                break;
            case "3":
                apprTypeName = "Accept";
                break;
            case "4":
                apprTypeName = "Review";
                break;
            case "5":
                apprTypeName = "Assign";
                break;
            case "6":
                apprTypeName = "Pending";
                break;
            case "7":
                apprTypeName = "Return";
                break;
            case "T000"://결재
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_app", CF.LanguageType.ko); break;//Resources.Approval.lbl_app; break;
            case "T001"://시행
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_ITrans", CF.LanguageType.ko); break;//Resources.Approval.lbl_ITrans; break;
            case "T002"://시행
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_ITrans", CF.LanguageType.ko); break;//Resources.Approval.lbl_ITrans; break;
            case "T003"://직인
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_OfficialSeal", CF.LanguageType.ko); break;//Resources.Approval.lbl_OfficialSeal; break;
            case "T004"://협조
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_assist", CF.LanguageType.ko); break;//Resources.Approval.lbl_assist; break;
            case "T005"://후결
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_review", CF.LanguageType.ko); break;//Resources.Approval.lbl_review; break;
            case "T006"://열람
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_reading", CF.LanguageType.ko); break;//Resources.Approval.lbl_reading; break;
            case "T007":
                apprTypeName = CF.Dictionary.GetDic("경유", CF.LanguageType.ko); break;//"경유"; break;
            case "T008"://담당
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_charge", CF.LanguageType.ko); break;//Resources.Approval.lbl_charge; break;
            case "T009"://합의
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_consent", CF.LanguageType.ko); break;//Resources.Approval.lbl_consent; break;
            case "T010"://예고
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_doc_pre2", CF.LanguageType.ko); break;//Resources.Approval.lbl_doc_pre2; break;
            case "T011"://담당
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_charge", CF.LanguageType.ko); break;//Resources.Approval.lbl_charge; break;
            case "T012"://담당
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_charge", CF.LanguageType.ko); break;//Resources.Approval.lbl_charge; break;
            case "T013"://참조
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_cc", CF.LanguageType.ko); break;//Resources.Approval.lbl_cc; break;
            case "T014"://통지
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_notice2", CF.LanguageType.ko); break;//Resources.Approval.lbl_notice2; break;
            case "T015"://협조
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_assist", CF.LanguageType.ko); break;//Resources.Approval.lbl_assist; break;
            case "T016"://감사
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_audit", CF.LanguageType.ko); break;//Resources.Approval.lbl_audit; break;
            case "T017"://공람
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_audit2", CF.LanguageType.ko); break;//Resources.Approval.lbl_audit2; break;
            case "T018"://감사
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_PublicInspect", CF.LanguageType.ko); break;//Resources.Approval.lbl_PublicInspect; break;
            case "A"://품의함
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_completedBox", CF.LanguageType.ko); break;//Resources.Approval.lbl_completedBox; break;
            case "R"://수신
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_receive", CF.LanguageType.ko); break;//Resources.Approval.lbl_receive; break;
            case "S"://발신
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_send", CF.LanguageType.ko); break;//Resources.Approval.lbl_send; break;
            case "E"://접수
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_receive", CF.LanguageType.ko); break;//Resources.Approval.lbl_receive; break;
            case "REQCMP"://신청처리
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_receive", CF.LanguageType.ko); break;//Resources.Approval.lbl_receive; break;
            case "P"://발신
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_send", CF.LanguageType.ko); break;//Resources.Approval.lbl_send; break;
            case "SP"://열람
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_reading", CF.LanguageType.ko); break;//Resources.Approval.lbl_reading; break;
            case "C"://합의기안
            case "AS"://협조기안
            case "AD"://감사기안
            case "AE"://준법기안
                apprTypeName = CF.Dictionary.GetDic("lbl_apv_app", CF.LanguageType.ko); break;//Resources.Approval.btn_redraft; break;
            default: apprTypeName = sKind; break;
        }
        return apprTypeName;
    }

    private string GetReadMode(string strBZState)
    {
        string strReadMode = "";
        switch (strBZState)
        {
            case "03_01_02": //수신부서기안대기   "Receive"
                strReadMode = "REDRAFT";
                break;
            case "01_02_01": //"PersonConsult" 개인
            case "01_04_01":
            case "01_04_02":
                strReadMode = "PCONSULT";
                break;
            case "01_03_01": //감사 개인
                strReadMode = "AUDIT";
                break;
            case "01_01_02": //"RecApprove"
                strReadMode = "RECAPPROVAL";
                break;
            case "03_01_03": //협조부서
            case "03_01_04": //"SubRedraft" 합의부서기안대기
            case "03_01_05": //감사 부서 대기
                strReadMode = "SUBREDRAFT";
                break;
            case "01_01_04": //"SubApprove" 부서결재
            case "01_02_02":
            case "01_01_05":
            case "01_03_02":
            case "01_03_03":
            case "01_02_03":
                strReadMode = "SUBAPPROVAL";
                break;
            case "02_02_01":   //기안부서 반려"Reject"
            case "02_02_02":
            case "02_02_03":
            case "02_02_04":
            case "02_02_05":
            case "02_02_06":
            case "02_02_07":
                strReadMode = "REJECT";
                break;
            case "03_01_06": //"Charge"
                strReadMode = "CHARGE";
                break;
            default:
                break;
        }
        return strReadMode;
    }
    private string convertKindToSignTypeByRTnUT(string sKind, string sParentUT, string sRT, string sUT, string customattribute2)
    {
        string sSignType = " ";
        string scustomattribute2 = (customattribute2 == "") ? "" : customattribute2;
        if (scustomattribute2 == "ExtType")
        {
            //sSignType = Resources.Approval.lbl_ExtType;
        }
        else if (scustomattribute2 == "audit_law")
        {
            //sSignType = Resources.Approval.lbl_person_audit2;
        }
        else
        {
            switch (sRT)
            {
                case "receive":
                    switch (sUT)
                    {
                        case "ou":
                            switch (sParentUT)
                            {
                                //case "ou": sSignType = Resources.Approval.lbl_ChargeDept; break;
                                case "person": sSignType = convertKindToSignType(sKind, scustomattribute2); break;
                            }
                            break;
                        case "role":
                        case "person":
                            //sSignType = Resources.Approval.lbl_receive; break;
                            sSignType = ""; break;
                        case "group":
                            //sSignType = Resources.Approval.lbl_receive; break;
                            sSignType = ""; break;
                    }
                    break;
                case "consult":
                    switch (sUT)
                    {
                        case "ou":
                            switch (sParentUT)
                            {
                                case "ou": //sSignType = Resources.Approval.lbl_DeptConsent; break;
                                case "role":
                                case "person": sSignType = convertKindToSignType(sKind, scustomattribute2); break;
                            }
                            break;
                        case "role":
                        case "person":
                            //sSignType = Resources.Approval.lbl_PersonConsent; break;
                            sSignType = ""; break;
                    }
                    break;
                case "assist":
                    switch (sUT)
                    {
                        case "ou":
                            switch (sParentUT)
                            {
                                //case "ou": sSignType = Resources.Approval.lbl_DeptAssist; break;
                                case "role":
                                case "person": sSignType = convertKindToSignType(sKind, scustomattribute2); break;
                            }
                            break;
                        case "role":
                        case "person":
                            //sSignType= Resources.Approval.lbl_assist ;break;
                            sSignType = scustomattribute2; break;
                    }
                    break;
                case "audit":
                    switch (sUT)
                    {
                        case "ou":
                            switch (sParentUT)
                            {
                                //case "ou": sSignType = (scustomattribute2 == "" ? Resources.Approval.lbl_audit : scustomattribute2); break;
                                case "role":
                                case "person": sSignType = convertKindToSignType(sKind, scustomattribute2); break;
                            }
                            break;
                        case "role":
                        case "person":
                            //sSignType = Resources.Approval.lbl_audit; break;
                            sSignType = ""; break;
                    }
                    break;
                case "review":
                //sSignType = Resources.Approval.lbl_PublicInspect; break;
                case "notify":
                //sSignType = Resources.Approval.lbl_SendInfo; break;
                case "approve":
                    switch (sUT)
                    {
                        case "role":
                        case "person":
                            sSignType = convertKindToSignType(sKind, scustomattribute2); break;
                        case "ou":
                            //sSignType = Resources.Approval.lbl_DeptApprv; break;
                            sSignType = ""; break;
                    }
                    break;
            }
        }
        return sSignType;
    }
    private string convertKindToSignType(string sKind, string customattribute2)
    {
        string sSignType;
        string scustomattribute2 = (customattribute2 == "") ? "" : customattribute2;
        switch (sKind)
        {
            case "normal":
            //sSignType = Resources.Approval.lbl_normalapprove; break;
            case "consent":
            //sSignType = Resources.Approval.lbl_investigation; break;
            case "authorize":
            //sSignType = Resources.Approval.lbl_authorize; break;
            case "substitute":
            //sSignType = Resources.Approval.lbl_substitue; break;
            case "review":
            //sSignType = Resources.Approval.lbl_review; break;
            case "bypass":
            //sSignType = Resources.Approval.lbl_bypass; break;
            case "charge":
            //sSignType = Resources.Approval.lbl_charge; break;
            case "confidential":
            //sSignType = Resources.Approval.lbl_Confidential; break;
            case "conveyance":
            //sSignType = Resources.Approval.lbl_forward; break;
            case "skip":
            //sSignType = Resources.Approval.lbl_NoApprvl + " " + scustomattribute2; break;
            case "confirm":
            //sSignType = Resources.Approval.lbl_Confirm; break;
            default:
                sSignType = " "; break;
        }
        return sSignType;
    }
    private string convertSignResult(string sResult, string sKind, string customattribute2)
    {
        string sSignResult = " ";

        switch (sResult)
        {
            case "inactive":
                //sSignResult = Resources.Approval.lbl_inactive; break;
                sSignResult = ""; break;
            case "pending":
                //sSignResult = Resources.Approval.lbl_inactive; break;
                sSignResult = ""; break;
            case "reserved":
                //sSignResult = Resources.Approval.lbl_hold; break;
                sSignResult = ""; break;
            case "completed":
                if (customattribute2 == "ExtType")
                {
                    //sSignResult = Resources.Approval.lbl_ExtType_agree;
                    sSignResult = ""; break;
                }
                else
                {
                    //sSignResult = (sKind == "charge") ? Resources.Approval.btn_draft : Resources.Approval.lbl_app;
                    sSignResult = ""; break;
                }
            case "rejected":
                if (customattribute2 == "ExtType")
                {
                    //sSignResult = Resources.Approval.lbl_ExtType_disagree;
                    sSignResult = ""; break;
                }
                else
                {
                    //sSignResult = Resources.Approval.lbl_reject;
                    sSignResult = "";
                }
                break;
            case "rejectedto":
                //sSignResult = Resources.Approval.lbl_reject; break;
                sSignResult = ""; break;
            case "authorized":
                //sSignResult = Resources.Approval.lbl_authorize; break;
                sSignResult = ""; break;
            case "reviewed":
                //sSignResult = Resources.Approval.lbl_review; break;
                sSignResult = ""; break;
            case "substituted":
                //sSignResult = Resources.Approval.lbl_substitue; break;
                sSignResult = ""; break;
            case "agreed":
                if (customattribute2 == "ExtType")
                {
                    //sSignResult = Resources.Approval.lbl_ExtType_agree;
                }
                else
                {
                    // sSignResult = Resources.Approval.lbl_consent;
                }
                break;
            case "disagreed":
                if (customattribute2 == "ExtType")
                {
                    //sSignResult = Resources.Approval.lbl_ExtType_disagree;
                    sSignResult = "";
                }
                else
                {
                    //sSignResult = Resources.Approval.lbl_disagree;
                    sSignResult = "";
                }
                break;
            case "bypassed":
                //sSignResult = Resources.Approval.lbl_bypass; break;
                sSignResult = ""; break;
            case "skipped":
                //sSignResult = Resources.Approval.lbl_NoApprvl; break;
                sSignResult = ""; break;
            case "confirmed":
                //sSignResult = Resources.Approval.lbl_confirmed; break;
                sSignResult = ""; break;
            default:
                sSignResult = " ";
                break;
        }
        return sSignResult;
    }
    private DataSet GetCirculationInfo(string fiid, string piid, string kind)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();

        try
        {
            string sSpName = "dbo.usp_wfform_getcirculation_view_new";
            using (SqlDacManager oDac = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                INPUT.add("@piid", piid);
                INPUT.add("@fiid", fiid);
                INPUT.add("@kind", kind);
                oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                oDS.DataSetName = "RESPONSE";
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        return oDS;
    }


    #endregion

    #endregion

    #region LGCNS Mobile 1차
    /// <summary>
    /// HLDS 추가 사항
    /// 2010.12 yu2mi
    /// 결재건수 조회
    /// </summary>
    [WebMethod]
    public string ApprovalCount(string ID, string Reserved1, string Reserved2, string Reserved3, string Reserved4)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        String sApprovalCount = "0";
        String sReturnCode = "9";
        String sReturnDesc = "Error";
        try
        {
            using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
            {
                INPUT.add("@USER_ID", ID);
                oDS = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wf_approvalcount", INPUT);
                oDS.DataSetName = "ApprovalCount";
                if (oDS != null && oDS.Tables.Count > 0)
                {
                    if (oDS.Tables[0].Rows[0]["APPROVAL"] != null)
                    {
                        sApprovalCount = oDS.Tables[0].Rows[0]["APPROVAL"].ToString();
                    }
                }
            }
            sReturnCode = "0";
            sReturnDesc = "Success";
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            sbXml.Append("<ApprovalCount>");
            sbXml.Append("<ReturnDesc>").Append(sReturnDesc).Append("</ReturnDesc>");
            sbXml.Append("<ReturnCode>").Append(sReturnCode).Append("</ReturnCode>");
            sbXml.Append("<EmpNo>").Append(ID).Append("</EmpNo>");
            sbXml.Append("<ApprCount>").Append(sApprovalCount).Append("</ApprCount>");
            sbXml.Append("<Reserved1></Reserved1>");
            sbXml.Append("<Reserved2></Reserved2>");
            sbXml.Append("<Reserved3></Reserved3>");
            sbXml.Append("<Reserved4></Reserved4>");
            sbXml.Append("</ApprovalCount>");
            sReturn = sbXml.ToString();
            sbXml = null;
            if (INPUT != null) INPUT.Dispose();
            if (oDS != null) oDS.Dispose();
        }
        return sReturn;
    }

    /// <summary>
    /// HLDS 추가 사항
    /// 2010.12 yu2mi
    /// 결재건수 조회
    /// Return Type Strint => XmlDocument 로 수정 (2011-02-15 leesh)
    /// </summary>
    [WebMethod]
    public XmlDocument ApprovalCountXml(string ID, string Reserved1, string Reserved2, string Reserved3, string Reserved4)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        String sApprovalCount = "0";
        String sReturnCode = "9";
        String sReturnDesc = "Error";
        XmlDocument xReturn = new XmlDocument();
        try
        {
            using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
            {
                INPUT.add("@USER_ID", ID);
                oDS = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wf_approvalcount", INPUT);
                oDS.DataSetName = "ApprovalCount";
                if (oDS != null && oDS.Tables.Count > 0)
                {
                    if (oDS.Tables[0].Rows[0]["APPROVAL"] != null)
                    {
                        sApprovalCount = oDS.Tables[0].Rows[0]["APPROVAL"].ToString();
                    }
                }
            }
            sReturnCode = "0";
            sReturnDesc = "Success";

        }
        catch (System.Exception ex)
        {

        }
        finally
        {
            sbXml.Append("<ApprovalCount>");
            sbXml.Append("<ReturnDesc>").Append(sReturnDesc).Append("</ReturnDesc>");
            sbXml.Append("<ReturnCode>").Append(sReturnCode).Append("</ReturnCode>");
            sbXml.Append("<EmpNo>").Append(ID).Append("</EmpNo>");
            sbXml.Append("<ApprCount>").Append(sApprovalCount).Append("</ApprCount>");
            sbXml.Append("<Reserved1></Reserved1>");
            sbXml.Append("<Reserved2></Reserved2>");
            sbXml.Append("<Reserved3></Reserved3>");
            sbXml.Append("<Reserved4></Reserved4>");
            sbXml.Append("</ApprovalCount>");

            xReturn.LoadXml(sbXml.ToString());

            sbXml = null;
            if (INPUT != null) INPUT.Dispose();
            if (oDS != null) oDS.Dispose();
        }
        return xReturn;
    }

    /// <summary>
    /// HLDS 추가 사항
    /// 2010.12 yu2mi
    /// 승인(할) 목록조회
    /// </summary>
    [WebMethod]
    public string ApprovalList(string ID, string ApprApproveStatusCode, string FromDate, string ToDate, string Reserved1, string Reserved2, string Reserved3, string Reserved4)
    {
        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        StringBuilder sbSubXml = new StringBuilder();
        StringBuilder sbURL = new StringBuilder();


        String sApprovalCount = "0";
        String sReturnCode = "9";
        String sReturnDesc = "Error";

        // 1753년 1월 1일 오전 12:00:00
        DateTime dtStart = new DateTime(1753, 1, 1, 0, 0, 0, 0);
        DateTime dtEnd = DateTime.MaxValue;

        try
        {

            string mode = "start";
            if (FromDate != null && FromDate != string.Empty)
            {
                dtStart = StringToDateTime(mode, FromDate, dtStart);
            }

            mode = "end";
            if (ToDate != null && ToDate != string.Empty)
            {
                dtEnd = StringToDateTime(mode, ToDate, dtEnd);
            }

            if (ID != string.Empty)
            {
                if (ApprApproveStatusCode == "01")
                {
                    string sSpName = "dbo.usp_wf_worklistquery01mobile_ApprovalList01";

                    using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                    {
                        INPUT.add("@USER_ID", ID);
                        oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDS.DataSetName = "MON_ApprovalList";
                    }
                }
                else if (ApprApproveStatusCode == "02")
                {
                    string sSpName = "dbo.usp_wf_worklistquery01mobile_ApprovalList02";

                    using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                    {
                        INPUT.add("@USER_ID", ID);
                        INPUT.add("@StartDate", dtStart);
                        INPUT.add("@EndDate", dtEnd);
                        oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDS.DataSetName = "MON_ApprovalList";
                    }
                }
            }

            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                oDS.Tables[0].TableName = "MON_ApprovalListDocument";

                oReturnXML.LoadXml(oDS.GetXml());

                //변환하기
                XmlNode oRoot = oReturnXML.DocumentElement;
                XmlNodeList oList = oRoot.SelectNodes("MON_ApprovalListDocument");

                String sApprSystemID = "";// ConfigurationManager.AppSettings["ApprSystemID"].ToString();//"13";
                String sApprSystemName = "";// ConfigurationManager.AppSettings["ApprSystemName"].ToString();// "일반결재";
                String sApprDocID = "";
                String sApprTypeID = "";
                String sApprCategory = "";
                String sApprRequestProgress = "";
                String sApprRequestStatus = "";
                foreach (System.Xml.XmlNode oNode in oList)
                {

                    // 문서종류코드, 문서종류명	

                    XmlNode oFNode = null;
                    if (oNode.SelectSingleNode("PI_DSCR").InnerText != null &&
                            oNode.SelectSingleNode("PI_DSCR").InnerText.Trim().Length > 0)
                    {
                        XmlDocument oXML = new System.Xml.XmlDocument();
                        oXML.LoadXml(oNode.SelectSingleNode("PI_DSCR").InnerText);
                        oFNode = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");

                        if (oFNode != null)
                        {
                            //sApprDocID = oFNode.Attributes["instanceid"].Value;
                            sApprTypeID = oFNode.Attributes["prefix"].Value;
                            sApprCategory = oFNode.Attributes["name"].Value;

                            #region URL
                            sbURL.Remove(0, sbURL.Length);
                            //sbURL.Append("http://").Append(ConfigurationManager.AppSettings["LinKURL"].ToString());
                            sbURL.Append("/WebSite/Approval/Forms/Form.aspx?mobileyn=Y");
                            sbURL.Append("&piid=").Append(oNode.SelectSingleNode("ApprDocPID").InnerText);
                            sbURL.Append("&wiid=").Append(oNode.SelectSingleNode("ApprDocWID").InnerText);
                            sbURL.Append("&fmpf=").Append(oFNode.Attributes["prefix"].Value);
                            sbURL.Append("&fmrv=").Append(oFNode.Attributes["revision"].Value);
                            sbURL.Append("&fiid=").Append(oFNode.Attributes["instanceid"].Value);
                            sbURL.Append("&fmid=").Append(oFNode.Attributes["id"].Value);
                            sbURL.Append("&scid=").Append(oFNode.Attributes["schemaid"].Value);
                            #endregion
                        }
                    }

                    var sPiState = oNode.SelectSingleNode("PI_STATE").InnerText;
                    var sWiState = oNode.SelectSingleNode("WI_STATE").InnerText;
                    var sPiBusinessState = oNode.SelectSingleNode("BUSINESS_STATE").InnerText;
                    sApprDocID = oNode
                        .SelectSingleNode("ApprDocWID").InnerText;
                    if (sPiState == "288")
                    {
                        sApprRequestProgress = "진행중";
                        if (sWiState == "528")
                        {
                            if (sPiBusinessState.IndexOf("02_01") > -1) sApprRequestStatus = "승인";
                            else if (sPiBusinessState.IndexOf("") > -1) sApprRequestStatus = "반려";

                        }
                    }
                    else if (sPiState == "528")
                    {
                        sApprRequestProgress = "완료";
                        if (sWiState == "528")
                        {
                            if (sPiBusinessState.IndexOf("02_01") > -1) sApprRequestStatus = "승인";
                            else if (sPiBusinessState.IndexOf("") > -1) sApprRequestStatus = "반려";

                        }
                    }

                    sbSubXml.Append("<MON_ApprovalListDocument>");
                    sbSubXml.Append("<ApprSystemID>").Append(sApprSystemID).Append("</ApprSystemID>");
                    sbSubXml.Append("<ApprSystemName>").Append(sApprSystemName).Append("</ApprSystemName>");
                    sbSubXml.Append("<ApprDocID>").Append(sApprDocID).Append("</ApprDocID>");
                    sbSubXml.Append("<ApprTypeID>").Append(sApprTypeID).Append("</ApprTypeID>");
                    sbSubXml.Append("<ApprCategory>").Append(sApprCategory).Append("</ApprCategory>");
                    sbSubXml.Append("<Subject>").Append(oNode.SelectSingleNode("ApprDocTitle").InnerText).Append("</Subject>");
                    sbSubXml.Append("<LinkUrl>").Append(sbURL.ToString()).Append("</LinkUrl>");
                    sbSubXml.Append("<ApprRequestEmpNo>").Append(oNode.SelectSingleNode("DrafterLoginID").InnerText).Append("</ApprRequestEmpNo>");
                    sbSubXml.Append("<ApprRequestEmpName>").Append(oNode.SelectSingleNode("DrafterName").InnerText).Append("</ApprRequestEmpName>");
                    sbSubXml.Append("<ApprRequestEmpDept>").Append(oNode.SelectSingleNode("DraftDprtName").InnerText).Append("</ApprRequestEmpDept>");
                    sbSubXml.Append("<ApprRequestEmpTitle>").Append(oNode.SelectSingleNode("PositionName").InnerText).Append("</ApprRequestEmpTitle>");
                    sbSubXml.Append("<ApprRequestDate>").Append(oNode.SelectSingleNode("ApprRequestDate").InnerText).Append("</ApprRequestDate>");
                    sbSubXml.Append("<ApprApproveDate>").Append(oNode.SelectSingleNode("ApprApproveDate").InnerText).Append("</ApprApproveDate>");
                    sbSubXml.Append("<ApprRequestProgress>").Append(sApprRequestProgress).Append("</ApprRequestProgress>");
                    sbSubXml.Append("<ApprRequestStatus>").Append(sApprRequestStatus).Append("</ApprRequestStatus>");
                    sbSubXml.Append("<Reserved1></Reserved1>");
                    sbSubXml.Append("<Reserved2></Reserved2>");
                    sbSubXml.Append("<Reserved3></Reserved3>");
                    sbSubXml.Append("<Reserved4></Reserved4>");
                    sbSubXml.Append("</MON_ApprovalListDocument>");
                }
            }
            sReturnCode = "0";
            sReturnDesc = "Success";
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            sbXml.Append("<MON_ApprovalList>");
            sbXml.Append("<ReturnDesc>").Append(sReturnDesc).Append("</ReturnDesc>");
            sbXml.Append("<ReturnCode>").Append(sReturnCode).Append("</ReturnCode>");
            sbXml.Append("<ApprovalDocumentList>");
            sbXml.Append(sbSubXml.ToString());
            sbXml.Append("</ApprovalDocumentList>");
            sbXml.Append("</MON_ApprovalList>");
            sReturn = sbXml.ToString();
            sbSubXml = null;
            sbXml = null;
            if (INPUT != null) INPUT.Dispose();
            if (oDS != null) oDS.Dispose();
        }
        return sReturn;

    }
    /// <summary>
    /// HLDS 추가 사항
    /// 2010.12 yu2mi
    /// 요청내역 목록조회
    /// </summary>
    [WebMethod]
    public string ApprovalRequestList(string ID, string FromDate, string ToDate, string Reserved1, string Reserved2, string Reserved3, string Reserved4)
    {
        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        StringBuilder sbSubXml = new StringBuilder();
        StringBuilder sbURL = new StringBuilder();


        String sApprovalCount = "0";
        String sReturnCode = "9";
        String sReturnDesc = "Error";

        // 1753년 1월 1일 오전 12:00:00
        DateTime dtStart = new DateTime(1753, 1, 1, 0, 0, 0, 0);
        DateTime dtEnd = DateTime.MaxValue;

        try
        {

            string mode = "start";
            if (FromDate != null && FromDate != string.Empty)
            {
                dtStart = StringToDateTime(mode, FromDate, dtStart);
            }

            mode = "end";
            if (ToDate != null && ToDate != string.Empty)
            {
                dtEnd = StringToDateTime(mode, ToDate, dtEnd);
            }

            if (ID != string.Empty)
            {
                string sSpName = "dbo.usp_wf_worklistquery01mobile_ApprovalList03";

                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@USER_ID", ID);
                    INPUT.add("@StartDate", dtStart);
                    INPUT.add("@EndDate", dtEnd);
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    oDS.DataSetName = "MON_ApprovalRequestList";
                }
            }

            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                oDS.Tables[0].TableName = "MON_RequestDocument";

                oReturnXML.LoadXml(oDS.GetXml());

                //변환하기
                XmlNode oRoot = oReturnXML.DocumentElement;
                XmlNodeList oList = oRoot.SelectNodes("MON_RequestDocument");

                String sApprSystemID = "";//ConfigurationManager.AppSettings["ApprSystemID"].ToString();//"13";
                String sApprSystemName = "";//ConfigurationManager.AppSettings["ApprSystemName"].ToString();// "일반결재";
                String sApprDocID = "";
                String sApprTypeID = "";
                String sApprCategory = "";
                String sApprRequestProgress = "";
                String sApprRequestStatus = "";
                foreach (System.Xml.XmlNode oNode in oList)
                {

                    // 문서종류코드, 문서종류명	

                    XmlNode oFNode = null;
                    if (oNode.SelectSingleNode("PI_DSCR").InnerText != null &&
                            oNode.SelectSingleNode("PI_DSCR").InnerText.Trim().Length > 0)
                    {
                        XmlDocument oXML = new System.Xml.XmlDocument();
                        oXML.LoadXml(oNode.SelectSingleNode("PI_DSCR").InnerText);
                        oFNode = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");

                        if (oFNode != null)
                        {

                            sApprTypeID = oFNode.Attributes["prefix"].Value;
                            sApprCategory = oFNode.Attributes["name"].Value;

                            #region URL
                            sbURL.Remove(0, sbURL.Length);
                            //sbURL.Append("http://").Append(ConfigurationManager.AppSettings["LinKURL"].ToString());
                            sbURL.Append("/WebSite/Approval/Forms/Form.aspx?mobileyn=Y");
                            sbURL.Append("&piid=").Append(oNode.SelectSingleNode("ApprDocPID").InnerText);
                            sbURL.Append("&wiid=").Append(oNode.SelectSingleNode("ApprDocWID").InnerText);
                            sbURL.Append("&fmpf=").Append(oFNode.Attributes["prefix"].Value);
                            sbURL.Append("&fmrv=").Append(oFNode.Attributes["revision"].Value);
                            sbURL.Append("&fiid=").Append(oFNode.Attributes["instanceid"].Value);
                            sbURL.Append("&fmid=").Append(oFNode.Attributes["id"].Value);
                            sbURL.Append("&scid=").Append(oFNode.Attributes["schemaid"].Value);
                            #endregion
                        }
                    }

                    var sPiState = oNode.SelectSingleNode("PI_STATE").InnerText;
                    var sWiState = oNode.SelectSingleNode("WI_STATE").InnerText;
                    var sPiBusinessState = oNode.SelectSingleNode("BUSINESS_STATE").InnerText;
                    sApprDocID = oNode.SelectSingleNode("ApprDocWID").InnerText;

                    if (sPiState == "288")
                    {
                        sApprRequestProgress = "진행중";
                        if (sWiState == "528")
                        {
                            if (sPiBusinessState.IndexOf("02_01") > -1) sApprRequestStatus = "승인";
                            else if (sPiBusinessState.IndexOf("") > -1) sApprRequestStatus = "반려";

                        }
                    }
                    else if (sPiState == "528")
                    {
                        sApprRequestProgress = "완료";
                        if (sWiState == "528")
                        {
                            if (sPiBusinessState.IndexOf("02_01") > -1) sApprRequestStatus = "승인";
                            else if (sPiBusinessState.IndexOf("") > -1) sApprRequestStatus = "반려";

                        }
                    }

                    sbSubXml.Append("<MON_RequestDocument>");
                    sbSubXml.Append("<ApprSystemID>").Append(sApprSystemID).Append("</ApprSystemID>");
                    sbSubXml.Append("<ApprSystemName>").Append(sApprSystemName).Append("</ApprSystemName>");
                    sbSubXml.Append("<ApprDocID>").Append(sWiState).Append("</ApprDocID>");
                    sbSubXml.Append("<ApprTypeID>").Append(sApprTypeID).Append("</ApprTypeID>");
                    sbSubXml.Append("<ApprCategory>").Append(sApprCategory).Append("</ApprCategory>");
                    sbSubXml.Append("<Subject>").Append(oNode.SelectSingleNode("ApprDocTitle").InnerText).Append("</Subject>");
                    sbSubXml.Append("<LinkUrl>").Append(sbURL.ToString()).Append("</LinkUrl>");
                    sbSubXml.Append("<ApprRequestEmpNo>").Append(oNode.SelectSingleNode("DrafterLoginID").InnerText).Append("</ApprRequestEmpNo>");
                    sbSubXml.Append("<ApprRequestEmpName>").Append(oNode.SelectSingleNode("DrafterName").InnerText).Append("</ApprRequestEmpName>");
                    sbSubXml.Append("<ApprRequestEmpDept>").Append(oNode.SelectSingleNode("DraftDprtName").InnerText).Append("</ApprRequestEmpDept>");
                    sbSubXml.Append("<ApprRequestEmpTitle>").Append(oNode.SelectSingleNode("PositionName").InnerText).Append("</ApprRequestEmpTitle>");
                    sbSubXml.Append("<ApprRequestDate>").Append(oNode.SelectSingleNode("ApprRequestDate").InnerText).Append("</ApprRequestDate>");
                    sbSubXml.Append("<ApprApproveDate>").Append(oNode.SelectSingleNode("ApprApproveDate").InnerText).Append("</ApprApproveDate>");
                    sbSubXml.Append("<ApprRequestProgress>").Append(sApprRequestProgress).Append("</ApprRequestProgress>");
                    sbSubXml.Append("<ApprRequestStatus>").Append(sApprRequestStatus).Append("</ApprRequestStatus>");
                    sbSubXml.Append("<Reserved1></Reserved1>");
                    sbSubXml.Append("<Reserved2></Reserved2>");
                    sbSubXml.Append("<Reserved3></Reserved3>");
                    sbSubXml.Append("<Reserved4></Reserved4>");
                    sbSubXml.Append("</MON_RequestDocument>");
                }
            }
            sReturnCode = "0";
            sReturnDesc = "Success";
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            sbXml.Append("<MON_ApprovalRequestList>");
            sbXml.Append("<ReturnDesc>").Append(sReturnDesc).Append("</ReturnDesc>");
            sbXml.Append("<ReturnCode>").Append(sReturnCode).Append("</ReturnCode>");
            sbXml.Append("<ApprovalDocumentList>");
            sbXml.Append(sbSubXml.ToString());
            sbXml.Append("</ApprovalDocumentList>");
            sbXml.Append("</MON_ApprovalRequestList>");
            sReturn = sbXml.ToString();
            sbXml = null;
            if (INPUT != null) INPUT.Dispose();
            if (oDS != null) oDS.Dispose();
        }
        return sReturn;

    }
    /// <summary>
    /// HLDS 추가 사항
    /// 2010.12 yu2mi
    /// 결재항목 상세보기
    /// </summary>
    [WebMethod]
    public string ApprovaDetail(string ApprSystemID, string ID, string ApprTypeID, string ApprDocID, string ApprDocStatus, string Reserved1, string Reserved2, string Reserved3, string Reserved4)
    {
        DataSet oDS = new DataSet();
        XmlDocument oReturnXML = new XmlDocument();
        DataPack INPUT = new DataPack();
        DateTime dtStart = DateTime.MinValue;
        DateTime dtEnd = DateTime.MaxValue;
        String szURL = String.Empty;
        StringBuilder sb = new StringBuilder();
        String ApprDocPID = String.Empty;

        String sApprSystemID = "";//ConfigurationManager.AppSettings["ApprSystemID"].ToString();//"13";

        try
        {

            if (ApprDocID != string.Empty)//&& ApprDocWID != string.Empty
            {

                ApprDocID = ApprDocID.Trim();


                string sSpName = "dbo.usp_wf_piid";
                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@WORKITEM_ID", ApprDocID);
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                }
                if (oDS != null && oDS.Tables[0].Rows.Count > 0) ApprDocPID = oDS.Tables[0].Rows[0]["PROCESS_ID"].ToString();

                sSpName = "dbo.usp_wf_worklistquery01mobile_ApprovaDetail";
                using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                {
                    INPUT.add("@ApprDocPID", ApprDocPID);
                    INPUT.add("@ApprDocWID", ApprDocID);
                    oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    oDS.DataSetName = "RESPONSE";
                }
                if (oDS != null && oDS.Tables[0].Rows.Count == 0)
                {
                    using (SqlDacManager oDac = new SqlDacManager("INST_ARCHIVE_ConnectionString"))
                    {
                        oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDS.DataSetName = "RESPONSE";
                    }
                }
            }

            if (oDS != null && oDS.Tables[0].Rows.Count > 0)
            {
                oDS.Tables[0].TableName = "MON_ApprovaDetail";
                oReturnXML.LoadXml(oDS.GetXml());

                //변환하기
                XmlNode oRoot = oReturnXML.DocumentElement;
                XmlNodeList oList = oRoot.SelectNodes("MON_ApprovaDetail");
                foreach (System.Xml.XmlNode oNode in oList)
                {
                    #region 기안정보

                    // 문서종류코드, 문서종류명, 첨부파일 유무	

                    XmlDocument oXML = new System.Xml.XmlDocument();
                    XmlNode oFNode = null;
                    if (oNode.SelectSingleNode("PI_DSCR").InnerText != null &&
                            oNode.SelectSingleNode("PI_DSCR").InnerText.Trim().Length > 0)
                    {
                        oXML.LoadXml(oNode.SelectSingleNode("PI_DSCR").InnerText);
                        oFNode = oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                    }



                    XmlElement oReturnDesc = oReturnXML.CreateElement("ReturnDesc");
                    oReturnDesc.InnerText = "Success";
                    oNode.AppendChild(oReturnDesc);

                    XmlElement oReturnCode = oReturnXML.CreateElement("ReturnCode");
                    oReturnCode.InnerText = "0";
                    oNode.AppendChild(oReturnCode);

                    XmlElement oApprSystemID = oReturnXML.CreateElement("ApprSystemID");
                    oApprSystemID.InnerText = sApprSystemID;
                    oNode.AppendChild(oApprSystemID);

                    XmlElement oApprDocID = oReturnXML.CreateElement("ApprDocID");
                    oApprDocID.InnerText = oNode.SelectSingleNode("ApprDocID").InnerText;
                    oNode.AppendChild(oApprDocID);

                    XmlElement oApprSummary = oReturnXML.CreateElement("ApprSummary");
                    //appTypeName.InnerText = "";
                    //oNode.AppendChild(appTypeName);

                    XmlElement oApprRequestEmpNo = oReturnXML.CreateElement("ApprRequestEmpNo");
                    oApprRequestEmpNo.InnerText = oNode.SelectSingleNode("ApprRequestEmpNo").InnerText;
                    oNode.AppendChild(oApprRequestEmpNo);

                    XmlElement oApprRequestEmpName = oReturnXML.CreateElement("ApprRequestEmpName");
                    String sApprRequestEmpName = oNode.SelectSingleNode("ApprRequestEmpName").InnerText;
                    oApprRequestEmpName.InnerText = sApprRequestEmpName.Split(';')[1];
                    oNode.AppendChild(oApprRequestEmpName);

                    XmlElement oApprRequestEmpDept = oReturnXML.CreateElement("ApprRequestEmpDept");
                    String sApprRequestEmpDept = oNode.SelectSingleNode("ApprRequestEmpDept").InnerText;
                    oApprRequestEmpDept.InnerText = sApprRequestEmpDept.Split(';')[1];
                    oNode.AppendChild(oApprRequestEmpDept);

                    XmlElement oApprRequestEmpTitle = oReturnXML.CreateElement("ApprRequestEmpTitle");
                    String sApprRequestEmpTitle = oNode.SelectSingleNode("ApprRequestEmpTitle").InnerText;
                    oApprRequestEmpTitle.InnerText = sApprRequestEmpTitle.Split(';')[1];
                    oNode.AppendChild(oApprRequestEmpTitle);

                    XmlElement oApprRequestEmpEmail = oReturnXML.CreateElement("ApprRequestEmpEmail");
                    oApprRequestEmpEmail.InnerText = oNode.SelectSingleNode("ApprRequestEmpEmail").InnerText;
                    oNode.AppendChild(oApprRequestEmpEmail);

                    XmlElement oApprRequestEmpOffice = oReturnXML.CreateElement("ApprRequestEmpOffice");
                    oApprRequestEmpOffice.InnerText = oNode.SelectSingleNode("ApprRequestEmpOffice").InnerText;
                    oNode.AppendChild(oApprRequestEmpOffice);

                    XmlElement oApprRequestEmpMobile = oReturnXML.CreateElement("ApprRequestEmpMobile");
                    oApprRequestEmpMobile.InnerText = oNode.SelectSingleNode("ApprRequestEmpMobile").InnerText;
                    oNode.AppendChild(oApprRequestEmpMobile);

                    XmlElement oApprRequestDate = oReturnXML.CreateElement("ApprRequestDate");
                    oApprRequestDate.InnerText = oNode.SelectSingleNode("ApprRequestDate").InnerText.Replace("-", "").Replace(" ", "").Replace(":", "");
                    oNode.AppendChild(oApprRequestDate);

                    oNode.RemoveChild(oNode.SelectSingleNode("ApprDocID"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestEmpNo"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestEmpName"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestEmpDept"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestEmpTitle"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestEmpEmail"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestEmpOffice"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestEmpMobile"));
                    oNode.RemoveChild(oNode.SelectSingleNode("ApprRequestDate"));
                    oNode.RemoveChild(oNode.SelectSingleNode("PI_DSCR"));


                    // 첨부파일 유무
                    XmlNodeList oAttFiles = null;
                    // 관련문서 유무
                    String sAttDocs = "";

                    DataSet oFInfo = null;
                    if (oFNode != null)
                    {

                        oFInfo = GetFormInfo(oFNode.Attributes["prefix"].Value,
                                                        oFNode.Attributes["revision"].Value,
                                                        oFNode.Attributes["instanceid"].Value);
                        #region URL
                        //sb.Append("http://").Append(ConfigurationManager.AppSettings["LinKURL"].ToString());
                        sb.Append("/WebSite/Approval/Forms/Form.aspx?mobileyn=Y");
                        //sb.Append("&piid=").Append(ApprDocPID);
                        sb.Append("&wiid=").Append(ApprDocID);
                        sb.Append("&fmpf=").Append(oFNode.Attributes["prefix"].Value);
                        sb.Append("&fmrv=").Append(oFNode.Attributes["revision"].Value);
                        sb.Append("&fiid=").Append(oFNode.Attributes["instanceid"].Value);
                        sb.Append("&fmid=").Append(oFNode.Attributes["id"].Value);
                        sb.Append("&scid=").Append(oFNode.Attributes["schemaid"].Value);
                        #endregion
                    }

                    if (oFInfo != null)
                    {
                        XmlDocument oFInfoXML = new XmlDocument();
                        oFInfoXML.LoadXml(oFInfo.GetXml());
                        XmlDocument oAttFileXML = new System.Xml.XmlDocument();


                        if (oFInfoXML.SelectSingleNode("RESPONSE/Table/ATTACH_FILE_INFO") != null)
                        {
                            string atchFileList = oFInfoXML.SelectSingleNode("RESPONSE/Table/ATTACH_FILE_INFO").InnerText.Trim();
                            if (atchFileList.Length > 0)
                            {
                                oAttFileXML.LoadXml(atchFileList);
                                oAttFiles = oAttFileXML.SelectNodes("fileinfos/fileinfo/file");
                            }
                        }
                        else
                        {
                        }

                        #region HTML
                        if (oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT") != null)
                        {
                            //XmlCDataSection CDataHTML;
                            //CDataHTML = oReturnXML.CreateCDataSection(oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT").InnerText);
                            //bodyHtml.AppendChild(CDataHTML);

                            string bodyContent = oFInfoXML.SelectSingleNode("RESPONSE/Table/BODY_CONTEXT").InnerText.Trim();
                            oApprSummary.InnerText = bodyContent;


                            XmlDocument oBodyContent = new System.Xml.XmlDocument();

                            #region BODY_CONTEXT 정보 가져 오기

                            if (bodyContent.Length > 0)
                            {
                                oBodyContent.LoadXml(bodyContent);
                                //DOCLINKS
                                XmlNode oDOCLINKS = oBodyContent.SelectSingleNode("BODY_CONTEXT/DOCLINKS");
                                sAttDocs = oDOCLINKS.InnerText;
                            }
                            #endregion

                        }
                        else
                        {
                            oApprSummary.InnerText = "";
                        }
                        #endregion

                    }
                    else
                    {
                        //attachFileCount.InnerText = "0";

                    }

                    //oNode.AppendChild(attachFileCount);

                    // HTML
                    oNode.AppendChild(oApprSummary);
                    #endregion
                    #region 결재라인
                    string sSpName = "dbo.usp_wf_worklistquery01mobile_ApprovaDetail_apprline";
                    DataSet oDSR = null;
                    INPUT.Clear();
                    using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
                    {
                        INPUT.add("@ApprDocPID", ApprDocPID);
                        INPUT.add("@ApprDocWID", ApprDocID);
                        oDSR = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                        oDSR.DataSetName = "DSApprLine";
                    }

                    if (oDSR != null && oDSR.Tables.Count > 0)
                    {
                        oDSR.Tables[0].TableName = "ApproverEmpList";
                        XmlDocument oApprLineXML = new XmlDocument();
                        oApprLineXML.LoadXml(oDSR.GetXml());

                        XmlNode oApprLineRoot = oApprLineXML.DocumentElement;
                        XmlNodeList approvalList = oApprLineRoot.SelectNodes("ApproverEmpList");
                        //XmlElement approvalLine = oReturnXML.CreateElement("ApprovalLine");

                        //병렬 합의 차수 계산
                        String sRouteTypePre = "";
                        int iAL = 0;
                        foreach (System.Xml.XmlNode oApproval in approvalList)
                        {
                            XmlElement approval = oReturnXML.CreateElement("ApproverEmpList");

                            //ApprApproveEmpNo	string	Y	결재자 아이디
                            XmlElement oALApprApproveEmpNo = oReturnXML.CreateElement("ApprApproveEmpNo");
                            oALApprApproveEmpNo.InnerText = oApproval.SelectSingleNode("ApproverLoginID").InnerText;
                            approval.AppendChild(oALApprApproveEmpNo);

                            //ApprApproveEmpName	string	Y	결재자 이름
                            XmlElement oALApprApproveEmpName = oReturnXML.CreateElement("ApprApproveEmpName");
                            String sALApprApproveEmpName = oApproval.SelectSingleNode("ApproverName").InnerText;
                            oALApprApproveEmpName.InnerText = sALApprApproveEmpName.Split(';')[1];
                            approval.AppendChild(oALApprApproveEmpName);

                            //병렬 합의 차수 계산
                            String sRouteType = oApproval.SelectSingleNode("RouteType").InnerText;
                            if (sRouteType == "consult" && sRouteTypePre == "consult")
                            {
                            }
                            else
                            {
                                iAL++;
                            }
                            //ApprApproveOrder	int	Y	결재 차수
                            XmlElement oALApprApproveOrder = oReturnXML.CreateElement("ApprApproveOrder");
                            oALApprApproveOrder.InnerText = iAL.ToString();
                            approval.AppendChild(oALApprApproveOrder);

                            //병렬 합의 차수 계산
                            sRouteTypePre = sRouteType;

                            //ApprApproveEmpDept	string	Y	결재자 부서
                            XmlElement oALApprApproveEmpDept = oReturnXML.CreateElement("ApprApproveEmpDept");
                            oALApprApproveEmpDept.InnerText = oApproval.SelectSingleNode("ApprDprtCode").InnerText;
                            approval.AppendChild(oALApprApproveEmpDept);

                            //ApprApproveEmpTitle	string	Y	결재자직위
                            XmlElement oALApprApproveEmpTitle = oReturnXML.CreateElement("ApprApproveEmpTitle");
                            String sALApprApproveEmpTitle = oApproval.SelectSingleNode("PositionName").InnerText;
                            oALApprApproveEmpTitle.InnerText = sALApprApproveEmpTitle.Split(';')[1];
                            approval.AppendChild(oALApprApproveEmpTitle);

                            //ApprApproveEmpEmail	string	Y	결재자 이메일
                            XmlElement oALApprApproveEmpEmail = oReturnXML.CreateElement("ApprApproveEmpEmail");
                            oALApprApproveEmpEmail.InnerText = oApproval.SelectSingleNode("EMAIL").InnerText;
                            approval.AppendChild(oALApprApproveEmpEmail);

                            //ApprApproveEmpOffice	string	Y	결재자 전화번호(사무실)
                            XmlElement oALApprApproveEmpOffice = oReturnXML.CreateElement("ApprApproveEmpOffice");
                            oALApprApproveEmpOffice.InnerText = oApproval.SelectSingleNode("OFFICE_TEL").InnerText;
                            approval.AppendChild(oALApprApproveEmpOffice);

                            //ApprApproveEmpMobile	string	Y	결재자 휴대폰번호
                            XmlElement oALApprApproveEmpMobile = oReturnXML.CreateElement("ApprApproveEmpMobile");
                            oALApprApproveEmpMobile.InnerText = oApproval.SelectSingleNode("MOBILE_TEL").InnerText;
                            approval.AppendChild(oALApprApproveEmpMobile);

                            //ApprApproveDate	string	Y	결재완료시각
                            XmlElement oALApprApproveDate = oReturnXML.CreateElement("ApprApproveDate");
                            oALApprApproveDate.InnerText = oApproval.SelectSingleNode("ApprDateTime").InnerText.Replace("-", "").Replace(" ", "").Replace(":", "");
                            approval.AppendChild(oALApprApproveDate);

                            String sApprYNCode = oApproval.SelectSingleNode("ApprYNCode").InnerText;

                            String sALApprApproveType = "";
                            String sALApprType = "";
                            switch (sApprYNCode)
                            {
                                case "inactive":
                                    sALApprApproveType = "00"; //00 : 미요청
                                    break;
                                case "completed":
                                    sALApprApproveType = "01"; //01 : Approved
                                    break;
                                case "rejected":
                                    sALApprApproveType = "02"; //02 : Rejected
                                    break;
                                case "pending":
                                    sALApprApproveType = "05"; //05 : In Progress
                                    break;
                            }
                            switch (sRouteType)
                            {
                                case "approve":
                                    sALApprType = "01"; //01 : 결제
                                    break;
                                case "":
                                    sALApprType = "02"; //02 : 합의
                                    break;
                                case "consult":
                                    sALApprType = "03"; //03 : 병렬합의 1
                                    break;
                                case " ":
                                    sALApprType = "04"; //04 : 병렬합의 2
                                    break;
                            }

                            //ApprApproveType	string	Y	결재승인종류
                            XmlElement oALApprApproveType = oReturnXML.CreateElement("ApprApproveType");
                            oALApprApproveType.InnerText = sALApprApproveType;
                            approval.AppendChild(oALApprApproveType);

                            //ApprType	string	Y	결재처리구분
                            XmlElement oALApprType = oReturnXML.CreateElement("ApprType");
                            oALApprType.InnerText = sALApprType;
                            approval.AppendChild(oALApprType);

                            //ApprComment	string	Y	결재처리의견
                            XmlElement oALApprComment = oReturnXML.CreateElement("ApprComment");
                            if (oApproval.SelectSingleNode("Comment") != null)
                            {
                                oALApprComment.InnerText = oApproval.SelectSingleNode("Comment").InnerText;
                            }
                            approval.AppendChild(oALApprComment);

                            oNode.AppendChild(approval);
                        }

                        oDSR.Dispose();

                    }
                    #endregion
                    #region 첨부파일 목록
                    // XmlElement attachFileList = oReturnXML.CreateElement("AttachFileList");
                    if (oAttFiles != null)
                    {
                        foreach (System.Xml.XmlNode oAFileXML in oAttFiles)
                        {
                            XmlElement attachFile = oReturnXML.CreateElement("Attachments");

                            if (oAFileXML != null)
                            {
                                // FileID	stirng	Y	첨부파일id                          
                                XmlElement attachFileID = oReturnXML.CreateElement("FileID");
                                attachFileID.InnerText = oAFileXML.Attributes["location"].Value.Substring(oAFileXML.Attributes["location"].Value.LastIndexOf("/") + 1);
                                attachFile.AppendChild(attachFileID);

                                // FileName	string	Y	첨부파일명                      
                                XmlElement attachFileName = oReturnXML.CreateElement("FileName");
                                attachFileName.InnerText = oAFileXML.Attributes["name"].Value;
                                attachFile.AppendChild(attachFileName);

                                // IsEdms	string	Y	EDMS 에 위치에 있는지  --> 정보 없음 수정 필요                 
                                XmlElement attachIsEdms = oReturnXML.CreateElement("IsEdms");
                                attachIsEdms.InnerText = "";
                                attachFile.AppendChild(attachIsEdms);

                                // FileSize	int	Y	파일크기                  
                                XmlElement attachFileSize = oReturnXML.CreateElement("FileSize");
                                attachFileSize.InnerText = oAFileXML.Attributes["size"].Value;
                                attachFile.AppendChild(attachFileSize);

                                // FileUrl	int	Y	파일크기                  
                                XmlElement attachFileUrl = oReturnXML.CreateElement("FileUrl");
                                attachFileUrl.InnerText = oAFileXML.Attributes["location"].Value;
                                attachFile.AppendChild(attachFileUrl);

                                oNode.AppendChild(attachFile);
                            }
                        }
                    }
                    #endregion

                    #region 관련문서
                    if (sAttDocs.Length > 0)
                    {
                        //2013-07-10 YHM 연결문서정보 간소화
                        /*
                        string[] aDoclist = sAttDocs.Split('^');
                        for (int iDoc = 0; iDoc < aDoclist.Length; iDoc++)
                        {
                            XmlElement attachDoc = oReturnXML.CreateElement("ApprLinkDocs");
                            string[] aDoclist2 = aDoclist[iDoc].Split('@'); //@@@로 나누는 방법 못찾음 ㅡㅡ; [0],[3],[6]
                            if (aDoclist2[1] != null)
                            {
                                string[] aDoclist3 = aDoclist2[3].Split(';');
                                string sDocFormInfo = aDoclist3[0];


                                // ApprLinkDocName	string	Y	관련문서명
                                XmlElement attachApprLinkDocName = oReturnXML.CreateElement("ApprLinkDocName");
                                attachApprLinkDocName.InnerText = aDoclist2[6].ToString();
                                attachDoc.AppendChild(attachApprLinkDocName);

                                // ApprLinkDocUrl	string	Y	관련문서 URL

                                #region URL
                                StringBuilder sbDocUrl = new StringBuilder();
                                XmlDocument oDocFormInfo = new XmlDocument();
                                oDocFormInfo.LoadXml(sDocFormInfo);
                                oXML.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                                XmlNode oDocNode = null;
                                if (oDocFormInfo != null)
                                {
                                    oDocNode = oDocFormInfo.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo");
                                    sbDocUrl.Remove(0, sbDocUrl.Length);
                                    //sbDocUrl.Append("http://").Append(ConfigurationManager.AppSettings["LinKURL"].ToString());
                                    sbDocUrl.Append("/WebSite/Approval/Forms/Form.aspx?mode=COMPLETE");
                                    sbDocUrl.Append("&piid=").Append(aDoclist3[1]);
                                    sbDocUrl.Append("&wiid=").Append(aDoclist2[0]);
                                    sbDocUrl.Append("&fmpf=").Append(oDocNode.Attributes["prefix"].Value);
                                    sbDocUrl.Append("&fmrv=").Append(oDocNode.Attributes["revision"].Value);
                                    sbDocUrl.Append("&fiid=").Append(oDocNode.Attributes["instanceid"].Value);
                                    sbDocUrl.Append("&fmid=").Append(oDocNode.Attributes["id"].Value);
                                    sbDocUrl.Append("&scid=").Append(oDocNode.Attributes["schemaid"].Value);
                                }
                                #endregion

                                XmlElement attachApprLinkDocUrl = oReturnXML.CreateElement("ApprLinkDocUrl");
                                attachApprLinkDocUrl.InnerText = sbDocUrl.ToString();
                                attachDoc.AppendChild(attachApprLinkDocUrl);

                            }
                            oNode.AppendChild(attachDoc);
                        }
                        */
                        string[] aDoclist = sAttDocs.Split(new string[] { "^^^" }, StringSplitOptions.None);
                        for (int iDoc = 0; iDoc < aDoclist.Length; iDoc++)
                        {
                            XmlElement attachDoc = oReturnXML.CreateElement("ApprLinkDocs");
                            string[] aDoclist2 = aDoclist[iDoc].Split(new string[] { "@@@" }, StringSplitOptions.None);
                            // ApprLinkDocName	string	Y	관련문서명
                            XmlElement attachApprLinkDocName = oReturnXML.CreateElement("ApprLinkDocName");
                            attachApprLinkDocName.InnerText = aDoclist2[2].ToString();
                            attachDoc.AppendChild(attachApprLinkDocName);

                            // ApprLinkDocUrl	string	Y	관련문서 URL
                            #region URL
                            StringBuilder sbDocUrl = new StringBuilder();
                            sbDocUrl.Remove(0, sbDocUrl.Length);
                            sbDocUrl.Append("/WebSite/Approval/Forms/Form.aspx?mode=COMPLETE");
                            sbDocUrl.Append("&piid=").Append(aDoclist2[0]);
                            #endregion

                            XmlElement attachApprLinkDocUrl = oReturnXML.CreateElement("ApprLinkDocUrl");
                            attachApprLinkDocUrl.InnerText = sbDocUrl.ToString();
                            attachDoc.AppendChild(attachApprLinkDocUrl);

                            oNode.AppendChild(attachDoc);
                        }
                    }
                    #endregion


                    #region 결재문서URL
                    //XmlElement ReturnURL = oReturnXML.CreateElement("ReturnURL");
                    //XmlElement ApprDocURL = oReturnXML.CreateElement("ApprDocURL");
                    //szURL = sb.ToString();
                    //sb = null;
                    //XmlCDataSection CData;
                    //CData = oReturnXML.CreateCDataSection(szURL);
                    //ApprDocURL.AppendChild(CData);
                    ////ApprDocURL.InnerText = szURL;
                    //ReturnURL.AppendChild(ApprDocURL);
                    //oNode.AppendChild(ReturnURL);
                    #endregion                }

                }
            }
            return oReturnXML.SelectSingleNode("RESPONSE").InnerXml;
        }
        catch (Exception ex)
        {
            StringBuilder sbXml = new StringBuilder();

            String sReturnCode = "9";
            String sReturnDesc = "Error";

            sbXml.Append("<MON_ApprovaDetail>");
            sbXml.Append("<ReturnDesc>").Append(sReturnDesc).Append("</ReturnDesc>");
            sbXml.Append("<ReturnCode>").Append(sReturnCode).Append("</ReturnCode>");
            sbXml.Append("</MON_ApprovaDetail>");
            return sbXml.ToString();
        }
        finally
        {
            oDS.Dispose();
        }
    }

    /// <summary>
    /// HLDS 추가 사항
    /// 2010.12 yu2mi
    /// 요청내역 목록조회
    /// </summary>
    [WebMethod]
    public string ApprovalProcess(string ApprSystemID, string ID, string ApprTypeID, string ApprDocID,
        string ApprType, string ApprComment, string Reserved1, string Reserved2, string Reserved3, string Reserved4)
    {
        String sApprovalCount = "0";
        String sReturnCode = "9";
        String sReturnDesc = "Error";

        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();
        string APPROVE = "1";
        string REJECT = "2";
        string sApvLine = string.Empty;


        ID = ID.Trim();
        string ApprDocPID = string.Empty;
        ApprDocID = ApprDocID.Trim();
        string ApproverID = string.Empty;
        ApprType = ApprType.Trim();
        ApprComment = ApprComment.Trim();

        try
        {
            string sSpName = "dbo.usp_wf_ApprovalProcess";
            int iReturn = 0;


            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                DataPack INPUT = new DataPack();
                INPUT.add("@wiid", ApprDocID);
                INPUT.add("@usid", ID);
                INPUT.add("@actionindex", ApprType);
                INPUT.add("@actioncomment", ApprComment);
                INPUT.add("@apvlist", sReturn);

                iReturn = oDac.ExecuteNonQuery(CommandType.StoredProcedure, sSpName, INPUT);
            }
            sReturnCode = "0";
            sReturnDesc = "Success";
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            sbXml.Append("<ApprovalCount>");
            sbXml.Append("<ReturnDesc>").Append(sReturnDesc).Append("</ReturnDesc>");
            sbXml.Append("<ReturnCode>").Append(sReturnCode).Append("</ReturnCode>");
            sbXml.Append("<EmpNo>").Append(ID).Append("</EmpNo>");
            sbXml.Append("<ApprCount>").Append(sApprovalCount).Append("</ApprCount>");
            sbXml.Append("<Reserved1></Reserved1>");
            sbXml.Append("<Reserved2></Reserved2>");
            sbXml.Append("<Reserved3></Reserved3>");
            sbXml.Append("<Reserved4></Reserved4>");
            sbXml.Append("</ApprovalCount>");
            sReturn = sbXml.ToString();
            sbXml = null;
        }
        return sReturn;
    }


    /// <summary>
    /// 2016.05 KWR
    /// 결재건수 조회 (미결함 Count Only)
    /// </summary>
    [WebMethod]
    public string ApprovalCount_ApprovalOnly(string ID)
    {
        DataPack INPUT = new DataPack();
        DataSet oDS = new DataSet();
        String sReturn = "";
        StringBuilder sbXml = new StringBuilder();

        String sApprovalCount = "0";
        String sReturnCode = "9";
        String sReturnDesc = "Error";
        try
        {
            using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
            {
                INPUT.add("@USER_ID", ID);
                oDS = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wf_approvalcount_ApprovalOnly", INPUT);
                oDS.DataSetName = "ApprovalCount";
                if (oDS != null && oDS.Tables.Count > 0)
                {
                    if (oDS.Tables[0].Rows[0]["APPROVAL"] != null)
                    {
                        sApprovalCount = oDS.Tables[0].Rows[0]["APPROVAL"].ToString();
                    }
                }
            }
            sReturnCode = "0";
            sReturnDesc = "Success";
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            sbXml.Append("<ApprovalCount>");
            sbXml.Append("<ReturnDesc>").Append(sReturnDesc).Append("</ReturnDesc>");
            sbXml.Append("<ReturnCode>").Append(sReturnCode).Append("</ReturnCode>");
            sbXml.Append("<EmpNo>").Append(ID).Append("</EmpNo>");
            sbXml.Append("<ApprCount>").Append(sApprovalCount).Append("</ApprCount>");
            sbXml.Append("</ApprovalCount>");
            sReturn = sbXml.ToString();
            sbXml = null;
            if (INPUT != null) INPUT.Dispose();
            if (oDS != null) oDS.Dispose();
        }
        return sReturn;
    }

    #endregion

    #region EngineCheck
    [WebMethod]
    public string EngineCheck(string Param1, string Param2, string Param3)
    {
        return Param1 + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    #endregion

    #region ink보기
    //ink보기를 위한 의견 Json 파일
    public class InkList
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string InsertDate { get; set; }
        public string Kind { get; set; }
        public string Comment { get; set; }
        public string SavePath { get; set; }
    }

    [WebMethod]
    public string GetCommentForInkView(string fiid)
    {
        string inkObj = string.Empty;
        List<InkList> li = new List<InkList>();
        DataSet oDS = new DataSet();
        DataPack INPUT = new DataPack();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        DataTable dt = new DataTable();

        try
        {

            string sSpName = "dbo.usp_wf_get_comment_list_ink";

            using (SqlDacManager oDac = new SqlDacManager("INST_ConnectionString"))
            {
                INPUT.add("@fiid", fiid);
                oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                dt = oDS.Tables[0];

            }

            foreach (DataRow row in dt.Rows)
            {
                li.Add(
                    new InkList
                    {
                        UserID = row["USER_ID"].ToString(),
                        UserName = row["USER_NAME"].ToString(),
                        InsertDate = row["INSERT_DATE"].ToString(),
                        Kind = row["KIND"].ToString(),
                        Comment = row["COMMENT"].ToString(),
                        SavePath = row["SAVE_PATH"].ToString()
                    });
            }


        }
        catch (System.Exception ex)
        {
            throw ex;
        }

        inkObj = serializer.Serialize(li);
        return inkObj;
    }
    #endregion

    #region 결재 안내메일 보내기
    /// <summary>
    /// 결재 안내 메일 보내기
    /// </summary>
    /// <param name="sMode">결재 mode</param>
    /// <param name="Piid">process_id</param>
    /// <param name="Wiid">Workitem_id</param>
    /// <param name="Ptid">Performar_id</param>
    /// <param name="Pfid">PF_id</param>
    /// <param name="Sender">발신자</param>
    /// <param name="Recipients">수신자</param>
    /// <param name="FORM_INFO_EXT">프로세스옵션정보값</param>
    /// <param name="APPROVERCONTEXT">결재선</param>
    /// <param name="DSCR">PInstance.description</param>
    [WebMethod]
    public string SendMessageWS(string sMode, string sOpenMode, string Piid, string Wiid, string Ptid, string Pfid, string Sender, string[] Recipients, System.Xml.XmlNode oFormInfoExt, System.Xml.XmlDocument oApvList, System.Xml.XmlDocument DSCR, bool bOMEntityTypeOU = false, string strContents = "", string strLanguage = "", string sReserved1 = "", string sReserved2 = "")
    {
        string sReturn = "";
        StringBuilder sbMailBody = new StringBuilder();
        StringBuilder sbLinkURL = new StringBuilder();
        StringBuilder sbMobileURL = new StringBuilder();
        string sSubjectMail = String.Empty;
        string sSubjectApproval = String.Empty;
        string sSubject = String.Empty;
        string sTodoListType = "ApprovalRequest";

        CfnCoreEngine.WfOrganizationManager OManager = null;

        #region 메일 제목
        sSubject = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["subject"].Value;
        switch (sMode)
        {
            case "APPROVAL":
                {
                    sSubjectMail = "[결재 진행]" + sSubject;
                    sTodoListType = "ApprovalRequest";
                    //sTodoListType = "ARRIVEDAPPROVAL";
                    break;
                }
            case "CHARGEJOB":
            case "REDRAFT":
                {
                    sSubjectMail = "[결재 진행]" + sSubject;
                    sTodoListType = "ApprovalRequest";
                    //sTodoListType = "ARRIVEDAPPROVAL";
                    break;
                }
            case "CHARGE":
            case "REDRAFT_RE":
            case "COMPLETE":
                {
                    sSubjectMail = "[결재 완료]" + sSubject;
                    sTodoListType = "ApprovalCompleted";
                    //sTodoListType = "ARRIVEDCOMPLETE";
                    break;
                }
            case "REJECT":
                {
                    sSubjectMail = "[결재 반송]" + sSubject;
                    sTodoListType = "ApprovalRejected";
                    break;
                }
            case "CCINFO":
            case "CIRCULATION":
                {
                    sSubjectMail = "[결재 참조/회람]" + sSubject;
                    sTodoListType = "ApprovalConsulted";
                    //CCINFO와 CIRCULATION 분리
                    //sTodoListType = "ARRIVEDCCINFO"; 
                    //sTodoListType = "ARRIVEDCIRCULATION"; 
                    break;
                }
            case "DELAY":
                {
                    sSubjectMail = "[결재 지연알림]" + sSubject;
                    break;
                }
            case "HOLD":
                {
                    sSubjectMail = "[결재 보류알림]" + sSubject;
                    sTodoListType = "ApprovalHold";
                    break;
                }
            case "WITHDRAW":
                {
                    sSubjectMail = "[결재 회수알림]" + sSubject;
                    sTodoListType = "ApprovalInitiatorWithdraw";
                    break;
                }
            case "ABORT":
                {
                    sSubjectMail = "[결재 취소알림]" + sSubject;
                    sTodoListType = "ApprovalInitiatorCancel";
                    break;
                }
            case "APPROVECANCEL":
                {
                    sSubjectMail = "[결재 승인취소알림]" + sSubject;
                    sTodoListType = "ApprovalCancel";
                    break;
                }
            case "ASSIGN_APPROVAL":
                {
                    sSubjectMail = "[결재 전달알림]" + sSubject;
                    sTodoListType = "ApprovalDelivery";
                    break;
                }
            case "ASSIGN_CHARGE":
                {
                    sSubjectMail = "[결재 담당자지정알림]" + sSubject;
                    sTodoListType = "ApprovalAssignChage";
                    break;
                }
            case "ASSIGN_R":
                {
                    sSubjectMail = "[결재 부서전달알림]" + sSubject;
                    sTodoListType = "ApprovalDeliveryDept";
                    break;
                }
            case "COMMENT":
                {
                    sSubjectMail = "[결재 의견알림]" + sSubject;
                    sTodoListType = "ApprovalComment";
                    break;
                }
            case "LEGACYERRER":
                {
                    sSubjectMail = "[전자결재 연동오류 안내]" + sSubject;
                    break;
                }
        }

        #endregion

        #region 메일 본문내용
        sbMailBody.Append("<MAIL>");
        switch (sMode)
        {
            case "APPROVAL":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 도착 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("새로운 결재문서가 결재할 문서함에 대기중입니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "REDRAFT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 도착 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("새로운 결재문서가 부서 수신함에 대기중입니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "CCINFO":
            case "CCINFO_U":
            case "CIRCULATION":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 참조/회람 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 참조/회람되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 참조/회람함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("회람 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "CHARGE":
            case "COMPLETE":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 완료 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 완료되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 완료함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "REDRAFT_RE":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 수신 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 수신되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 부서 공람함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "REJECT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 반려 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 반송 되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 반송함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "CHARGEJOB":
                {
                    //[2015-11-26 modi 유영재 과장] 알림 메시지 수정
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 도착 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("새로운 결재문서가 담당업무함에 대기중입니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "DELAY":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 지연 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 지연되고 있습니다..").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "HOLD":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 보류 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 보류 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("보류 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "WITHDRAW":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 회수 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 기안자에 의해 회수 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("회수 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "ABORT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 취소 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 기안자에 의해 취소 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("취소 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "APPROVECANCEL":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 승인취소 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 결재자에 의해 취소 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("취소 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "ASSIGN_APPROVAL":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 전달 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 전달 되었습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "ASSIGN_CHARGE":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 담당자 지정 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 담당자로 지정 되었습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "ASSIGN_R":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 부서전달 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 부서함으로 전달 되었습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "COMMENT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 의견 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재자가 결재문서에 결재의견을 입력하였습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "LEGACYERRER":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 연동오류 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("오류 내용 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
        }

        #region 결재 상세 내용
        if (sOpenMode != "NONE")
        {
            string sInitName = "";
            string sInitOUName = "";
            XmlNodeList oPersonList = oApvList.SelectNodes("steps/division/step/ou/person");

            //담당부서 기안자인지 체크 재기안 전에는 kind가 normal임
            string sDivisionOucode = string.Empty;

            foreach (System.Xml.XmlNode oPerson in oPersonList)
            {
                XmlNode oPersonTask = oPerson.SelectSingleNode("taskinfo");

                if (oPersonTask != null)
                {
                    string sKind = "", sStatus = "", sOuName = "", sName = "", sTitle = "", sComment = "", sDate = "";

                    bool bDisplay = false;

                    if (oPersonTask.Attributes["visible"] != null)
                    {
                        if (oPersonTask.Attributes["visible"].Value != "n")
                        {
                            bDisplay = true;
                        }
                    }
                    else if (oPersonTask.Attributes["kind"].Value != "conveyance")
                    {
                        bDisplay = true;
                    }

                    //담당부서 기안자인지 체크 재기안 전에는 kind가 normal임
                    bool bReceiveCharge = false;
                    if (oPersonTask.ParentNode.ParentNode.ParentNode.ParentNode.Attributes["divisiontype"] != null)
                    {
                        if (oPersonTask.ParentNode.ParentNode.ParentNode.ParentNode.Attributes["divisiontype"].Value == "receive")
                        {
                            if (sDivisionOucode != oPersonTask.ParentNode.ParentNode.ParentNode.ParentNode.Attributes["oucode"].Value)
                            {
                                bReceiveCharge = true;
                            }
                            sDivisionOucode = oPersonTask.ParentNode.ParentNode.ParentNode.ParentNode.Attributes["oucode"].Value;
                        }
                    }
                    if (bDisplay)
                    {
                        switch (oPersonTask.Attributes["kind"].Value)
                        {
                            case "authorize": sKind = "전결"; break;
                            case "review": sKind = "후결"; break;
                            case "normal":
                                if (bReceiveCharge)
                                    sKind = "수신";
                                else
                                    sKind = "결재";
                                break;
                            case "charge":
                                if (bReceiveCharge)
                                    sKind = "수신";
                                else
                                    sKind = "기안";
                                break;
                            case "substitute": sKind = "대결"; break;
                            case "bypass": sKind = "후열"; break;
                            case "consent": sKind = "합의"; break;
                            case "consult": sKind = "합의"; break;
                            case "confirm": sKind = "확인"; break;
                            case "reference": sKind = "참조"; break;
                        }

                        switch (oPersonTask.Attributes["result"].Value)
                        {
                            case "authorized": sStatus = "승인"; break;
                            case "completed":
                                if (bReceiveCharge)
                                    sStatus = "접수";
                                else
                                    sStatus = "승인";
                                break;
                            case "reviewed": sStatus = "승인"; break;
                            case "agreed": sStatus = "동의"; break;
                            case "rejected":
                                if (bReceiveCharge)
                                    sStatus = "거부";
                                else
                                    sStatus = "반려";
                                break;
                            case "bypass": sStatus = "후열"; break;
                            case "disagreed": sStatus = "거부"; break;
                            case "reserved": sStatus = "보류"; break;
                            case "confirmed": sStatus = "확인"; break;
                        }

                        sOuName = Covi.Framework.Dictionary.GetDicInfo(oPerson.Attributes["ouname"].Value, CF.LanguageType.ko);
                        sName = Covi.Framework.Dictionary.GetDicInfo(oPerson.Attributes["name"].Value, CF.LanguageType.ko);
                        if (oPersonTask.Attributes["datecompleted"] != null)
                        {
                            sDate = Convert.ToDateTime(oPersonTask.Attributes["datecompleted"].Value).ToString("yyyy-MM-dd HH:mm");
                        }

                        if (oPersonTask.Attributes["kind"].Value == "charge")
                        {
                            sInitName = sName;
                            sInitOUName = sOuName;
                        }

                        if (oPerson.Attributes["title"].Value.Split(';').Length > 1)
                        {
                            sTitle = oPerson.Attributes["title"].Value.Split(';')[1];
                        }

                        if (oPerson.SelectSingleNode("taskinfo/comment") != null)
                        {
                            sComment = oPerson.SelectSingleNode("taskinfo/comment").InnerText.Replace("\n", "##br");
                        }

                        sbMailBody.Append("<APVLINE>");
                        sbMailBody.Append("<KIND><![CDATA[").Append(sKind).Append("]]></KIND>");
                        sbMailBody.Append("<STATUS><![CDATA[").Append(sStatus).Append("]]></STATUS>");
                        sbMailBody.Append("<OUNAME><![CDATA[").Append(sOuName).Append("]]></OUNAME>");
                        sbMailBody.Append("<NAME><![CDATA[").Append(sName).Append("]]></NAME>");
                        sbMailBody.Append("<TITLE><![CDATA[").Append(sTitle).Append("]]></TITLE>");
                        sbMailBody.Append("<COMMENT><![CDATA[").Append(sComment).Append("]]></COMMENT>");
                        sbMailBody.Append("<DATE><![CDATA[").Append(sDate).Append("]]></DATE>");
                        sbMailBody.Append("</APVLINE>");
                    }
                }
            }
            //결재선 끝

            string sFormName = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["name"].Value;
            string sDocsec = "";
            string sKeepyear = "";

            string sCatname = oFormInfoExt.SelectSingleNode("docinfo/catname").InnerText;

            switch (oFormInfoExt.SelectSingleNode("docinfo/docsec").InnerText)
            {
                case "100": sDocsec = "1등급"; break;
                case "200": sDocsec = "2등급"; break;
                case "300": sDocsec = "3등급"; break;
                default: sDocsec = "기타문서"; break;
            }

            if (oFormInfoExt.SelectSingleNode("docinfo/keepyear").InnerText == "99")
            {
                sKeepyear = "영구";
            }
            else
            {
                sKeepyear = oFormInfoExt.SelectSingleNode("docinfo/keepyear").InnerText + "년";
            }

            sbMailBody.Append("<HEADNAME><![CDATA[").Append(sFormName).Append("]]></HEADNAME>");
            sbMailBody.Append("<SUBJECT><![CDATA[").Append(sSubject).Append("]]></SUBJECT>");
            sbMailBody.Append("<DOCSEC><![CDATA[").Append(sDocsec).Append("]]></DOCSEC>");
            sbMailBody.Append("<KEEPYEAR><![CDATA[").Append(sKeepyear).Append("]]></KEEPYEAR>");
            sbMailBody.Append("<CATNAME><![CDATA[").Append(sCatname).Append("]]></CATNAME>");
            sbMailBody.Append("<INITNAME><![CDATA[").Append(sInitName).Append("]]></INITNAME>");
            sbMailBody.Append("<INITOUNAME><![CDATA[").Append(sInitOUName).Append("]]></INITOUNAME>");

            //링크 정보  
            //#region 부분 SSL 체크
            //try
            //{
            //    // 부분SSL 사용 : http 와 https가 페이지 설정으로 자동으로 바뀜.
            //    if (GetBaseConfig("UsePartiSSL").Equals("Y"))
            //    {
            //        if (SSLFlag.Secure == _sSSLFlag) // Secure
            //        {
            //            sbLinkURL.Append("https://");
            //        }
            //        else if (SSLFlag.InSecure == _sSSLFlag) // InSecure
            //        {
            //            sbLinkURL.Append("http://");
            //        }
            //    }
            //    // 부분SSL 사용 : http 와 https가 페이지 설정으로 자동으로 바뀌지는 않으나 Secure가 설정된 페이지에서는 https로 자동으로 바뀜.
            //    else if (GetBaseConfig("UsePartiSSL").Equals("S"))
            //    {
            //        if (SSLFlag.Secure == _sSSLFlag)
            //        {
            //            sbLinkURL.Append("https://");
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //}
            //#endregion

            //[2016-06-16 modi kh] 기초설정값 이용하여 설정
            //sbLinkURL.Append("http://");
            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("ApprovalProtocol", "0"));

            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
            sbLinkURL.Append("/WebSite/Approval/NotifyMail.aspx?a=");

            //[2016-06-16 modi kh] 기초설정값 이용하여 설정
            //sbLinkURL.Append("http://");
            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("ApprovalProtocol", "0"));

            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
            sbLinkURL.Append("/WebSite/Approval/Forms/Form.aspx?");
            sbLinkURL.Append("mode=").Append(sOpenMode);
            sbLinkURL.Append("&piid=").Append(Piid);
            sbLinkURL.Append("&wiid=").Append(Wiid);
            sbLinkURL.Append("&pfid=").Append(Pfid);
            sbLinkURL.Append("&ptid=").Append(Ptid);
            //참조/회람 메일에서 양식 open시점에 Read_date Update를 위해 추가 2016-05-03 KWR
            if (sMode == "CCINFO" || sMode == "CIRCULATION")
            {
                sbLinkURL.Append("&mailmode=").Append("Y");
                if (sMode == "CIRCULATION")
                    sbLinkURL.Append("&isdepart=").Append(bOMEntityTypeOU);
                else
                    sbLinkURL.Append("&isdepart=").Append("true");
            }
            sbMailBody.Append("<URL><![CDATA[");
            sbMailBody.Append(sbLinkURL.ToString());
            sbMailBody.Append("]]></URL>");
        }
        #endregion

        sbMailBody.Append("</MAIL>");

        #endregion

        #region 신세계 MAIL 제목, 내용 관련

        #region 메일 제목
        //XmlNode Initiator = oApvList.SelectSingleNode("steps/division[@divisiontype='send']/step/ou/person[taskinfo/@kind='charge']");

        ////sSubjectMail += Environment.NewLine;
        //sSubjectMail += "* 제목 : " + sSubject;
        //if (Initiator != null)
        //{
        //    sSubjectMail += Environment.NewLine;
        //    //sSubjectMail += "* 기안자 : " + ((Initiator.Attributes["name"].Value.Split(';').Length > 1) ? (!String.IsNullOrEmpty(Initiator.Attributes["name"].Value.Split(';')[0])) ? Initiator.Attributes["name"].Value.Split(';')[0] : Initiator.Attributes["name"].Value.Split(';')[1] : Initiator.Attributes["name"].Value) + " " + ((Initiator.Attributes["title"].Value.Split(';').Length > 1) ? ((!String.IsNullOrEmpty(Initiator.Attributes["title"].Value.Split(';')[0])) ? Initiator.Attributes["title"].Value.Split(';')[0] : Initiator.Attributes["title"].Value.Split(';')[1]) : Initiator.Attributes["title"].Value);
        //    sSubjectMail += "* 기안자 : " + ((Initiator.Attributes["name"].Value.Split(';').Length > 1) ? (!String.IsNullOrEmpty(Initiator.Attributes["name"].Value.Split(';')[0])) ? Initiator.Attributes["name"].Value.Split(';')[0] : Initiator.Attributes["name"].Value.Split(';')[1] : Initiator.Attributes["name"].Value) + " " + ((Initiator.Attributes["title"].Value.Split(';').Length > 1) ? Initiator.Attributes["title"].Value.Split(';')[1] : Initiator.Attributes["title"].Value);
        //    if (Initiator.HasChildNodes && Initiator.SelectSingleNode("taskinfo") != null)
        //    {
        //        XmlNode Initiatortask = Initiator.SelectSingleNode("taskinfo");
        //        sSubjectMail += Environment.NewLine;
        //        sSubjectMail += "* 기안일시 : " + Initiatortask.Attributes["datecompleted"].Value;
        //    }
        //}

        //switch (sMode)
        //{
        //    case "REJECT":
        //        {
        //            if (!String.IsNullOrEmpty(strContents))
        //            {
        //                sSubjectMail += Environment.NewLine;
        //                sSubjectMail = "* 반송 : " + strContents;
        //            }
        //            else
        //            {
        //                XmlNode rejectedPerson = oApvList.SelectSingleNode("steps/division/step/ou/person[taskinfo/@status ='rejected']");
        //                if (rejectedPerson != null && rejectedPerson.SelectSingleNode("taskinfo/comment") != null)
        //                {
        //                    sSubjectMail += Environment.NewLine;
        //                    //sSubjectMail = "* 반송(" + ((rejectedPerson.Attributes["name"].Value.Split(';').Length > 1) ? rejectedPerson.Attributes["name"].Value.Split(';')[0] : rejectedPerson.Attributes["name"].Value) + " " + (rejectedPerson.Attributes["title"].Value.Split(';').Length > 1 ? rejectedPerson.Attributes["title"].Value.Split(';')[1] : rejectedPerson.Attributes["title"].Value) + ") : " + rejectedPerson.InnerText;
        //                    sSubjectMail = "* 반송(" + ((rejectedPerson.Attributes["name"].Value.Split(';').Length > 1) ? rejectedPerson.Attributes["name"].Value.Split(';')[0] : rejectedPerson.Attributes["name"].Value) + " " + (rejectedPerson.Attributes["title"].Value.Split(';').Length > 1 ? rejectedPerson.Attributes["title"].Value.Split(';')[1] : rejectedPerson.Attributes["title"].Value) + ") : " + rejectedPerson.InnerText;
        //                }
        //            }
        //            break;
        //        }

        //    case "HOLD":
        //        {
        //            if (!String.IsNullOrEmpty(strContents))
        //            {
        //                XmlNode holdPerson = oApvList.SelectSingleNode("steps/division/step/ou/person[taskinfo/comment/@relatedresult='reserved']");

        //                if (holdPerson != null)
        //                {
        //                    sSubjectMail += Environment.NewLine;
        //                    sSubjectMail = "* 보류(" + (holdPerson.Attributes["name"].Value.Split(';').Length > 1 ? holdPerson.Attributes["name"].Value.Split(';')[0] : holdPerson.Attributes["name"].Value) + " " + (holdPerson.Attributes["title"].Value.Split(';').Length > 1 ? holdPerson.Attributes["title"].Value.Split(';')[1] : holdPerson.Attributes["title"].Value) + ") : " + strContents;
        //                }
        //                else
        //                {
        //                    sSubjectMail += Environment.NewLine;
        //                    sSubjectMail = "* 보류 : " + strContents;
        //                }
        //            }
        //            break;
        //        }
        //    case "COMMENT":
        //        {
        //            sSubjectMail = "* 의견 : " + strContents;
        //            break;
        //        }
        //}
        #endregion

        #region 결재문서 원본보기 URL
        ////sbLinkURL.Append("https://");
        ////sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
        //sbLinkURL.Append("/WebSite/Approval/Forms/Form.aspx?");
        //sbLinkURL.Append("mode=").Append(sOpenMode);
        //sbLinkURL.Append("&piid=").Append(Piid);
        //sbLinkURL.Append("&wiid=").Append(Wiid);
        //sbLinkURL.Append("&pfid=").Append(Pfid);
        //sbLinkURL.Append("&ptid=").Append(Ptid);
        #endregion

        #region 결재문서 Mobile URL

        /////WebSite/Mobile/Approval/ApprovalView.aspx?System=Approval&Alias=P.APPROVAL.APPROVAL&PIID=5b537c6c-a63f-489e-9492-6e4e6540e797&WIID=ed1d5bfc-f6c7-4698-90e3-2cc54933fa79&DocTypeCode=T000&Page=1&SearchType=ALL&SearchText=
        //string mobileAlias = string.Empty;
        //switch (sOpenMode)
        //{
        //    case "APPROVAL":
        //        mobileAlias = "P.APPROVAL.APPROVAL";
        //        break;
        //    case "PROCESS":
        //        mobileAlias = "P.PROCESS.PROCESS";
        //        break;
        //    case "COMPLETE":
        //        mobileAlias = "P.COMPLETE.COMPLETE";
        //        break;
        //    case "REJECT":
        //        mobileAlias = "P.REJECT.REJECT";
        //        break;
        //    case "CCINFO":
        //    case "CIRCULATION":
        //    case "TCINFO":
        //        mobileAlias = "P.TCINFO.TCINFO";
        //        break;
        //    default:
        //        mobileAlias = "";
        //        break;
        //}
        ////sbMobileURL.Append("https://");
        ////sbMobileURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
        //sbMobileURL.Append("/WebSite/Mobile/Approval/ApprovalView.aspx?System=Approval");
        //sbMobileURL.Append("&Alias=").Append(mobileAlias);
        //sbMobileURL.Append("&PIID=").Append(Piid);
        //sbMobileURL.Append("&WIID=").Append(Wiid);
        //sbMobileURL.Append("&DocTypeCode="); //사용하지 않는 값이라고해서 빈값넘김...
        //sbMobileURL.Append("&Page=1&SearchType=ALL&SearchText=");

        #endregion

        #endregion

        #region 양식별 결재 알림 관리
        // [2015-10-22 modi] 마리아디비 관련 수정 - 박경연
        string sFormID = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["id"].Value;
        string sEntCode = "";
        if (oFormInfoExt == null)
        {
            sEntCode = Session["DN_CODE"].ToString();
        }
        else
        {
            if (oFormInfoExt.SelectSingleNode("entcode") == null) sEntCode = Session["DN_CODE"].ToString();
            else sEntCode = oFormInfoExt.SelectSingleNode("entcode").InnerText;
        }
        string sUseAppMailForm = "N";
        {
            DataSet ds = null;
            DataPack INPUT = null;

            try
            {
                ds = new DataSet();
                INPUT = new DataPack();

                using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
                {
                    INPUT.add("@FormID", sFormID);
                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_selectfromwfforms", INPUT);
                    ds.Tables[0].TableName = "FORMINFO";
                }

                XmlDocument oFomrInfo = new XmlDocument();
                oFomrInfo.LoadXml(ds.Tables[0].Rows[0]["EXT_INFO"].ToString());
                if (oFomrInfo.SelectSingleNode("ExtInfo/ExtInfoData/fmMailList") != null)
                {
                    string sMailList = oFomrInfo.SelectSingleNode("ExtInfo/ExtInfoData/fmMailList").InnerText;
                    if (sMailList == "")
                    {
                        sUseAppMailForm = "Y";
                    }
                    else
                    {
                        if (sMailList.IndexOf(";" + sEntCode) > -1) sUseAppMailForm = "Y";
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (ds != null)
                {
                    ds.Dispose();
                    ds = null;
                }
                if (INPUT != null)
                {
                    INPUT.Dispose();
                    INPUT = null;
                }
            }
        }
        #endregion

        if (bOMEntityTypeOU)//부서로 온내용은 부서원으로 분리
        {
            // [2015-10-22 modi] 마리아디비 관련 수정 - 김한솔
            DataSet ds = null;
            DataPack INPUT = null;

            try
            {
                string sRecipientsOu = "";

                ds = new DataSet();
                INPUT = new DataPack();

                foreach (string sRecipientOu in Recipients)
                {
                    if (sRecipientOu != "") sRecipientsOu += "," + sRecipientOu + "";
                }

                using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
                {
                    INPUT.add("@UNIT_CODE", sRecipientsOu.Substring(1));
                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_get_UnitPersonInfo", INPUT);
                }

                System.Xml.XmlDocument oOUInfo = null;
                oOUInfo = new XmlDocument();
                oOUInfo.LoadXml(ds.GetXml());

                string sRecipientsTemp = "";
                foreach (System.Xml.XmlNode oOU in oOUInfo.SelectNodes("//Table"))
                {
                    sRecipientsTemp += oOU.SelectSingleNode("PERSON_CODE").InnerText + ";";
                }
                string[] aOuPerson = sRecipientsTemp.Split(';');
                Recipients = aOuPerson;
            }
            catch (System.Exception ex)
            {
                throw;
            }
            finally
            {
                if (ds != null)
                {
                    ds.Dispose();
                    ds = null;
                }
                if (INPUT != null)
                {
                    INPUT.Dispose();
                    INPUT = null;
                }

            }

        }
        #region mail/sms/messenger/todolist/mdm 보내기
        System.IO.StringWriter oTW = null;
        try
        {
            if (sUseAppMailForm == "Y")
            {
                string sContents = string.Empty;
                System.Xml.Xsl.XsltSettings settings = new System.Xml.Xsl.XsltSettings(true, true);
                System.Xml.Xsl.XslCompiledTransform xslsteps = new System.Xml.Xsl.XslCompiledTransform();
                {
                    if (sOpenMode == "NONE")
                    {
                        xslsteps.Load(Server.MapPath("\\\\WebSite\\Common\\Mail.xsl"), settings, new System.Xml.XmlUrlResolver());
                    }
                    else
                    {
                        xslsteps.Load(Server.MapPath("\\\\WebSite\\Common\\Mail_App.xsl"), settings, new System.Xml.XmlUrlResolver());
                    }

                }
                System.Xml.XPath.XPathDocument oXPathDoc = new System.Xml.XPath.XPathDocument(new System.IO.StringReader(sbMailBody.ToString()));
                oTW = new System.IO.StringWriter();
                xslsteps.Transform(oXPathDoc, null, oTW);
                oTW.GetStringBuilder().Remove(0, 39);
                sContents = oTW.ToString().Replace("##br", "<br />");
                oTW.Close();
                oTW = null;

                OManager = new CfnCoreEngine.WfOrganizationManager();
                string sSenderMail = OManager.GetPersonInfo(Sender).SelectSingleNode("om/person").Attributes["email"].Value;
                string sSenderName = Covi.Framework.Dictionary.GetDicInfo(OManager.GetPersonInfo(Sender).SelectSingleNode("om/person").Attributes["name"].Value, CF.LanguageType.ko);
                string sSenderCode = Covi.Framework.Dictionary.GetDicInfo(OManager.GetPersonInfo(Sender).SelectSingleNode("om/person").Attributes["code"].Value, CF.LanguageType.ko);

                {

                    System.Xml.XmlDocument oRecipients = OManager.GetOMEntityInfo(Recipients, CfnEngineInterfaces.IWfOrganization.OMEntityType.ettpPerson);
                    System.Xml.XmlNodeList oRecipientList = oRecipients.SelectNodes("om/person");
                    string sRecipientMails = System.String.Empty;

                    #region MAIL/SMS/MESSENGER/TODOLIST/MDM 보내기
                    foreach (System.Xml.XmlNode oRecipient in oRecipientList)
                    {
                        string sMedivum = ""; //변수 초기화 시점 이동, 해당변수는 개개인별로 다른 값을 가져야 함 2015.11.04 박은선
                        string sRecipientMail = oRecipient.Attributes["email"].Value;
                        string sRecipientName = oRecipient.Attributes["name"].Value;
                        string sRecipientCode = oRecipient.Attributes["sipaddress"].Value;

                        string sRecipientPersonCode = oRecipient.Attributes["code"].Value;

                        //알림 사용유무 확인
                        //개인 정보 확인
                        // [2018-02-09] 개인 환경설정 값(BASE_OBJECT_UR - ApprovalArrivalMedium)이 없는 경우 오류나는 현상 처리
                        if (oRecipient.Attributes["recnotify"].Value != null && oRecipient.Attributes["recnotify"].Value != "")
                        {
                            XmlDocument oXml = new XmlDocument();
                            oXml.LoadXml(oRecipient.Attributes["recnotify"].Value);
                            if (oXml.SelectSingleNode("mailconfig/" + sMode) != null)
                            {
                                sMedivum = oXml.SelectSingleNode("mailconfig/" + sMode).InnerText;
                            }
                        }

                        //무조건 알림을 받아야 하는 알림 종류 확인
                        Boolean bEntConfig = false;

                        string strDN_ID = "0";
                        using (DataTable dtDomain = CF.Utils.FilterDataTable(CF.CacheManager.GetCacheTable(CF.CacheKind.DomainInfo), string.Format("DN_Code='{0}'", sEntCode)))
                        {
                            foreach (DataRow row in dtDomain.Rows)
                            {
                                strDN_ID = row["DN_ID"].ToString();
                            }
                        }
                        // string sEntConfig = CF.ConfigurationManager.GetBaseConfig("DefaultArramKind", CF.CacheManager.GetCacheSigngle("DN_ID", sEntCode));
                        string sEntConfig = CF.ConfigurationManager.GetBaseConfig("DefaultArramKind", strDN_ID);

                        if (sEntConfig.IndexOf(sMode) > -1) bEntConfig = true;

                        #region MAIL
                        if ((sMedivum.IndexOf("MAIL") > -1 || bEntConfig) && sRecipientMail != "") //Mail 보내기
                        {
                            sRecipientMails += "," + sRecipientMail;
                        }
                        #endregion

                        #region SMS
                        if (sMedivum.IndexOf("SMS") > -1)//SMS 보내기
                        {
                            string sMobileNo = oRecipient.Attributes["mobile"].Value.Replace("-", "").Replace(" ", "").Replace("+", "");
                            if (sMobileNo != "")
                            {
                                using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
                                {
                                    DataPack INPUT = new DataPack();
                                    INPUT.add("@INDATE", DateTime.Now.ToString("yyyyMMdd"));
                                    INPUT.add("@INTIME", DateTime.Now.ToString("hhmmss"));
                                    INPUT.add("@RPHONE1", sMobileNo);
                                    INPUT.add("@RPHONE2", "");
                                    INPUT.add("@RPHONE3", "");
                                    INPUT.add("@RECVNAME", sRecipientName);
                                    INPUT.add("@MSG", "결재문서도착(The notice of arrival for approval)[" + sSubjectMail + "]" + sSenderName);
                                    //SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wfform_sendsms", INPUT); //테이블 수정 필요
                                }
                            }
                        }
                        #endregion

                        #region MESSENGER
                        if (sMedivum.IndexOf("MESSENGER") > -1)//MESSENGER 보내기
                        {

                            String strWebMsg = string.Empty;
                            StringBuilder sb = new StringBuilder();
                            StringBuilder sTargetNames = new StringBuilder("");
                            StringBuilder sTargetIDs = new StringBuilder("");
                            MemoService.MemoService oWS = new MemoService.MemoService();
                            {
                                sb.Append("<WebService><Message>");
                                sb.Append("<SenderName>").Append(sSenderName).Append("</SenderName>");
                                sb.Append("<SenderID>").Append(sSenderCode).Append("</SenderID>");
                                sb.Append("<TargetName>");
                                sb.Append(sRecipientName);
                                sb.Append("</TargetName>");
                                sb.Append("<TargetID>");
                                sb.Append(sRecipientCode);
                                sb.Append("</TargetID>");
                                sb.Append("<Content><![CDATA[");
                                sb.Append(sSubjectMail).Append("|").Append(sbLinkURL.ToString());
                                sb.Append("]]></Content>");
                                sb.Append("</Message></WebService>");

                                strWebMsg = sb.ToString();
                                sReturn = oWS.WriteMemo(strWebMsg);
                            }
                        }
                        #endregion

                        #region Todolist
                        if (sMedivum.IndexOf("TODOLIST") > -1)
                        {
                            Int32 iWidth = 790;
                            Int32 iHeight = 900;

                            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
                            {

                                DataPack INPUT = new DataPack();

                                //INPUT.add("@UR_Code", Recipients[0]);
                                INPUT.add("@UR_Code", sRecipientPersonCode);

                                INPUT.add("@Category", "Approval");
                                INPUT.add("@MsgType", sTodoListType);
                                INPUT.add("@Title", sSubject);      //INPUT.add("@Title", sSubject);
                                INPUT.add("@URL", sbLinkURL.ToString());
                                INPUT.add("@Message", sSubject);    //INPUT.add("@Message", sSubject);
                                INPUT.add("@OpenType", "OPENPOPUP");
                                INPUT.add("@PusherCode", sSenderCode);
                                INPUT.add("@Width", iWidth);
                                INPUT.add("@Height", iHeight);
                                INPUT.add("@ReservedStr1", "FORM");
                                INPUT.add("@ReservedStr2", "scroll");
                                SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.Com_U_TodoList_C", INPUT);
                            }
                        }
                        #endregion

                        #region MDM
                        if (sMedivum.IndexOf("MDM") > -1)
                        {
                            // 2016-03-22 hpark MDM알림에 링크 추가 s (모바일팀과 맞추기위해 일단 개발,배포,운영 서버만 반영함)
                            //MDMUtils.SendPush_UserID(sRecipientPersonCode, sSubjectMail);

                            //mnid & alias 가져오기
                            Boolean b_MakeUrlChk = true;    // URL 만들지 체크하는 값
                            string _strMnidPipe = "";       // 쿼리해온 mnid 값들(pipe로구분)
                            string _strAliasPipe = "";      // 쿼리해온 alias 값들(pipe로구분)
                            string _strMnid = "";           // 모드에따른 mnid
                            string _strAlias = "";          // 모드에따른 alias
                            string sbLinkURL_Mobile = "";   // mobile link
                            string _strMobileParam = "";    // 실제 넘기는 파라미터

                            DataSet ds = null;
                            DataPack INPUT = null;

                            try
                            {
                                ds = new DataSet();
                                INPUT = new DataPack();

                                using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                                {
                                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wf_getBaseMenuInfo", INPUT);
                                }

                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    _strAliasPipe += dr["Alias"].ToString() + "|";
                                    _strMnidPipe += dr["MN_ID"].ToString() + "|";
                                }
                            }
                            catch (System.Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                if (ds != null)
                                {
                                    ds.Dispose();
                                    ds = null;
                                }
                                if (INPUT != null)
                                {
                                    INPUT.Dispose();
                                    INPUT = null;
                                }
                            }

                            // URL 만들기
                            if (sOpenMode == "APPROVAL")
                            {
                                _strAlias = _strAliasPipe.Split('|')[0];
                                _strMnid = _strMnidPipe.Split('|')[0];
                            }
                            else if (sOpenMode == "PROCESS")
                            {
                                _strAlias = _strAliasPipe.Split('|')[1];
                                _strMnid = _strMnidPipe.Split('|')[1];
                            }
                            else if (sOpenMode == "COMPLETE")
                            {
                                _strAlias = _strAliasPipe.Split('|')[2];
                                _strMnid = _strMnidPipe.Split('|')[2];
                            }
                            else
                            {
                                b_MakeUrlChk = false;   // sOpenMode 값이 APPROVAL, PROCESS, COMPLETE 가 아니면 URL 만들지않음
                                MDMUtils.SendPush_UserID(sRecipientPersonCode, sSubjectMail);
                            }

                            if (b_MakeUrlChk)
                            {
                                sbLinkURL_Mobile = CF.ConfigurationManager.GetBaseConfigDomain("WebServiceProtocol_Mobile", "0");
                                sbLinkURL_Mobile += "://";
                                sbLinkURL_Mobile += CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain_Mobile", "0");
                                sbLinkURL_Mobile += GetPgModule("Mobile.ApprovalView");
                                sbLinkURL_Mobile += "?system=Approval";
                                sbLinkURL_Mobile += "&alias=" + _strAlias;
                                sbLinkURL_Mobile += "&fdid=";
                                sbLinkURL_Mobile += "&mnid=" + _strMnid;
                                sbLinkURL_Mobile += "&piid=" + Piid;
                                sbLinkURL_Mobile += "&wiid=" + Wiid;

                                // [2017-02-23] gbhwang 모바일 결재 알림 제목 수정
                                _strMobileParam = "LINK|";
                                _strMobileParam += sSubjectMail;
                                _strMobileParam += "|";
                                _strMobileParam += sbLinkURL_Mobile;

                                MDMUtils.SendPush_UserID(sRecipientPersonCode, _strMobileParam);
                            }
                            // 2016-03-22 hpark MDM알림에 링크 추가 e

                        }
                        #endregion
                    }
                    #endregion

                    #region ME(내 업무) 보내기

                    foreach (string oRecipient in Recipients)
                    {
                        if (oRecipient != null && oRecipient.Trim() != "")
                        {
                            string sMedivum = string.Empty; //변수 초기화 시점 이동, 해당변수는 개개인별로 다른 값을 가져야 함 2015.11.04 박은선
                            string sMeMedivum = string.Empty;
                            string sRecipientMail = string.Empty;
                            string sRecipientName = string.Empty;
                            string sRecipientCode = string.Empty;

                            string recnotify = string.Empty;

                            DataSet ds = new DataSet();
                            DataPack _INPUT = new DataPack();

                            try
                            {
                                using (SqlDacManager SqlDbAgent = new SqlDacManager())
                                {
                                    _INPUT.Clear();
                                    _INPUT.add("@UR_Code", oRecipient);
                                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.Com_U_MessagingSettingListMeApproval_R", _INPUT);//프로시저실행

                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        sMeMedivum = ds.Tables[0].Rows[0]["MeMedium"].ToString();
                                        sRecipientMail = ds.Tables[0].Rows[0]["email"].ToString();
                                        sRecipientName = ds.Tables[0].Rows[0]["name"].ToString();
                                        sRecipientCode = ds.Tables[0].Rows[0]["sipaddress"].ToString();
                                        recnotify = ds.Tables[0].Rows[0]["ApprovalMedium"].ToString();
                                    }
                                    //변경
                                    else
                                    {
                                        sMeMedivum = string.Empty;
                                        recnotify = string.Empty;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                if (_INPUT != null)
                                {
                                    _INPUT.Dispose();
                                }
                                if (ds != null)
                                {
                                    ds.Dispose();
                                }
                            }


                            #region ME
                            if (!String.IsNullOrEmpty(sMeMedivum) && sMeMedivum.Contains(sMode))
                            {
                                Int32 iWidth = 790;
                                Int32 iHeight = 900;

                                using (SqlDacManager SqlDbAgent = new SqlDacManager())
                                {
                                    DataPack INPUT = new DataPack();

                                    INPUT.add("@UR_Code", oRecipient);

                                    INPUT.add("@Category", "Approval");
                                    INPUT.add("@MsgType", sTodoListType);
                                    INPUT.add("@Title", sSubject);		//INPUT.add("@Title", sSubject);
                                    INPUT.add("@URL", sbLinkURL.ToString());
                                    INPUT.add("@MobileURL", sbMobileURL.ToString());
                                    INPUT.add("@Message", sSubject);	//INPUT.add("@Message", sSubject);
                                    INPUT.add("@OpenType", "OPENPOPUP");
                                    INPUT.add("@PusherCode", sSenderCode);
                                    INPUT.add("@Width", iWidth);
                                    INPUT.add("@Height", iHeight);
                                    INPUT.add("@ReservedStr1", "FORM");
                                    INPUT.add("@ReservedStr2", "scroll");
                                    SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.Com_U_TodoList_C", INPUT);

                                    //변경
                                    WriteMessage_log4("SendmessageResult", "* MsgType : " + sTodoListType + " * SendDate : " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " * sSubjectMail : " + sSubject + " * oRecipient : " + oRecipient + " * result : " + sReturn);
                                }
                            }
                            #endregion

                            #region TODO : [신세계-확인필요] 결재 단계 알람(기안자에게 결재 단계 진행 Push 전송)
                            //if (sMode == "APPROVAL")
                            //{
                            //    sSubjectApproval += "* 제목 : " + sSubject;

                            //    ExternalWebService EW = new ExternalWebService();
                            //    XmlNode oCharge = oApvList.SelectSingleNode("steps/division[@divisiontype='send']/step/ou/person[taskinfo/@kind='charge']");
                            //    XmlNodeList oApprovers = oApvList.SelectNodes("steps/division/step/ou/person[taskinfo/@result='completed'] | steps/division/step/ou/role[taskinfo/@result='completed']");

                            //    if (oApprovers != null && oApprovers.Count > 0 && (Sender != oApprovers[oApprovers.Count - 1].Attributes["code"].Value))
                            //    {
                            //        if (oCharge.Attributes["oucode"].Value.ToLower().IndexOf("ic0") > -1 || oCharge.Attributes["oucode"].Value.ToLower().IndexOf("ae0") > -1)
                            //        {
                            //            sSubjectApproval += Environment.NewLine;
                            //            sSubjectApproval += "* 결재자 : " + Covi.Framework.Dictionary.GetDicInfo(oApprovers[oApprovers.Count - 1].Attributes["name"].Value, CF.LanguageType.ko) + " " + ((oApprovers[oApprovers.Count - 1].Attributes["title"].Value.Split(';').Length > 1) ? oApprovers[oApprovers.Count - 1].Attributes["title"].Value.Split(';')[1] : oApprovers[oApprovers.Count - 1].Attributes["title"].Value);
                            //            sSubjectApproval += Environment.NewLine;
                            //            sSubjectApproval += "* 결재일시 : " + DateTime.Now.ToString("yyyy-MM-dd hh:mm");
                            //            sReturn = EW.SendBotsPushService("APPROVAL", "ARRIVEDAPPROVALSTEP", oCharge.Attributes["code"].Value, sSubjectApproval, "", "OPENPOPUP", "790", "900", sbLinkURL.ToString(), "");
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                    }

                    #endregion

                    #region SMS 주석 소스입니다. (이선민)
                    //if (sMode == "COMPLETE" || sMode == "REJECT")
                    //{
                    //    if (sMedivum.IndexOf("SMS") > -1 || DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["CompleteSendSms"].Value == "1") //SMS 보내기
                    //    if (sMedivum.IndexOf("SMS") > -1 || (DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["CompleteSendSms"] != null && DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["CompleteSendSms"].Value == "1"))
                    //    {
                    //        CommonService cs = new CommonService();
                    //        DataSet oDS = new DataSet();
                    //        DataPack INPUT = new DataPack();

                    //        //String pTxt = "결재문서완료(The notice of complete for approval)[" + sSubjectMail + "] " + sSenderName; //SMS 내용
                    //        String pTxt = "[제목 : " + sSubject + "] 결재문서가 완결되었습니다."; //SMS 내용

                    //        string sApproverPhone = string.Empty;
                    //        string sInitiatorPhone = string.Empty;
                    //        string sSenderMobileNumber = string.Empty;
                    //        string sRecMobileNumber = string.Empty;

                    //        string sSpName = "dbo.usp_userinfo"; // ID 로 핸드폰 번호 조회하는 SP

                    //        // 발신자 핸드폰 번호 구하기
                    //        using (SqlDacManager oDac = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    //        {
                    //            INPUT.add("@PERSON_CODE", Sender);
                    //            oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    //        }

                    //        if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                    //        {
                    //            sSenderMobileNumber = oDS.Tables[0].Rows[0]["AD_Mobile"].ToString();
                    //        }

                    //        XmlNodeList oPersonList = oApvList.SelectNodes("steps/division/step/ou/person");

                    //        string[] sApvLinePersonId;

                    //        //string sFmpf = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["prefix"].Value;


                    //        oDS = new DataSet();
                    //        INPUT = new DataPack();

                    //        using (SqlDacManager oDac = new SqlDacManager("ORG_ConnectionString"))
                    //        {
                    //            INPUT.add("@PERSON_CODE", oPersonList[0].Attributes["code"].Value);
                    //            oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    //        }

                    //        if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                    //        {
                    //            sRecMobileNumber = oDS.Tables[0].Rows[0]["AD_Mobile"].ToString();
                    //        }

                    //        // Send_SMS(string pTxt, string pNums, string pNumsCount, string pReturnNum, string pType) pType = SMS - S, MMS - M
                    //        cs.SendMessage(Sender, pTxt, sInitiatorPhone, "1", sSenderMobileNumber, "S", "I", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString());
                    //    }
                    //}
                    #endregion

                    if (sRecipientMails != "")
                    {
                        //2019-08-05 ygkim 메일 발송 인원이 200명 이상일 경우 메일 발송이 불가하여 나눠서 Insert 하도록 수정
                        string[] sRecipients = sRecipientMails.Split(',');
                        int iRecipientsCount = Convert.ToInt32(GetBaseConfig("ApvRecipientsCount"));

                        if (sRecipients.Length > iRecipientsCount)
                        {
                            string[] arrRecipientMails = sRecipients.Take(iRecipientsCount).ToArray();
                            string[] arrRests = sRecipients.Skip(iRecipientsCount).ToArray();
                            List<String> arrRecipientsMail = new List<string>();
                            
                            arrRecipientsMail.Add(string.Join(",", arrRecipientMails.Skip(1)));

                            if (iRecipientsCount > arrRests.Length)
                            {
                                arrRecipientsMail.Add(string.Join(",", arrRests));
                            }
                            else
                            {
                                while (arrRests.Length > iRecipientsCount)
                                {
                                    arrRecipientMails = arrRests.Take(iRecipientsCount).ToArray();
                                    arrRests = arrRests.Skip(iRecipientsCount).ToArray();

                                    arrRecipientsMail.Add(string.Join(",", arrRecipientMails));
                                }

                                arrRecipientsMail.Add(string.Join(",", arrRests));
                            }

                            foreach (string sMail in arrRecipientsMail)
                            {
                                using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                                {
                                    DataPack INPUT = new DataPack();
                                    INPUT.add("@sender", sSenderMail + "|" + sSenderName);
                                    INPUT.add("@subject", sSubjectMail);
                                    INPUT.add("@to", sMail);
                                    INPUT.add("@cc", "");
                                    INPUT.add("@bcc", "");
                                    INPUT.add("@body_format", "HTML");
                                    INPUT.add("@body", sContents);
                                    INPUT.add("@piid", Piid);
                                    SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wf_mail_insert", INPUT);
                                }
                            }
                        }
                        else  // 기존 소스
                        {
                            using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                            {
                                DataPack INPUT = new DataPack();
                                INPUT.add("@sender", sSenderMail + "|" + sSenderName);
                                INPUT.add("@subject", sSubjectMail);
                                INPUT.add("@to", sRecipientMails.Substring(1));
                                INPUT.add("@cc", "");
                                INPUT.add("@bcc", "");
                                INPUT.add("@body_format", "HTML");
                                INPUT.add("@body", sContents);
                                INPUT.add("@piid", Piid);
                                SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wf_mail_insert", INPUT);
                            }
                        }
                    }
                }
            }
            sReturn = "OK";
        }
        catch (System.Exception ex)
        {
            sReturn = ex.Message;
        }
        finally
        {
            if (oTW != null)
            {
                oTW.Close();
                oTW = null;
            }
            if (OManager != null)
            {
                OManager.Dispose();
                OManager = null;
            }
        }
        #endregion



        #region 모바일 결재 양식별(추후 고려 후 개발)

        //'//모바일 결재 양식의 경우 모바일 사용 결재자는 push시킴2008.10.25
        //'//sDeputyMobileYN , sMobileYN
        //Dim sMobileUse As System.String = String.Empty
        //If Not oFormInfoExt.SelectSingleNode("IsMobile") Is Nothing Then
        //    If oFormInfoExt.SelectSingleNode("IsMobile").InnerText = "true" Then
        //        sMobileUse = "Y"
        //    End If
        //End If
        //If oInitiatorNode Is Nothing Then
        //Else
        //    sInitiatorName = oInitiatorNode.Attributes("name").Value & " " & oInitiatorNode.Attributes("title").Value.Split(";")(1) & "(" & oInitiatorNode.Attributes("ouname").Value & ")"
        //End If

        //If sMobileUse = "Y" And sMobileYN = "Y" Then
        //    Dim sConnectionString2 As System.String = CfnDatabaseUtility.WfDatabaseDirectory.GetConnectionString("mobile")
        //    Dim oConn As System.Data.SqlClient.SqlConnection = Nothing
        //    Dim oComm As System.Data.SqlClient.SqlCommand = Nothing

        //    Try
        //        oConn = New System.Data.SqlClient.SqlConnection(sConnectionString2)
        //        oConn.Open()
        //        oComm = New System.Data.SqlClient.SqlCommand("dbo.MPOP_PROC_PUSH_EVENT", oConn)
        //        oComm.CommandType = CommandType.StoredProcedure

        //        If Workitem.deputyId <> String.Empty Then
        //            oComm.Parameters.AddWithValue("@touser_id", Workitem.deputyId)               '-- 사번 
        //        Else
        //            oComm.Parameters.AddWithValue("@touser_id", Workitem.performerId)               '-- 사번 
        //        End If
        //        oComm.Parameters.AddWithValue("@msg_event", "APPR_NEW")           '-- push event 종류(메일:MAIL_NEW, 전자결재:APPR_NEW, CRM:CRM_NEW)
        //        oComm.Parameters.AddWithValue("@mailkey", String.Empty)      '-- 메일키(URL)
        //        oComm.Parameters.AddWithValue("@crmkey", String.Empty)     '--CRM 문서키 
        //        oComm.Parameters.AddWithValue("@piid", PInstance.id)            '-- 전자결재 문서키 1
        //        oComm.Parameters.AddWithValue("@wiid", Workitem.id)           '-- 전자결재 문서키 2
        //        oComm.Parameters.AddWithValue("@pfid", sPFID)       '-- 전자결재 문서키 3
        //        oComm.Parameters.AddWithValue("@ptid", sPTID)        '-- 전자결재 문서키 4
        //        If Workitem.deputyId <> String.Empty Then
        //            oComm.Parameters.AddWithValue("@usid", Workitem.deputyId)               '-- 사번 
        //        Else
        //            oComm.Parameters.AddWithValue("@usid", Workitem.performerId)               '-- 사번 
        //        End If

        //        '//oComm.Parameters.AddWithValue("@usid", sRecipients(0))            '-- 전자결재 문서키 5
        //        oComm.Parameters.AddWithValue("@fiid", oFNode.Attributes("instanceid").Value)     '-- 전자결재 문서키 6
        //        oComm.Parameters.AddWithValue("@fmpf", oFNode.Attributes("prefix").Value)         '-- 전자결재 문서키 7
        //        oComm.Parameters.AddWithValue("@fmrv", oFNode.Attributes("revision").Value)      '-- 전자결재 문서키 8
        //        oComm.Parameters.AddWithValue("@sender", sInitiatorName)          '-- 메일:보낸사람 메일주소, 전자결재,CRM:기안자 이름
        //        oComm.Parameters.AddWithValue("@write_dt", Now())          '등록자명
        //        oComm.Parameters.AddWithValue("@important", String.Empty)  '-- 메일 중요도(0:낮음,1:보통,2:높음) 
        //        If oFNode.Attributes("isfile").Value = "0" Then
        //            oComm.Parameters.AddWithValue("@attach", "N")  '-- 첨부파일 여부 (Y:첨부있음, N:첨부없음)
        //        Else
        //            oComm.Parameters.AddWithValue("@attach", "Y")  '-- 첨부파일 여부 (Y:첨부있음, N:첨부없음)
        //        End If
        //        oComm.Parameters.AddWithValue("@subject", PInstance.subject)    '-- 제목
        //        oComm.Parameters.AddWithValue("@doc_type", "미결")            '-- 전자결재 문서종류(미결,반송)
        //        oComm.Parameters.AddWithValue("@doc_status", String.Empty)          '-- 전자결재 문서상태(취소:CANCEL, 회수:RETURN)

        //        oComm.ExecuteNonQuery()

        //    Catch ex As System.Data.SqlClient.SqlException
        //    Finally
        //        '모든 경우에 최종적으로 반드시 처리되어야 하는 코드
        //        If Not oConn Is Nothing Then
        //            oConn.Close()
        //            oConn.Dispose()
        //            oConn = Nothing
        //        End If

        //        If Not oComm Is Nothing Then
        //            oComm.Dispose()
        //            oComm = Nothing
        //        End If
        //    End Try
        //End If

        #endregion

        return sReturn;
    }
    #endregion


    #region 결재 안내메일 보내기 마이그레이션문서
    /// <summary>
    /// 결재 안내 메일 보내기
    /// </summary>
    /// <param name="sMode">결재 mode</param>
    /// <param name="Piid">process_id</param>
    /// <param name="Wiid">Workitem_id</param>
    /// <param name="Ptid">Performar_id</param>
    /// <param name="Pfid">PF_id</param>
    /// <param name="Sender">발신자</param>
    /// <param name="Recipients">수신자</param>
    /// <param name="FORM_INFO_EXT">프로세스옵션정보값</param>
    /// <param name="APPROVERCONTEXT">결재선</param>
    /// <param name="DSCR">PInstance.description</param>
    [WebMethod]
    public string SendMessageWSMig(string sMode, string sOpenMode, string Piid, string Wiid, string Ptid, string Pfid, string Sender, string[] Recipients, System.Xml.XmlNode oFormInfoExt, System.Xml.XmlDocument DSCR, bool bOMEntityTypeOU = false, string strContents = "", string strLanguage = "", string sReserved1 = "", string sReserved2 = "")
    {
        string sReturn = "";
        StringBuilder sbMailBody = new StringBuilder();
        StringBuilder sbLinkURL = new StringBuilder();
        StringBuilder sbMobileURL = new StringBuilder();
        string sSubjectMail = String.Empty;
        string sSubjectApproval = String.Empty;
        string sSubject = String.Empty;
        string sTodoListType = "ApprovalRequest";

        CfnCoreEngine.WfOrganizationManager OManager = null;

        #region 메일 제목
        sSubject = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["subject"].Value;
        switch (sMode)
        {
            case "APPROVAL":
                {
                    sSubjectMail = "[결재 진행]" + sSubject;
                    sTodoListType = "ApprovalRequest";
                    //sTodoListType = "ARRIVEDAPPROVAL";
                    break;
                }
            case "CHARGEJOB":
            case "REDRAFT":
                {
                    sSubjectMail = "[결재 진행]" + sSubject;
                    sTodoListType = "ApprovalRequest";
                    //sTodoListType = "ARRIVEDAPPROVAL";
                    break;
                }
            case "CHARGE":
            case "REDRAFT_RE":
            case "COMPLETE":
                {
                    sSubjectMail = "[결재 완료]" + sSubject;
                    sTodoListType = "ApprovalCompleted";
                    //sTodoListType = "ARRIVEDCOMPLETE";
                    break;
                }
            case "REJECT":
                {
                    sSubjectMail = "[결재 반송]" + sSubject;
                    sTodoListType = "ApprovalRejected";
                    break;
                }
            case "CCINFO":
            case "CIRCULATION":
                {
                    sSubjectMail = "[결재 참조/회람]" + sSubject;
                    sTodoListType = "ApprovalConsulted";
                    //CCINFO와 CIRCULATION 분리
                    //sTodoListType = "ARRIVEDCCINFO"; 
                    //sTodoListType = "ARRIVEDCIRCULATION"; 
                    break;
                }
            case "DELAY":
                {
                    sSubjectMail = "[결재 지연알림]" + sSubject;
                    break;
                }
            case "HOLD":
                {
                    sSubjectMail = "[결재 보류알림]" + sSubject;
                    sTodoListType = "ApprovalHold";
                    break;
                }
            case "WITHDRAW":
                {
                    sSubjectMail = "[결재 회수알림]" + sSubject;
                    sTodoListType = "ApprovalInitiatorWithdraw";
                    break;
                }
            case "ABORT":
                {
                    sSubjectMail = "[결재 취소알림]" + sSubject;
                    sTodoListType = "ApprovalInitiatorCancel";
                    break;
                }
            case "APPROVECANCEL":
                {
                    sSubjectMail = "[결재 승인취소알림]" + sSubject;
                    sTodoListType = "ApprovalCancel";
                    break;
                }
            case "ASSIGN_APPROVAL":
                {
                    sSubjectMail = "[결재 전달알림]" + sSubject;
                    sTodoListType = "ApprovalDelivery";
                    break;
                }
            case "ASSIGN_CHARGE":
                {
                    sSubjectMail = "[결재 담당자지정알림]" + sSubject;
                    sTodoListType = "ApprovalAssignChage";
                    break;
                }
            case "ASSIGN_R":
                {
                    sSubjectMail = "[결재 부서전달알림]" + sSubject;
                    sTodoListType = "ApprovalDeliveryDept";
                    break;
                }
            case "COMMENT":
                {
                    sSubjectMail = "[결재 의견알림]" + sSubject;
                    sTodoListType = "ApprovalComment";
                    break;
                }
            case "LEGACYERRER":
                {
                    sSubjectMail = "[전자결재 연동오류 안내]" + sSubject;
                    break;
                }
        }

        #endregion

        #region 메일 본문내용
        sbMailBody.Append("<MAIL>");
        switch (sMode)
        {
            case "APPROVAL":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 도착 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("새로운 결재문서가 결재할 문서함에 대기중입니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "REDRAFT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 도착 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("새로운 결재문서가 부서 수신함에 대기중입니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "CCINFO":
            case "CCINFO_U":
            case "CIRCULATION":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 참조/회람 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 참조/회람되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 참조/회람함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("회람 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "CHARGE":
            case "COMPLETE":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 완료 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 완료되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 완료함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "REDRAFT_RE":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 수신 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 수신되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 부서 공람함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "REJECT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 반려 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 반송 되었습니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("전자결재의 반송함에서 해당 문서를 확인하여 주시기 바랍니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "CHARGEJOB":
                {
                    //[2015-11-26 modi 유영재 과장] 알림 메시지 수정
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 도착 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("새로운 결재문서가 담당업무함에 대기중입니다.").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "DELAY":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 지연 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 지연되고 있습니다..").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("빠른 확인 부탁드립니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "HOLD":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 보류 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 보류 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("보류 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "WITHDRAW":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 회수 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 기안자에 의해 회수 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("회수 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "ABORT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 취소 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 기안자에 의해 취소 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("취소 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "APPROVECANCEL":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 승인취소 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 결재자에 의해 취소 되었습니다.").Append("]]]></CONTENT>");
                    if (strContents != "") sbMailBody.Append("<CONTENT><![CDATA[[").Append("취소 의견 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
            case "ASSIGN_APPROVAL":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 전달 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 전달 되었습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "ASSIGN_CHARGE":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 담당자 지정 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 담당자로 지정 되었습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "ASSIGN_R":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 부서전달 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재문서가 부서함으로 전달 되었습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "COMMENT":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 의견 알림 ").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("결재자가 결재문서에 결재의견을 입력하였습니다.").Append("]]]></CONTENT>");
                    break;
                }
            case "LEGACYERRER":
                {
                    sbMailBody.Append("<TITLE><![CDATA[").Append("전자결재(Approval)").Append("]]></TITLE>");
                    sbMailBody.Append("<CONTENT BOLD='YES'><![CDATA[[").Append("결재문서 연동오류 알림").Append("]]]></CONTENT>");
                    sbMailBody.Append("<CONTENT><![CDATA[[").Append("오류 내용 : ").Append(strContents).Append("]]]></CONTENT>");
                    break;
                }
        }

        #region 결재 상세 내용
        if (sOpenMode != "NONE")
        {
            string sInitName = "";
            string sInitOUName = "";

            string sFormName = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["name"].Value;
            string sDocsec = "";
            string sKeepyear = "";

            string sCatname = oFormInfoExt.SelectSingleNode("docinfo/catname").InnerText;

            switch (oFormInfoExt.SelectSingleNode("docinfo/docsec").InnerText)
            {
                case "100": sDocsec = "1등급"; break;
                case "200": sDocsec = "2등급"; break;
                case "300": sDocsec = "3등급"; break;
                default: sDocsec = "기타문서"; break;
            }

            if (oFormInfoExt.SelectSingleNode("docinfo/keepyear").InnerText == "99")
            {
                sKeepyear = "영구";
            }
            else
            {
                sKeepyear = oFormInfoExt.SelectSingleNode("docinfo/keepyear").InnerText + "년";
            }

            sbMailBody.Append("<HEADNAME><![CDATA[").Append(sFormName).Append("]]></HEADNAME>");
            sbMailBody.Append("<SUBJECT><![CDATA[").Append(sSubject).Append("]]></SUBJECT>");
            sbMailBody.Append("<DOCSEC><![CDATA[").Append(sDocsec).Append("]]></DOCSEC>");
            sbMailBody.Append("<KEEPYEAR><![CDATA[").Append(sKeepyear).Append("]]></KEEPYEAR>");
            sbMailBody.Append("<CATNAME><![CDATA[").Append(sCatname).Append("]]></CATNAME>");
            sbMailBody.Append("<INITNAME><![CDATA[").Append(sInitName).Append("]]></INITNAME>");
            sbMailBody.Append("<INITOUNAME><![CDATA[").Append(sInitOUName).Append("]]></INITOUNAME>");

            //링크 정보  
            //#region 부분 SSL 체크
            //try
            //{
            //    // 부분SSL 사용 : http 와 https가 페이지 설정으로 자동으로 바뀜.
            //    if (GetBaseConfig("UsePartiSSL").Equals("Y"))
            //    {
            //        if (SSLFlag.Secure == _sSSLFlag) // Secure
            //        {
            //            sbLinkURL.Append("https://");
            //        }
            //        else if (SSLFlag.InSecure == _sSSLFlag) // InSecure
            //        {
            //            sbLinkURL.Append("http://");
            //        }
            //    }
            //    // 부분SSL 사용 : http 와 https가 페이지 설정으로 자동으로 바뀌지는 않으나 Secure가 설정된 페이지에서는 https로 자동으로 바뀜.
            //    else if (GetBaseConfig("UsePartiSSL").Equals("S"))
            //    {
            //        if (SSLFlag.Secure == _sSSLFlag)
            //        {
            //            sbLinkURL.Append("https://");
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //}
            //#endregion

            //[2016-06-16 modi kh] 기초설정값 이용하여 설정
            //sbLinkURL.Append("http://");
            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("ApprovalProtocol", "0"));

            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
            sbLinkURL.Append("/WebSite/Approval/NotifyMail.aspx?a=");

            //[2016-06-16 modi kh] 기초설정값 이용하여 설정
            //sbLinkURL.Append("http://");
            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("ApprovalProtocol", "0"));

            sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
            sbLinkURL.Append("/WebSite/Approval/Forms/Form.aspx?");
            sbLinkURL.Append("mode=").Append(sOpenMode);
            sbLinkURL.Append("&piid=").Append(Piid);
            sbLinkURL.Append("&wiid=").Append(Wiid);
            sbLinkURL.Append("&pfid=").Append(Pfid);
            sbLinkURL.Append("&ptid=").Append(Ptid);
            //참조/회람 메일에서 양식 open시점에 Read_date Update를 위해 추가 2016-05-03 KWR
            if (sMode == "CCINFO" || sMode == "CIRCULATION")
            {
                sbLinkURL.Append("&mailmode=").Append("Y");
                if (sMode == "CIRCULATION")
                    sbLinkURL.Append("&isdepart=").Append(bOMEntityTypeOU);
                else
                    sbLinkURL.Append("&isdepart=").Append("true");
            }
            sbMailBody.Append("<URL><![CDATA[");
            sbMailBody.Append(sbLinkURL.ToString());
            sbMailBody.Append("]]></URL>");
        }
        #endregion

        sbMailBody.Append("</MAIL>");

        #endregion

        #region 신세계 MAIL 제목, 내용 관련

        #region 메일 제목
        //XmlNode Initiator = oApvList.SelectSingleNode("steps/division[@divisiontype='send']/step/ou/person[taskinfo/@kind='charge']");

        ////sSubjectMail += Environment.NewLine;
        //sSubjectMail += "* 제목 : " + sSubject;
        //if (Initiator != null)
        //{
        //    sSubjectMail += Environment.NewLine;
        //    //sSubjectMail += "* 기안자 : " + ((Initiator.Attributes["name"].Value.Split(';').Length > 1) ? (!String.IsNullOrEmpty(Initiator.Attributes["name"].Value.Split(';')[0])) ? Initiator.Attributes["name"].Value.Split(';')[0] : Initiator.Attributes["name"].Value.Split(';')[1] : Initiator.Attributes["name"].Value) + " " + ((Initiator.Attributes["title"].Value.Split(';').Length > 1) ? ((!String.IsNullOrEmpty(Initiator.Attributes["title"].Value.Split(';')[0])) ? Initiator.Attributes["title"].Value.Split(';')[0] : Initiator.Attributes["title"].Value.Split(';')[1]) : Initiator.Attributes["title"].Value);
        //    sSubjectMail += "* 기안자 : " + ((Initiator.Attributes["name"].Value.Split(';').Length > 1) ? (!String.IsNullOrEmpty(Initiator.Attributes["name"].Value.Split(';')[0])) ? Initiator.Attributes["name"].Value.Split(';')[0] : Initiator.Attributes["name"].Value.Split(';')[1] : Initiator.Attributes["name"].Value) + " " + ((Initiator.Attributes["title"].Value.Split(';').Length > 1) ? Initiator.Attributes["title"].Value.Split(';')[1] : Initiator.Attributes["title"].Value);
        //    if (Initiator.HasChildNodes && Initiator.SelectSingleNode("taskinfo") != null)
        //    {
        //        XmlNode Initiatortask = Initiator.SelectSingleNode("taskinfo");
        //        sSubjectMail += Environment.NewLine;
        //        sSubjectMail += "* 기안일시 : " + Initiatortask.Attributes["datecompleted"].Value;
        //    }
        //}

        //switch (sMode)
        //{
        //    case "REJECT":
        //        {
        //            if (!String.IsNullOrEmpty(strContents))
        //            {
        //                sSubjectMail += Environment.NewLine;
        //                sSubjectMail = "* 반송 : " + strContents;
        //            }
        //            else
        //            {
        //                XmlNode rejectedPerson = oApvList.SelectSingleNode("steps/division/step/ou/person[taskinfo/@status ='rejected']");
        //                if (rejectedPerson != null && rejectedPerson.SelectSingleNode("taskinfo/comment") != null)
        //                {
        //                    sSubjectMail += Environment.NewLine;
        //                    //sSubjectMail = "* 반송(" + ((rejectedPerson.Attributes["name"].Value.Split(';').Length > 1) ? rejectedPerson.Attributes["name"].Value.Split(';')[0] : rejectedPerson.Attributes["name"].Value) + " " + (rejectedPerson.Attributes["title"].Value.Split(';').Length > 1 ? rejectedPerson.Attributes["title"].Value.Split(';')[1] : rejectedPerson.Attributes["title"].Value) + ") : " + rejectedPerson.InnerText;
        //                    sSubjectMail = "* 반송(" + ((rejectedPerson.Attributes["name"].Value.Split(';').Length > 1) ? rejectedPerson.Attributes["name"].Value.Split(';')[0] : rejectedPerson.Attributes["name"].Value) + " " + (rejectedPerson.Attributes["title"].Value.Split(';').Length > 1 ? rejectedPerson.Attributes["title"].Value.Split(';')[1] : rejectedPerson.Attributes["title"].Value) + ") : " + rejectedPerson.InnerText;
        //                }
        //            }
        //            break;
        //        }

        //    case "HOLD":
        //        {
        //            if (!String.IsNullOrEmpty(strContents))
        //            {
        //                XmlNode holdPerson = oApvList.SelectSingleNode("steps/division/step/ou/person[taskinfo/comment/@relatedresult='reserved']");

        //                if (holdPerson != null)
        //                {
        //                    sSubjectMail += Environment.NewLine;
        //                    sSubjectMail = "* 보류(" + (holdPerson.Attributes["name"].Value.Split(';').Length > 1 ? holdPerson.Attributes["name"].Value.Split(';')[0] : holdPerson.Attributes["name"].Value) + " " + (holdPerson.Attributes["title"].Value.Split(';').Length > 1 ? holdPerson.Attributes["title"].Value.Split(';')[1] : holdPerson.Attributes["title"].Value) + ") : " + strContents;
        //                }
        //                else
        //                {
        //                    sSubjectMail += Environment.NewLine;
        //                    sSubjectMail = "* 보류 : " + strContents;
        //                }
        //            }
        //            break;
        //        }
        //    case "COMMENT":
        //        {
        //            sSubjectMail = "* 의견 : " + strContents;
        //            break;
        //        }
        //}
        #endregion

        #region 결재문서 원본보기 URL
        ////sbLinkURL.Append("https://");
        ////sbLinkURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
        //sbLinkURL.Append("/WebSite/Approval/Forms/Form.aspx?");
        //sbLinkURL.Append("mode=").Append(sOpenMode);
        //sbLinkURL.Append("&piid=").Append(Piid);
        //sbLinkURL.Append("&wiid=").Append(Wiid);
        //sbLinkURL.Append("&pfid=").Append(Pfid);
        //sbLinkURL.Append("&ptid=").Append(Ptid);
        #endregion

        #region 결재문서 Mobile URL

        /////WebSite/Mobile/Approval/ApprovalView.aspx?System=Approval&Alias=P.APPROVAL.APPROVAL&PIID=5b537c6c-a63f-489e-9492-6e4e6540e797&WIID=ed1d5bfc-f6c7-4698-90e3-2cc54933fa79&DocTypeCode=T000&Page=1&SearchType=ALL&SearchText=
        //string mobileAlias = string.Empty;
        //switch (sOpenMode)
        //{
        //    case "APPROVAL":
        //        mobileAlias = "P.APPROVAL.APPROVAL";
        //        break;
        //    case "PROCESS":
        //        mobileAlias = "P.PROCESS.PROCESS";
        //        break;
        //    case "COMPLETE":
        //        mobileAlias = "P.COMPLETE.COMPLETE";
        //        break;
        //    case "REJECT":
        //        mobileAlias = "P.REJECT.REJECT";
        //        break;
        //    case "CCINFO":
        //    case "CIRCULATION":
        //    case "TCINFO":
        //        mobileAlias = "P.TCINFO.TCINFO";
        //        break;
        //    default:
        //        mobileAlias = "";
        //        break;
        //}
        ////sbMobileURL.Append("https://");
        ////sbMobileURL.Append(CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain", "0"));
        //sbMobileURL.Append("/WebSite/Mobile/Approval/ApprovalView.aspx?System=Approval");
        //sbMobileURL.Append("&Alias=").Append(mobileAlias);
        //sbMobileURL.Append("&PIID=").Append(Piid);
        //sbMobileURL.Append("&WIID=").Append(Wiid);
        //sbMobileURL.Append("&DocTypeCode="); //사용하지 않는 값이라고해서 빈값넘김...
        //sbMobileURL.Append("&Page=1&SearchType=ALL&SearchText=");

        #endregion

        #endregion

        #region 양식별 결재 알림 관리
        // [2015-10-22 modi] 마리아디비 관련 수정 - 박경연
        string sFormID = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["id"].Value;
        string sEntCode = "";
        if (oFormInfoExt == null)
        {
            sEntCode = Session["DN_CODE"].ToString();
        }
        else
        {
            if (oFormInfoExt.SelectSingleNode("entcode") == null) sEntCode = Session["DN_CODE"].ToString();
            else sEntCode = oFormInfoExt.SelectSingleNode("entcode").InnerText;
        }
        string sUseAppMailForm = "N";
        {
            DataSet ds = null;
            DataPack INPUT = null;

            try
            {
                ds = new DataSet();
                INPUT = new DataPack();

                using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
                {
                    INPUT.add("@FormID", sFormID);
                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_selectfromwfforms", INPUT);
                    ds.Tables[0].TableName = "FORMINFO";
                }

                XmlDocument oFomrInfo = new XmlDocument();
                oFomrInfo.LoadXml(ds.Tables[0].Rows[0]["EXT_INFO"].ToString());
                if (oFomrInfo.SelectSingleNode("ExtInfo/ExtInfoData/fmMailList") != null)
                {
                    string sMailList = oFomrInfo.SelectSingleNode("ExtInfo/ExtInfoData/fmMailList").InnerText;
                    if (sMailList == "")
                    {
                        sUseAppMailForm = "Y";
                    }
                    else
                    {
                        if (sMailList.IndexOf(";" + sEntCode) > -1) sUseAppMailForm = "Y";
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (ds != null)
                {
                    ds.Dispose();
                    ds = null;
                }
                if (INPUT != null)
                {
                    INPUT.Dispose();
                    INPUT = null;
                }
            }
        }
        #endregion

        if (bOMEntityTypeOU)//부서로 온내용은 부서원으로 분리
        {
            // [2015-10-22 modi] 마리아디비 관련 수정 - 김한솔
            DataSet ds = null;
            DataPack INPUT = null;

            try
            {
                string sRecipientsOu = "";

                ds = new DataSet();
                INPUT = new DataPack();

                foreach (string sRecipientOu in Recipients)
                {
                    if (sRecipientOu != "") sRecipientsOu += "," + sRecipientOu + "";
                }

                using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
                {
                    INPUT.add("@UNIT_CODE", sRecipientsOu.Substring(1));
                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_get_UnitPersonInfo", INPUT);
                }

                System.Xml.XmlDocument oOUInfo = null;
                oOUInfo = new XmlDocument();
                oOUInfo.LoadXml(ds.GetXml());

                string sRecipientsTemp = "";
                foreach (System.Xml.XmlNode oOU in oOUInfo.SelectNodes("//Table"))
                {
                    sRecipientsTemp += oOU.SelectSingleNode("PERSON_CODE").InnerText + ";";
                }
                string[] aOuPerson = sRecipientsTemp.Split(';');
                Recipients = aOuPerson;
            }
            catch (System.Exception ex)
            {
                throw;
            }
            finally
            {
                if (ds != null)
                {
                    ds.Dispose();
                    ds = null;
                }
                if (INPUT != null)
                {
                    INPUT.Dispose();
                    INPUT = null;
                }

            }

        }
        #region mail/sms/messenger/todolist/mdm 보내기
        System.IO.StringWriter oTW = null;
        try
        {
            if (sUseAppMailForm == "Y")
            {
                string sContents = string.Empty;
                System.Xml.Xsl.XsltSettings settings = new System.Xml.Xsl.XsltSettings(true, true);
                System.Xml.Xsl.XslCompiledTransform xslsteps = new System.Xml.Xsl.XslCompiledTransform();
                {

                    xslsteps.Load(Server.MapPath("\\\\WebSite\\Common\\Mail_App_Mig.xsl"), settings, new System.Xml.XmlUrlResolver());


                }
                System.Xml.XPath.XPathDocument oXPathDoc = new System.Xml.XPath.XPathDocument(new System.IO.StringReader(sbMailBody.ToString()));
                oTW = new System.IO.StringWriter();
                xslsteps.Transform(oXPathDoc, null, oTW);
                oTW.GetStringBuilder().Remove(0, 39);
                sContents = oTW.ToString().Replace("##br", "<br />");
                oTW.Close();
                oTW = null;

                OManager = new CfnCoreEngine.WfOrganizationManager();
                string sSenderMail = OManager.GetPersonInfo(Sender).SelectSingleNode("om/person").Attributes["email"].Value;
                string sSenderName = Covi.Framework.Dictionary.GetDicInfo(OManager.GetPersonInfo(Sender).SelectSingleNode("om/person").Attributes["name"].Value, CF.LanguageType.ko);
                string sSenderCode = Covi.Framework.Dictionary.GetDicInfo(OManager.GetPersonInfo(Sender).SelectSingleNode("om/person").Attributes["code"].Value, CF.LanguageType.ko);

                {

                    System.Xml.XmlDocument oRecipients = OManager.GetOMEntityInfo(Recipients, CfnEngineInterfaces.IWfOrganization.OMEntityType.ettpPerson);
                    System.Xml.XmlNodeList oRecipientList = oRecipients.SelectNodes("om/person");
                    string sRecipientMails = System.String.Empty;

                    #region MAIL/SMS/MESSENGER/TODOLIST/MDM 보내기
                    foreach (System.Xml.XmlNode oRecipient in oRecipientList)
                    {
                        string sMedivum = ""; //변수 초기화 시점 이동, 해당변수는 개개인별로 다른 값을 가져야 함 2015.11.04 박은선
                        string sRecipientMail = oRecipient.Attributes["email"].Value;
                        string sRecipientName = oRecipient.Attributes["name"].Value;
                        string sRecipientCode = oRecipient.Attributes["sipaddress"].Value;

                        string sRecipientPersonCode = oRecipient.Attributes["code"].Value;

                        //알림 사용유무 확인
                        //개인 정보 확인
                        // [2018-02-09] 개인 환경설정 값(BASE_OBJECT_UR - ApprovalArrivalMedium)이 없는 경우 오류나는 현상 처리
                        if (oRecipient.Attributes["recnotify"].Value != null && oRecipient.Attributes["recnotify"].Value != "")
                        {
                            XmlDocument oXml = new XmlDocument();
                            oXml.LoadXml(oRecipient.Attributes["recnotify"].Value);
                            if (oXml.SelectSingleNode("mailconfig/" + sMode) != null)
                            {
                                sMedivum = oXml.SelectSingleNode("mailconfig/" + sMode).InnerText;
                            }
                        }

                        //무조건 알림을 받아야 하는 알림 종류 확인
                        Boolean bEntConfig = false;

                        string strDN_ID = "0";
                        using (DataTable dtDomain = CF.Utils.FilterDataTable(CF.CacheManager.GetCacheTable(CF.CacheKind.DomainInfo), string.Format("DN_Code='{0}'", sEntCode)))
                        {
                            foreach (DataRow row in dtDomain.Rows)
                            {
                                strDN_ID = row["DN_ID"].ToString();
                            }
                        }
                        // string sEntConfig = CF.ConfigurationManager.GetBaseConfig("DefaultArramKind", CF.CacheManager.GetCacheSigngle("DN_ID", sEntCode));
                        string sEntConfig = CF.ConfigurationManager.GetBaseConfig("DefaultArramKind", strDN_ID);

                        if (sEntConfig.IndexOf(sMode) > -1) bEntConfig = true;

                        #region MAIL
                        if ((sMedivum.IndexOf("MAIL") > -1 || bEntConfig) && sRecipientMail != "") //Mail 보내기
                        {
                            sRecipientMails += "," + sRecipientMail;
                        }
                        #endregion

                        #region SMS
                        if (sMedivum.IndexOf("SMS") > -1)//SMS 보내기
                        {
                            string sMobileNo = oRecipient.Attributes["mobile"].Value.Replace("-", "").Replace(" ", "").Replace("+", "");
                            if (sMobileNo != "")
                            {
                                using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
                                {
                                    DataPack INPUT = new DataPack();
                                    INPUT.add("@INDATE", DateTime.Now.ToString("yyyyMMdd"));
                                    INPUT.add("@INTIME", DateTime.Now.ToString("hhmmss"));
                                    INPUT.add("@RPHONE1", sMobileNo);
                                    INPUT.add("@RPHONE2", "");
                                    INPUT.add("@RPHONE3", "");
                                    INPUT.add("@RECVNAME", sRecipientName);
                                    INPUT.add("@MSG", "결재문서도착(The notice of arrival for approval)[" + sSubjectMail + "]" + sSenderName);
                                    //SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wfform_sendsms", INPUT); //테이블 수정 필요
                                }
                            }
                        }
                        #endregion

                        #region MESSENGER
                        if (sMedivum.IndexOf("MESSENGER") > -1)//MESSENGER 보내기
                        {

                            String strWebMsg = string.Empty;
                            StringBuilder sb = new StringBuilder();
                            StringBuilder sTargetNames = new StringBuilder("");
                            StringBuilder sTargetIDs = new StringBuilder("");
                            MemoService.MemoService oWS = new MemoService.MemoService();
                            {
                                sb.Append("<WebService><Message>");
                                sb.Append("<SenderName>").Append(sSenderName).Append("</SenderName>");
                                sb.Append("<SenderID>").Append(sSenderCode).Append("</SenderID>");
                                sb.Append("<TargetName>");
                                sb.Append(sRecipientName);
                                sb.Append("</TargetName>");
                                sb.Append("<TargetID>");
                                sb.Append(sRecipientCode);
                                sb.Append("</TargetID>");
                                sb.Append("<Content><![CDATA[");
                                sb.Append(sSubjectMail).Append("|").Append(sbLinkURL.ToString());
                                sb.Append("]]></Content>");
                                sb.Append("</Message></WebService>");

                                strWebMsg = sb.ToString();
                                sReturn = oWS.WriteMemo(strWebMsg);
                            }
                        }
                        #endregion

                        #region Todolist
                        if (sMedivum.IndexOf("TODOLIST") > -1)
                        {
                            Int32 iWidth = 790;
                            Int32 iHeight = 900;

                            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
                            {

                                DataPack INPUT = new DataPack();

                                //INPUT.add("@UR_Code", Recipients[0]);
                                INPUT.add("@UR_Code", sRecipientPersonCode);

                                INPUT.add("@Category", "Approval");
                                INPUT.add("@MsgType", sTodoListType);
                                INPUT.add("@Title", sSubject);      //INPUT.add("@Title", sSubject);
                                INPUT.add("@URL", sbLinkURL.ToString());
                                INPUT.add("@Message", sSubject);    //INPUT.add("@Message", sSubject);
                                INPUT.add("@OpenType", "OPENPOPUP");
                                INPUT.add("@PusherCode", sSenderCode);
                                INPUT.add("@Width", iWidth);
                                INPUT.add("@Height", iHeight);
                                INPUT.add("@ReservedStr1", "FORM");
                                INPUT.add("@ReservedStr2", "scroll");
                                SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.Com_U_TodoList_C", INPUT);
                            }
                        }
                        #endregion

                        #region MDM
                        if (sMedivum.IndexOf("MDM") > -1)
                        {
                            // 2016-03-22 hpark MDM알림에 링크 추가 s (모바일팀과 맞추기위해 일단 개발,배포,운영 서버만 반영함)
                            //MDMUtils.SendPush_UserID(sRecipientPersonCode, sSubjectMail);

                            //mnid & alias 가져오기
                            Boolean b_MakeUrlChk = true;    // URL 만들지 체크하는 값
                            string _strMnidPipe = "";       // 쿼리해온 mnid 값들(pipe로구분)
                            string _strAliasPipe = "";      // 쿼리해온 alias 값들(pipe로구분)
                            string _strMnid = "";           // 모드에따른 mnid
                            string _strAlias = "";          // 모드에따른 alias
                            string sbLinkURL_Mobile = "";   // mobile link
                            string _strMobileParam = "";    // 실제 넘기는 파라미터

                            DataSet ds = null;
                            DataPack INPUT = null;

                            try
                            {
                                ds = new DataSet();
                                INPUT = new DataPack();

                                using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                                {
                                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wf_getBaseMenuInfo", INPUT);
                                }

                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    _strAliasPipe += dr["Alias"].ToString() + "|";
                                    _strMnidPipe += dr["MN_ID"].ToString() + "|";
                                }
                            }
                            catch (System.Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                if (ds != null)
                                {
                                    ds.Dispose();
                                    ds = null;
                                }
                                if (INPUT != null)
                                {
                                    INPUT.Dispose();
                                    INPUT = null;
                                }
                            }

                            // URL 만들기
                            if (sOpenMode == "APPROVAL")
                            {
                                _strAlias = _strAliasPipe.Split('|')[0];
                                _strMnid = _strMnidPipe.Split('|')[0];
                            }
                            else if (sOpenMode == "PROCESS")
                            {
                                _strAlias = _strAliasPipe.Split('|')[1];
                                _strMnid = _strMnidPipe.Split('|')[1];
                            }
                            else if (sOpenMode == "COMPLETE")
                            {
                                _strAlias = _strAliasPipe.Split('|')[2];
                                _strMnid = _strMnidPipe.Split('|')[2];
                            }
                            else
                            {
                                b_MakeUrlChk = false;   // sOpenMode 값이 APPROVAL, PROCESS, COMPLETE 가 아니면 URL 만들지않음
                                MDMUtils.SendPush_UserID(sRecipientPersonCode, sSubjectMail);
                            }

                            if (b_MakeUrlChk)
                            {
                                sbLinkURL_Mobile = CF.ConfigurationManager.GetBaseConfigDomain("WebServiceProtocol_Mobile", "0");
                                sbLinkURL_Mobile += "://";
                                sbLinkURL_Mobile += CF.ConfigurationManager.GetBaseConfigDomain("WebServerDomain_Mobile", "0");
                                sbLinkURL_Mobile += GetPgModule("Mobile.ApprovalView");
                                sbLinkURL_Mobile += "?system=Approval";
                                sbLinkURL_Mobile += "&alias=" + _strAlias;
                                sbLinkURL_Mobile += "&fdid=";
                                sbLinkURL_Mobile += "&mnid=" + _strMnid;
                                sbLinkURL_Mobile += "&piid=" + Piid;
                                sbLinkURL_Mobile += "&wiid=" + Wiid;

                                // [2017-02-23] gbhwang 모바일 결재 알림 제목 수정
                                _strMobileParam = "LINK|";
                                _strMobileParam += sSubjectMail;
                                _strMobileParam += "|";
                                _strMobileParam += sbLinkURL_Mobile;

                                MDMUtils.SendPush_UserID(sRecipientPersonCode, _strMobileParam);
                            }
                            // 2016-03-22 hpark MDM알림에 링크 추가 e

                        }
                        #endregion
                    }
                    #endregion

                    #region ME(내 업무) 보내기

                    foreach (string oRecipient in Recipients)
                    {
                        if (oRecipient != null && oRecipient.Trim() != "")
                        {
                            string sMedivum = string.Empty; //변수 초기화 시점 이동, 해당변수는 개개인별로 다른 값을 가져야 함 2015.11.04 박은선
                            string sMeMedivum = string.Empty;
                            string sRecipientMail = string.Empty;
                            string sRecipientName = string.Empty;
                            string sRecipientCode = string.Empty;

                            string recnotify = string.Empty;

                            DataSet ds = new DataSet();
                            DataPack _INPUT = new DataPack();

                            try
                            {
                                using (SqlDacManager SqlDbAgent = new SqlDacManager())
                                {
                                    _INPUT.Clear();
                                    _INPUT.add("@UR_Code", oRecipient);
                                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.Com_U_MessagingSettingListMeApproval_R", _INPUT);//프로시저실행

                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        sMeMedivum = ds.Tables[0].Rows[0]["MeMedium"].ToString();
                                        sRecipientMail = ds.Tables[0].Rows[0]["email"].ToString();
                                        sRecipientName = ds.Tables[0].Rows[0]["name"].ToString();
                                        sRecipientCode = ds.Tables[0].Rows[0]["sipaddress"].ToString();
                                        recnotify = ds.Tables[0].Rows[0]["ApprovalMedium"].ToString();
                                    }
                                    //변경
                                    else
                                    {
                                        sMeMedivum = string.Empty;
                                        recnotify = string.Empty;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                if (_INPUT != null)
                                {
                                    _INPUT.Dispose();
                                }
                                if (ds != null)
                                {
                                    ds.Dispose();
                                }
                            }


                            #region ME
                            if (!String.IsNullOrEmpty(sMeMedivum) && sMeMedivum.Contains(sMode))
                            {
                                Int32 iWidth = 790;
                                Int32 iHeight = 900;

                                using (SqlDacManager SqlDbAgent = new SqlDacManager())
                                {
                                    DataPack INPUT = new DataPack();

                                    INPUT.add("@UR_Code", oRecipient);

                                    INPUT.add("@Category", "Approval");
                                    INPUT.add("@MsgType", sTodoListType);
                                    INPUT.add("@Title", sSubject);		//INPUT.add("@Title", sSubject);
                                    INPUT.add("@URL", sbLinkURL.ToString());
                                    INPUT.add("@MobileURL", sbMobileURL.ToString());
                                    INPUT.add("@Message", sSubject);	//INPUT.add("@Message", sSubject);
                                    INPUT.add("@OpenType", "OPENPOPUP");
                                    INPUT.add("@PusherCode", sSenderCode);
                                    INPUT.add("@Width", iWidth);
                                    INPUT.add("@Height", iHeight);
                                    INPUT.add("@ReservedStr1", "FORM");
                                    INPUT.add("@ReservedStr2", "scroll");
                                    SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.Com_U_TodoList_C", INPUT);

                                    //변경
                                    WriteMessage_log4("SendmessageResult", "* MsgType : " + sTodoListType + " * SendDate : " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " * sSubjectMail : " + sSubject + " * oRecipient : " + oRecipient + " * result : " + sReturn);
                                }
                            }
                            #endregion

                            #region TODO : [신세계-확인필요] 결재 단계 알람(기안자에게 결재 단계 진행 Push 전송)
                            //if (sMode == "APPROVAL")
                            //{
                            //    sSubjectApproval += "* 제목 : " + sSubject;

                            //    ExternalWebService EW = new ExternalWebService();
                            //    XmlNode oCharge = oApvList.SelectSingleNode("steps/division[@divisiontype='send']/step/ou/person[taskinfo/@kind='charge']");
                            //    XmlNodeList oApprovers = oApvList.SelectNodes("steps/division/step/ou/person[taskinfo/@result='completed'] | steps/division/step/ou/role[taskinfo/@result='completed']");

                            //    if (oApprovers != null && oApprovers.Count > 0 && (Sender != oApprovers[oApprovers.Count - 1].Attributes["code"].Value))
                            //    {
                            //        if (oCharge.Attributes["oucode"].Value.ToLower().IndexOf("ic0") > -1 || oCharge.Attributes["oucode"].Value.ToLower().IndexOf("ae0") > -1)
                            //        {
                            //            sSubjectApproval += Environment.NewLine;
                            //            sSubjectApproval += "* 결재자 : " + Covi.Framework.Dictionary.GetDicInfo(oApprovers[oApprovers.Count - 1].Attributes["name"].Value, CF.LanguageType.ko) + " " + ((oApprovers[oApprovers.Count - 1].Attributes["title"].Value.Split(';').Length > 1) ? oApprovers[oApprovers.Count - 1].Attributes["title"].Value.Split(';')[1] : oApprovers[oApprovers.Count - 1].Attributes["title"].Value);
                            //            sSubjectApproval += Environment.NewLine;
                            //            sSubjectApproval += "* 결재일시 : " + DateTime.Now.ToString("yyyy-MM-dd hh:mm");
                            //            sReturn = EW.SendBotsPushService("APPROVAL", "ARRIVEDAPPROVALSTEP", oCharge.Attributes["code"].Value, sSubjectApproval, "", "OPENPOPUP", "790", "900", sbLinkURL.ToString(), "");
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                    }

                    #endregion

                    #region SMS 주석 소스입니다. (이선민)
                    //if (sMode == "COMPLETE" || sMode == "REJECT")
                    //{
                    //    if (sMedivum.IndexOf("SMS") > -1 || DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["CompleteSendSms"].Value == "1") //SMS 보내기
                    //    if (sMedivum.IndexOf("SMS") > -1 || (DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["CompleteSendSms"] != null && DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["CompleteSendSms"].Value == "1"))
                    //    {
                    //        CommonService cs = new CommonService();
                    //        DataSet oDS = new DataSet();
                    //        DataPack INPUT = new DataPack();

                    //        //String pTxt = "결재문서완료(The notice of complete for approval)[" + sSubjectMail + "] " + sSenderName; //SMS 내용
                    //        String pTxt = "[제목 : " + sSubject + "] 결재문서가 완결되었습니다."; //SMS 내용

                    //        string sApproverPhone = string.Empty;
                    //        string sInitiatorPhone = string.Empty;
                    //        string sSenderMobileNumber = string.Empty;
                    //        string sRecMobileNumber = string.Empty;

                    //        string sSpName = "dbo.usp_userinfo"; // ID 로 핸드폰 번호 조회하는 SP

                    //        // 발신자 핸드폰 번호 구하기
                    //        using (SqlDacManager oDac = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    //        {
                    //            INPUT.add("@PERSON_CODE", Sender);
                    //            oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    //        }

                    //        if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                    //        {
                    //            sSenderMobileNumber = oDS.Tables[0].Rows[0]["AD_Mobile"].ToString();
                    //        }

                    //        XmlNodeList oPersonList = oApvList.SelectNodes("steps/division/step/ou/person");

                    //        string[] sApvLinePersonId;

                    //        //string sFmpf = DSCR.SelectSingleNode("ClientAppInfo/App/forminfos/forminfo").Attributes["prefix"].Value;


                    //        oDS = new DataSet();
                    //        INPUT = new DataPack();

                    //        using (SqlDacManager oDac = new SqlDacManager("ORG_ConnectionString"))
                    //        {
                    //            INPUT.add("@PERSON_CODE", oPersonList[0].Attributes["code"].Value);
                    //            oDS = oDac.ExecuteDataSet(CommandType.StoredProcedure, sSpName, INPUT);
                    //        }

                    //        if (oDS != null && oDS.Tables[0].Rows.Count > 0)
                    //        {
                    //            sRecMobileNumber = oDS.Tables[0].Rows[0]["AD_Mobile"].ToString();
                    //        }

                    //        // Send_SMS(string pTxt, string pNums, string pNumsCount, string pReturnNum, string pType) pType = SMS - S, MMS - M
                    //        cs.SendMessage(Sender, pTxt, sInitiatorPhone, "1", sSenderMobileNumber, "S", "I", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString());
                    //    }
                    //}
                    #endregion

                    if (sRecipientMails != "")
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                        {
                            DataPack INPUT = new DataPack();
                            INPUT.add("@sender", sSenderMail + "|" + sSenderName);
                            INPUT.add("@subject", sSubjectMail);
                            INPUT.add("@to", sRecipientMails.Substring(1));
                            INPUT.add("@cc", "");
                            INPUT.add("@bcc", "");
                            INPUT.add("@body_format", "HTML");
                            INPUT.add("@body", sContents);
                            INPUT.add("@piid", Piid);
                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wf_mail_insert", INPUT);
                        }
                    }

                }
            }
            sReturn = "OK";
        }
        catch (System.Exception ex)
        {
            sReturn = ex.Message;
        }
        finally
        {
            if (oTW != null)
            {
                oTW.Close();
                oTW = null;
            }
            if (OManager != null)
            {
                OManager.Dispose();
                OManager = null;
            }
        }
        #endregion



        #region 모바일 결재 양식별(추후 고려 후 개발)

        //'//모바일 결재 양식의 경우 모바일 사용 결재자는 push시킴2008.10.25
        //'//sDeputyMobileYN , sMobileYN
        //Dim sMobileUse As System.String = String.Empty
        //If Not oFormInfoExt.SelectSingleNode("IsMobile") Is Nothing Then
        //    If oFormInfoExt.SelectSingleNode("IsMobile").InnerText = "true" Then
        //        sMobileUse = "Y"
        //    End If
        //End If
        //If oInitiatorNode Is Nothing Then
        //Else
        //    sInitiatorName = oInitiatorNode.Attributes("name").Value & " " & oInitiatorNode.Attributes("title").Value.Split(";")(1) & "(" & oInitiatorNode.Attributes("ouname").Value & ")"
        //End If

        //If sMobileUse = "Y" And sMobileYN = "Y" Then
        //    Dim sConnectionString2 As System.String = CfnDatabaseUtility.WfDatabaseDirectory.GetConnectionString("mobile")
        //    Dim oConn As System.Data.SqlClient.SqlConnection = Nothing
        //    Dim oComm As System.Data.SqlClient.SqlCommand = Nothing

        //    Try
        //        oConn = New System.Data.SqlClient.SqlConnection(sConnectionString2)
        //        oConn.Open()
        //        oComm = New System.Data.SqlClient.SqlCommand("dbo.MPOP_PROC_PUSH_EVENT", oConn)
        //        oComm.CommandType = CommandType.StoredProcedure

        //        If Workitem.deputyId <> String.Empty Then
        //            oComm.Parameters.AddWithValue("@touser_id", Workitem.deputyId)               '-- 사번 
        //        Else
        //            oComm.Parameters.AddWithValue("@touser_id", Workitem.performerId)               '-- 사번 
        //        End If
        //        oComm.Parameters.AddWithValue("@msg_event", "APPR_NEW")           '-- push event 종류(메일:MAIL_NEW, 전자결재:APPR_NEW, CRM:CRM_NEW)
        //        oComm.Parameters.AddWithValue("@mailkey", String.Empty)      '-- 메일키(URL)
        //        oComm.Parameters.AddWithValue("@crmkey", String.Empty)     '--CRM 문서키 
        //        oComm.Parameters.AddWithValue("@piid", PInstance.id)            '-- 전자결재 문서키 1
        //        oComm.Parameters.AddWithValue("@wiid", Workitem.id)           '-- 전자결재 문서키 2
        //        oComm.Parameters.AddWithValue("@pfid", sPFID)       '-- 전자결재 문서키 3
        //        oComm.Parameters.AddWithValue("@ptid", sPTID)        '-- 전자결재 문서키 4
        //        If Workitem.deputyId <> String.Empty Then
        //            oComm.Parameters.AddWithValue("@usid", Workitem.deputyId)               '-- 사번 
        //        Else
        //            oComm.Parameters.AddWithValue("@usid", Workitem.performerId)               '-- 사번 
        //        End If

        //        '//oComm.Parameters.AddWithValue("@usid", sRecipients(0))            '-- 전자결재 문서키 5
        //        oComm.Parameters.AddWithValue("@fiid", oFNode.Attributes("instanceid").Value)     '-- 전자결재 문서키 6
        //        oComm.Parameters.AddWithValue("@fmpf", oFNode.Attributes("prefix").Value)         '-- 전자결재 문서키 7
        //        oComm.Parameters.AddWithValue("@fmrv", oFNode.Attributes("revision").Value)      '-- 전자결재 문서키 8
        //        oComm.Parameters.AddWithValue("@sender", sInitiatorName)          '-- 메일:보낸사람 메일주소, 전자결재,CRM:기안자 이름
        //        oComm.Parameters.AddWithValue("@write_dt", Now())          '등록자명
        //        oComm.Parameters.AddWithValue("@important", String.Empty)  '-- 메일 중요도(0:낮음,1:보통,2:높음) 
        //        If oFNode.Attributes("isfile").Value = "0" Then
        //            oComm.Parameters.AddWithValue("@attach", "N")  '-- 첨부파일 여부 (Y:첨부있음, N:첨부없음)
        //        Else
        //            oComm.Parameters.AddWithValue("@attach", "Y")  '-- 첨부파일 여부 (Y:첨부있음, N:첨부없음)
        //        End If
        //        oComm.Parameters.AddWithValue("@subject", PInstance.subject)    '-- 제목
        //        oComm.Parameters.AddWithValue("@doc_type", "미결")            '-- 전자결재 문서종류(미결,반송)
        //        oComm.Parameters.AddWithValue("@doc_status", String.Empty)          '-- 전자결재 문서상태(취소:CANCEL, 회수:RETURN)

        //        oComm.ExecuteNonQuery()

        //    Catch ex As System.Data.SqlClient.SqlException
        //    Finally
        //        '모든 경우에 최종적으로 반드시 처리되어야 하는 코드
        //        If Not oConn Is Nothing Then
        //            oConn.Close()
        //            oConn.Dispose()
        //            oConn = Nothing
        //        End If

        //        If Not oComm Is Nothing Then
        //            oComm.Dispose()
        //            oComm = Nothing
        //        End If
        //    End Try
        //End If

        #endregion

        return sReturn;
    }
    #endregion


    #region 연동결재처리(SAP WSDL, XI, RFC처리)
    /// <summary>
    /// 연동결재 처리 main
    /// </summary>
    /// <param name="fmpf">form prefix</param>
    /// <param name="BODYCONTEXT">결재본문값</param>
    /// <param name="FORM_INFO_EXT">프로세스옵션정보값</param>
    /// <param name="APPROVERCONTEXT">결재선</param>
    /// <param name="PreApproveProcess">기안결재여부 true : 기안상태 넘김, false : 완료상태넘김</param>
    /// <param name="ApvResult">결재결과 rejected외:승인, rejected = 반려</param>
    /// <param name="DocNumber">결재문서번호</param>
    /// <param name="ApproverId">결재자id</param>
    /// <param name="fiid">form instance id</param>
    [WebMethod]
    public string ExecuteLegacy(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid = "", string sReserved1 = "", string sReserved2 = "")
    {       
        string sReturn = "";
        string Vac = Covi.Framework.ConfigurationManager.GetBaseConfig("VacationForm", "0");
        string[] VacForm = Vac.Split('^');
        string VacRe = VacForm[1];
        string VacCan = VacForm[0];
        try
        {
            //상태변경
            LegacyAppStatusChange(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode);
            //로그저장
            LegacyCallLog(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode);

            switch (fmpf)
            {
                case "WF_FORM_VACATION_REQUEST": // 휴가신청
                //  case "WF_FORM_VACATION_REQUEST2": // 휴가신청
                case "NEW_WF_VACATION": // 휴가신청
                    execWF_FORM_VACATION_REQUEST(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);	// [2015-02-05 modi] 
                    break;
                //  case "WF_FORM_VACATIONCANCEL": //휴가취소신청
                //    execWF_FORM_VACATIONCANCEL(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);	// [2015-02-05 modi] 
                //    break;
                case "WF_FORM_DRAFT_License": // Corp Card Expense
                    execWF_FORM_DRAFT_License(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode);
                    break;
                case "WF_FORM_CASH_EXPENSE": // Cash Expense
                    execWF_FORM_CASH_EXPENSE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode);
                    break;

                case "WF_FORM_CORP_CARD_EXPENSE": // Corp Card Expense
                    execWF_FORM_CORP_CARD_EXPENSE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode);
                    break;
                //case "WF_FORM_CONTRACT_DRAFT": //계약품의(연관프로젝트) 처리
                case "WF_FORM_CONTRACT_DRAT": //계약품의 처리
                    execWF_FORM_CONTRACT_DRAT(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode);
                    break;
                case "WF_BUDGET_REQUEST": //예산신청서
                    execWF_BUDGET_REQUEST(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_BUDGET_SHIFT_REQUEST": //예산변경신청서
                    execWF_BUDGET_SHIFT_REQUEST(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_PROJECTEXPENSE_007": //프로젝트 경비신청서
                    execWF_FORM_PROJECTEXPENSE_007(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_PROJECTCODE": // 프로젝트 코드신청서
                    execWF_FORM_PROJECTCODE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_DRAFT_ORDER": // 발주 품의
                    execWF_FORM_DRAFT_ORDER(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_PROJECT_EXECPLAN": //프로젝트 집행계획서
                    execWF_FORM_PROJECT_EXECPLAN(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_COVI_03": //프로젝트 수당청구서
                    execWF_COVI_03(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_EXPENSE_3DEPTH_CARD": // 카드지출 품의서
                    if (apvMode == "COMPLETE")
                    {
                        CoviEAccount.Xpense.ERPInterface createXpense = new CoviEAccount.Xpense.ERPInterface();
                        createXpense.DouzonAccountCardExpenseDocCreate(fiid, "", "", "", "", DocNumber);
                    }


                    break;
                case "WF_EXPENSE_3DEPTH_EXPENSE": // 비용지출품의서
                    if (apvMode == "COMPLETE")
                    {
                        CoviEAccount.Xpense.ERPInterface createXpense = new CoviEAccount.Xpense.ERPInterface();
                        createXpense.DouzonAccountDocCreate(fiid, "", "", "", "", DocNumber);
                    }
                    break;

                case "WF_FORM_EX_MAIL": // 외부메일 요청서
                    execWF_FORM_EX_MAIL(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_FI_TEST": // 회계전표 테스트                 
                    execWF_FORM_FI_TEST(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_LEGACY_FI_STATE": // 일반결의전표
                case "WF_FORM_LEGACY_FI_STATE_NEW": // 일반결의전표(신규양식) 
                case "WF_FORM_LEGACY_FI_STATE_UNBAN": // 운반매입의전표   
                case "WF_FORM_LEGACY_FI_STATE_PUR": // 구매매입전표   
                    execWF_FORM_LEGACY_FI_STATE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_LEGACY_INVEST": // 투자공사 결과보고서
                    execWF_FORM_LEGACY_INVEST(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_LEGACY_INVEST_CE": // 투자품의(자본적지출)
                    execWF_FORM_LEGACY_INVEST_CE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_LEGACY_INVEST_COM": // 공사 완료보고서
                    execWF_FORM_LEGACY_INVEST_COM(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_ID_NEW": // ID신규신청서                
                    execWF_FORM_ID_NEW(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_ID_CHANGE": // 재직자 ID 신청              
                    execWF_FORM_ID_CHANGE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_CORPORATE_SENSE": // 법인임감날인 품의서
                case "WF_FORM_CORPORATE_SENSE_B":    // 법인인감날인 품의서(본)
                    execWF_FORM_CORPORATE_SENSE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_HOLIDAY_PLAN":    // 휴일근무 계획서
                case "WF_FORM_HOLIDAY_CHECK":   // 휴일근무 확인서
                    execWF_FORM_HOLIDAY_CHECK_PLAN(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_METAL":    // 고철대지급요청명세서
                    execWF_FORM_METAL(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_ITEM_MATCH": // 유사품목매칭               
                    execWF_FORM_ITEM_MATCH(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_GITEM_INSERT": // 신규품목등록               
                    execWF_FORM_GITEM_INSERT(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_DEVELOPMENT_AN":    //양식테스트
                    execWF_FORM_DEVELOPMENT_AN(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
					
				case "WF_FORM_SPI_SLM00001":    //전산개발 변경적용 의뢰서
                    execWF_FORM_SPI_SLM00001(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_SR": // SR요청서 
                    execWF_FORM_SR(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_SPRAIL_ESTIMATE": // 분기기 견적서  
                    execWF_FORM_SPRAIL_ESTIMATE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_SPI_SPIFI0002": // 환불품의서  
                    execWF_FORM_SPI_SPIFI0002(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_SEIZE_C": // 가압류(지급보류)요청
                    execWF_FORM_SEIZE_C(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_EV_CHECK": // 환경안전_시정조치 요구서
                    execWF_FORM_EV_CHECK(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                case "WF_FORM_S5110MA1_KO455": // 예외대체의뢰서 
                    execWF_FORM_S5110MA1_KO455(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_NEW_PME203MA1_KO455": // 프로젝트정산요청서(NEW)
                    execWF_FORM_NEW_PME203MA1_KO455(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_F2103MA4_KO455": // 예산편성신청서(ERP연동)
                    execWF_FORM_F2103MA4_KO455(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                    
                case "WF_FORM_LOAN_CON_G": // 여신품의서
                case "WF_FORM_LOAN_CON_C": 
                case "WF_FORM_LOAN_MAN_G": 
                case "WF_FORM_LOAN_MAN_C": 
                case "WF_FORM_LOAN_AGE":
                case "WF_FORM_LOAN_RECYCLE":
                case "WF_FORM_LOAN_SCRAP":
                case "WF_FORM_LOAN_AGE_V0":   //대리점
                case "WF_FORM_LOAN_CON_C_V0": //건설사보증
                case "WF_FORM_LOAN_MAN_C_V0": //제조사신용
                case "WF_FORM_LOAN_MAN_G_V0": //제조사보증
                    execWF_FORM_LOAN(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_SPRAIL_CONSTRUCTION_RE": // 공사작업일보
                    execWF_FORM_SPRAIL_CONSTRUCTION_RE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_M_PUR_OFFICER_APP": // 구매품의승인용
                    execWF_FORM_M_PUR_OFFICER_APP(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
                    
                case "WF_FORM_VACATION_BEFORE_APP": // 휴가원(사전승인)
                    execWF_FORM_VACATION_BEFORE_APP(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_M_PUR_OFFICER_APP_NDW": // 구매품의승인(에스피네이처)
                    execWF_FORM_M_PUR_OFFICER_APP_NDW(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                case "WF_FORM_LEGACY_OUT_CONSTRUCT": // 외주공사 품의 등록  
                    execWF_FORM_LEGACY_OUT_CONSTRUCT(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;

                    /*case "WF_FORM_MIN":   // 휴일근무 확인서
                        execWF_WF_FORM_MIN(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                        break;
                      */
				case "WF_FORM_SRTEST": // SR요청서 기능추가양식(WEB연동)
                    execWF_FORM_SRTEST(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
                    break;
            }

            #region [경비결재 시스템] =============================================================

            if (CF.ConfigurationManager.GetBaseConfigDomain("ExpenseForm", "0").ToLower().IndexOf(fmpf.ToLower()) != -1)
            {
                //InterfaceProxy callInterfaceStatus = new InterfaceProxy();
                ///* 경비결재용 - 운영배포 금지
                EXECWF_EXPENSE(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
            }

            #endregion

            //인사평가 양식의 경우 인사평가 결과를 저장함.
            if (getIncludeFormPrefix(Covi.Framework.ConfigurationManager.GetBaseConfig("EvaluationFormPrefix", "0").Trim(), "^", fmpf))
            {
                execPersonnelEvaluation(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);
            }

            //휴가신청서, 휴가 취소신청서 연동
            if (fmpf == VacRe && apvMode.Equals("COMPLETE")) //휴가신청서
            {
                execWF_FORM_VACATION_REQUEST(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);	// [2015-02-05 modi] 
            }
            else if (fmpf == VacCan && apvMode.Equals("COMPLETE")) //휴가취소신청서
            {
                execWF_FORM_VACATIONCANCEL(fmpf, BODYCONTEXT, FORM_INFO_EXT, APPROVERCONTEXT, PreApproveProcess, ApvResult, DocNumber, ApproverId, fiid, apvMode, piid);	// [2015-02-05 modi] 
            }
            sReturn = "OK";
        }
        catch (System.Exception ex)
        {
            sReturn = ex.Message + " " + ex.StackTrace;
        }
        finally
        {
        }

        return sReturn;
    }

    #region Cash Expense
    private void execWF_FORM_CASH_EXPENSE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        #region 소스에서 반복문 돌면서 SP호출
                        /*
                        XmlNodeList oList = oRoot.SelectNodes("tblCash");
                        foreach(System.Xml.XmlNode oNode in oList)
                        {
                            DataPack INPUT = new DataPack();
                            INPUT.add("@pthday_", oNode.SelectSingleNode("_MULTI_DATE").InnerText.Replace("-", ""));   //사용날짜
                            INPUT.add("@pthcct_", oRoot.SelectSingleNode("INITIATOR_OU_CODE_DP").InnerText);    //부서코드 or 매장번호
                            INPUT.add("@pthpid_", oNode.SelectSingleNode("_MULTI_ACNT").InnerText);    //Account 코드
                            INPUT.add("@pthamb_", oNode.SelectSingleNode("_MULTI_AMT").InnerText.Replace(",", ""));    //사용금액
                            INPUT.add("@pthamn_", oNode.SelectSingleNode("_MULTI_AMT").InnerText.Replace(",", ""));    //공급가
                            INPUT.add("@pthrem_", oNode.SelectSingleNode("_MULTI_DESC").InnerText);    //내용
                            INPUT.add("@pthatp_", oNode.SelectSingleNode("_MULTI_PLATFORM").InnerText);    //사용처(00:본사, 02:MDS, 04: Kiosk, 06:McCafe)
                            INPUT.add("@pthusc_", oNode.SelectSingleNode("_MULTI_PERSONS").InnerText);    //같이 사용한 인원
                            INPUT.add("@pthuid_", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);    //사용자 사원번호 or 매장ID
                            INPUT.add("@pthwho_", oNode.SelectSingleNode("_MULTI_WITH_WHOM").InnerText);    //같이 사용한 명단
                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_mcd_cash_expense_insert", INPUT);
                        }*/
                        #endregion

                        #region DB에서 오라클 SP를 여러번 호출
                        DataPack INPUT = new DataPack();
                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);     //BODY_CONTEXT
                        INPUT.add("@DEPT_CODE", oRoot.SelectSingleNode("INITIATOR_OU_CODE_DP").InnerText);              //부서코드
                        INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);   //사원번호
                        INPUT.add("@FMPF", fmpf);                   //PFMP
                        INPUT.add("@TYPE", null);                   //FLAG
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_mcd_coprcard_approval", INPUT);
                        #endregion
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            /*
            // 오류 메일 발송
            if (String.IsNullOrEmpty(CF.ConfigurationManager.GetBaseConfigDomain("MANAGER_INFO", "0")) == false)
            {
                List<string> mailReceivers = new List<string>();
                // 진명완;7076;myungwan.jin@samyang.com¶육미란;7086;miran.yook@samyang.com
                string[] arrManager = CF.ConfigurationManager.GetBaseConfigDomain("MANAGER_INFO", "0").Split('¶');
                foreach (var item in arrManager)
                {
                    if (String.IsNullOrEmpty(item) == false)
                    {
                        mailReceivers.Add(item.Split(';')[2]);
                    }
                }
                foreach (var item in mailReceivers)
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                    {
                        DataPack errorINPUT = new DataPack();
                        errorINPUT.add("@sender", strInfoMailSender);
                        errorINPUT.add("@subject", "통합 전자결재 Approval WebService Error");
                        errorINPUT.add("@to", item);
                        errorINPUT.add("@cc", "");
                        errorINPUT.add("@bcc", "");
                        errorINPUT.add("@body_format", "HTML");
                        errorINPUT.add("@body", ex.Message + " " + ex.StackTrace);
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wf_mail_insert", errorINPUT);
                    }
                }
            }
            */
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region Corp Card Expense
    private void execWF_FORM_CORP_CARD_EXPENSE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        #region DB에서 오라클 SP를 여러번 호출
                        DataPack INPUT = new DataPack();
                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);     //BODY_CONTEXT
                        INPUT.add("@DEPT_CODE", null);              //부서코드
                        INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_NAME_DP2").InnerText);   //사원번호
                        INPUT.add("@FMPF", fmpf);                   //PFMP
                        INPUT.add("@TYPE", "D");                    //FLAG
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_mcd_coprcard_approval", INPUT);
                        #endregion
                    }
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        #region DB에서 오라클 SP를 여러번 호출
                        DataPack INPUT = new DataPack();
                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);     //BODY_CONTEXT
                        INPUT.add("@DEPT_CODE", null);              //부서코드
                        INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_NAME_DP2").InnerText);   //사원번호
                        INPUT.add("@FMPF", fmpf);                   //PFMP
                        INPUT.add("@TYPE", "A");                    //FLAG
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_mcd_coprcard_approval", INPUT);
                        #endregion
                    }
                    break;
                case "REJECT": //반려
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        #region DB에서 오라클 SP를 여러번 호출
                        DataPack INPUT = new DataPack();
                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);     //BODY_CONTEXT
                        INPUT.add("@DEPT_CODE", null);              //부서코드
                        INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_NAME_DP2").InnerText);   //사원번호
                        INPUT.add("@FMPF", fmpf);                   //PFMP
                        INPUT.add("@TYPE", "B");                    //FLAG
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_mcd_coprcard_approval", INPUT);
                        #endregion
                    }
                    break;
                case "WITHDRAW": // 회수
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        #region DB에서 오라클 SP를 여러번 호출
                        DataPack INPUT = new DataPack();
                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);     //BODY_CONTEXT
                        INPUT.add("@DEPT_CODE", null);              //부서코드
                        INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_NAME_DP2").InnerText);   //사원번호
                        INPUT.add("@FMPF", fmpf);                   //PFMP
                        INPUT.add("@TYPE", "R");                    //FLAG
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_mcd_coprcard_approval", INPUT);
                        #endregion
                    }
                    break;
                case "APPROVAL": // 결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        #region DB에서 오라클 SP를 여러번 호출
                        DataPack INPUT = new DataPack();
                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);     //BODY_CONTEXT
                        INPUT.add("@DEPT_CODE", null);              //부서코드
                        INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_NAME_DP2").InnerText);   //사원번호
                        INPUT.add("@FMPF", fmpf);                   //PFMP
                        INPUT.add("@TYPE", "A");                    //FLAG
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_mcd_coprcard_approval", INPUT);
                        #endregion
                    }
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            /*
            // 오류 메일 발송
            if (String.IsNullOrEmpty(CF.ConfigurationManager.GetBaseConfigDomain("MANAGER_INFO", "0")) == false)
            {
                List<string> mailReceivers = new List<string>();
                // 진명완;7076;myungwan.jin@samyang.com¶육미란;7086;miran.yook@samyang.com
                string[] arrManager = CF.ConfigurationManager.GetBaseConfigDomain("MANAGER_INFO", "0").Split('¶');
                foreach (var item in arrManager)
                {
                    if (String.IsNullOrEmpty(item) == false)
                    {
                        mailReceivers.Add(item.Split(';')[2]);
                    }
                }
                foreach (var item in mailReceivers)
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
                    {
                        DataPack errorINPUT = new DataPack();
                        errorINPUT.add("@sender", strInfoMailSender);
                        errorINPUT.add("@subject", "통합 전자결재 Approval WebService Error");
                        errorINPUT.add("@to", item);
                        errorINPUT.add("@cc", "");
                        errorINPUT.add("@bcc", "");
                        errorINPUT.add("@body_format", "HTML");
                        errorINPUT.add("@body", ex.Message + " " + ex.StackTrace);
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wf_mail_insert", errorINPUT);
                    }
                }
            }
            */
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="APPROVERCONTEXT"></param>
    private string LastManager(string APPROVERCONTEXT)
    {
        string pEmpId = "";          // 기안자 ID
        string pDeptCd = "";         // 부서 ID
        string pLastManager = "";     // 최종결재자 사원 ID
        string pLastOrgPosition = ""; // 최종결재자 부서 ID


        XmlDocument oApvList = new XmlDocument();
        oApvList.LoadXml(APPROVERCONTEXT);

        XmlNode oCharge = oApvList.SelectSingleNode("steps/division[@divisiontype='send']/step/ou/person[taskinfo/@kind='charge']");
        XmlNodeList oPersons = oApvList.SelectNodes("steps/division[@divisiontype='send']/step/ou/person[taskinfo/@kind!='review' and taskinfo/@kind!='bypass' and taskinfo/@kind!='skip']");

        pEmpId = oCharge.Attributes["code"].Value;
        pDeptCd = oCharge.Attributes["oucode"].Value;

        foreach (XmlNode oPerson in oPersons)
        {
            pLastManager = oPerson.Attributes["code"].Value;
            pLastOrgPosition = oPerson.Attributes["oucode"].Value;
        }

        return pLastManager;
    }

    #endregion

    /// <summary>
    /// 양식테이블의 dField에 값 저장
    /// </summary>
    /// <param name="fmpf"></param>
    /// <param name="fiid"></param>
    /// <param name="dfield"></param>
    /// <param name="context"></param>
    public void SetDFieldContext(string fmpf, string fiid, string dfield, string context)
    {
        using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
        {
            DataPack INPUT = new DataPack();
            INPUT.add("@fmpf", fmpf);
            INPUT.add("@fiid", fiid);
            INPUT.add("@dfield", dfield);
            INPUT.add("@context", context);
            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.usp_wfform_forminfo_update_by_fmpf", INPUT);
        }
    }

    private void execWF_FORM_CONTRACT_DRAT(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode)
    {
        if (apvMode == "COMPLETE")
        {
            string szreturn = string.Empty;
            XmlDocument oReturnXML = new XmlDocument();
            oReturnXML.LoadXml(BODYCONTEXT);
            //body_context_변환하기
            XmlNode oRoot = oReturnXML.DocumentElement;

            //변수처리
            string nYear = oRoot.SelectNodes("nYear")[0].InnerText; //[ContractDocNumber] 파라메터 - 년도
            string nfiid = oRoot.SelectNodes("nfiid")[0].InnerText; //[ContractDocNumber] 파라메터 - fiid
            string salesNm = oRoot.SelectNodes("salesNm")[0].InnerText; //[BDT_CUSTOM] 파라메터 - salesNm
            string customNm = oRoot.SelectNodes("Client_NAME")[0].InnerText; //[BDT_CUSTOM] 파라메터 - customNm

            int nCnt = oRoot.SelectNodes("MULTI_TABLE")[0].ChildNodes.Count;
            int nCount;
            int IdCnt = 1;
            string[] DamName = new string[nCnt];//담당자이름
            string[] DamNum = new string[nCnt];//담당자연락처
            string[] DamEmail = new string[nCnt];//담당자이메일

            DataPack INPUT = new DataPack();
            try
            {
                //Legacy Process 처리
                using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                {
                    //1. 계약번호, 고객명 연동 부분
                    //SqlDbAgent.ConnectionString = Covision.Framework.Common.Configuration.ConfigurationApproval.ApprovalConfig("COVI_FLOW_SI_ConnectionString").ToString();
                    //insert sp호출
                    INPUT.add("@nYear", nYear);
                    INPUT.add("@nfiid", nfiid);
                    INPUT.add("@salesNm", salesNm);
                    INPUT.add("@customNm", customNm);


                    WriteMessage_log4("PREVIOUS_1", "계약번호, 고객명 연동 시작  :  계약번호 = fiid - " + nfiid + "     ,   nYear  - " + nYear + "     /   고객명 = salesNm - " + salesNm);

                    SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.Contract_num", INPUT);//sp명                    

                    WriteMessage_log4("SUCCESS_1", " 계약번호, 고객명 연동 성공  :   fiid - " + nfiid);

                    INPUT.Clear();

                    // 2. 담당자 연동 부분
                    if (nCnt > 0)
                    {

                        for (nCount = 0; nCount < nCnt; nCount++)
                        {
                            DamName[nCount] = oRoot.SelectNodes("MULTI_TABLE")[0].ChildNodes[nCount].ChildNodes[1].InnerText; //담당자 이름
                            DamNum[nCount] = oRoot.SelectNodes("MULTI_TABLE")[0].ChildNodes[nCount].ChildNodes[2].InnerText;//담당자 연락처
                            DamEmail[nCount] = oRoot.SelectNodes("MULTI_TABLE")[0].ChildNodes[nCount].ChildNodes[3].InnerText;//담당자 이메일

                            INPUT.add("@customName", customNm);//고객명
                            INPUT.add("@DamName", DamName[nCount]);//이름
                            INPUT.add("@DamNum", DamNum[nCount]);//연락처
                            INPUT.add("@DamEmail", DamEmail[nCount]);//이메일

                            WriteMessage_log4("PREVIOUS_2", " 담당자 연동 시작  :  고객명 - customNm :" + customNm + "     ,   이름  -  DamName[" + nCount + "] :  " + DamName[nCount] + "     ,  연락처  -   DamNum[" + nCount + "] :  " + DamNum[nCount] + "     ,   이메일  -  DamEmail[" + nCount + "] :  " + DamEmail[nCount]);

                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.Contract_PERSON", INPUT);//sp명                    

                            WriteMessage_log4("SUCCESS_2", " 담당자 연동 성공  :   COUNT  - " + nCount);


                            IdCnt++;

                            INPUT.Clear();
                        }

                    }

                }

            }
            catch (System.Exception ex)
            {
                WriteMessage_log4("ERROR", "fiid  -   " + nfiid + "    ,    " + ex.ToString());
                throw ex;
            }
            finally
            {
            }
        }
    }

    #region 휴가신청서
    private void execWF_FORM_VACATION_REQUEST(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        if (Covi.Framework.ConfigurationManager.GetBaseConfig("VACATION_INERLOCK", "0") == "Y")
                        {
                            DataPack INPUT = new DataPack();
                            DateTime Nowdate = DateTime.Today;

                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);														//BODY_CONTEXT
                            //INPUT.add("@APVTYPE", "A");																//결재상태
                            INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);					//사원ID
                            INPUT.add("@App_Date", oRoot.SelectSingleNode("INITIATOR_DATE_INFO").InnerText.Substring(0, 10));   //기안일
                            INPUT.add("@End_Date", Nowdate.ToString().Substring(0, 10));								//기안종료일
                            INPUT.add("@fiid", fiid);																	//Form_inst_id
                            INPUT.add("@UNAME", oRoot.SelectSingleNode("INITIATOR_DP").InnerText);						//사원이름
                            INPUT.add("@VacYear", oRoot.SelectSingleNode("Sel_Year").InnerText);						//Year
                            //INPUT.add("@TOTAL_DAYS", oRoot.SelectSingleNode("_MULTI_TOTAL_DAYS").InnerText);          //PFMP

                            // INPUT.add("@VACTYPE", oRoot.SelectSingleNode("VACATION_TYPE").InnerText);                   //FLAG
                            INPUT.add("@VAC_REASON", oRoot.SelectSingleNode("VAC_REASON").InnerText);                   //FLAG
                            INPUT.add("@piid", piid);																	// [2015-02-05 add] 
                            INPUT.add("@GUBUN", "신청");																	// [2015-02-05 add] 신청, 취소 구분
                            INPUT.add("@DEPUTY_NAME", oRoot.SelectSingleNode("DEPUTY_NAME").InnerText);                  // [2016-02-23 kimjh add] 직무대행자
                            SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.VM_Info_U", INPUT);
                        }
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 휴가취소신청서
    private void execWF_FORM_VACATIONCANCEL(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;



        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    if (Covi.Framework.ConfigurationManager.GetBaseConfig("VACATION_INERLOCK", "0") == "Y")
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            DataPack INPUT = new DataPack();
                            XmlNodeList oNodeList = oRoot.SelectNodes("tblVacInfo");

                            foreach (XmlNode _node in oNodeList)
                            {
                                _node.SelectSingleNode("_MULTI_DAYS").InnerText = "-" + _node.SelectSingleNode("_MULTI_DAYS").InnerText;
                            }

                            DateTime Nowdate = DateTime.Today;
                            //oRoot.SelectSingleNode("_MULTI_DAYS").InnerText = "-" + oRoot.SelectSingleNode("_MULTI_DAYS").InnerText;
                            INPUT.add("@BODYCONTEXT", oBody.OuterXml.ToString());														//BODY_CONTEXT
                            //INPUT.add("@APVTYPE", "A");																//결재상태
                            INPUT.add("@UID", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);					//사원ID
                            INPUT.add("@App_Date", oRoot.SelectSingleNode("INITIATOR_DATE_INFO").InnerText.Substring(0, 10));   //기안일
                            INPUT.add("@End_Date", Nowdate.ToString().Substring(0, 10));								//기안종료일
                            INPUT.add("@fiid", fiid);																	//Form_inst_id
                            INPUT.add("@UNAME", oRoot.SelectSingleNode("INITIATOR_DP").InnerText);						//사원이름
                            INPUT.add("@VacYear", oRoot.SelectSingleNode("Sel_Year").InnerText);						//Year
                            //INPUT.add("@TOTAL_DAYS", oRoot.SelectSingleNode("_MULTI_TOTAL_DAYS").InnerText);          //PFMP

                            // INPUT.add("@VACTYPE", oRoot.SelectSingleNode("VACATION_TYPE").InnerText);                   //FLAG
                            INPUT.add("@VAC_REASON", oRoot.SelectSingleNode("VAC_REASON").InnerText);                   //FLAG
                            INPUT.add("@piid", piid);																	// [2015-02-05 add] 
                            INPUT.add("@GUBUN", "취소");																	// [2015-02-05 add] 신청, 취소 구분
                            SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.VM_Info_U", INPUT);
                        }

                        // [2017-01-11] gbhwang 휴가 취소 여부 업데이트를 위한 프로시저 호출
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            DataPack INPUT = new DataPack();

                            INPUT.add("@RequestFIID", oRoot.SelectSingleNode("HID_REQUEST_FIID").InnerText);            //휴가신청서의 FIID
                            INPUT.add("@CancelFIID", fiid);                                                             //휴가취소신청서의 FIID

                            SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.VM_CancelStatus_U", INPUT);
                        }
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 인사평가표
    private void execPersonnelEvaluation(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        try
        {
            XmlDocument oBody = new XmlDocument();
            oBody.LoadXml(BODYCONTEXT);

            XmlNode oRoot = oBody.DocumentElement;

            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        DataPack INPUT = new DataPack();
                        INPUT.add("@BODY_CONTEXT", BODYCONTEXT);
                        INPUT.add("@UR_CODE", ApproverId.Replace("@", ""));    //결재자
                        INPUT.add("@PROCESS_ID", piid);    //PROCESS ID
                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.PE_AssessResult_P", INPUT);
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 라이센스
    private void execWF_FORM_DRAFT_License(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "COMPLETE": // 최종결재
                    DataSet ds = new DataSet();
                    string SerialNumber = string.Empty;
                    string year = DateTime.Now.Year.ToString();
                    string month = DateTime.Now.Month.ToString();
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
                    {
                        DataPack INPUT = new DataPack();
                        //BODY_CONTEXT
                        INPUT.add("@UNIT_CODE", "CO-LI" + year.Substring(2, 2) + month);     //인증번호
                        INPUT.add("@FISCAL_YEAR", year);   //년 뒤에2자리 (2014->14)
                        INPUT.add("@DOC_LIST_TYPE", "1");   //증가식
                        INPUT.add("@UNIT_ABBR", "");   //
                        INPUT.add("@CATEGORY_NUMBER", "");   //
                        ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wfform_RegisterDocumentNumber_Software", INPUT);//프로시저실행
                    }
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        SerialNumber = ds.Tables[0].Rows[0]["SERIAL_NUMBER"].ToString();
                    }
                    else if (ds.Tables[0].Rows.Count <= 0)
                    {
                        throw new System.Exception("라이센스 번호가 발번되지 않았습니다.");
                    }

                    XmlNode lisence = oBody.DocumentElement.SelectSingleNode("SerialNumber");
                    lisence.InnerText = "CO-LI-" + year.Substring(2, 2) + month + "-" + SerialNumber;

                    using (SqlDacManager sm = new SqlDacManager("FORM_INST_ConnectionString"))
                    {
                        DataPack INPUT = new DataPack();
                        INPUT.add("@FIID", fiid);
                        INPUT.add("@BODYCONTEXT", oBody.OuterXml);
                        sm.ExecuteNonQuery(CommandType.StoredProcedure, "[dbo].[usp_form_UpdateBodyContext]", INPUT);
                    }

                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 경비결재 시스템

    /// <summary>
    /// 경비결재 시스템 연동 처리
    /// </summary>
    /// <param name="fmpf">Form Prefix</param>
    /// <param name="BODYCONTEXT">결재 본문값</param>
    /// <param name="FORM_INFO_EXT">프로세스 옵션 정보값</param>
    /// <param name="APPROVERCONTEXT">결재선</param>
    /// <param name="PreApproveProcess">기안결재 여부 - true: 기안상태 / false: 완료상태</param>
    /// <param name="ApvResult">결재 결과 - rejected: 반려 / 그외: 승인</param>
    /// <param name="DocNumber">결재 문서번호</param>
    /// <param name="ApproverId">결재자 ID</param>
    /// <param name="fiid">결재 문서 Instance ID</param>
    /// <param name="apvMode">결재 상태</param>
    /// <param name="piid">프로세스 ID</param>
    private void EXECWF_EXPENSE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        string result = string.Empty;
        ///* 경비결재용 - 운영배포 금지
        InterfaceProxy interfaceProxy = new InterfaceProxy();
        result = interfaceProxy.CallInterfaceProcess(
            fmpf
            , BODYCONTEXT
            , FORM_INFO_EXT
            , APPROVERCONTEXT
            , PreApproveProcess
            , ApvResult
            , DocNumber
            , ApproverId
            , fiid
            , apvMode
            , piid);

        if (result.StartsWith("ERROR"))
        {
            throw new Exception(result);
        }
        // */
    }

    #endregion

    #region 예산신청서

    /// <summary>
    /// 예산신청서
    /// </summary>
    /// <param name="fmpf">양식 접두어</param>
    /// <param name="BODYCONTEXT">본문데이터</param>
    /// <param name="FORM_INFO_EXT">본문 확장 정보</param>
    /// <param name="APPROVERCONTEXT">결재선</param>
    /// <param name="PreApproveProcess">기안 유무</param>
    /// <param name="ApvResult">결재 결과</param>
    /// <param name="DocNumber">문서번호</param>
    /// <param name="ApproverId">결재사번</param>
    /// <param name="fiid">양식 관리 번호</param>
    /// <param name="apvMode">진행 상태</param>
    /// <param name="piid">프로세스 아이디</param>
    private void execWF_BUDGET_REQUEST(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    //운영배포 금지
                    BudgetService bs = new BudgetService();
                    string rtVal = bs.InsertBudgetData(oRoot.SelectSingleNode("//txtxmlData").InnerText);

                    if (rtVal != "OK")
                    {
                        throw new Exception("[ERROR] : " + rtVal);
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 예산변경신청서

    /// <summary>
    /// 예산변경신청서
    /// </summary>
    /// <param name="fmpf">양식 접두어</param>
    /// <param name="BODYCONTEXT">본문데이터</param>
    /// <param name="FORM_INFO_EXT">본문 확장 정보</param>
    /// <param name="APPROVERCONTEXT">결재선</param>
    /// <param name="PreApproveProcess">기안 유무</param>
    /// <param name="ApvResult">결재 결과</param>
    /// <param name="DocNumber">문서번호</param>
    /// <param name="ApproverId">결재사번</param>
    /// <param name="fiid">양식 관리 번호</param>
    /// <param name="apvMode">진행 상태</param>
    /// <param name="piid">프로세스 아이디</param>
    private void execWF_BUDGET_SHIFT_REQUEST(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    //운영배포 금지
                    BudgetService bs = new BudgetService();
                    string rtVal = bs.UpdateBudgetData(oRoot.SelectSingleNode("//txtxmlData").InnerText);

                    if (rtVal != "OK")
                    {
                        throw new Exception("[ERROR] : " + rtVal);
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 프로젝트경비신청서
    private void execWF_FORM_PROJECTEXPENSE_007(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        DataPack INPUT = new DataPack();

                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);														            //BODY_CONTEXT
                        //INPUT.add("@fiid", fiid);										                                        //FORM_INSTANCE_ID
                        INPUT.add("@piid", piid);																	            //프로세스 ID
                        INPUT.add("@pProject_code", oRoot.SelectSingleNode("PROJECT_NAME").InnerText);					        //프로젝트 코드
                        INPUT.add("@pExpense_user_name", oRoot.SelectSingleNode("INITIATOR_DP").InnerText);						//지출 사용자
                        INPUT.add("@pExpense_user_id", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);				    //지출 사용자 ID
                        //INPUT.add("@pRequest_YM", oRoot.SelectSingleNode("REQUEST_YM").InnerText);					            //청구년월
                        INPUT.add("@pRequest_Y", Convert.ToInt32(oRoot.SelectSingleNode("REQUEST_Y").InnerText.ToString()));                                //청구년도
                        INPUT.add("@pRequest_M", Convert.ToInt32(oRoot.SelectSingleNode("REQUEST_M").InnerText.ToString()));
                        INPUT.add("@pRegist_date", oRoot.SelectSingleNode("INITIATOR_DATE_INFO").InnerText);   //등록날짜
                        INPUT.add("@pRegist_ID", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);					    //등록자 ID

                        SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PMO_PROJECT_EXPENSE_insert", INPUT);
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 프로젝트코드신청서
    private void execWF_FORM_PROJECTCODE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        String[] splitManagerName = oRoot.SelectSingleNode("PMName").InnerText.Split(' ');
        String[] splitCEOName = oRoot.SelectSingleNode("CEO").InnerText.Split(' ');


        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        DataPack INPUT = new DataPack();
                        //DateTime Nowdate = DateTime.Today;

                        //INPUT.add("@BODYCONTEXT", BODYCONTEXT);														//BODY_CONTEXT
                        //INPUT.add("@FIID", fiid);																	//Form_inst_id
                        INPUT.add("@pPROCESS_ID", piid);                                                            //PROCESS ID
                        INPUT.add("@pREGIST_DATE", oRoot.SelectSingleNode("INITIATOR_DATE_INFO").InnerText);   //기안일자
                        INPUT.add("@pREGIST_ID", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);					//기안자 ID
                        //INPUT.add("@pPROJECT_CODE", oRoot.SelectSingleNode("").InnerText);						//프로젝트 코드
                        INPUT.add("@pPROJECT_NAME", oRoot.SelectSingleNode("projectName").InnerText);				    //프로젝트명
                        INPUT.add("@pPROJECT_MANAGER_ID", oRoot.SelectSingleNode("PM_Code").InnerText);                     //PM ID
                        INPUT.add("@pPROJECT_MANAGER_NAME", splitManagerName[1]);                                           //PM 이름
                        INPUT.add("@pPROJECT_CEO_NAME", splitCEOName[1]);                                           //CEO 이름
                        INPUT.add("@pPROJECT_STARTDATE", oRoot.SelectSingleNode("DAY_START").InnerText);                    //예상 프로젝트 시작일
                        INPUT.add("@pPROJECT_ENDDATE", oRoot.SelectSingleNode("DAY_FINISH").InnerText);                     //예상 프로젝트 종료일
                        SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PMO_PROJECT_MASTER_insert", INPUT);
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 발주 품의
    private void execWF_FORM_DRAFT_ORDER(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        //string[] splitOrderTotal = oRoot.SelectSingleNode("orderTotal").InnerText.Split('\\');

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    if (oRoot.SelectSingleNode("PROJECT_NAME").InnerText != "input_self")
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            DataPack INPUT = new DataPack();
                            //DateTime Nowdate = DateTime.Today;

                            //INPUT.add("@BODYCONTEXT", BODYCONTEXT);														//BODY_CONTEXT
                            INPUT.add("@FIID", fiid);																	//Form_inst_id
                            INPUT.add("@pPROCESS_ID", piid);                                                            //PROCESS ID
                            INPUT.add("@pREGIST_DATE", oRoot.SelectSingleNode("INITIATOR_DATE_INFO").InnerText);   //기안일자
                            INPUT.add("@pREGIST_ID", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);					//기안자 ID
                            INPUT.add("@pPROJECT_CODE", oRoot.SelectSingleNode("PROJECT_NAME").InnerText);						//프로젝트 코드
                            //INPUT.add("@pPROJECT_NAME", oRoot.SelectSingleNode("PROJECT_NAME_TEXT").InnerText);				    //프로젝트명
                            INPUT.add("@pBUSINESS_NAME", oRoot.SelectSingleNode("businessName").InnerText);				    //업체명
                            //INPUT.add("@pFINAL_AMOUNT", Int32.Parse(oRoot.SelectSingleNode("orderTotal").InnerText));	    //최종발주금액
                            INPUT.add("@pFINAL_AMOUNT", oRoot.SelectSingleNode("orderTotal").InnerText);	    //최종발주금액
                            if (oRoot.SelectSingleNode("RDO_condition").InnerText == "2")
                            {
                                INPUT.add("@pPAY_CONDITION", oRoot.SelectSingleNode("etc").InnerText);	        //지불조건(기타)
                            }
                            else
                            {
                                INPUT.add("@pPAY_CONDITION", oRoot.SelectSingleNode("RDO_condition_TEXT").InnerText);	        //지불조건
                            }

                            SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PMO_PROJECT_ORDER_insert", INPUT);
                        }
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 프로젝트 집행계획서
    private void execWF_FORM_PROJECT_EXECPLAN(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    if (oRoot.SelectSingleNode("PROJECT_NAME").InnerText != "input_self")
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            DataPack INPUT = new DataPack();
                            //DateTime Nowdate = DateTime.Today;

                            INPUT.add("@FIID", fiid);																	//Form_inst_id
                            INPUT.add("@pPROCESS_ID", piid);
                            INPUT.add("@pPROJECT_CODE", oRoot.SelectSingleNode("PROJECT_NAME").InnerText);						//프로젝트 코드
                            INPUT.add("@pPROJECT_NAME", oRoot.SelectSingleNode("PROJECT_NAME_TEXT").InnerText);				    //프로젝트명

                            SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PMO_PROJECT_CONTRACT_insert", INPUT);
                        }
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 프로젝트수당청구서
    private void execWF_COVI_03(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        DataPack INPUT = new DataPack();

                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);														            //BODY_CONTEXT
                        //INPUT.add("@fiid", fiid);										                                        //FORM_INSTANCE_ID
                        INPUT.add("@piid", piid);																	            //프로세스 ID
                        //INPUT.add("@pProject_code", oRoot.SelectSingleNode("PRO_NAME").InnerText);					        //프로젝트 코드
                        //INPUT.add("@pProject_name", oRoot.SelectSingleNode("PRO_NAME_TEXT").InnerText);					    //프로젝트명
                        INPUT.add("@pExpense_user_name", oRoot.SelectSingleNode("INITIATOR_DP").InnerText);						//지출 사용자
                        INPUT.add("@pExpense_user_id", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);				    //지출 사용자 ID
                        //INPUT.add("@pRequest_YM", oRoot.SelectSingleNode("T_DATE").InnerText);					                //청구년월
                        INPUT.add("@pRequest_Y", Convert.ToInt32(oRoot.SelectSingleNode("REQUEST_Y").InnerText));                                //청구년도
                        INPUT.add("@pRequest_M", Convert.ToInt32(oRoot.SelectSingleNode("REQUEST_M").InnerText));                                //청구월
                        INPUT.add("@pRegist_date", oRoot.SelectSingleNode("INITIATOR_DATE_INFO").InnerText);   //등록날짜
                        INPUT.add("@pRegist_ID", oRoot.SelectSingleNode("INITIATOR_CODE_DP").InnerText);					    //등록자 ID
                        INPUT.add("@pCost", oRoot.SelectSingleNode("COST").InnerText);					                        //프로젝트 수당

                        SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_PMO_PROJECT_EXPENSE_insert_pay", INPUT);
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 외부메일 요청서
    private void execWF_FORM_EX_MAIL(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        string rslt;
        XmlDocument oResult = new XmlDocument();

        ExternalWebService EWS = new ExternalWebService();
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string mkey = string.Empty;
        mkey = oRoot.SelectSingleNode("//BODY_CONTEXT/mKey").InnerText;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    rslt = EWS.SetExtMailApvComplete(mkey, "C");
                    oResult.LoadXml(rslt);
                    break;
                case "REJECT": //반려
                    rslt = EWS.SetExtMailApvComplete(mkey, "R");
                    oResult.LoadXml(rslt);
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 회계전표 테스트
    private void execWF_FORM_FI_TEST(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        ExternalWebService EWS = new ExternalWebService();
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@EMP_NO", ApproverId);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_LEGACY_STATE_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 매입결의전표
    private void execWF_FORM_LEGACY_FI_STATE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");


        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);


                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_LEGACY_STATE_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion


    #region 투자공사 결과보고서
    private void execWF_FORM_LEGACY_INVEST(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        string seq = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;
        seq = oRoot.SelectSingleNode("//BODY_CONTEXT/seq").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        INPUT.add("@SEQ", seq);


                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_LEGACY_INVEST_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 투자공사 완료보고서
    private void execWF_FORM_LEGACY_INVEST_COM(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        string seq = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;
        seq = oRoot.SelectSingleNode("//BODY_CONTEXT/seq").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        // INPUT.add("@SEQ", seq);


                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_LEGACY_INVEST_COM_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 투자품의(자본적지출)
    private void execWF_FORM_LEGACY_INVEST_CE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        string benefit = string.Empty;
        string risk = string.Empty;
        string inv_desc = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;
        benefit = oRoot.SelectSingleNode("//BODY_CONTEXT/BENEFIT").InnerText;
        risk = oRoot.SelectSingleNode("//BODY_CONTEXT/RISK").InnerText;
        inv_desc = oRoot.SelectSingleNode("//BODY_CONTEXT/INV_DESC").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@CTRL_NO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@BENEFIT", benefit);
                        INPUT.add("@RISK", risk);
                        INPUT.add("@INV_DESC", inv_desc);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_LEGACY_INVEST_CE_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region ID신규신청
    private void execWF_FORM_ID_NEW(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;


        DataPack INPUT = new DataPack();

        string g_emp_no = string.Empty; // 통합사번
        string user_id = string.Empty;  // 신청ID
        string remark = string.Empty;   // 신청사유



        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안                
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재   
                    try
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);

                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_ID_NEW", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }


        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 재직자 ID 신청
    private void execWF_FORM_ID_CHANGE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;


        DataPack INPUT = new DataPack();

        string g_emp_no = string.Empty; // 통합사번
        string user_id = string.Empty;  // 신청ID
        string remark = string.Empty;   // 신청사유



        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안                
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재   
                    try
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);
                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_ID_CHANGE", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }


        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 법인인감날인 품의서
    private void execWF_FORM_CORPORATE_SENSE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;


        DataPack INPUT = new DataPack();

        string initiatorName = string.Empty; // 기안자
        string initiatorCode = string.Empty; // 기안자사번

        string draftDate = string.Empty;    // 신청일자
        string companyNM = string.Empty;      // 법인명
        string companyCD = string.Empty;        // 법인코드
        string deptCD = string.Empty;   // 신청부서코드
        string deptNM = string.Empty;   // 신청부서명
        string submi = string.Empty;    // 제출처
        string docInfo = string.Empty;  // 날인문서
        string docReason = string.Empty;    // 날인사유
        string docCount = string.Empty; // 날인회수
        string docDate = string.Empty;  // 날인일자
        string bigo = string.Empty; // 비고




        string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        initiatorName = oRoot.SelectSingleNode("//BODY_CONTEXT/INITIATOR_DP").InnerText;
        initiatorCode = oRoot.SelectSingleNode("//BODY_CONTEXT/USER_ID").InnerText;
        draftDate = oRoot.SelectSingleNode("//BODY_CONTEXT/DATE").InnerText;
        companyNM = oRoot.SelectSingleNode("//BODY_CONTEXT/COR_TEXT").InnerText;
        companyCD = oRoot.SelectSingleNode("//BODY_CONTEXT/COR").InnerText;
        deptNM = oRoot.SelectSingleNode("//BODY_CONTEXT/DEPT").InnerText;
        deptCD = oRoot.SelectSingleNode("//BODY_CONTEXT/DEPT").InnerText;
        submi = oRoot.SelectSingleNode("//BODY_CONTEXT/SUBMISSION").InnerText;
        docInfo = oRoot.SelectSingleNode("//BODY_CONTEXT/DOCINFO").InnerText;
        docReason = oRoot.SelectSingleNode("//BODY_CONTEXT/DOCINFO_REASON").InnerText;
        docCount = oRoot.SelectSingleNode("//BODY_CONTEXT/CHECK_COUNT").InnerText;
        docDate = date;
        bigo = oRoot.SelectSingleNode("//BODY_CONTEXT/DOCINFO_BIGO").InnerText;





        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안                
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재   
                    try
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {

                            INPUT.add("@FIID", fiid);
                            INPUT.add("@PIID", piid);
                            INPUT.add("@INITIATOR_CODE", initiatorCode);
                            INPUT.add("@INITIATOR_NAME", initiatorName);
                            INPUT.add("@DRAFT_DATE", draftDate);
                            INPUT.add("@COMPANY_CODE", companyCD);
                            INPUT.add("@COMPANY_NAME", companyNM);
                            INPUT.add("@DEPT_CODE", deptCD);
                            INPUT.add("@DEPT_NAME", deptNM);
                            INPUT.add("@SUBMISSION", submi);
                            INPUT.add("@DOCINFO", docInfo);
                            INPUT.add("@DOC_REASON", docReason);
                            INPUT.add("@DOC_COUNT", docCount);
                            INPUT.add("@DOC_DATE", docDate);
                            INPUT.add("@DOC_BIGO", bigo);
                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.TB_CORPORATE_SENSE_UPDATE", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }


        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 휴일근무 계획서, 확인서
    private void execWF_FORM_HOLIDAY_CHECK_PLAN(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();
        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string usid = string.Empty;
        usid = oRoot.SelectSingleNode("//BODY_CONTEXT/U_ID").InnerText;

        //2019-07-23 ygkim 최종결재자 정보 확인
        string strLastApprover = "";
        strLastApprover = LastManager(APPROVERCONTEXT);
        //

        string type = string.Empty;

        DataPack INPUT = new DataPack();

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안                
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재   
                    try
                    {
                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            if (fmpf == "WF_FORM_HOLIDAY_CHECK")
                            {   // 확인서면 C
                                type = "C";
                            }
                            else
                            {    // 계획서면 P
                                type = "P";
                            }

                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);
                            INPUT.add("@TYPE", type);
                            INPUT.add("@APP_USER_ID", usid);
                            INPUT.add("@LAST_APPROVER_ID", strLastApprover);
                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_HOLIDAY_CHECK_PLAN", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }


        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion



#region 전산개발 변경적용 의뢰서
    private void execWF_FORM_SPI_SLM00001(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        string seq = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;
        seq = oRoot.SelectSingleNode("//BODY_CONTEXT/seq").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        // INPUT.add("@SEQ", seq);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_SPI_SLM00001_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion






 #region SR요청서 : 형상관리 연동
    private void execWF_FORM_SR(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {             	
        DataPack INPUT = new DataPack();
        XmlDocument oApv = new XmlDocument();
        oApv.LoadXml(APPROVERCONTEXT);

        try
        {
            XmlDocument oBody = new XmlDocument();  //0526추가
            oBody.LoadXml(BODYCONTEXT);   //0526추가
            XmlNode oRoot_Body = oBody.DocumentElement; //0526추가

            //기안자 이름
            string usid = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/U_ID").InnerText.Replace(";", "");  //0527 기안자이름추가22
            //기안자 메일
            string usem = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/USEM").InnerText;  //0527 기안자메일추가22
            //기안자 부서코드
            string dpid = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/DPID").InnerText;  //0527 기안자부서 코드22
            //기안자 부서명
            string dpdn = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/DPDN").InnerText.Replace(";", "");  //0527 기안자부서명22
            //업무명
            string taskname = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/TASK_NAME").InnerText;  //업무명 0526추가
            //완료예정일          
            string compredate = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/COMPLETE_REQUEST_DATE").InnerText;  //요청일 0526추가
            //제목
            string sub = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/SUB").InnerText;  //제목 0602추가
            //최종결재여부
            string appyn = string.Empty;

            XmlNode oRoot_Apv = oApv.DocumentElement;
            XmlNodeList apvNodes = oRoot_Apv.SelectNodes("//steps/division/step");

            string pEmpId = string.Empty, startDate = string.Empty, lastUsid = string.Empty, finishDate = string.Empty;

	    // [2020-09-14 Covision 우영호] SR요청서 로그 추가
            WriteMessage_log4("SR요청서", "결재모드 : " + apvMode + ", 제목 : " + sub + " , USEM : " + usem + ", 전송시간 : " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"));


            //LEGACY PROCESS 처리
            switch (apvMode.ToString())
            {
                case "DRAFT": //기안     
                    //기안자
                    pEmpId = apvNodes[0].SelectSingleNode("ou/person").Attributes["code"].Value;
                    //기안시간
                    startDate = apvNodes[0].SelectSingleNode("ou/person/taskinfo").Attributes["datecompleted"].Value;
                    //최종결재여부
                    appyn = "N";
                    IF_ServiceRequest(fiid, DocNumber, piid, usid, usem, dpid, dpdn, taskname, compredate, sub, appyn, pEmpId, startDate, lastUsid, finishDate);

                    break;

                case "REDRAFT"://재기안
   
                    //2020 0717 재기안부분추가
				    //기안자
                    pEmpId = apvNodes[0].SelectSingleNode("ou/person").Attributes["code"].Value;
                    //기안시간
                    startDate = apvNodes[0].SelectSingleNode("ou/person/taskinfo").Attributes["datecompleted"].Value;
                    //최종결재여부
                    appyn = "N";
                    IF_ServiceRequest(fiid, DocNumber, piid, usid, usem, dpid, dpdn, taskname, compredate, sub, appyn, pEmpId, startDate, lastUsid, finishDate);

                    break;



                case "COMPLETE": // 최종결재                    
                    //최종결재여부
                    appyn = "Y";
                    //최종결재자
                    lastUsid = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person").Attributes["code"].Value;
                    //결재완료시간
                    finishDate = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person/taskinfo").Attributes["datecompleted"].Value;
                    IF_ServiceRequest(fiid, DocNumber, piid, usid, usem, dpid, dpdn, taskname, compredate, sub, appyn, pEmpId, startDate, lastUsid, finishDate);

                    break;

                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    

    private void IF_ServiceRequest(string fiid, string DocNumber, string piid, string usid, string usem, string dpid, string dpdn, string taskname, string compredate, string sub, string appyn, string pEmpId, string startDate, string lastUsid, string finishDate)
    {
        DataPack INPUT = new DataPack();
        try
        {
            DateTime Nowdate = DateTime.Today;

            string xmlValue = "<INTERFACE></INTERFACE>";
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            string linkid = string.Empty;

            if (appyn == "Y")
            {
               // linkid = "http://bluesam.sampyo.co.kr/WebSite/Approval/Forms/Form.aspx?mode=COMPLETE&piid=" + piid;
            }
            string tmpsearch = "?FI_ID=" + fiid +"&SR_DOC_NO=" + DocNumber + "&TITLE=" + sub + "&REQ_CMP_DATE=" + compredate + "&CONTENTS=" + piid + "&REQ_USER_ID=" + usem + "&REQ_USER_NAME=" + usid + "&REQ_GROUP_ID=" + dpid + "&REQ_GROUP_NAME=" + dpdn + "&REQ_DATE=" + startDate + "&DEV_USER_ID=" + lastUsid + "&APP_YN=" + appyn;

            HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create("http://10.50.11.11:8080/jsp/doc/sr/interface/Add_Sr_Request_Info.jsp" + tmpsearch);

            byte[] postBytes = encoding.GetBytes(xmlValue);
            httprequest.ContentLength = postBytes.Length;
            httprequest.Method = "POST";

            System.IO.Stream postStream = httprequest.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Close();
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            INPUT.Dispose();
        }
    }

    #endregion


    #region 휴가원(사전승인)
    private void execWF_FORM_VACATION_BEFORE_APP(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        string pEmpId = string.Empty;       //기안자 ID
        string startDate = string.Empty;    //기안시간
        string lastUsid = string.Empty;     //최종 결재권자 ID
        string finishDate = string.Empty;   //최종 결재시간
        string corpCd = string.Empty;       //법인
        string empNo = string.Empty;       //사번
        string gEmpNo = string.Empty;     //통합사번

        DataPack INPUT = new DataPack();

        XmlDocument oApv = new XmlDocument();
        oApv.LoadXml(APPROVERCONTEXT);

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);
        XmlNode oRoot = oBody.DocumentElement;

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode.ToString())
            {
                case "DRAFT": //기안  
                    break;

                case "REDRAFT"://재기안   
                    break;

                case "COMPLETE": // 최종결재    
                    try
                    {
                        XmlNode oRoot_Apv = oApv.DocumentElement;
                        XmlNodeList apvNodes = oRoot_Apv.SelectNodes("//steps/division/step");

                        //기안자
                        pEmpId = apvNodes[0].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //기안시간
                        startDate = apvNodes[0].SelectSingleNode("//steps/division/step/ou/person/taskinfo").Attributes["datecompleted"].Value;
                        //최종결재자
                        lastUsid = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //결재완료시간
                        finishDate = apvNodes[apvNodes.Count - 1].SelectSingleNode("//steps/division/step/ou/person/taskinfo").Attributes["datecompleted"].Value;
                        //법인
                        corpCd = oRoot.SelectSingleNode("//BODY_CONTEXT/CORP_CD").InnerText;
                        //사번
                        empNo = oRoot.SelectSingleNode("//BODY_CONTEXT/EMP_NO").InnerText;
                        //통합사번
                        gEmpNo = oRoot.SelectSingleNode("//BODY_CONTEXT/G_EMP_NO").InnerText;

                        DateTime Nowdate = DateTime.Today;

                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            INPUT.add("@FirstUsid", pEmpId);
                            INPUT.add("@StartDate", startDate);
                            INPUT.add("@LastUsid", lastUsid);
                            INPUT.add("@FinishDate", finishDate);
                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);
                            INPUT.add("@CorpCd", corpCd);
                            INPUT.add("@EmpNo", empNo);
                            INPUT.add("@GEmpNo", gEmpNo);

                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_VACATION_BEFORE_APP_INSERT", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;

                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;

                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion


    #region 구매품의승인용
    private void execWF_FORM_M_PUR_OFFICER_APP(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        string poNo = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;

        string strLastApprover = "";
        strLastApprover = LastManager(APPROVERCONTEXT);

        poNo = lkey.Split('_')[1];

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {

                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@OFF_REMARK", DocNumber);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", strLastApprover);
                        //INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        INPUT.add("@PO_NO", poNo);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_M_PUR_OFFICER_APP_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion

    #region 구매품의승인(에스피네이처)
    private void execWF_FORM_M_PUR_OFFICER_APP_NDW(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        string poNo = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;

        string strLastApprover = "";
        strLastApprover = LastManager(APPROVERCONTEXT);

        poNo = lkey.Split('_')[1];

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "A";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {

                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@OFF_REMARK", DocNumber);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", strLastApprover);
                        //INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        INPUT.add("@PO_NO", poNo);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_M_PUR_OFFICER_APP_UPDATE_NDW", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion


    #region 외주공사 품의 등록
    private void execWF_FORM_LEGACY_OUT_CONSTRUCT(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)

    {

        XmlDocument oResult = new XmlDocument();
        XmlDocument oBody = new XmlDocument();

        oBody.LoadXml(BODYCONTEXT);
        XmlNode oRoot = oBody.DocumentElement;

        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        string workNo = string.Empty;
        string strLastApprover = "";
        strLastApprover = LastManager(APPROVERCONTEXT);


        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;

        workNo = lkey.Split('_')[1];
        DataPack INPUT = new DataPack();

        int iReturn;
        string state = string.Empty;

        try
        {
            //LEGACY PROCESS 처리

            switch (apvMode)
            {
                case "DRAFT": //기안  
                    state = "D";

                    break;

                case "REDRAFT"://재기안 
                    break;

                case "COMPLETE": // 최종결재 
                    state = "C";
                    break;

                case "REJECT": //반려
                    state = "R";
                    break;

                case "WITHDRAW": // 회수
                    state = "B";
                    break;

                case "APPROVAL": // 결재
                    break;

                case "DELETE": // 삭제
                    break;

                default:
                    break;

            }



            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))

                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", strLastApprover);
                        //INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        INPUT.add("@WORK_NO", workNo);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_LEGACY_OUT_CONSTRUCT_UPDATE", INPUT);


                    }
                }
            }

            catch (System.Exception ex)
            {
                throw ex;
            }

            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }

    #endregion

    #region 공사작업일보
    private void execWF_FORM_SPRAIL_CONSTRUCTION_RE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        string pjrNo = string.Empty;
        string workDt = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;

        pjrNo = lkey.Split('_')[1];
        workDt = lkey.Split('_')[2];

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        INPUT.add("@PJT_NO", pjrNo);
                        INPUT.add("@WORK_DT", workDt);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_SPRAIL_CONSTRUCTION_REPORT_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion


    #region 분기기견적서

    private void execWF_FORM_SPRAIL_ESTIMATE(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)

    {

        XmlDocument oResult = new XmlDocument();



        XmlDocument oBody = new XmlDocument();

        oBody.LoadXml(BODYCONTEXT);



        XmlNode oRoot = oBody.DocumentElement;

        string lkey = string.Empty;

        string ip = string.Empty;

        string db = string.Empty;

        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");



        string estNo = string.Empty;

        string verCd = string.Empty;



        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;

        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;

        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;



        estNo = lkey.Split('_')[1];

        verCd = lkey.Split('_')[2];



        DataPack INPUT = new DataPack();

        int iReturn;

        string state = string.Empty;

        try

        {

            //LEGACY PROCESS 처리

            switch (apvMode)

            {

                case "DRAFT": //기안      

                    state = "D";

                    break;

                case "REDRAFT"://재기안   

                    break;

                case "COMPLETE": // 최종결재                   

                    state = "C";

                    break;

                case "REJECT": //반려

                    state = "R";

                    break;

                case "WITHDRAW": // 회수

                    state = "B";

                    break;

                case "APPROVAL": // 결재

                    break;

                case "DELETE": // 삭제

                    break;

                default:

                    break;

            }



            try

            {

                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")

                {

                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))

                    {

                        INPUT.add("@GWNO", lkey);

                        INPUT.add("@STATE", state);

                        INPUT.add("@DOC_NO", DocNumber);

                        INPUT.add("@FORM_INST_ID", fiid);

                        INPUT.add("@PROCESS_ID", piid);

                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));

                        INPUT.add("@DATE", date);

                        INPUT.add("@IP", ip);

                        INPUT.add("@DB", db);

                        INPUT.add("@ESTIMATE_NO", estNo);

                        INPUT.add("@VER_CD", verCd);



                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_SPRAIL_ESTIMATE_UPDATE", INPUT);



                    }

                }

            }

            catch (System.Exception ex)

            {

                throw ex;

            }

            finally

            {

                INPUT.Dispose();

            }

        }

        catch (System.Exception ex)

        {

            throw ex;

        }

        finally

        {

        }



    }

    #endregion

    #region 환불품의서
    private void execWF_FORM_SPI_SPIFI0002(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");


        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;


        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GWNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        // INPUT.add("@SEQ", seq);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_SPI_SPIFI0002_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion
    
    #region 예외대체의뢰서
    private void execWF_FORM_S5110MA1_KO455(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");


        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;


        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@REQNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        // INPUT.add("@SEQ", seq);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_S5110MA1_KO455_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion

    #region 프로젝트정산요청서(NEW)
    private void execWF_FORM_NEW_PME203MA1_KO455(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");


        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;


        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@ADJ_NO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_PME203MA1_KO455_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion

    #region 예산편성신청서(erp연동)
    private void execWF_FORM_F2103MA4_KO455(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");


        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;


        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@GF_NO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_F2103MA4_KO455_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion

    #region 여신품의서
    private void execWF_FORM_LOAN(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");


        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@CLNO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_LOAN_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion

    #region 고철대지급요명세서
    private void execWF_FORM_METAL(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안                
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재
                    XmlDocument oApv = new XmlDocument();
                    XmlDocument oBody = new XmlDocument();
                    oBody.LoadXml(BODYCONTEXT);
                    oApv.LoadXml(APPROVERCONTEXT);

                    XmlNode oRoot_Body = oBody.DocumentElement;
                    XmlNode oRoot_Apv = oApv.DocumentElement;

                    string lastUsid = string.Empty;//최종 결재권자 ID
                    string finishDate = string.Empty;//최종 결재시간
                    string selectFactory = string.Empty;//공장구분값
                    string selectDate = string.Empty;//요청일자
                    DataPack INPUT = new DataPack();

                    try
                    {


                        //공장
                        selectFactory = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/FACTORY").InnerText;
                        //요청일
                        selectDate = oRoot_Body.SelectSingleNode("//BODY_CONTEXT/REQUEST_DATE").InnerText;


                        XmlNodeList apvNodes = oRoot_Apv.SelectNodes("division/step");
                        //최종결재자 UR_CODE
                        lastUsid = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //결재완료시간
                        finishDate = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person/taskinfo").Attributes["datecompleted"].Value;

                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            INPUT.add("@LastUsid", lastUsid);
                            INPUT.add("@FinishDate", finishDate);
                            INPUT.add("@SelectFactory", selectFactory);
                            INPUT.add("@SelectDate", selectDate);
                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_METAL_COMPLITE", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;
                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }


        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 유사품목연결요청서
    //private void execWF_FORM_ITEM_MATCH(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    //{
    //    XmlDocument oResult = new XmlDocument();
    //    XmlDocument oBody = new XmlDocument();
    //    XmlDocument oApv = new XmlDocument();
    //    XmlDocument oApvList = new XmlDocument();


    //    oBody.LoadXml(BODYCONTEXT);
    //    oApvList.LoadXml(APPROVERCONTEXT);
    //    XmlNode oRoot = oBody.DocumentElement;
    //    XmlNode oRoot_Apv = oApv.DocumentElement;

    //    DataPack INPUT = new DataPack();

    //    string pEmpId = "";                 //기안자 ID
    //    string startDate = string.Empty;    //기안시간
    //    string lastUsid = string.Empty;     //최종결재권자 ID
    //    string finishDate = string.Empty;   //최종결재시간
    //    //상태값
    //    string state = string.Empty;

    //    try
    //    {
    //        //LEGACY PROCESS 처리
    //        switch (apvMode)
    //        {
    //            case "DRAFT": //기안  
    //                state = "D";
    //                XmlNode oCharge = oApvList.SelectSingleNode("steps/division[@divisiontype='send']/step/ou/person[taskinfo/@kind='charge']");

    //                try
    //                {
    //                    //기안자
    //                    pEmpId = oCharge.Attributes["code"].Value;

    //                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
    //                    {
    //                        INPUT.add("@STATE", state);
    //                        INPUT.add("@FirstUsid", pEmpId);
    //                        INPUT.add("@LastUsid", lastUsid);
    //                        INPUT.add("@FinishDate", finishDate);
    //                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);
    //                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_ITEM_MATCH_UPDATE", INPUT);
    //                    }
    //                }
    //                catch (System.Exception ex)
    //                {
    //                    throw ex;
    //                }
    //                finally
    //                {
    //                    INPUT.Dispose();
    //                }
    //                break;

    //            case "REDRAFT"://재기안   
    //                break;
    //            case "COMPLETE": // 최종결재  
    //                state = "C";
    //                XmlNodeList apvNodes = oRoot_Apv.SelectNodes("division/step");

    //                try
    //                {
    //                    //최종결재자 UR_CODE
    //                    lastUsid = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person").Attributes["code"].Value;
    //                    //결재완료시간
    //                    finishDate = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person/taskinfo").Attributes["datecompleted"].Value;

    //                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
    //                    {
    //                        INPUT.add("@STATE", state);
    //                        INPUT.add("@FirstUsid", pEmpId);
    //                        INPUT.add("@LastUsid", lastUsid);
    //                        INPUT.add("@FinishDate", finishDate);
    //                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);
    //                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_ITEM_MATCH_UPDATE", INPUT);
    //                    }
    //                }
    //                catch (System.Exception ex)
    //                {
    //                    throw ex;
    //                }
    //                finally
    //                {
    //                    INPUT.Dispose();
    //                }
    //                break;
    //            case "REJECT": //반려
    //                state = "R";
    //                try
    //                {
    //                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
    //                    {
    //                        INPUT.add("@STATE", state);
    //                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);
    //                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_ITEM_MATCH_UPDATE", INPUT);
    //                    }
    //                }
    //                catch (System.Exception ex)
    //                {
    //                    throw ex;
    //                }
    //                finally
    //                {
    //                    INPUT.Dispose();
    //                }
    //                break;
    //            case "WITHDRAW": // 회수
    //                state = "B";
    //                try
    //                {
    //                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
    //                    {
    //                        INPUT.add("@STATE", state);
    //                        INPUT.add("@BODYCONTEXT", BODYCONTEXT);
    //                        SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_ITEM_MATCH_UPDATE", INPUT);
    //                    }
    //                }
    //                catch (System.Exception ex)
    //                {
    //                    throw ex;
    //                }
    //                finally
    //                {
    //                    INPUT.Dispose();
    //                }
    //                break;
    //            case "APPROVAL": // 결재
    //                break;
    //            case "DELETE": // 삭제
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //    catch (System.Exception ex)
    //    {
    //        throw ex;
    //    }
    //    finally
    //    {
    //    }
    //}

    #endregion
	
	#region 전산개발의뢰서 : 결재완료 후 연동 테스트
    private void execWF_FORM_DEVELOPMENT_AN(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {

        string pEmpId = string.Empty;       //기안자 ID
        string startDate = string.Empty;    //기안시간
        string lastUsid = string.Empty;     //최종 결재권자 ID
        string finishDate = string.Empty;   //최종 결재시간
        string bunlyu = string.Empty;       //양식분류
        string title = string.Empty;        //양식명

        DataPack INPUT = new DataPack();

        XmlDocument oApv = new XmlDocument();
        oApv.LoadXml(APPROVERCONTEXT);

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode.ToString())
            {
                case "DRAFT": //기안  
                    break;

                case "REDRAFT"://재기안   
                    break;

                case "COMPLETE": // 최종결재    
                    try
                    {
                        XmlNode oRoot_Apv = oApv.DocumentElement;
                        XmlNodeList apvNodes = oRoot_Apv.SelectNodes("//steps/division/step");

                        //기안자
                        pEmpId = apvNodes[0].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //기안시간
                        startDate = apvNodes[0].SelectSingleNode("ou/person/taskinfo").Attributes["datecompleted"].Value;
                        //최종결재자
                        lastUsid = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //결재완료시간
                        finishDate = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person/taskinfo").Attributes["datecompleted"].Value;

                        bunlyu = "신청";
                        title = "전산개발의뢰";

                        DateTime Nowdate = DateTime.Today;

                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            INPUT.add("@FirstUsid", pEmpId);
                            INPUT.add("@StartDate", startDate);
                            INPUT.add("@LastUsid", lastUsid);
                            INPUT.add("@FinishDate", finishDate);
                            INPUT.add("@DocNumber", DocNumber);
                            INPUT.add("@bunlyu", bunlyu);
                            INPUT.add("@title", title);
                            INPUT.add("@FORM_INST_ID", fiid);
                            INPUT.add("@PROCESS_ID", piid);
                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);

                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_KO", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;

                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;

                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    #region 가압류(지급보류)요청
    private void execWF_FORM_SEIZE_C(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string seq = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        string benefit = string.Empty;
        string risk = string.Empty;
        string inv_desc = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        seq = oRoot.SelectSingleNode("//BODY_CONTEXT/seq").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@BP_CD", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@BENEFIT", benefit);
                        INPUT.add("@RISK", risk);
                        INPUT.add("@INV_DESC", inv_desc);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);
                        INPUT.add("@SEQ", seq);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_SEIZE_C_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    #endregion

    private void execWF_FORM_ITEM_MATCH(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {

        string pEmpId = string.Empty;       //기안자 ID
        string startDate = string.Empty;    //기안시간
        string lastUsid = string.Empty;     //최종 결재권자 ID
        string finishDate = string.Empty;   //최종 결재시간

        DataPack INPUT = new DataPack();

        XmlDocument oApv = new XmlDocument();
        oApv.LoadXml(APPROVERCONTEXT);

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode.ToString())
            {
                case "DRAFT": //기안  
                    break;

                case "REDRAFT"://재기안   
                    break;

                case "COMPLETE": // 최종결재    
                    try
                    {
                        XmlNode oRoot_Apv = oApv.DocumentElement;
                        XmlNodeList apvNodes = oRoot_Apv.SelectNodes("//steps/division/step");

                        //기안자
                        pEmpId = apvNodes[0].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //기안시간
                        startDate = apvNodes[0].SelectSingleNode("//steps/division/step/ou/person/taskinfo").Attributes["datecompleted"].Value;
                        //최종결재자
                        lastUsid = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //결재완료시간
                        finishDate = apvNodes[apvNodes.Count - 1].SelectSingleNode("//steps/division/step/ou/person/taskinfo").Attributes["datecompleted"].Value;

                        DateTime Nowdate = DateTime.Today;

                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            INPUT.add("@FirstUsid", pEmpId);
                            INPUT.add("@StartDate", startDate);
                            INPUT.add("@LastUsid", lastUsid);
                            INPUT.add("@FinishDate", finishDate);
                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);
                            
                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_ITEM_MATCH_UPDATE", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
               
                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;

                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }
    
    private void execWF_FORM_GITEM_INSERT(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        string pEmpId = string.Empty;       //기안자 ID
        string startDate = string.Empty;    //기안시간
        string lastUsid = string.Empty;     //최종 결재권자 ID
        string finishDate = string.Empty;   //최종 결재시간

        DataPack INPUT = new DataPack();

        XmlDocument oApv = new XmlDocument();
        oApv.LoadXml(APPROVERCONTEXT);

        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode.ToString())
            {
                case "DRAFT": //기안  
                    break;

                case "REDRAFT"://재기안   
                    break;

                case "COMPLETE": // 최종결재    
                    try
                    {
                        XmlNode oRoot_Apv = oApv.DocumentElement;
                        XmlNodeList apvNodes = oRoot_Apv.SelectNodes("//steps/division/step");

                        //기안자
                        pEmpId = apvNodes[0].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //기안시간
                        startDate = apvNodes[0].SelectSingleNode("//steps/division/step/ou/person/taskinfo").Attributes["datecompleted"].Value;
                        //최종결재자
                        lastUsid = apvNodes[apvNodes.Count - 1].SelectSingleNode("ou/person").Attributes["code"].Value;
                        //결재완료시간
                        finishDate = apvNodes[apvNodes.Count - 1].SelectSingleNode("//steps/division/step/ou/person/taskinfo").Attributes["datecompleted"].Value;

                        DateTime Nowdate = DateTime.Today;

                        using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                        {
                            INPUT.add("@FirstUsid", pEmpId);
                            INPUT.add("@StartDate", startDate);
                            INPUT.add("@LastUsid", lastUsid);
                            INPUT.add("@FinishDate", finishDate);
                            INPUT.add("@BODYCONTEXT", BODYCONTEXT);

                            SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_GITEM_INSERT", INPUT);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;

                    }
                    finally
                    {
                        INPUT.Dispose();
                    }
                    break;

                case "REJECT": //반려
                    break;
                case "WITHDRAW": // 회수
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }

    //환경안전 시정조치요구서
    private void execWF_FORM_EV_CHECK(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;

        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
                if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
                {
                    using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
                    {
                        INPUT.add("@CHECK_NO", lkey);
                        INPUT.add("@STATE", state);
                        INPUT.add("@DOC_NO", DocNumber);
                        INPUT.add("@FORM_INST_ID", fiid);
                        INPUT.add("@PROCESS_ID", piid);
                        INPUT.add("@EMP_NO", ApproverId.Replace("@", ""));
                        INPUT.add("@DATE", date);
                        INPUT.add("@IP", ip);
                        INPUT.add("@DB", db);

                        iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_WF_FORM_EV_CHK_UPDATE", INPUT);

                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }

    public static void WriteMessage_log4(string state, string message)
    {
        string path = @"D:\DevLog\ApvLog";
        DateTime dt = DateTime.Now;

        int year = dt.Year;
        int month = dt.Month;
        int daty = dt.Day;

        //string strMessage = dt.ToLocalTime() + "\r\n" + message + " " + company_cde + " , " + doc_nbr + "   " + st_app + "  ,  " + id_approval + "   " + no_doc + "\r\n";
        string strMessage = "[" + state + "]" + "   -   " + "[" + message + "]\r\n";

        string FilePath = path.ToString() + "\\App_Code_" + dt.ToShortDateString() + ".txt";
        FileStream fs = null;

        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Byte[] info = new UTF8Encoding(true).GetBytes(strMessage);

            fs = File.Open(FilePath, FileMode.Append);
            fs.Write(info, 0, info.Length);

        }
        finally
        {
            if (fs != null) fs.Close();
        }
    }

    private bool getIncludeFormPrefix(string formStr, string gubun, string fmpf)
    {
        string[] arrForms = formStr.Split(gubun.ToCharArray());
        bool check = false;
        for (int i = 0; i < arrForms.Length; i++)
        {
            if (arrForms[i] == fmpf)
            {
                check = true;
                break;
            }
        }
        return check;
    }

    /// <summary>
    /// 결재이미지정보 조회
    /// </summary>
    /// <remarks>사용자 서명이미지 호출 by 2007.12 for loreal</remarks>
    /// <param name="UserID">사용자 id(person code)</param>
    /// <param name="SignType">서명/인장 구분</param>
    /// <returns>서명이미지경로</returns>
    private string GetSigninform(string UserID, string SignType)
    {
        string strReturn = "";

        DataSet ds = new DataSet();
        DataPack INPUT = null;
        try
        {
            INPUT = new DataPack();
            INPUT.add("@UR_Code", UserID);  // 사용자 코드
            INPUT.add("@IS_USE", "Y");                  // 사용유무

            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
            {
                ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_APV_SIGNIMAGE_R", INPUT);
            }
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                strReturn = dr["FILE_NAME"].ToString();
            }
        }
        catch (System.Exception ex)
        {
            throw new System.Exception("GetApvImage", ex);
        }
        finally
        {
            if (INPUT != null)
            {
                INPUT.Dispose();
            }
        }

        return strReturn;
    }

    /// <summary>
    /// 결재이미지정보 조회
    /// </summary>
    /// <remarks>사용자 서명이미지 호출 by 2007.12 for loreal</remarks>
    /// <param name="UserID">사용자 id(person code)</param>
    /// <param name="SignType">서명/인장 구분</param>
    /// <returns>서명이미지경로</returns>
    [WebMethod(Description = "등록된 사인이미지 가져오기")]
    public DataSet GetSignImageList(string UserID)
    {
        DataSet ds = new DataSet();
        DataPack INPUT = null;
        try
        {
            INPUT = new DataPack();
            INPUT.add("@UR_Code", UserID);  // 사용자 코드
            INPUT.add("@DISPLAYCOUNT", CF.ConfigurationManager.GetBaseConfig("NumberUseSignImage")); //리스트업 할 사인이미지 갯수

            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
            {
                ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_APV_SIGNIMAGE_LIST_R", INPUT);
            }

        }
        catch (System.Exception ex)
        {
            throw new System.Exception("GetApvImage", ex);
        }
        finally
        {
            if (INPUT != null)
            {
                INPUT.Dispose();
            }
        }

        return ds;
    }

    /// <summary>
    /// 사용자 양식에 따른 결재암호 사용여부 결과 return 
    /// </summary>
    /// <param name="pPersonCode"></param>
    /// <param name="pDocTypeCode"></param>
    /// <returns></returns>
    [WebMethod(true)]
    public string GetPersonInfo(string pPersonCode, string pDocTypeCode)
    {
        string strAPPROVAL_PWD_IS_USE = "N";
        DataSet ds = null;
        DataPack INPUT = null;

        try
        {
            ds = new DataSet();
            INPUT = new DataPack();

            string szQuery = "dbo.usp_wfform_getFormInfo_pwd";
            INPUT.add("@PERSON_CODE", pPersonCode);
            INPUT.add("@FORM_PREFIX", pDocTypeCode);

            using (SqlDacManager SqlDbAgent = new SqlDacManager("FORM_DEF_ConnectionString"))
            {
                ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, szQuery, INPUT);
            }

            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                strAPPROVAL_PWD_IS_USE = (Convert.IsDBNull(dr["IS_PWD"]) ? "" : dr["IS_PWD"].ToString());
            }
        }
        catch (System.Exception ex)
        {
            throw new System.Exception(null, ex);
        }
        finally
        {
            if (INPUT != null)
            {
                INPUT.Dispose();
                INPUT = null;
            }
            if (ds != null)
            {
                ds.Dispose();
                ds = null;
            }
        }
        return strAPPROVAL_PWD_IS_USE;
    }
    // 개인 정보 가져오기
    [WebMethod(Description = "전자결재비번사용여부 반환")]
    public string GetPersonInfo1(string strPersonCode)
    {
        string strAPPROVAL_PWD_IS_USE = "N";
        DataSet ds = null;
        DataPack INPUT = null;
        try
        {
            ds = new DataSet();
            INPUT = new DataPack();

            string szQuery = "dbo.usp_PersonSetting_R";
            INPUT.add("@PERSON_CODE", strPersonCode);

            using (SqlDacManager SqlDbAgent = new SqlDacManager("ORG_ConnectionString"))
            {
                ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, szQuery, INPUT);
            }

            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                strAPPROVAL_PWD_IS_USE = (Convert.IsDBNull(dr["APPROVAL_PWD_IS_USE"]) ? "" : dr["APPROVAL_PWD_IS_USE"].ToString());
            }
        }
        catch (System.Exception ex)
        {
            throw new System.Exception(null, ex);
        }
        finally
        {
            if (INPUT != null)
            {
                INPUT.Dispose();
                INPUT = null;
            }
            if (ds != null)
            {
                ds.Dispose();
                ds = null;
            }
        }
        return strAPPROVAL_PWD_IS_USE;
    }
    [WebMethod(true)]
    public string getProcessDomainData(string pProcessId)
    {
        string strReturn = string.Empty;
        DataSet ds = null;
        DataPack INPUT = null;
        try
        {
            INPUT = new DataPack();
            INPUT.add("@PIID", pProcessId);
            using (SqlDacManager SqlDbAgent = new SqlDacManager("INST_ConnectionString"))
            {
                ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wf_getdomaindata", INPUT);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow odr = ds.Tables[0].Rows[0];
                    strReturn = Convert.ToString(odr[0]);
                }
                //부서합의는 상위 Process 정보값 가져와야 함
                else
                {
                    ds = SqlDbAgent.ExecuteDataSet(CommandType.StoredProcedure, "dbo.usp_wf_getdomaindata_parent", INPUT);
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow odr = ds.Tables[0].Rows[0];
                        strReturn = Convert.ToString(odr[0]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new System.Exception(null, ex);
        }
        finally
        {
            if (INPUT != null)
            {
                INPUT.Dispose();
                INPUT = null;
            }
            if (ds != null)
            {
                ds.Dispose();
                ds = null;
            }
        }

        return strReturn;
    }

    /// <summary>
    /// 모바일 결재시 현재 결재자의 taskinfo에 mobileyn attribute추가 
    /// </summary>
    /// <param name="pPersonCode"></param>
    /// <returns></returns>
    #region //--> SetMobileAtt
    [WebMethod(true)]
    public string SetMobileAtt(string pApprovalLineXml)
    {
        string strApvlist = "";

        try
        {
            XmlDocument oApvListXML = new XmlDocument();
            oApvListXML.LoadXml(pApprovalLineXml);

            XmlNode oPerson = oApvListXML.DocumentElement.SelectSingleNode("division/step/ou/person[@code='" + Session["UR_CODE"].ToString() + "' and taskinfo/@status='pending']");
            XmlNode oTaskinfo = oPerson.SelectSingleNode("taskinfo");
            if (oTaskinfo.Attributes.GetNamedItem("mobilegubun") == null)
            {
                XmlAttribute oAttr = oApvListXML.CreateAttribute("mobilegubun");
                oTaskinfo.Attributes.SetNamedItem(oAttr);
            }
            oTaskinfo.Attributes.GetNamedItem("mobilegubun").Value = "y";
            strApvlist = oApvListXML.InnerXml;
        }
        catch (System.Exception ex)
        {
            throw new System.Exception(null, ex);
        }
        finally
        {
        }
        return strApvlist;
    }
    #endregion


    /// <summary>
    /// getInfo
    /// </summary>
    /// <remarks>사용자 서명이미지 호출 by 2007.12 for loreal</remarks>
    /// <param name="UserID">사용자 id(person code)</param>
    /// <param name="SignType">서명/인장 구분</param>
    /// <returns>서명이미지경로</returns>
    private string getInfo(string sValue, XmlElement SignType)
    {
        return "";
    }

    /// <summary>
    /// ExecuteLegacy 결재 상태값 변경
    /// </summary>
    /// <param name="fmpf"></param>
    /// <param name="BODYCONTEXT"></param>
    /// <param name="FORM_INFO_EXT"></param>
    /// <param name="APPROVERCONTEXT"></param>
    /// <param name="PreApproveProcess"></param>
    /// <param name="ApvResult"></param>
    /// <param name="DocNumber"></param>
    /// <param name="ApproverId"></param>
    /// <param name="fiid"></param>
    /// <param name="apvMode"></param>
    private static void LegacyAppStatusChange(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode)
    {
        //DB에 연동 로그 남기기  추가(2011-07-15 leesh)
        SqlDacManager sm = null;
        DataPack dataPack = null;

        try
        {
            XmlDocument oBody = new XmlDocument();
            oBody.LoadXml(BODYCONTEXT);

            XmlNode oRoot = oBody.DocumentElement;
            if (oRoot.SelectSingleNode("mKey") == null) return;

            using (sm = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
            {
                if (apvMode.Equals("DRAFT"))
                {

                    dataPack = new DataPack();
                    dataPack.add("@LEGACY_FORM_ID", oRoot.SelectSingleNode("mLegacy_form").InnerText);
                    dataPack.add("@LEGACY_KEY", oRoot.SelectSingleNode("mKey").InnerText);
                    dataPack.add("@REQUEST_USERID", ApproverId.Replace("@", ""));
                    dataPack.add("@APPROVAL_FMPF", fmpf);
                    dataPack.add("@BODY_CONTEXT", BODYCONTEXT);
                    dataPack.add("@APPROVAL_STATUS", apvMode);
                    dataPack.add("@FORM_INST_ID", fiid);

                    sm.ExecuteNonQuery(CommandType.StoredProcedure, "[dbo].[usp_legacy_data_C]", dataPack);
                }
                else if (apvMode.Equals("APPROVAL") || apvMode.Equals("COMPLETE"))
                {
                    dataPack = new DataPack();
                    dataPack.add("@APPROVAL_STATUS", apvMode);
                    dataPack.add("@FORM_INST_ID", fiid);
                    sm.ExecuteNonQuery(CommandType.StoredProcedure, "[dbo].[usp_legacy_data_U]", dataPack);
                }
                else if (apvMode.Equals("REJECT") || apvMode.Equals("WITHDRAW") || apvMode.Equals("DELETE"))
                {
                    dataPack = new DataPack();
                    dataPack.add("@FORM_INST_ID", fiid);
                    sm.ExecuteNonQuery(CommandType.StoredProcedure, "[dbo].[usp_legacy_data_D]", dataPack);
                }
            }
        }
        catch (Exception ex)
        {
            if (sm != null) { sm.Dispose(); sm = null; }
        }
        finally
        {
            if (dataPack != null) { dataPack.Dispose(); dataPack = null; }
        }
    }

    /// <summary>
    /// ExecuteLegacy 호출 내역을 DB 에 저장
    /// </summary>
    /// <param name="fmpf"></param>
    /// <param name="BODYCONTEXT"></param>
    /// <param name="FORM_INFO_EXT"></param>
    /// <param name="APPROVERCONTEXT"></param>
    /// <param name="PreApproveProcess"></param>
    /// <param name="ApvResult"></param>
    /// <param name="DocNumber"></param>
    /// <param name="ApproverId"></param>
    /// <param name="fiid"></param>
    /// <param name="apvMode"></param>
    private static void LegacyCallLog(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode)
    {
        //DB에 연동 로그 남기기  추가(2011-07-15 leesh)
        SqlDacManager sm = null;
        DataPack dataPack = null;

        try
        {
            using (sm = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
            {
                dataPack = new DataPack();
                dataPack.add("@fmpf", fmpf);
                dataPack.add("@BODYCONTEXT", BODYCONTEXT);
                dataPack.add("@FORM_INFO_EXT", FORM_INFO_EXT);
                dataPack.add("@APPROVERCONTEXT", APPROVERCONTEXT);
                dataPack.add("@PreApproveProcess", PreApproveProcess);
                dataPack.add("@ApvResult", ApvResult);
                dataPack.add("@DocNumber", DocNumber);
                dataPack.add("@ApproverId", ApproverId);
                dataPack.add("@fiid", fiid);
                dataPack.add("@APV_MODE", apvMode);

                sm.ExecuteNonQuery(CommandType.StoredProcedure, "[dbo].[USP_LEGACY_LOG_INSERT]", dataPack);
            }

        }
        catch (Exception ex)
        {
            if (sm != null) { sm.Dispose(); sm = null; }
        }
        finally
        {
            if (dataPack != null) { dataPack.Dispose(); dataPack = null; }
        }
    }

    public string gBodyContentHtml = String.Empty;  //결재본문HTML
    [WebMethod(true)]
    public string GetFormHTML(string gPIID, string gUserID, string pUrl)
    {
        string sProgramPath = String.Empty;
        string sSaveFolderPath = String.Empty;
        string sFileNm = String.Empty;
        string sFilePath = String.Empty;
        Boolean bReturn = false;

        sProgramPath = "D:\\GROUPWARE\\WindowApplication\\WebBrowserTool\\WebBrowserTool\\bin\\Debug\\WebBrowserTool.exe";
        //sProgramPath = Covi.Framework.ConfigurationManager.GetBaseConfig("WebBrowserTool", "0");  //프로그램실행파일 경로 (D:\Application\WebBrowserTool\WebBrowserTool\bin\Debug\WebBrowserTool.exe)

        sSaveFolderPath = "C:\\TEMP\\";  //로컬테스트경로
        //sSaveFolderPath = Covi.Framework.ConfigurationManager.GetBaseConfig("MobileTempFolderPath", "0");  //모바일에서 결재원문보기할때 보여줄 결재원문HTML 임시파일(txt)이 생성되는 폴더경로 (로컬테스트경로:C:\\TEMP\\ , 개발경로:\\NASSTG1V\groupware$\GWStorage\e-sign\Mobile_TempFile\)
        sFileNm = gPIID + "_" + gUserID + ".txt";  //결재문서Html을 담고있는 txt파일명 (PIID_사용자ID.txt)
        sFilePath = sSaveFolderPath + sFileNm;

        if (!Directory.Exists(sSaveFolderPath))
            Directory.CreateDirectory(sSaveFolderPath);

        try
        {
            //#### WebBrowser프로그램 실행 #########################################
            //1.웹브라우져Tool프로그램에서 FormLink_Mobile.aspx(자동인증페이지) 호출하면 2.Form.aspx로 이동 3.최종 MobileImageView.aspx페이지 호출하게되면 이페이지에서 결재문서Html을 txt파일로 생성)
            //  --> 위방식에서 3번은 속도문제로 인해 타지않고 1번과 2번만 실행되고 txt파일은 WebBrowserTool.exe에서 생성하는걸로 변경함.

            //string pUrl = "http://www.no1.com/WebSite/Approval/FormLink.aspx?LogonID=yu2mi&piid=91738b1d-bcdc-4f52-a955-340359e6975e&mode=COMPLETE";
            String ARGUMENT = pUrl + " " + gPIID + " " + gUserID + " " + sSaveFolderPath;   //PIID, 사용자ID, 호출할웹페이지경로 (인자가 복수일때의 인자간 구분은 공백임)

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(sProgramPath);
            startInfo.Arguments = ARGUMENT;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;  //프로그램을 안보이게해서 실행 (최소화는 ProcessWindowStyle.Minimized)
            System.Diagnostics.Process oProc = System.Diagnostics.Process.Start(startInfo);
            oProc.WaitForExit(500);

            //######################################################################

            //txt파일 읽어오기
            for (int i = 0; i < 15; i++)
            {
                bReturn = CheckIsFile(sFilePath);

                if (bReturn)
                    break;

                System.Threading.Thread.Sleep(1000);
            }

            //WebBrowser프로그램 프로세스 종료 (프로세스명:WebBrowserTool.exe)
            oProc.Kill();

            if (bReturn == false)
                gBodyContentHtml = "결재문서 HTML 생성 실패 !";
        }
        catch (Exception ex)
        {
            //throw ex;
            bReturn = false;
        }

        return gBodyContentHtml;
    }
    #region txt파일(결재문서본문Html) 존재유무 체크 및 읽어오기
    private bool CheckIsFile(string pFilePath)
    {
        FileInfo _finfo = new FileInfo(pFilePath);

        if (_finfo.Exists)
        {
            FileStream fs = new FileStream(pFilePath, FileMode.Open);
            StreamReader sw = new StreamReader(fs, System.Text.Encoding.Default);  //Encoding.Default: 한글제대로 나오게..
            gBodyContentHtml = sw.ReadToEnd();    //글자가깨진다면.. Server.HtmlDecode(sw.ReadToEnd()); or Server.HtmlEncode(sw.ReadToEnd()); 
            sw.Close();
            fs.Close();

            //파일삭제
            //File.Delete(pFilePath);

            //Response.Write(gBodyContentHtml);  //내용 확인용(나중에 주석처리)
            return true;
        }
        else
            return false;
    }
    #endregion

    #region "문서유통 대외공문 완결 후 문서 생성 : WSGovDocUpdate()"
    /// <summary>
    /// 문서유통 대외공문 완결 후 문서 생성 : WSGovDocUpdate()
    /// </summary>
    /// <param name="FORM_INST_ID">문서 UNIQUE_ID</param>
    /// <returns>True / False</returns>
    /// <remarks></remarks>
    [WebMethod(true, Description = "문서유통 대외공문 완결 후 문서 생성")]
    public string WSGovDocUpdate(string FORM_INST_ID, string APPROVERCONTEXT, string DocNumber)
    {
        string sReturn = string.Empty;
        return sReturn;
    }
    #endregion
	
	#region SR요청서 기능추가양식(web연동)
    private void execWF_FORM_SRTEST(string fmpf, string BODYCONTEXT, string FORM_INFO_EXT, string APPROVERCONTEXT, bool PreApproveProcess, string ApvResult, string DocNumber, string ApproverId, string fiid, string apvMode, string piid)
    {
        XmlDocument oResult = new XmlDocument();

        XmlDocument oBody = new XmlDocument();
        oBody.LoadXml(BODYCONTEXT);

        XmlNode oRoot = oBody.DocumentElement;
        string lkey = string.Empty;
        string ip = string.Empty;
        string db = string.Empty;

        lkey = oRoot.SelectSingleNode("//BODY_CONTEXT/LegacyKey").InnerText;
        ip = oRoot.SelectSingleNode("//BODY_CONTEXT/ip").InnerText;
        db = oRoot.SelectSingleNode("//BODY_CONTEXT/db").InnerText;


        DataPack INPUT = new DataPack();
        int iReturn;
        string state = string.Empty;
        try
        {
            //LEGACY PROCESS 처리
            switch (apvMode)
            {
                case "DRAFT": //기안      
                    state = "D";
                    break;
                case "REDRAFT"://재기안   
                    break;
                case "COMPLETE": // 최종결재                   
                    state = "C";
                    break;
                case "REJECT": //반려
                    state = "R";
                    break;
                case "WITHDRAW": // 회수
                    state = "B";
                    break;
                case "APPROVAL": // 결재
                    break;
                case "DELETE": // 삭제
                    break;
                default:
                    break;
            }

            try
            {
				if (!string.IsNullOrEmpty(lkey)){
					if (apvMode == "DRAFT" || apvMode == "COMPLETE" || apvMode == "REJECT" || apvMode == "WITHDRAW")
					{
						using (SqlDacManager SqlDbAgent = new SqlDacManager("COVI_FLOW_SI_ConnectionString"))
						{
							INPUT.add("@IP", ip);
							INPUT.add("@DB", db);
							INPUT.add("@BOARD_NO", lkey);
							INPUT.add("@PROCESS_ID", piid);
							INPUT.add("@STATE", state);
	
							iReturn = SqlDbAgent.ExecuteNonQuery(CommandType.StoredProcedure, "dbo.USP_F_F3201QA1_KO455_test_UPDATE", INPUT);
						}
					}
				}
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                INPUT.Dispose();
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
        }

    }
    #endregion
}