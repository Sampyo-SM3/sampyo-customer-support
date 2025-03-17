package com.example.connectBoard.service;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.connectBoard.dto.CommentDTO;
import com.example.connectBoard.repository.CommentRepository;
import com.example.connectBoard.repository.RequireRepository;

@Service
public class CommentService {
    
    private final CommentRepository commentRepository;
    
    public CommentService(CommentRepository commentMapper) {
        this.commentRepository = commentMapper;
    }    

    public List<CommentDTO> getCommentsByPostId(Long postId) {
    	System.out.println("postId -> " + postId);
        return commentRepository.findByPostIdOrderByCreatedAtAsc(postId);
    }
    
    public void insertComment(CommentDTO comment) {
        System.out.println("insertComment() -> postId: " + comment.getPostId() + ", userId: " + comment.getUserId());
        commentRepository.insertComment(comment);
    }
    
}
