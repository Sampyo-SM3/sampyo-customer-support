package com.example.connectBoard.dto;

import java.time.LocalDate;
import java.time.LocalDateTime;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class LibraryDTO {
	private Integer seq;
	private String title;
	private String content;
	private String insertId;
	private String category;
	private String viewCount;
	private String importantYn;
	private String dpNm;
	private LocalDate insertDt;
	private LocalDate updateDt;
	
}