package com.example.testProject.dto;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class RequireSearchCriteria {
    private Integer seq;
    private String startDate;
    private String endDate;
    private String manager;
    private String productType;
    private String requesterId;
    
    // Getters and Setters
    // ...
}
