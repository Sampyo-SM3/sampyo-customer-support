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
    
    
    @GetMapping("/status/list")
    public List<StatusDTO> getAllRequires() {
    	return statusService.getAllStatuses();
    }
       
}
