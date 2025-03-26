package com.example.connectBoard.repository.blueaccount;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

import com.example.connectBoard.dto.EmployeePreferenceDto;

@Mapper
public interface BlueLoginRepository {
    

    /**
     * 사용자 ID로 블루샘 계정 정보 가져오기
     * 
     * @param id 사용자 ID     * 
     * @return 사용자 정보 DTO, 없으면 null
     */
    EmployeePreferenceDto findBlueAccountById(
            @Param("id") String id);               
    

}