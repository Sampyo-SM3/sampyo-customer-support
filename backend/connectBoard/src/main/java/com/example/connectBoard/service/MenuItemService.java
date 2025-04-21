package com.example.connectBoard.service;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

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
    
    public List<Map<String, Object>> getAllMenu() {
        // 원본 메뉴 아이템 리스트 가져오기
        List<MenuItemDTO> menuItems = menuitemRepository.getAllMenu();
        System.out.println("원본 메뉴 아이템 수: " + menuItems.size());
        
        // 결과를 담을 리스트 초기화
        List<Map<String, Object>> allMenus = new ArrayList<>();
        
        // 대메뉴 코드 기준으로 그룹화
        Map<String, List<MenuItemDTO>> groupedMenus = new HashMap<>();
        Map<String, String> groupNames = new HashMap<>();
        
        // 먼저 메뉴 아이템을 대메뉴 코드 기준으로 분류
        for (MenuItemDTO item : menuItems) {
            String mCode = item.getMCode();
            // 코드가 2자리인 경우 대메뉴로 간주
            if (mCode != null && mCode.length() == 2) {
                groupNames.put(mCode, item.getMName());
                System.out.println("대메뉴 발견: " + mCode + " - " + item.getMName());
                continue;
            }
            
            // 하위 메뉴인 경우 대메뉴 코드 추출 (앞 2자리)
            if (mCode != null && mCode.length() > 2) {
                String groupKey = mCode.substring(0, 2);
                if (!groupedMenus.containsKey(groupKey)) {
                    groupedMenus.put(groupKey, new ArrayList<>());
                }
                groupedMenus.get(groupKey).add(item);
                System.out.println("하위메뉴 추가: " + mCode + " -> 그룹 " + groupKey);
            }
        }
        
        // 그룹화된 메뉴를 결과 형식으로 변환
        for (String groupKey : groupedMenus.keySet()) {
            Map<String, Object> groupMap = new HashMap<>();
            groupMap.put("groupLabel", groupNames.getOrDefault(groupKey, groupKey));
            groupMap.put("groupKey", groupKey);
            groupMap.put("selected", new ArrayList<>());
            groupMap.put("checked", false);
            
            // 옵션 목록 생성
            List<Map<String, String>> options = new ArrayList<>();
            for (MenuItemDTO item : groupedMenus.get(groupKey)) {
                Map<String, String> option = new HashMap<>();
                option.put("label", item.getMName());
                option.put("value", item.getMCode());
                options.add(option);
            }
            
            groupMap.put("options", options);
            allMenus.add(groupMap);
            
            System.out.println("그룹 생성: " + groupKey + " (" + groupNames.getOrDefault(groupKey, groupKey) + "), 하위메뉴 " + options.size() + "개");
        }
        
        // 최종 결과 로그 출력
        System.out.println("최종 결과 그룹 수: " + allMenus.size());
        for (int i = 0; i < allMenus.size(); i++) {
            Map<String, Object> group = allMenus.get(i);
            System.out.println(i + ". " + group.get("groupLabel") + " (" + group.get("groupKey") + ")");
            
            @SuppressWarnings("unchecked")
            List<Map<String, String>> options = (List<Map<String, String>>) group.get("options");
            for (Map<String, String> opt : options) {
                System.out.println("   - " + opt.get("label") + " (" + opt.get("value") + ")");
            }
        }
        
        return allMenus;
    }
}