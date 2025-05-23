package com.example.connectBoard.controller;

import com.example.connectBoard.dto.CodeDTO;
import com.example.connectBoard.service.CodeService;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api")
public class CodeController {

    private final CodeService codeService;
    
    // ✅ 두 개의 서비스 모두 초기화
    public CodeController(CodeService codeService) {
        this.codeService = codeService;
    }
    
    //접수상태 리스트 조회
    @GetMapping("/code/list")
    public List<CodeDTO> getCodes(@RequestParam String category) {
    	return codeService.getCodes(category);
    }
    
    //문의유형별 count
    @GetMapping("/code/count")
    public List<CodeDTO> getCodeCount(@RequestParam String startDate, @RequestParam String endDate, @RequestParam String writerId, @RequestParam String dpId) {
        return codeService.getCodeCount(startDate, endDate, writerId, dpId);
    } 
       
}
