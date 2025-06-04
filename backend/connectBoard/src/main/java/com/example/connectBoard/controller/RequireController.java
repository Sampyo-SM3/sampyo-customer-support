package com.example.connectBoard.controller;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;
import com.example.connectBoard.service.RequireService;

import ch.qos.logback.core.recovery.ResilientSyslogOutputStream;

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
    
    @GetMapping("/require/search-user")
    public ResponseEntity<?> searchRequires2(@ModelAttribute RequireSearchCriteria criteria) {    	
        try {        	
            List<RequireDTO> requires = requireService.searchRequiresByCriteriaUser(criteria);
            
            if (requires.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(requires);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /require/search\": " + e.getMessage());
        }
    }    
    
    @GetMapping("/require/search-depart-admin")
    public ResponseEntity<?> searchRequires3Admin(@ModelAttribute RequireSearchCriteria criteria) {    	
        try {        	
            List<RequireDTO> requires = requireService.searchRequiresByCriteriaDepartAdmin(criteria);
            
            if (requires.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(requires);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /require/search\": " + e.getMessage());
        }
    }  
     
    
    @GetMapping("/require/search-depart")
    public ResponseEntity<?> searchRequires3(@ModelAttribute RequireSearchCriteria criteria) {    	
        try {        	
            List<RequireDTO> requires = requireService.searchRequiresByCriteriaDepart(criteria);
            
            if (requires.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(requires);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /require/search\": " + e.getMessage());
        }
    }        
    
	/* 게시글 최초등록 */
    @PostMapping("/require/insert")
    public ResponseEntity<?> insertRequire(@RequestBody RequireDTO require) {    	
        try {
        	int seq = requireService.insertRequire(require);        	
        	return ResponseEntity.ok(seq);
        	
        } catch (Exception e) {
        	System.out.println("서버 오류 발생: " + e.getMessage());
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }    
    
	// 문의글 수정
    @PostMapping("/require/updateForm")
    public ResponseEntity<?> updateForm(@RequestBody RequireDTO require) {    	
        try {
        	requireService.updateForm(require);
            return ResponseEntity.ok("접수상태가 변경되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
	// SR요청서 수정
    @PostMapping("/require/updateSrForm")
    public ResponseEntity<?> updateSrForm(@RequestBody RequireDTO require) {
        try {
        	requireService.updateSrForm(require);
            return ResponseEntity.ok("접수상태가 변경되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
    // 통계조회
    @GetMapping("/require/search-dashboard")
    public ResponseEntity<?> searchDashBoard(@ModelAttribute RequireSearchCriteria criteria) {    	
        try {        	
            List<RequireDTO> requires = requireService.getDashboardData(criteria);
            
            if (requires.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(requires);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /require/search-dashboard\": " + e.getMessage());
        }
    }        
     
    
}
