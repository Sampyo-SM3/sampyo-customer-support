package com.example.connectBoard.dto;

import lombok.Getter;
import lombok.Setter;
import lombok.ToString;

@Getter
@Setter
@ToString
public class EmployeePreferenceDto {
	private String companyCd;
	private String id;
	private String name;
	private String phone;
	private String email;
	private String deptCd;
	private boolean admin;
	private byte[] pwd;
}
