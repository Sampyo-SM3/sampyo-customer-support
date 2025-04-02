package com.example.connectBoard.dto;


import java.time.LocalDate;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UserPopDTO {
	private String corpNm;
	private String deptNm;
	private String name;
	private String rollPstnNm;
	private String usrId;
	private String emailAddr;
	private String handTelNo;
}