package com.example.connectBoard.controller;

import com.example.connectBoard.dto.LibraryDTO;
import com.example.connectBoard.service.LibraryService;

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
public class LibraryController {

    private final LibraryService libraryService;
    

    // ✅ 두 개의 서비스 모두 초기화
    public LibraryController(LibraryService libraryService) {
        this.libraryService = libraryService;
    }
    
    @GetMapping("/library/list")
    public ResponseEntity<?> searchLibrary(@RequestParam("title") String title) {    	
        try {        	
            List<LibraryDTO> requires = libraryService.searchLibrary(title);
            
            if (requires.isEmpty()) {
                return ResponseEntity.ok().body("검색 조건에 해당하는 데이터가 존재하지 않습니다.");
            }
            
            return ResponseEntity.ok(requires);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생_ /library/list\": " + e.getMessage());
        }
    }    
    
    
    @GetMapping("/library/detail")
    public ResponseEntity<?> getLibrary(@RequestParam("seq") int seq) {
        try {
        	LibraryDTO library = libraryService.getLibrary(seq);
            if (library == null) {
                return ResponseEntity.badRequest().body("해당 SEQ(" + seq + ")에 대한 데이터가 존재하지 않습니다.");
            }
            return ResponseEntity.ok(library);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
	/* 게시글 최초등록 */
    @PostMapping("/library/insert")
    public ResponseEntity<?> insertLibrary(@RequestBody LibraryDTO library) {    	
        try {
        	int seq = libraryService.insertLibrary(library);        	
        	return ResponseEntity.ok(seq);
        	
        } catch (Exception e) {
        	System.out.println("서버 오류 발생: " + e.getMessage());
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }    
    
	// 문의글 수정
    @PostMapping("/library/update")
    public ResponseEntity<?> updateLibrary(@RequestBody LibraryDTO library) {    	
        try {
        	libraryService.updateLibrary(library);
            return ResponseEntity.ok("접수상태가 변경되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
    
}
