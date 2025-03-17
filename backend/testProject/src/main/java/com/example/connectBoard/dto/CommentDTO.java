package com.example.connectBoard.dto;

import java.time.LocalDate;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class CommentDTO {
    private Long commentId;
    private Long postId;
    private String userId;
    private String content;
    private Long parentId;
    private int depth;
    private LocalDate createdAt;
    private String deleteYn;
}