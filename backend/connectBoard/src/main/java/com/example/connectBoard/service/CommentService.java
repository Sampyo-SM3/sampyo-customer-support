package com.example.connectBoard.service;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.CommentDTO;
import com.example.connectBoard.repository.spc.CommentRepository;
import com.example.connectBoard.repository.spc.RequireRepository;

@Service
public class CommentService {
    
    private final CommentRepository commentRepository;
    
    public CommentService(CommentRepository commentMapper) {
        this.commentRepository = commentMapper;
    }    

    public List<CommentDTO> getCommentsByPostId(Long postId) {    	
        return commentRepository.findByPostIdOrderByCreatedAtAsc(postId);
    }
    
    public void insertComment(CommentDTO comment) {        
        commentRepository.insertComment(comment);
    }
    
    public void deleteComment(Integer commentId) {
        commentRepository.deleteComment(commentId);
    }

    
}
