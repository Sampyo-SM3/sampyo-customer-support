package com.example.connectBoard.repository.spc;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;

import java.util.List;
import java.util.Map;

@Mapper
public interface RequireRepository {
/*    @Select("SELECT * FROM User")*/
    List<RequireDTO> getAllRequires();
    
	RequireDTO getRequire(int seq);
	
	List<RequireDTO> searchRequiresByCriteria(RequireSearchCriteria criteria);
	
	List<RequireDTO> getDashboardData(RequireSearchCriteria criteria);
	
	List<RequireDTO> searchRequiresByCriteriaUser(RequireSearchCriteria criteria);
	
	List<RequireDTO> searchRequiresByCriteriaDepart(RequireSearchCriteria criteria);

	List<RequireDTO> searchRequiresByCriteriaDepartAdmin(RequireSearchCriteria criteria);
	
	List<Map<String, Object>> getDashBoardMonthlyTotal(RequireSearchCriteria criteria);
	
	
	
	void insertRequire(RequireDTO require);
	
	void updateForm(RequireDTO require);
	
	void updateSrForm(RequireDTO require);

}