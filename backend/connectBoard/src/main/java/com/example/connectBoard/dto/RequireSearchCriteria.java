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
    private String productType;
    private String sub;
    private String status;
    private Integer countComment;
    // Getters and Setters
    // ...
}
