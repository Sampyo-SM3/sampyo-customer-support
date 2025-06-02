package com.example.connectBoard.dto;

import java.time.LocalDate;

import lombok.Getter;
import lombok.Setter;
import lombok.ToString;

@Getter
@Setter
@ToString
public class RequireSearchCriteria {
    private Integer seq;
    private String startDate;
    private String endDate;
    private String manager;
    private String managerId;
    private String managerDeptCd;
    private String writerId;    
    private String productType;
    private String sub;
    private String status;
    private Integer countComment;
    private String dpId;
    private String writerDp;
    private String division;
    
    // Getters and Setters
    // ...
}
