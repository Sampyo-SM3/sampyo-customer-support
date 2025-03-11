package com.example.testProject.controller;

import com.example.testProject.dto.RequireDTO;
import com.example.testProject.dto.RequireSearchCriteria;
import com.example.testProject.dto.UserDTO;
import com.example.testProject.service.RequireService;
import com.example.testProject.service.UserService;
import com.example.testProject.service.testService;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestParam;

@RestController
@RequestMapping("/api")
public class requireController {

    private final RequireService requireService;

    // ✅ 두 개의 서비스 모두 초기화
    public requireController(RequireService requireService) {
        this.requireService = requireService;
    }
    
    
    @GetMapping("/require/list")
    public List<RequireDTO> getAllRequires() {
    	return requireService.getAllRequires();
    }
    
    @GetMapping("/require/detail")
    public ResponseEntity<?> getRequire(@RequestParam("seq") int seq) {
        try {
            RequireDTO require = requireService.getRequire(seq);
            if (require == null) {
                return ResponseEntity.badRequest().body("해당 SEQ(" + seq + ")에 대한 데이터가 존재하지 않습니다.");
            }
            return ResponseEntity.ok(require);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
    @GetMapping("/require/search")
    public ResponseEntity<?> searchRequires(@ModelAttribute RequireSearchCriteria criteria) {    	
        try {        	
            List<RequireDTO> requires = requireService.searchRequiresByCriteria(criteria);
            
            if (requires.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(requires);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /require/search\": " + e.getMessage());
        }
    }    

    
}
