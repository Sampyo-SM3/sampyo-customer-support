package com.example.testProject.controller;

import com.example.testProject.dto.CommentDTO;
import com.example.testProject.service.CommentService;


import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestParam;

@RestController
@RequestMapping("/api")
public class CommentController {    
	
    private final CommentService commentService;
    

    // ✅ 두 개의 서비스 모두 초기화
    public CommentController(CommentService commentService) {
        this.commentService = commentService;
    }
      
    
    @GetMapping("/comments")
    public ResponseEntity<?> getCommentsByPostId(@RequestParam("postId") Long postId) {	
       System.out.println("------getCommentsByPostId()-----");
       System.out.println("Post ID: " + postId);

       try {
           List<CommentDTO> comments = commentService.getCommentsByPostId(postId);

           if (comments.isEmpty()) {
               return ResponseEntity.ok().body("해당 게시글에 댓글이 존재하지 않습니다.");
           }

           return ResponseEntity.ok(comments);
       } catch (Exception e) {
           return ResponseEntity.status(500).body("서버 오류 발생_ /comments/{postId}: " + e.getMessage());
       }
    }      

   
    
}
