package com.example.connectBoard.dto;


import java.time.LocalDate;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UserAuthDTO {
	private String id;
	private String mCode;
	private int auth;
	private String name;
	private String rollPstnNm;
	private String companyCd;
	private String deptNm;
	private String corpNm;
	private String menuCode;
	private String insertUser;
	private String updateUser;
}