package com.example.connectBoard.controller;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;
import com.example.connectBoard.service.RequireService;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestParam;

@RestController
@RequestMapping("/api")
public class RequireController {

    private final RequireService requireService;
    

    // ✅ 두 개의 서비스 모두 초기화
    public RequireController(RequireService requireService) {
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
    	System.out.println("------searchRequires()-----");
        System.out.println("Search Criteria: " + criteria);
        System.out.println("startDate: " + criteria.getStartDate());
        System.out.println("endDate: " + criteria.getEndDate());
        System.out.println("requesterId: " + criteria.getRequesterId());
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
