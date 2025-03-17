package com.example.testProject.controller;

import java.util.Map;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.testProject.dto.EmployeePreferenceDto;
import com.example.testProject.exception.Exceptions;
import com.example.testProject.service.LoginService;

@RestController
@RequestMapping("/api/login")
public class LoginController {

    private final LoginService loginService;

    public LoginController(LoginService loginService) {
        this.loginService = loginService;
    }

    @PostMapping
    public ResponseEntity<?> login(@RequestBody Map<String, String> loginParams) {
    	System.out.println("--login--");
        try {
            String id = loginParams.get("id");
            String password = loginParams.get("password");
            String companyCd = "CEMENT";
            
            // 필수 파라미터 검증
            if (id == null || password == null || companyCd == null) {
                return ResponseEntity.badRequest()
                       .body(Map.of("message", "아이디, 비밀번호, 회사코드는 필수 항목입니다."));
            }
            
            EmployeePreferenceDto result = loginService.login(id, password, companyCd);
            return ResponseEntity.ok(result);
        } catch (Exceptions.UserNotFoundException e) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                   .body(Map.of("message", "존재하지 않는 사용자입니다."));
        } catch (Exceptions.PasswordMismatchException e) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                   .body(Map.of("message", "비밀번호가 일치하지 않습니다."));
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                   .body(Map.of("message", "로그인 처리 중 오류가 발생했습니다."));
        }
    }
}