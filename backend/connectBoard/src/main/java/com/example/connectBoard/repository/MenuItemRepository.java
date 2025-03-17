package com.example.connectBoard.repository;

import java.util.List;
import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.request.MenuItemRequestDto;
import com.example.connectBoard.dto.response.MenuItemResponseDto.MenuItem;

@Mapper
public interface MenuItemRepository {
    List<MenuItem> getMenuItems(MenuItemRequestDto requestDto);
}