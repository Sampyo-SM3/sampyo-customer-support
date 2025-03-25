package com.example.connectBoard.dto;


import java.time.LocalDate;
import java.time.LocalDateTime;

import lombok.Getter;
import lombok.Setter;


@Getter
@Setter
public class KakaoDTO {
   private Integer mtPr;
   private String mtRefkey;
   private Character priority;
   private LocalDateTime dateClientReq;
   private String subject;
   private String content;
   private String callback;
   private Character msgStatus;
   private String recipientNum;
   private LocalDateTime dateMtSent;
   private LocalDateTime dateRslt;
   private LocalDateTime dateMtReport;
   private Character reportCode;
   private String rsId;
   private String countryCode;
   private Integer msgType;
   private Character cryptoYn;
   private Character ataId;
   private LocalDateTime regDate;
   private String senderKey;
   private String templateCode;
   private String responseMethod;
   private Character adFlag;
   private Character kkoBtnType;
   private String kkoBtnInfo;
   private String imgUrl;
   private String imgLink;
   private String etcText1;
   private String etcText2;
   private String etcText3;
   private Integer etcNum1;
   private Integer etcNum2;
   private Integer etcNum3;
   private LocalDateTime etcDate1;
   private String trEtc1;
   private String trEtc2;
   private String trEtc3;
   private String trEtc4;
   private String trEtc5;
   private String trEtc6;
   private String trEtc7;
   private LocalDateTime entDt;

}