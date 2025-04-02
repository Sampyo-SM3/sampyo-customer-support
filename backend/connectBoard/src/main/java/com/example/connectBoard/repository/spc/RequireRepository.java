package com.example.connectBoard.repository.spc;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;

import java.util.List;

@Mapper
public interface RequireRepository {
/*    @Select("SELECT * FROM User")*/
    List<RequireDTO> getAllRequires();
    
	RequireDTO getRequire(int seq);
	
	List<RequireDTO> searchRequiresByCriteria(RequireSearchCriteria criteria);
	
	void insertRequire(RequireDTO require);
	
	void updateForm(RequireDTO require);
	
	void updateSrForm(RequireDTO require);

}