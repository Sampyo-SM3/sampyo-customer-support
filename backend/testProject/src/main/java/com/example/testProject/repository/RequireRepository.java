package com.example.testProject.repository;

import com.example.testProject.dto.RequireDTO;
import com.example.testProject.dto.RequireSearchCriteria;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Select;

import java.util.List;

@Mapper
public interface RequireRepository {
/*    @Select("SELECT * FROM User")*/
    List<RequireDTO> getAllRequires();
    
	RequireDTO getRequire(int seq);
	
	List<RequireDTO> searchRequiresByCriteria(RequireSearchCriteria criteria);

}