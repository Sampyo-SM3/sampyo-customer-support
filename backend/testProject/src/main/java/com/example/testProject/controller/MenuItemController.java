package com.example.testProject.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import com.example.testProject.dto.request.MenuItemRequestDto;
import com.example.testProject.dto.response.MenuItemResponseDto;
import com.example.testProject.service.MenuItemService;


@RestController
@RequestMapping("/api/menuitem")
public class MenuItemController {

    @Autowired
    private MenuItemService menuitemService;

    @GetMapping
    // test커밋
    public MenuItemResponseDto getMenuItems(@RequestParam(required = false) String auth, @RequestParam String id) {
    	System.out.println("------------getMenuItems-------------");
    	System.out.println(auth);
    	System.out.println(id);
        MenuItemRequestDto requestDto = new MenuItemRequestDto(auth, id);
                
		return menuitemService.getMenuItems(requestDto);
	}
}