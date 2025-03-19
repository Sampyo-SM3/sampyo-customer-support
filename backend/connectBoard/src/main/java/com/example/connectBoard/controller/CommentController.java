package com.example.connectBoard.controller;

import com.example.connectBoard.dto.CommentDTO;
import com.example.connectBoard.service.CommentService;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
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
    
    @GetMapping("/comments/{postId}")
    public ResponseEntity<?> getCommentsByPostIdWithPath(@PathVariable("postId") Long postId) {	
        System.out.println("------ getCommentsByPostIdWithPath() -----");
        System.out.println("Post ID: " + postId);

        try {
            List<CommentDTO> comments = commentService.getCommentsByPostId(postId);
            if (comments.isEmpty()) {
                return ResponseEntity.ok().body("해당 게시글에 댓글이 존재하지 않습니다.");
            }
            return ResponseEntity.ok(comments);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
	/* 댓글 등록 */
    @PostMapping("/insertComment")
    public ResponseEntity<?> insertComment(@RequestBody CommentDTO comment) {
        try {
            commentService.insertComment(comment);
            return ResponseEntity.ok("댓글이 성공적으로 등록되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
	/* 댓글 삭제 */
    @PostMapping("/deleteComment/{commentId}")
    public ResponseEntity<?> deleteComment(@PathVariable("commentId") Integer commentId) {
        try {
            commentService.deleteComment(commentId);
            return ResponseEntity.ok("댓글이 성공적으로 삭제되었습니다.");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("서버 오류 발생: " + e.getMessage());
        }
    }
    
    
}
