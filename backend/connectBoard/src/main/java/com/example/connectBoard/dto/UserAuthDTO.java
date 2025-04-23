package com.example.connectBoard.dto;


import java.time.LocalDate;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UserAuthDTO {
	private String id;
	private String mCode;
	private String auth;
	private String name;
	private String rollPstnNm;
	private String deptNm;
	private String corpNm;
}