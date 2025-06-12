package com.example.connectBoard.repository.naehwa;

import java.util.List;

import org.apache.ibatis.annotations.Mapper;

import com.example.connectBoard.dto.NaeHwaDTO;
import com.example.connectBoard.dto.RequireDTO;
import com.example.connectBoard.dto.RequireSearchCriteria;

@Mapper
public interface NaeHwaRepository {	
	
	List<NaeHwaDTO> selectNaehwaChartData(NaeHwaDTO naeHwaDTO);
	
	List<NaeHwaDTO> selectNaehwaConfig(NaeHwaDTO naeHwaDTO);
	
	Integer selectNaehwaLength(NaeHwaDTO naeHwaDTO);


}
