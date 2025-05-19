package com.example.connectBoard.repository.spc;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.CodeDTO;

@Mapper
public interface CodeRepository {
	List<CodeDTO> getCodes(String category);
}