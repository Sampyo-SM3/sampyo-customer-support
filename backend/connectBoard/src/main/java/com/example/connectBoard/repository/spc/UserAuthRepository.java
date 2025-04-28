package com.example.connectBoard.repository.spc;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

import com.example.connectBoard.dto.UserAuthDTO;

@Mapper
public interface UserAuthRepository {
    
    List<UserAuthDTO> getAllUserAuth();
    
    List<UserAuthDTO> getUserAuth(String userId);
    
    // 추가된 권한 여부 확인
    int checkMenuAuthExists(UserAuthDTO userAuthDTO);

    // 권한 업데이트
    int updateMenuAuth(UserAuthDTO userAuthDTO);

    // 권한 삽입
    int insertMenuAuth(UserAuthDTO userAuthDTO);
    
    // 권한 전체 삭제
    int deleteUserAuth(UserAuthDTO userAuthDTO);
    
}