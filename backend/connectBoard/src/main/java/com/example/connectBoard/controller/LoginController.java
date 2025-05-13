package com.example.connectBoard.controller;

import java.util.Map;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.connectBoard.dto.EmployeePreferenceDto;
import com.example.connectBoard.exception.Exceptions;
import com.example.connectBoard.service.LoginService;

@RestController
@RequestMapping("/api")
public class LoginController {

    private final LoginService loginService;

    public LoginController(LoginService loginService) {
        this.loginService = loginService;
    }
           

    @PostMapping("/login")
    public ResponseEntity<?> login(@RequestBody Map<String, String> loginParams) {                        //
//    	System.out.println("-- login --");
        String id = loginParams.get("id");
        String password = loginParams.get("password");
        String name = loginParams.get("name");
        String companyCd = "CEMENT";
        String phone = loginParams.get("phone");
        String email = loginParams.get("email"); 
        String deptCd = loginParams.get("deptCd"); 

        System.out.println("🔍 loginParams 전체" + loginParams );
        
        try {
            // 필수 파라미터 검증
            if (id == null || password == null || companyCd == null) {
                return ResponseEntity.badRequest()
                    .body(Map.of("message", "아이디, 비밀번호, 회사코드는 필수 항목입니다."));
            }       
//            System.out.println(id);
//            System.out.println(password);
//            System.out.println(companyCd);
//            System.out.println(phone);
//            System.out.println(email);
            EmployeePreferenceDto result = loginService.login(id, password, deptCd, companyCd, phone, email);
            
//            System.out.println("22");
            return ResponseEntity.ok(result);
        } catch (Exceptions.UserNotFoundException e) {
            // 여기서 외부에 선언된 변수들에 접근 가능
            try {
                String userNameToUse = (name != null && !name.trim().isEmpty()) ? name : "New User";
//                System.out.println("test!!");
//                System.out.println(name);
//                System.out.println(phone);
                loginService.insertUser(id, password, deptCd, userNameToUse, phone, email);
                
                return ResponseEntity.ok(Map.of(
                    "message", "새로운 사용자로 등록되었습니다.",
                    "registered", true
                ));
            } catch (Exception insertEx) {
                return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body(Map.of("message", "사용자 등록 중 오류가 발생했습니다: " + insertEx.getMessage()));
            }
        } catch (Exceptions.PasswordMismatchException e) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                .body(Map.of("message", "비밀번호가 일치하지 않습니다."));
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                .body(Map.of("message", "로그인 처리 중 오류가 발생했습니다."));
        }
    }
    
    @PostMapping("/validate-blue-id")
    public ResponseEntity<?> validate_blue_id(@RequestBody Map<String, String> loginParams) {    	
        try {
        	
            String id = loginParams.get("id");
            String password = loginParams.get("password");            
//            String companyCd = "CEMENT";                       
            
            // 필수 파라미터 검증
            if (id == null || password == null || id.trim().isEmpty() || password.trim().isEmpty()) {            	
                return ResponseEntity.badRequest()
                       .body(Map.of("message", "아이디, 비밀번호는 필수 항목입니다."));
            }                        
            // 블루샘에 존재하는 아이디인지 확인
            EmployeePreferenceDto result = loginService.validate_blue_id(id, password);

            return ResponseEntity.ok(result);
        } catch (Exceptions.UserNotFoundException e) {        	
          return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
          .body(Map.of("message", "존재하지 않는 사용자입니다. (bluesam)"));        	
        } catch (Exception e) {
        	System.out.println("2");
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                   .body(Map.of("message", "로그인 처리 중 오류가 발생했습니다."));
        }
    }    
}