package com.example.connectBoard.controller;

import java.util.List;
import java.util.Map;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.connectBoard.dto.UserPopDTO;
import com.example.connectBoard.exception.Exceptions;
import com.example.connectBoard.service.LoginService;
import com.example.connectBoard.service.UserPopService;

@RestController
@RequestMapping("/api")
public class UserPopController {

    private final UserPopService userPopService;

    public UserPopController(UserPopService userPopService) {
        this.userPopService = userPopService;
    }
           
    //접수상태 리스트 조회
    @GetMapping("/userPop/list")
    public List<UserPopDTO> getAllUser(UserPopDTO userPop) {
    	return userPopService.getAllUser(userPop);
    }  
}