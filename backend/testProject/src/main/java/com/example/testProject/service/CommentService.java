package com.example.testProject.service;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.List;

import java.util.stream.Collectors;

import org.springframework.stereotype.Service;

import com.example.testProject.dto.CommentDTO;
import com.example.testProject.repository.CommentRepository;

@Service
public class CommentService {
    
    private final CommentRepository commentRepository;
    
    public CommentService(CommentRepository commentMapper) {
        this.commentRepository = commentMapper;
    }    

    public List<CommentDTO> getCommentsByPostId(Long postId) {
        List<CommentDTO> comments = commentRepository.findByPostIdOrderByCreatedAtAsc(postId);

        // 필요하면, 각 CommentDTO에 대해 날짜를 포맷팅하여 확인할 수도 있습니다.
        comments.forEach(commentDTO -> {
            System.out.println(commentDTO.getFormattedCreatedAt());
        });

        return comments;
    }
    

    public int addComment(CommentDTO commentDTO) {
        commentDTO.setCreatedAt(LocalDateTime.now());    	
        commentDTO.setDeleteYn("N");
        return commentRepository.addComment(commentDTO);
    }    
    
    
}
