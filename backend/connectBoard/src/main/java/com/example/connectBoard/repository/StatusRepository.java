package com.example.connectBoard.repository;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.CommentDTO;
import com.example.connectBoard.dto.StatusDTO;

@Mapper
public interface StatusRepository {

	List<StatusDTO> getAllStatuses();

}