package com.example.connectBoard.repository.spc;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.StatusDTO;

@Mapper
public interface StatusRepository {

	List<StatusDTO> getAllStatuses();

	void updateStatus(RequireDTO require);
}