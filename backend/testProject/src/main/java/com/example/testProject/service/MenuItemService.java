package com.example.testProject.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.testProject.dto.request.MenuItemRequestDto;
import com.example.testProject.dto.response.MenuItemResponseDto;
import com.example.testProject.repository.MenuItemRepository;

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