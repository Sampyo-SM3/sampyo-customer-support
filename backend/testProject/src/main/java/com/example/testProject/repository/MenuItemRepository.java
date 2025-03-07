package com.example.testProject.repository;

import java.util.List;
import org.apache.ibatis.annotations.Mapper;
import com.example.testProject.dto.request.MenuItemRequestDto;
import com.example.testProject.dto.response.MenuItemResponseDto.MenuItem;

@Mapper
public interface MenuItemRepository {
    List<MenuItem> getMenuItems(MenuItemRequestDto requestDto);
}