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
        List<MenuItemDTO> menuItems = menuitemRepository.getAllMenu();
        System.out.println("원본 메뉴 아이템 수: " + menuItems.size());

        List<Map<String, Object>> allMenus = new ArrayList<>();

        Map<String, String> topMenuNames = new HashMap<>();
        Map<String, List<MenuItemDTO>> midMenus = new HashMap<>();
        Map<String, List<MenuItemDTO>> bottomMenus = new HashMap<>();

        for (MenuItemDTO item : menuItems) {
            String code = item.getMCode();
            if (code == null) continue;

            if (code.length() == 2) {
                topMenuNames.put(code, item.getMName());
                System.out.println("대메뉴 발견: " + code + " - " + item.getMName());
            } else if (code.length() == 4) {
                String topKey = code.substring(0, 2);
                midMenus.computeIfAbsent(topKey, k -> new ArrayList<>()).add(item);
                System.out.println("중메뉴 추가: " + code + " -> 그룹 " + topKey);
            } else if (code.length() > 4) {
                String midKey = code.substring(0, 4);
                bottomMenus.computeIfAbsent(midKey, k -> new ArrayList<>()).add(item);
                System.out.println("소메뉴 추가: " + code + " -> 그룹 " + midKey);
            }
        }

        for (String topKey : topMenuNames.keySet()) {
            Map<String, Object> groupMap = new HashMap<>();
            groupMap.put("groupLabel", topMenuNames.getOrDefault(topKey, topKey));
            groupMap.put("groupKey", topKey);
            groupMap.put("selected", new ArrayList<>());
            groupMap.put("checked", false);

            List<Map<String, Object>> options = new ArrayList<>();

            List<MenuItemDTO> mids = midMenus.getOrDefault(topKey, new ArrayList<>());
            for (MenuItemDTO mid : mids) {
                Map<String, Object> midOption = new HashMap<>();
                midOption.put("label", mid.getMName());
                midOption.put("value", mid.getMCode());

                List<Map<String, String>> children = new ArrayList<>();
                List<MenuItemDTO> bottoms = bottomMenus.getOrDefault(mid.getMCode(), new ArrayList<>());
                for (MenuItemDTO bottom : bottoms) {
                    Map<String, String> child = new HashMap<>();
                    child.put("label", bottom.getMName());
                    child.put("value", bottom.getMCode());
                    children.add(child);
                }

                midOption.put("children", children);
                options.add(midOption);
            }

            groupMap.put("options", options);
            allMenus.add(groupMap);

            System.out.println("그룹 생성: " + topKey + " (" + topMenuNames.getOrDefault(topKey, topKey) + "), 중메뉴 " + options.size() + "개");
        }

        System.out.println("최종 결과 그룹 수: " + allMenus.size());
        for (int i = 0; i < allMenus.size(); i++) {
            Map<String, Object> group = allMenus.get(i);
            System.out.println(i + ". " + group.get("groupLabel") + " (" + group.get("groupKey") + ")");

            @SuppressWarnings("unchecked")
            List<Map<String, Object>> options = (List<Map<String, Object>>) group.get("options");
            for (Map<String, Object> opt : options) {
                System.out.println("   - 중메뉴: " + opt.get("label") + " (" + opt.get("value") + ")");
                @SuppressWarnings("unchecked")
                List<Map<String, String>> children = (List<Map<String, String>>) opt.get("children");
                for (Map<String, String> child : children) {
                    System.out.println("     - 소메뉴: " + child.get("label") + " (" + child.get("value") + ")");
                }
            }
        }

        return allMenus;
    }
}
