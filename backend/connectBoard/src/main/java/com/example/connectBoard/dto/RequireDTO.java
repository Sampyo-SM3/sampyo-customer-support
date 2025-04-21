package com.example.connectBoard.dto;


import java.time.LocalDate;
import java.time.LocalDateTime;

import lombok.Getter;
import lombok.Setter;
import lombok.ToString;

@Getter
@Setter
@ToString
public class RequireDTO {
	private Integer seq;
	private String sub;
	private String context;
	private String taskName;
	private String help;
	private String necessity;
	private String effect;
	private String module;
	private String beforeTaskContent;
	private String afterTaskContent;
	private String useDept;
	private String attachDoc;
	private LocalDate requestDate;
	private LocalDateTime requestDateTime;
	private LocalDate acceptDate;
	private LocalDate completeRequestDate;
	private LocalDate completeDate;
	private String etc;
	private String uId;
	private String usem;
	private String dpId;
	private String dpDn;
	private String writerId;
	private String manager;
	private String managerId;
	private String managerTel;
	private String managerEmail;
	private String division;
	private String saveFlag;
	private String srFlag;
	private String docNum;
	private String processState;
	private String statusNm;
	private LocalDate insertDt;
	private LocalDate updateDt;
	private LocalDate completeDt;
	private Integer countComment;
	
	
}