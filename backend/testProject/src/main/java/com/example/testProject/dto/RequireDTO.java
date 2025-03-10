package com.example.testProject.dto;


import java.time.LocalDate;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class RequireDTO {
    private Integer seq;                 // 순번 (IDENTITY)
    private String projectName;          // 과제명
    private String businessSector;       // 사업부문
    private String projectOverview;      // 과제 개요
    private String requesterPosition;    // 요청자의 직급
    private String currentIssue;         // 기존 문제점
    private String expectedEffect;       // 기대효과
    private String finalDeliverables;    // 최종산출물
    private String requesterId;          // 작성자 ID
    private String requesterEmail;       // 작성자 이메일
    private String requesterPhone;       // 작성자 핸드폰번호
    private String processState;         // 처리상태
    private LocalDate insertDt;      // 생성일
    private LocalDate updateDt;      // 수정일
    private LocalDate completeDt;    // 완료일
}