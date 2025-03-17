package com.example.testProject.repository;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;

import com.example.testProject.dto.CommentDTO;

@Mapper
public interface CommentRepository {
    // 게시글 ID로 댓글 조회 (생성일 오름차순)
    List<CommentDTO> findByPostIdOrderByCreatedAtAsc(Long postId);

    void insertComment(CommentDTO comment);
}