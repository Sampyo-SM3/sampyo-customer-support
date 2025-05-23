package com.example.connectBoard.dto;

import java.time.LocalDate;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class CodeDTO {
    private String codeId;      // 상태 코드 (예: "P", "I", "H", "C")
    private String codeName;    // 상태명 (예: "미처리", "진행", "보류중", "종결")
    private String codeDesc;    // 상태 설명 (선택 사항)
    private String category;    // 코드 카테고리 (기본값: "STATUS")
    private String parentCodeId; // 상위 코드 ID (NULL이면 최상위 코드)
    private String useYn;       // 활성 여부 (Y/N)
    private Integer orderNum;	//순번
    private  LocalDate insertDt; // 생성 날짜
    private  LocalDate updateDt; // 업데이트 날짜
    private Integer cnt;	
    private String writerId;       // 작성자id
    private String dpId;       
}