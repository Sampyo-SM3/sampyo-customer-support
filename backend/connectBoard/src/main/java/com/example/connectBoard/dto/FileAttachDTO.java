package com.example.connectBoard.dto;

import java.time.LocalDate;
import java.time.LocalDateTime;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class FileAttachDTO {
    private Long seq;
    private Long boardSeq;
    private String fileName;
    private Long fileSize;
    private String fileType;
    private String boardType;
    private LocalDateTime insertDt;
    private LocalDateTime updateDt;
}