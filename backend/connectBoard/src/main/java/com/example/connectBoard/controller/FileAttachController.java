package com.example.connectBoard.controller;


import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.example.connectBoard.dto.CommentDTO;
import com.example.connectBoard.dto.FileAttachDTO;
import com.example.connectBoard.service.FileAttachService;


@RestController
@RequestMapping("/api/file-attach")
public class FileAttachController {

    private final FileAttachService fileattachService;
    

    // ✅ 두 개의 서비스 모두 초기화
    public FileAttachController(FileAttachService fileattachService) {
        this.fileattachService = fileattachService;
    }
        
    
	/* 파일첨부 등록 */
    @PostMapping("/insert")
    public ResponseEntity<?> fileattachRequire(@RequestBody FileAttachDTO fileattach) {               
        try {
            // 수정된 부분
        	fileattachService.insertFileAttach(fileattach);
            return ResponseEntity.ok("파일이 성공적으로 등록되었습니다.");
        } catch (Exception e) {
            System.out.println("서버 오류 발생: " + e.getMessage());
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
	/* 파일첨부 삭제 */
    @PostMapping("/deleteFile")
    public ResponseEntity<?> deleteFileAttach(@RequestBody FileAttachDTO fileattach) {          
    	System.out.println("fileattach::::" + fileattach.getFileName());
    	
        try {
            // 수정된 부분
        	fileattachService.deleteFileAttach(fileattach);
            return ResponseEntity.ok("파일이 성공적으로 삭제되었습니다.");
        } catch (Exception e) {
            System.out.println("서버 오류 발생: " + e.getMessage());
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
    @GetMapping("/fileList")
    public ResponseEntity<?> getFileList(@RequestParam("seq") Long seq) {	       
       try {
           List<FileAttachDTO> fileAttach = fileattachService.getFileList(seq);

           if (fileAttach.isEmpty()) {
               return ResponseEntity.ok().body("해당 게시글에 첨부파일이 존재하지 않습니다.");
           }

           return ResponseEntity.ok(fileAttach);
       } catch (Exception e) {
           return ResponseEntity.status(500).body("서버 오류 발생");
       }
    }     
    
}
