package com.example.connectBoard.dto;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class EmployeePreferenceDto {
    private String companyCd;
    private String id;
    private String name;
    private boolean admin;
    private byte[] pwd;
}
