package com.example.connectBoard.service;

import java.util.ArrayList;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.MenuItemDTO;
import com.example.connectBoard.dto.request.MenuItemRequestDto;
import com.example.connectBoard.dto.response.MenuItemResponseDto;
import com.example.connectBoard.repository.spc.MenuItemRepository;


@Service
public class MenuItemService {

    @Autowired
    private MenuItemRepository menuitemRepository;

    public MenuItemResponseDto getMenuItems(MenuItemRequestDto requestDto) {
        List<MenuItemResponseDto.MenuItem> menuItems = menuitemRepository.getMenuItems(requestDto);
        MenuItemResponseDto responseDto = new MenuItemResponseDto();
        responseDto.setMenuItems(menuItems);
                             
        return responseDto;
    }
    
    public List<MenuItemDTO> getAllMenu() {
        // 결과를 담을 리스트 초기화
        List<MenuItemDTO> allMenus = new ArrayList<>();
        
        // 대메뉴 코드 목록 조회
        List<MenuItemDTO> mainMenuItems = menuitemRepository.getMainMenuCodes();
        
        // 각 대메뉴에 대해 반복하여 프로시저 호출
        for (MenuItemDTO mainMenu : mainMenuItems) {
            // 대메뉴 코드로 해당 메뉴와 하위 메뉴 조회
            String menuCode = mainMenu.getMCode();
            List<MenuItemDTO> menuItems = menuitemRepository.getAllMenu(menuCode);
            
            // 결과 리스트에 추가
            if (menuItems != null && !menuItems.isEmpty()) {
                allMenus.addAll(menuItems);
            }
        }
        
        // 최종 결과 로그 출력
        System.out.println("============ 최종 결과 메뉴 목록 ============");
        System.out.println("총 메뉴 개수: " + allMenus.size());
        for (int i = 0; i < allMenus.size(); i++) {
            MenuItemDTO menu = allMenus.get(i);
            System.out.println(i + ") " + menu.getMCode() + " - " + menu.getMName() + 
                              " (레벨: " + menu.getMLev() + ")");
        }
        System.out.println("============ 결과 목록 끝 ============");
        
        return allMenus;
    }
}