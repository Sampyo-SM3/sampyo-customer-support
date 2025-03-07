package com.example.testProject.dto.response;

import java.util.List;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter
@Setter
public class MenuItemResponseDto {
    private List<MenuItem> menuItems;

    @Getter
    @Setter
    @NoArgsConstructor  // 기본 생성자
    @AllArgsConstructor // 모든 필드를 받는 생성자
    public static class MenuItem {
        private String mCode;
        private String mName;
        private String mComment;
        private String mLev;
        private String mAuth;       
		private String mIcon;
        private String mGBN;
               
        
    }
}