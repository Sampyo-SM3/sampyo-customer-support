package com.example.connectBoard.service;

import java.util.List;

import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.CommentDTO;
import com.example.connectBoard.repository.spc.CommentRepository;

@Service
public class CommentService {
    
    private final CommentRepository commentRepository;
    
    public CommentService(CommentRepository commentMapper) {
        this.commentRepository = commentMapper;
    }    

    public List<CommentDTO> getCommentsByPostId(Long postId) {    	
        return commentRepository.findByPostIdOrderByCreatedAtAsc(postId);
    }
    
    public List<CommentDTO> checkCommentsChildByCommentId(Long postId) {    	
        return commentRepository.checkCommentsChildByCommentId(postId);
    }    
    
    
    
    public void insertComment(CommentDTO comment) {        
        commentRepository.insertComment(comment);
    }

    public void updateComment(CommentDTO comment) {
        commentRepository.updateComment(comment);
    }
    
    public void deleteComment(Integer commentId) {
        commentRepository.deleteComment(commentId);
    }

    
}
