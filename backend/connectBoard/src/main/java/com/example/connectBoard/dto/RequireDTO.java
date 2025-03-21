package com.example.connectBoard.dto;


import java.time.LocalDate;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class RequireDTO {
    private Integer seq;
    private String projectName;
    private String projectContent;
    private String businessSector;
    private String projectOverview;
    private String requesterPosition;
    private String currentIssue;
    private String expectedEffect;
    private String finalDeliverables;
    private String detailTask;
    private String detailContent;
    private String detailItDevRequest;
    private String processState;
    private String requesterId;
    private String requesterName;
    private String requesterDeptCd;
    private String requesterDeptNm;
    private String requesterEmail;
    private String requesterPhone;
    private LocalDate insertDt;
    private LocalDate updateDt;
    private LocalDate completeDt;

}