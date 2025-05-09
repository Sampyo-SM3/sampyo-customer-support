package com.example.connectBoard.dto;

import java.time.LocalDateTime;

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
    private LocalDateTime  createdAt;
    private String deleteYn;
}