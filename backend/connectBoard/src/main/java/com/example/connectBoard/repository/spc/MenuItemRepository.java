package com.example.connectBoard.repository.spc;

import java.util.List;
import java.util.Map;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.MenuItemDTO;
import com.example.connectBoard.dto.request.MenuItemRequestDto;
import com.example.connectBoard.dto.response.MenuItemResponseDto.MenuItem;

@Mapper
public interface MenuItemRepository {
    List<MenuItem> getMenuItems(MenuItemRequestDto requestDto);
    
    List<MenuItemDTO> getAllMenu();
    
    List<MenuItemDTO> getMainMenuCodes();
}