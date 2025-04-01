package com.example.connectBoard.repository.spc;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

import com.example.connectBoard.dto.UserPopDTO;

@Mapper
public interface UserPopRepository {
    
    List<UserPopDTO> getAllUser(UserPopDTO userPop);
    
}