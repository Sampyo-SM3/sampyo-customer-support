package com.example.connectBoard.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

import com.example.connectBoard.dto.request.MenuItemRequestDto;
import com.example.connectBoard.dto.response.MenuItemResponseDto;
import com.example.connectBoard.service.MenuItemService;


@RestController
@RequestMapping("/api/menuitem")
public class MenuItemController {

    @Autowired
    private MenuItemService menuitemService;

    @GetMapping
    // test커밋 나는 세림세림
    public MenuItemResponseDto getMenuItems(@RequestParam(required = false) String auth, @RequestParam String id) {    	
        MenuItemRequestDto requestDto = new MenuItemRequestDto(auth, id);
                
		return menuitemService.getMenuItems(requestDto);
	}
}