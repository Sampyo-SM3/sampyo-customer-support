package com.example.connectBoard.dto.request;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter
@Setter
@NoArgsConstructor  // 기본 생성자
@AllArgsConstructor // 모든 필드를 받는 생성자
public class MenuItemRequestDto {
    private String auth;
    private String id;


}

 