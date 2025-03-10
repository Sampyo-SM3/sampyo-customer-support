package com.example.testProject.repository;

import com.example.testProject.dto.RequireDTO;
import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Select;

import java.util.List;

@Mapper
public interface RequireMapper {
/*    @Select("SELECT * FROM User")*/
    List<RequireDTO> getAllRequires();
    
	RequireDTO getRequire(int seq);

}