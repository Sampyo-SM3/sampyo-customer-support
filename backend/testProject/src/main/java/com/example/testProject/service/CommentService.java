package com.example.testProject.service;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.example.testProject.dto.CommentDTO;
import com.example.testProject.repository.CommentRepository;
import com.example.testProject.repository.RequireRepository;

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
}
