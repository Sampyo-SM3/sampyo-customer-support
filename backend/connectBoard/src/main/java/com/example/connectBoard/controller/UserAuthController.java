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

import com.example.connectBoard.dto.UserAuthDTO;
import com.example.connectBoard.exception.Exceptions;
import com.example.connectBoard.service.LoginService;
import com.example.connectBoard.service.UserAuthService;
import com.example.connectBoard.service.UserPopService;

@RestController
@RequestMapping("/api")
public class UserAuthController {

    private final UserAuthService userAuthService;

    public UserAuthController(UserAuthService userAuthService) {
        this.userAuthService = userAuthService;
    }
           
    //메뉴에 대한 권한이 있는 user 조회
    @GetMapping("/userAuth/list")
    public List<UserAuthDTO> getAllUserAuth() {
    	return userAuthService.getAllUserAuth();
    }  
    
    //user의 권한 조회
    @GetMapping("/userAuth/detailList")
    public List<UserAuthDTO> getUserAuth(String userId) {
    	return userAuthService.getUserAuth(userId);
    }  
}