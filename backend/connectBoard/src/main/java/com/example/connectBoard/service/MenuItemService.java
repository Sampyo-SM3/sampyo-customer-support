package com.example.connectBoard.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.request.MenuItemRequestDto;
import com.example.connectBoard.dto.response.MenuItemResponseDto;
import com.example.connectBoard.repository.spc.MenuItemRepository;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

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
}