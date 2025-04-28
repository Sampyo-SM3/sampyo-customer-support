package com.example.connectBoard.controller;

import java.util.List;
import java.util.Map;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
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
    
    // 권한 등록 또는 수정 (Upsert)
    @PostMapping("/userAuth/save")
    public ResponseEntity<String> saveUserAuth(@RequestBody UserAuthDTO userAuthDTO) {
        try {
            userAuthService.saveOrUpdateUserAuth(userAuthDTO);
            return ResponseEntity.ok("권한이 성공적으로 저장되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("권한 저장 중 오류가 발생했습니다." + e.getMessage());
        }
    }
    
    //user의 권한 조회
    @PostMapping("/userAuth/deleteUser")
    public ResponseEntity<String> deleteUserAuth(@RequestBody UserAuthDTO userAuthDTO) {
    	System.out.println("🧪 삭제 요청 ID = " + userAuthDTO.getId());
    	
        try {
            userAuthService.deleteUserAuth(userAuthDTO);  // → 서비스에서 삭제 로직 실행
            return ResponseEntity.ok("권한이 성공적으로 삭제되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("권한 삭제 중 오류가 발생했습니다.");
        }
    }  
}