package com.example.connectBoard.controller;

import com.example.connectBoard.dto.NaeHwaDTO;
import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;
import com.example.connectBoard.service.LoginService;
import com.example.connectBoard.service.MenuItemService;
import com.example.connectBoard.service.NaehwaService;
import com.example.connectBoard.service.RequireService;

import ch.qos.logback.core.recovery.ResilientSyslogOutputStream;

import java.util.List;
import java.util.Map;

import org.springframework.beans.factory.annotation.Autowired;
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
public class NaeHwaController {
	
	@Autowired
    private NaehwaService naeHwaService;
	
	

    @GetMapping("/naehwa/select-chartdata")
    public ResponseEntity<?> selectNaehwaChartData(@ModelAttribute NaeHwaDTO naeHwaDTO) {
    	System.out.println("--selectNaehwaChartData--");
    	System.out.println(naeHwaDTO.toString());
        try {        	
            List<NaeHwaDTO> naehwas = naeHwaService.selectNaehwa(naeHwaDTO);
            
            if (naehwas.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(naehwas);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /naehwa/select-chartdata " + e.getMessage());
        }
    }    
    
    @GetMapping("/naehwa/select-config")
    public ResponseEntity<?> selectNaehwaConfig(@ModelAttribute NaeHwaDTO naeHwaDTO) {
    	System.out.println("--selectNaehwaConfig--");
    	System.out.println(naeHwaDTO.toString());
        try {        	
            List<NaeHwaDTO> naehwas = naeHwaService.selectNaehwaConfig(naeHwaDTO);
            
            if (naehwas.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(naehwas);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /naehwa/select-config " + e.getMessage());
        }
    }        
   
    @GetMapping("/naehwa/select-length")
    public ResponseEntity<?> selectNaehwaLength(@ModelAttribute NaeHwaDTO naeHwaDTO) {
        System.out.println("--selectNaehwaLength--");
        System.out.println(naeHwaDTO.toString());
        try {
            Integer length = naeHwaService.selectNaehwaLength(naeHwaDTO);
            
            if (length == null) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(length);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /naehwa/select-length " + e.getMessage());
        }
    }      
        
}
