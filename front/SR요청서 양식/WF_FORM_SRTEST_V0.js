//양식별 다국어 정의 부분
var localLang_ko = {
    localLangItems: {
    }
};

var localLang_en = {
    localLangItems: {
    }
};

var localLang_ja = {
    localLangItems: {
    }
};

var localLang_zh = {
    localLangItems: {
    }
};


//양식별 후처리를 위한 필수 함수 - 삭제 시 오류 발생
function postRenderingForTemplate() {

console.log("--postRenderingForTemplate()--");
console.log(getInfo("templatemode"));

    // 체크박스, radio 등 공통 후처리 
    postJobForDynamicCtrl();
    //읽기 모드 일 경우
    if (getInfo("templatemode") == "Read") {
        $('[data-mode="writeOnly"]').each(function () {
            $(this).hide();
        });
		
		document.getElementById("ATTACH_DATA_DIV").style.display = "none";
		
		if (getInfo("lkey") && getInfo("lkey").trim() !== '') {
			bindingLegacyData("R");
		}	
        
    }
    else {
        $('[data-mode="readOnly"]').each(function () {
            $(this).hide();
        });
		
		document.getElementById("ATTACH_DATA").style.display = "none";
		
        // 에디터 처리
        if (formJson.oFormData.mode == "DRAFT" || formJson.oFormData.mode == "TEMPSAVE") {
			
            document.getElementById("INITIATOR_OU_DP").value = m_oFormMenu.getLngLabel(getInfo("dpdn"), false);
            document.getElementById("INITIATOR_DP").value = m_oFormMenu.getLngLabel(getInfo("usdn"), false);
			
			if (getInfo("lkey") && getInfo("lkey").trim() !== '') {
				if (formJson.oFormData.mode == "DRAFT") {
					bindingLegacyData("W");
					$("#LegacyKey").val(getInfo("lkey"));
					$("#ip").val(getInfo("ip"));
					$("#db").val(getInfo("db"));
				}
			}
			      
        }
    }
}

function setLabel() {
}

function setFormInfoDraft() {
}

function checkForm(bTempSave) {
    if (bTempSave) {
        return true;
    } else {
        // 필수 입력 필드 체크
        $("#U_ID").val(getInfo("usdn"));
        $("#USEM").val(getInfo("usem"));
        $("#DPID").val(getInfo("dpid"));
        $("#DPDN").val(getInfo("dpdn"));
        $("#SUB").val($("#SUBJECT").val());
       // $("#INID").val(getInfo("INITIATOR_ID"));
        return EASY.check().result;
    }
}

function setBodyContext(sBodyContext) {
}

//본문 XML로 구성
function makeBodyContext() {
    var sBodyContext = "";
    sBodyContext = "<BODY_CONTEXT>" + getFields("mField") + "</BODY_CONTEXT>";

    return sBodyContext;
}


function bindingLegacyData(pType) {

    if (pType == "W") {
		$('#DOC_CLASS_NAME').val('업무일반');
		
        var connectionname = "COVI_FLOW_SI_ConnectionString";
        var pXML = "dbo.USP_F_F3201QA1_KO455_test_hj";

        var aXML = "<param><name>DATE</name><type>varchar</type><length>100</length><value><![CDATA[" + getInfo("lkey") + "]]></value></param>";
        aXML += "<param><name>IP</name><type>varchar</type><length>32</length><value><![CDATA[" + getInfo("ip") + "]]></value></param>";
        aXML += "<param><name>DB</name><type>varchar</type><length>32</length><value><![CDATA[" + getInfo("db") + "]]></value></param>";
        var sXML = "<Items><connectionname>" + connectionname + "</connectionname><xslpath></xslpath><sql><![CDATA[" + pXML + "]]></sql><type>sp</type>" + aXML + "</Items>";
        var szURL = "../GetXMLQuery.aspx";


        // 서버에 AJAX 요청 보내기
        CFN_CallAjax(szURL, sXML, function (data) {
            // 응답 데이터를 저장 (향후 읽기 모드에서 사용하기 위해)
            $("#LegacyData").val(CFN_XmlToString(data));
            
			if ($(data).find("response>NewDataSet>Table").length >= 1) {
				$("#LegacyData").val(CFN_XmlToString(data));
	
				$("#SUBJECT").val($(data).find("SUBJECT").text());
				$("#TASK_NAME").val($(data).find("TASK_NAME").text());
				$("#HELP").val($(data).find("HELP").text());
				$("#NECESSITY").val($(data).find("NECESSITY").text());
				$("#EFFECT").val($(data).find("EFFECT").text());
				$("#MODULE").val($(data).find("MODULE").text());
				$("#BEFORE_TASK_CONTENT").val($(data).find("BEFORE_TASK_CONTENT").text());
				$("#AFTER_TASK_CONTENT").val($(data).find("AFTER_TASK_CONTENT").text());
				$("#USE_DEPT").val($(data).find("USE_DEPT").text());
				$("#ATTACH_DOC").val($(data).find("ATTACH_DOC").text());
				$("#REQUEST_DATE").val($(data).find("REQUEST_DATE").text());
				$("#ACCEPT_DATE").val($(data).find("ACCEPT_DATE").text());
				$("#COMPLETE_REQUEST_DATE").val($(data).find("COMPLETE_REQUEST_DATE").text());
				$("#COMPLETE_DATE").val($(data).find("COMPLETE_DATE").text());
				$("#ETC").val($(data).find("ETC").text());
				
				// 숨겨진 필드 값 설정
				$("#U_ID").val($(data).find("uId").text() || getInfo("usdn"));
				$("#USEM").val($(data).find("usem").text() || getInfo("usem"));
				$("#DPID").val($(data).find("dpId").text() || getInfo("dpid"));
				$("#DPDN").val($(data).find("dpDn").text() || getInfo("dpdn"));
				$("#SUB").val($(data).find("sub").text() || $("#SUBJECT").val());
       
            }
	
            var attachFile = "";
            if ($(data).find("response>NewDataSet>Table1").length >= 1) {
				$(data).find("response>NewDataSet>Table1").each(function (i, item) {
					filePath = $(item).find("filePath").text();
                    fileName = $(item).find("fileName").text();
					
                    attachFile += getAttachImage($(item).find("fileName").text().substring($(item).find("fileName").text().lastIndexOf('.') + 1, $(item).find("fileName").text().length))
                    attachFile += "<b style='margin-top:3px;'><a href='#' onclick=\"event.preventDefault(); fileDownload_Legacy('" + filePath + "','" + fileName + "')\">" + $(item).find("fileName").text() + "</a></b><br />";
				}); 
				
				$("#ATTACH_DATA").val(attachFile);
                $("#ATTACH_DATA_DIV").append(attachFile);
            }		
			
            // 받아온 데이터로 각 필드 채우기
            // 예시: 개발 요청서 양식 필드들
            
        }, false, "xml");
    } 
    else { // 읽기 모드일 때
        try {
            var xmlLegacyData = $.parseXML($("#LegacyData").val().replace(/&/gi, "&amp;"));
            
			if ($(xmlLegacyData).find("response>NewDataSet>Table").length >= 1) {
				$("#TASK_NAME").closest("td").text($(xmlLegacyData).find("TASK_NAME").text());
				$("#HELP").closest("td").text($(xmlLegacyData).find("HELP").text());
				$("#NECESSITY").closest("td").text($(xmlLegacyData).find("NECESSITY").text());
				$("#EFFECT").closest("td").text($(xmlLegacyData).find("EFFECT").text());
				$("#MODULE").closest("td").text($(xmlLegacyData).find("MODULE").text());
				$("#BEFORE_TASK_CONTENT").closest("td").text($(xmlLegacyData).find("BEFORE_TASK_CONTENT").text());
				$("#AFTER_TASK_CONTENT").closest("td").text($(xmlLegacyData).find("AFTER_TASK_CONTENT").text());
				$("#USE_DEPT").closest("td").text($(xmlLegacyData).find("USE_DEPT").text());
				$("#ATTACH_DOC").closest("td").text($(xmlLegacyData).find("ATTACH_DOC").text());
				$("#REQUEST_DATE").closest("td").text($(xmlLegacyData).find("REQUEST_DATE").text());
				$("#ACCEPT_DATE").closest("td").text($(xmlLegacyData).find("ACCEPT_DATE").text());
				$("#COMPLETE_REQUEST_DATE").closest("td").text($(xmlLegacyData).find("COMPLETE_REQUEST_DATE").text());
				$("#COMPLETE_DATE").closest("td").text($(xmlLegacyData).find("COMPLETE_DATE").text());
				$("#ETC").closest("td").text($(xmlLegacyData).find("ETC").text())
				$("#ATTACH_DATA").closest("td").text($(xmlLegacyData).find("ATTACH_DATA").text());     
            }
			
			var attachFile = "";
			if ($(xmlLegacyData).find("response>NewDataSet>Table1").length >= 1) {
                $(xmlLegacyData).find("response>NewDataSet>Table1").each(function (i, item) {
					filePath = $(item).find("filePath").text();
                    fileName = $(item).find("fileName").text();
					
                    attachFile += getAttachImage($(item).find("fileName").text().substring($(item).find("fileName").text().lastIndexOf('.') + 1, $(item).find("fileName").text().length))
                    attachFile += "<b style='margin-top:3px;'><a href='#' onclick=\"event.preventDefault(); fileDownload_Legacy('" + filePath + "','" + fileName + "')\">" + $(item).find("fileName").text() + "</a></b><br />";
				}); 
				
				$("#ATTACH_DATA_TD").append(attachFile);
				$("#ATTACH_DATA_DIV").append(attachFile);
            }
            
            // 저장된 XML 데이터로 각 필드의 값을 표시
            // 위와 동일한 필드 매핑 로직 사용
			
        } 
        catch (e) {
            console.error("레거시 데이터 파싱 오류: ", e);
        }
    }
}


function fileDownload_Legacy(filePath, name) {
    var l_sHost = document.location.protocol + '//' + _HostName;
    var l_sParam = "url=" + encodeURIComponent(filePath) + "&name=" + encodeURIComponent(name);
    var l_sDownURL = '/WebSite/Common/ExControls/FileDownload/FileDownload_Legacy.aspx?';

    var downloadUrl = l_sHost + l_sDownURL + l_sParam;

    // 동적으로 a 태그 생성 후 클릭
    var link = document.createElement('a');
    link.href = downloadUrl;
    link.download = name; // 이건 서버가 attachment로 내려줄 때만 유효
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
	
	showDownloadToast("다운로드가 시작되었습니다: " + name);
}

function showDownloadToast(message) {
    var toast = document.createElement('div');
    toast.innerText = message;
    toast.style.position = 'fixed';
    toast.style.bottom = '30px';
    toast.style.right = '30px';
    toast.style.backgroundColor = '#333';
    toast.style.color = '#fff';
    toast.style.padding = '10px 20px';
    toast.style.borderRadius = '8px';
    toast.style.boxShadow = '0 2px 8px rgba(0,0,0,0.3)';
    toast.style.fontSize = '14px';
    toast.style.zIndex = '9999';
    toast.style.opacity = '0';
    toast.style.transition = 'opacity 0.3s ease';

    document.body.appendChild(toast);
    setTimeout(() => {
        toast.style.opacity = '1';
    }, 10);

    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => {
            document.body.removeChild(toast);
        }, 300);
    }, 3000);
}
