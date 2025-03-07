package com.example.testProject.controller;

import com.example.testProject.dto.UserDTO;
import com.example.testProject.service.UserService;
import com.example.testProject.service.testService;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api")
public class testController {

    private final testService testService;
    private final UserService userService;

    // ✅ 두 개의 서비스 모두 초기화
    public testController(testService testService, UserService userService) {
        this.testService = testService;
        this.userService = userService;
    }

    @GetMapping("/hello")
    public String sayHello() {
        return testService.getHelloMessage();
    }

    @GetMapping("/goodbye")
    public String sayGoodbye() {
        return testService.getGoodbyeMessage();
    }

    @GetMapping("/users")
    public List<UserDTO> getAllUsers() {
    	System.out.println("test!~!");    	
        return userService.getAllUsers();
    }
    
    // update할 땐 그냥 객체를 파라미터로 받자 귀찮다
    @PostMapping("/user/update")
    public ResponseEntity<String> updateUser(@RequestBody UserDTO user) {
        int result = userService.updateUser(user);
        if (result > 0) {
            return ResponseEntity.ok("사용자 정보가 업데이트되었습니다.");
        } else {
            return ResponseEntity.notFound().build();
        }
    }
    
  
    
}
