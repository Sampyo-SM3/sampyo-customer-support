package com.example.connectBoard.repository.spc;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

import com.example.connectBoard.dto.UserAuthDTO;

@Mapper
public interface UserAuthRepository {
    
    List<UserAuthDTO> getAllUserAuth();
    
    List<UserAuthDTO> getUserAuth(String userId);
    
}