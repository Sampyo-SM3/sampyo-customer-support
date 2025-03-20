package com.example.connectBoard.controller;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;
import com.example.connectBoard.dto.StatusDTO;
import com.example.connectBoard.service.RequireService;
import com.example.connectBoard.service.StatusService;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestParam;

@RestController
@RequestMapping("/api")
public class StatusController {

    private final StatusService statusService;
    
    // ✅ 두 개의 서비스 모두 초기화
    public StatusController(StatusService statusService) {
        this.statusService = statusService;
    }
    
    //접수상태 리스트 조회
    @GetMapping("/status/list")
    public List<StatusDTO> getAllRequires() {
    	return statusService.getAllStatuses();
    }
    
	// 접수상태 변경
    @PostMapping("/updateStatus")
    public ResponseEntity<?> updateStatus(@RequestBody RequireDTO require) {
        try {
        	statusService.updateStatus(require);
            return ResponseEntity.ok("접수상태가 변경되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
       
}
